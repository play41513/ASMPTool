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
        // 新增針對 PLCControlDLL 的 3 參數 Delegate
        // 假設第三個參數是 string，如果是 int 請改為 int 並且移除 MarshalAs
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        delegate string MacroTestDelegate3Param(IntPtr ownerHwnd, [MarshalAs(UnmanagedType.LPStr)] string iniPath, bool Retry);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        private static readonly ConcurrentDictionary<string, IntPtr> _nativeDllHandles = new();
        private static readonly ConcurrentDictionary<string, IAutomationPlugin> _netPluginInstances = new();

        private static readonly ConcurrentDictionary<string, Delegate> _delegateCache = new();

        private static readonly ConcurrentDictionary<string, object> _libraryLoadLocks = new();

        public static string ExecuteSpecificPlugin(string dllFile, string iniPath, IntPtr ownerHwnd,bool retry = false)
        {
            if (IsNetAssembly(dllFile))
            {
                return ExecuteNetPluginInternal(dllFile, iniPath, ownerHwnd);
            }
            else
            {
                return ExecuteNativePluginInternal(dllFile, iniPath, ownerHwnd,retry);
            }
        }

        private static string ExecuteNetPluginInternal(string assemblyPath, string iniPath, IntPtr ownerHwnd)
        {
            object fileLock = _libraryLoadLocks.GetOrAdd(assemblyPath, _ => new object());
            lock (fileLock)
            {
                // NLogDAL.Instance.LogWarning(new NLogModel("載入 .NET 組件: " + assemblyPath, "INFO"));

                if (!_netPluginInstances.TryGetValue(assemblyPath, out IAutomationPlugin? pluginInstance))
                {
                    Console.WriteLine($"[DLLManagerDAL] 首次載入 .NET DLL: {Path.GetFileName(assemblyPath)}"); // [Log]
                    try
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

                        object? instance = Activator.CreateInstance(pluginType);
                        if (instance is IAutomationPlugin plugin)
                        {
                            pluginInstance = plugin;
                        }
                        else
                        {
                            throw new InvalidOperationException($"無法建立 {pluginType.FullName} 的實體。");
                        }
                        _netPluginInstances[assemblyPath] = pluginInstance;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DLLManagerDAL] 載入 .NET DLL 發生例外: {ex.Message}"); // [Log]
                        throw;
                    }
                }
                string result = pluginInstance!.MacroTest(ownerHwnd, iniPath);
                return result;
            }
        }
        private static string ExecuteNativePluginInternal(string dllFile, string iniPath, IntPtr ownerHwnd, bool retry)
        {
            // 1. 嘗試從快取中直接取得 Delegate
            if (!_delegateCache.TryGetValue(dllFile, out Delegate? macroTestAny))
            {
                // 2. 如果快取中沒有，才進入準備階段
                object loadLock = _libraryLoadLocks.GetOrAdd(dllFile, _ => new object());

                // 3. 鎖定載入過程，確保 DLL 只被載入一次
                lock (loadLock)
                {
                    // 4. 雙重檢查，可能在等待鎖的期間，別的執行緒已經完成了載入
                    if (!_delegateCache.TryGetValue(dllFile, out macroTestAny))
                    {
                        Console.WriteLine($"[DLLManagerDAL] 首次載入 Native DLL: {Path.GetFileName(dllFile)}");
                        if (!_nativeDllHandles.TryGetValue(dllFile, out IntPtr dllHandle))
                        {
                            dllHandle = LoadLibrary(dllFile);
                            if (dllHandle == IntPtr.Zero)
                            {
                                int errorCode = Marshal.GetLastWin32Error();
                                Console.WriteLine($"[DLLManagerDAL] LoadLibrary 失敗! ErrorCode: {errorCode} (Path: {dllFile})");
                                return $"LOG:ERROR_LOAD_LIBRARY_{errorCode}#";
                            }
                            _nativeDllHandles.TryAdd(dllFile, dllHandle);
                        }

                        IntPtr macroTestPtr = GetProcAddress(dllHandle, "MacroTest");
                        if (macroTestPtr == IntPtr.Zero)
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            Console.WriteLine($"[DLLManagerDAL] 找不到函式進入點 'MacroTest'! ErrorCode: {errorCode}");
                            return $"LOG:ERROR_LOAD_LIBRARY_GET_PROC_ADDRESS_{errorCode}#";
                        }

                        Delegate macroTest;
                        if (dllFile.EndsWith("PLCControlDLL.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            macroTest = Marshal.GetDelegateForFunctionPointer<MacroTestDelegate3Param>(macroTestPtr);
                        }
                        else
                        {
                            macroTest = Marshal.GetDelegateForFunctionPointer<MacroTestDelegate>(macroTestPtr);
                        }
                        _delegateCache.TryAdd(dllFile, macroTest);
                        macroTestAny = macroTest;
                    }
                }
            } 
            // 5. 在鎖的外面執行函式呼叫，允許多個執行緒同時呼叫 C++ DLL
            try
            {
                // 根據 Delegate 類型進行轉型與呼叫
                if (macroTestAny is MacroTestDelegate3Param method3)
                {
                    return method3(ownerHwnd, iniPath, retry);
                }
                else if (macroTestAny is MacroTestDelegate method2)
                {
                    return method2(ownerHwnd, iniPath);
                }
                return "LOG:ERROR_UNKNOWN_DELEGATE_TYPE#";
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
            catch (Exception ex)
            {
                Console.WriteLine($"[DLLManagerDAL] 檢查 DLL 格式時發生錯誤: {ex.Message}"); 
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
                Console.WriteLine($"[DLLManagerDAL] 已釋放 Native DLL: {Path.GetFileName(dllFile)}");
            }
            _netPluginInstances.TryRemove(dllFile, out _);
        }

        public static void ReleaseAllDlls()
        {
            Console.WriteLine("[DLLManagerDAL] 正在釋放所有 DLL 資源...");
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
