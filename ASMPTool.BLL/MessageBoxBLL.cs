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
        {

            msg = string.Empty;
            // Open the program
            Process process = Process.Start(FilePath);
            process.WaitForExit(); // Wait for the program to close

            // Read the registry value
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\MessageBoxResult")!;
            if (key != null)
            {
                msg = (string)key.GetValue("ExitStatus", string.Empty);
                if (msg.Contains("ERROR"))
                    return false;
            }

            return true;
        }
    }

}
