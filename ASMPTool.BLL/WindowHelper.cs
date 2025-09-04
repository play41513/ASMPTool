
using System;
using System.Runtime.InteropServices;

namespace ASMPTool.BLL
{
    /// <summary>
    /// 一個提供原生視窗操作功能的輔助類別。
    /// </summary>
    public static class WindowHelper
    {
        #region Win32 API Declarations



        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        #endregion

        /// <summary>
        /// 強制將指定的視窗帶到最前景並設定焦點。
        /// </summary>
        /// <param name="hWnd">要設定焦點的視窗句柄。</param>
        public static void ForceFocus(IntPtr hWnd)
        {
            // 1. 取得當前前景視窗的線程ID
            IntPtr hForeWnd = GetForegroundWindow();
            uint dwForeThread = GetWindowThreadProcessId(hForeWnd, IntPtr.Zero);

            // 2. 取得我們自己視窗的線程ID
            uint dwAppThread = GetCurrentThreadId();

            // 如果已經是前景，就不需要做任何事
            if (hForeWnd == hWnd) return;

            // 3. 附加線程輸入，讓系統以為是前景程式，允許設定焦點
            AttachThreadInput(dwForeThread, dwAppThread, true);

            // 4. 設定視窗為最上層，並嘗試取得焦點
            // 執行一連串的設定來確保焦點被搶奪過來
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            SetForegroundWindow(hWnd);
            SetActiveWindow(hWnd);
            SetFocus(hWnd);

            // 5. 解除附加，讓各個程式的輸入處理恢復正常
            AttachThreadInput(dwForeThread, dwAppThread, false);

            // 為了確保效果，可以再設定一次
            if (GetForegroundWindow() != hWnd)
            {
                SetForegroundWindow(hWnd);
            }
        }
    }
}