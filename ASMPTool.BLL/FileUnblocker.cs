
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ASMPTool.BLL
{
    /// <summary>
    /// 一個靜態工具類別，用於以程式設計方式解除 Windows 對下載檔案的封鎖。
    /// </summary>
    public static class FileUnblocker
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFileW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        /// <summary>
        /// 解除單一檔案的封鎖。
        /// </summary>
        /// <param name="filePath">要解除封鎖的檔案的完整路徑。</param>
        /// <returns>如果成功刪除 Zone.Identifier 或檔案原本就未被封鎖，則為 true。</returns>
        public static bool Unblock(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            string adsPath = filePath + ":Zone.Identifier";
            return DeleteFileW(adsPath);
        }

        /// <summary>
        /// 遞迴地解除一個資料夾內所有檔案的封鎖。
        /// </summary>
        /// <param name="directoryPath">要處理的資料夾路徑。</param>
        public static void UnblockDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            // 解除目前資料夾中的所有檔案
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                Unblock(filePath);
            }

            // 遞迴處理所有子資料夾
            foreach (string subDirectory in Directory.GetDirectories(directoryPath))
            {
                UnblockDirectory(subDirectory);
            }
        }
    }
}