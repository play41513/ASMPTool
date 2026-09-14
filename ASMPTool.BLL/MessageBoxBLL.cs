using ASMPTool.DAL;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ASMPTool.BLL
{
    public class MessageBoxBLL
    {
        public static bool ExecuteMessageBox(string FilePath, out string msg)
        {//目前沒使用這個功能，已經用了MessageWindowsDLL替代

            msg = string.Empty;

            if (!OperatingSystem.IsWindows())
            {
                // 如果不是 Windows，直接回傳 true
                return true;
            }
            try
            {
                // Open the program
                Process process = Process.Start(FilePath);
                process?.WaitForExit(); // Wait for the program to close

                // Read the registry value
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\MessageBoxResult");
                if (key != null)
                {
                    msg = (string?)key.GetValue("ExitStatus", string.Empty) ?? string.Empty;
                    if (msg.Contains("ERROR"))
                        return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

            return true;
        }
    }

}
