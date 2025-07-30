using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ASMPTool.Model;
using System.Collections.Concurrent;
using PluginContracts;
using System.Reflection;


namespace ASMPTool.DAL
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "IDE0079:SpecifyValidationMode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2101:SpecifyValidationMode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.SysLib", "SYSLIB1054")]

    public class IDLLManagerDAL
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        delegate string MacroTestDelegate(IntPtr ownerHwnd, [MarshalAs(UnmanagedType.LPStr)] string iniPath);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        private static readonly ConcurrentDictionary<string, IntPtr> _nativeDllHandles = new();
        private static readonly ConcurrentDictionary<string, IAutomationPlugin> _netPluginInstances = new();

        private static readonly ConcurrentDictionary<string, MacroTestDelegate> _delegateCache = new();

        private static readonly ConcurrentDictionary<string, object> _libraryLoadLocks = new();

        public static string ExecuteSpecificPlugin(string dllFile, string iniPath, IntPtr ownerHwnd)
        {
            if (IsNetAssembly(dllFile))
            {
                return ExecuteNetPluginInternal(dllFile, iniPath, ownerHwnd);
            }
            else
            {
                return ExecuteNativePluginInternal(dllFile, iniPath, ownerHwnd);
            }
        }

        private static string ExecuteNetPluginInternal(string assemblyPath, string iniPath, IntPtr ownerHwnd)
        {
            object fileLock = _libraryLoadLocks.GetOrAdd(assemblyPath, _ => new object());
            lock (fileLock)
            {
                // NLogDAL.Instance.LogWarning(new NLogModel("載入 .NET 組件: " + assemblyPath, "INFO"));

                if (!_netPluginInstances.TryGetValue(assemblyPath, out IAutomationPlugin pluginInstance))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    Type? pluginType = null;
                    foreach (var type in assembly.GetExportedTypes())
                    {
                        if (typeof(IAutomationPlugin).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            pluginType = type;
                            break;
                        }
                    }

                    if (pluginType == null) throw new InvalidOperationException($"在 {assemblyPath} 中找不到實作 IAutomationPlugin 的類別。");

                    pluginInstance = (IAutomationPlugin)Activator.CreateInstance(pluginType);
                    _netPluginInstances[assemblyPath] = pluginInstance;
                }

                try
                {
                    // NLogDAL.Instance.LogWarning(new NLogModel("進入 .NET 外掛 Execute 函式", "INFO"));
                    string result = pluginInstance.MacroTest(ownerHwnd, iniPath);
                    return result;
                }
                finally
                {
                    ResetWindowTopMost(ownerHwnd);
                }
            }
        }
        private static string ExecuteNativePluginInternal(string dllFile, string iniPath, IntPtr ownerHwnd)
        {
            // 1. 嘗試從快取中直接取得 Delegate
            if (!_delegateCache.TryGetValue(dllFile, out MacroTestDelegate macroTest))
            {
                // 2. 如果快取中沒有，才進入準備階段
                object loadLock = _libraryLoadLocks.GetOrAdd(dllFile, _ => new object());

                // 3. 鎖定載入過程，確保 DLL 只被載入一次
                lock (loadLock)
                {
                    // 4. 雙重檢查，可能在等待鎖的期間，別的執行緒已經完成了載入
                    if (!_delegateCache.TryGetValue(dllFile, out macroTest))
                    {
                        if (!_nativeDllHandles.TryGetValue(dllFile, out IntPtr dllHandle))
                        {
                            dllHandle = LoadLibrary(dllFile);
                            if (dllHandle == IntPtr.Zero)
                            {
                                int errorCode = Marshal.GetLastWin32Error();
                                return $"LOG:ERROR_LOAD_LIBRARY_{errorCode}#";
                            }
                            _nativeDllHandles.TryAdd(dllFile, dllHandle);
                        }

                        IntPtr macroTestPtr = GetProcAddress(dllHandle, "MacroTest");
                        if (macroTestPtr == IntPtr.Zero)
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            return $"LOG:ERROR_LOAD_LIBRARY_GET_PROC_ADDRESS_{errorCode}#";
                        }

                        // 建立 Delegate 並存入快取
                        macroTest = Marshal.GetDelegateForFunctionPointer<MacroTestDelegate>(macroTestPtr);
                        _delegateCache.TryAdd(dllFile, macroTest);
                    }
                }
            } 
            // 5. 在鎖的外面執行函式呼叫，允許多個執行緒同時呼叫 C++ DLL
            try
            {
                // 在鎖的外面執行函式呼叫
                string result = macroTest(ownerHwnd, iniPath);
                return result;
            }
            finally
            {
                ResetWindowTopMost(ownerHwnd);
            }
        }
        private static void ResetWindowTopMost(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                // 呼叫 SetWindowPos 將視窗放在 HWND_NOTOPMOST 層級
                // 使用 SWP_NOMOVE 和 SWP_NOSIZE 旗標來避免移動或改變視窗大小，只改變 Z-order
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }
        // 判斷檔案是否為 .NET 組件
        private static bool IsNetAssembly(string path)
        {
            try
            {
                // AssemblyName.GetAssemblyName 會在檔案不是有效的 .NET 組件時擲回例外
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                // 這明確表示它不是一個 .NET 組件
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static void ReleaseDll(string dllFile)
        {
            // 從 Delegate 快取中移除
            _delegateCache.TryRemove(dllFile, out _);

            // 從原生 DLL 控制代碼快取中移除並釋放
            if (_nativeDllHandles.TryRemove(dllFile, out IntPtr handle))
            {
                FreeLibrary(handle);
            }
            _netPluginInstances.TryRemove(dllFile, out _);
        }

        public static void ReleaseAllDlls()
        {
            _delegateCache.Clear();
            foreach (var dllHandle in _nativeDllHandles.Values)
            {
                FreeLibrary(dllHandle);
            }
            _nativeDllHandles.Clear();
            _netPluginInstances.Clear();
        }
        public static void PosMessage(nint mhwd, int msg,int parm = 0)
        {
            _ = PostMessage(mhwd, msg, parm, 0);
        }
    }
}
