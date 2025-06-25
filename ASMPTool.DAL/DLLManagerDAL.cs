using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ASMPTool.Model;


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

        private static readonly Dictionary<string, IntPtr> _dllHandles = [];

        public static string ExecuteSpecificPlugin(string dllFile, string iniPath, IntPtr ownerHwnd)
        {
            NLogDAL.Instance.LogWarning(new NLogModel("載入 DLL: " + dllFile, "INFO"));
            NLogDAL.Instance.LogWarning(new NLogModel("INI路徑: " + iniPath, "INFO"));
            if (_dllHandles.TryGetValue(dllFile, out IntPtr dllHandle))
            {
                // 如果已經存在相同的 DLL 處理器，則直接使用現有的處理器
                NLogDAL.Instance.LogWarning(new NLogModel("已經存在相同的 DLL 處理器，直接使用現有的處理器", "INFO"));
                IntPtr macroTestPtr = GetProcAddress(dllHandle, "MacroTest");
                if (macroTestPtr == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    return $"LOG:ERROR_LOAD_LIBRARY_GET_PROC_ADDRESS_{errorCode}#";
                }
                NLogDAL.Instance.LogWarning(new NLogModel("進入DLL MacroTest函式", "INFO"));
                MacroTestDelegate macroTest = Marshal.GetDelegateForFunctionPointer<MacroTestDelegate>(macroTestPtr);
                string result = macroTest(ownerHwnd,iniPath);
                NLogDAL.Instance.LogWarning(new NLogModel("MacroTest函式結束 回傳: " + result, "INFO"));
                return result;
            }
            else
            {
                // 如果不存在相同的 DLL 處理器，則載入 DLL 並存放處理器
                NLogDAL.Instance.LogWarning(new NLogModel("不存在相同的 DLL 處理器，載入 DLL 並存放處理器", "INFO"));
                dllHandle = LoadLibrary(dllFile);
                NLogDAL.Instance.LogWarning(new NLogModel("載入完成", "INFO"));
                if (dllHandle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    return $"LOG:ERROR_LOAD_LIBRARY_{errorCode}#";
                }
                _dllHandles[dllFile] = dllHandle;

                IntPtr macroTestPtr = GetProcAddress(dllHandle, "MacroTest");
                if (macroTestPtr == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    return $"LOG:ERROR_LOAD_LIBRARY_GET_PROC_ADDRESS_{errorCode}#";
                }
                NLogDAL.Instance.LogWarning(new NLogModel("進入DLL MacroTest函式", "INFO"));
                MacroTestDelegate macroTest = Marshal.GetDelegateForFunctionPointer<MacroTestDelegate>(macroTestPtr);
                string result = macroTest(ownerHwnd,iniPath);
                NLogDAL.Instance.LogWarning(new NLogModel("MacroTest函式結束 回傳: " + result, "INFO"));
                return result;
            }
        }
        // 當不再需要 DLL 檔案時，可以呼叫以下方法來釋放 DLL 檔案
        public static void ReleaseDll(string dllFile)
        {
            if (_dllHandles.TryGetValue(dllFile, out nint value))
            {
                FreeLibrary(value);
                _dllHandles.Remove(dllFile);
            }
        }

        // 當不再需要任何 DLL 檔案時，可以呼叫以下方法來釋放所有 DLL 檔案
        public static void ReleaseAllDlls()
        {
            foreach (var dllHandle in _dllHandles.Values)
            {
                FreeLibrary(dllHandle);
            }
            _dllHandles.Clear();
        }
        public static void PosMessage(nint mhwd, int msg,int parm = 0)
        {
            _ = PostMessage(mhwd, msg, parm, 0);
        }
    }
}
