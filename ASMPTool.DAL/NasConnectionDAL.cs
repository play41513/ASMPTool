using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Net.NetworkInformation;


namespace ASMPTool.DAL
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "IDE0079:SpecifyValidationMode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2101:SpecifyValidationMode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.SysLib", "SYSLIB1054")]

    public class NasConnectionDAL
    {
        // Windows API
        [DllImport("mpr.dll", CharSet = CharSet.Ansi)]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);


        [DllImport("mpr.dll", CharSet = CharSet.Ansi)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public int Scope;
            public int Type;
            public int DisplayType;
            public int Usage;
            public string? LocalName;
            public string? RemoteName;
            public string? Comment;
            public string? Provider;
        }

        private const int RESOURCETYPE_DISK = 0x00000001;
        private const int CONNECT_TEMPORARY = 0x00000004;

        // 預設路徑
        private const string DEFAULT_LOG_PATH = @"\\swtool\swtool\logs";

        /// <summary>
        /// 檢查並建立LOG的連線
        /// </summary>
        /// <param name="logPath">log路徑  預設為\\swtool\swtool\log</param>
        /// <returns>連接是否成功</returns>
        public static bool CheckLogPathConnection(string logPath = DEFAULT_LOG_PATH)
        {
            try
            {
                // 檢查當前網段
                string currentIPAddress = NasConnectionDAL.GetLocalIPAddress();
                Console.WriteLine($"IP地址: {currentIPAddress}");

                // 是否為可用的網路路徑
                if (NasConnectionDAL.IsPathAccessible(logPath))
                {
                    Console.WriteLine("LOG路徑可用！");
                    return true;
                }

                // 判斷是否需要帳密驗證
                if (NasConnectionDAL.IsInAuthRequiredSubnet(currentIPAddress))
                {
                    Console.WriteLine("當前網段需要帳密驗證");
                    return ConnectWithCredentials(logPath, "user1234", "user1234");
                }
                else
                {
                    Console.WriteLine("當前網段不需要帳密驗證");
                    return ConnectWithoutCredentials(logPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"連接網路路徑時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 獲取本地IP地址
        /// </summary>
        /// <returns>本地IP地址字符串</returns>
        private static string GetLocalIPAddress()
        {
            string localIP = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        /// <summary>
        /// 檢查IP地址是否在需要帳密驗證的網段中
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>是否需要帳密驗證</returns>
        private static bool IsInAuthRequiredSubnet(string ipAddress)
        {
            // 檢查是否在192.168.20.x網段
            return ipAddress.StartsWith("192.168.20.");
        }

        /// <summary>
        /// 檢查路徑是否可訪問
        /// </summary>
        /// <param name="path">網路路徑</param>
        /// <returns>路徑是否可訪問</returns>
        private static bool IsPathAccessible(string path)
        {
            try
            {
                // 嘗試列出目錄內容來驗證
                Directory.GetDirectories(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用帳密連接到NAS
        /// </summary>
        /// <param name="networkPath">路徑</param>
        /// <param name="username">帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>連接是否成功</returns>
        private static bool ConnectWithCredentials(string networkPath, string username, string password)
        {
            try
            {
                // 取消所有現有的連線
                _ = WNetCancelConnection2(networkPath, 0, true);

                // 設置
                NetResource netResource = new()
                {
                    Scope = 2,
                    Type = RESOURCETYPE_DISK,
                    DisplayType = 3,
                    Usage = 1,
                    LocalName = null,
                    RemoteName = networkPath,
                    Comment = null,
                    Provider = null
                };

                // 連接
                int result = WNetAddConnection2(netResource, password, username, CONNECT_TEMPORARY);

                if (result == 0)
                {
                    Console.WriteLine("使用帳密成功連接到NAS");
                    return true;
                }
                else
                {
                    Console.WriteLine($"連接失敗，錯誤代碼: {result}");
                    throw new Win32Exception(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"使用帳密連接時，發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 無帳密連接
        /// </summary>
        /// <param name="networkPath">路徑</param>
        /// <returns>連接是否成功</returns>
        private static bool ConnectWithoutCredentials(string networkPath)
        {
            try
            {                
                if (!NasConnectionDAL.IsPathAccessible(networkPath))
                {
                    // 如果直接連接失敗，嘗試使用空帳密連接
                    NetResource netResource = new()
                    {
                        Scope = 2,
                        Type = RESOURCETYPE_DISK,
                        DisplayType = 3,
                        Usage = 1,
                        LocalName = null,
                        RemoteName = networkPath,
                        Comment = null,
                        Provider = null
                    };

                    int result = WNetAddConnection2(netResource, "", "", CONNECT_TEMPORARY);

                    if (result == 0)
                    {
                        Console.WriteLine("無帳密成功連接NAS");
                        return true;
                    }
                    else
                    {
                        result = WNetAddConnection2(netResource, "user1234", "user1234", CONNECT_TEMPORARY);
                        if (result == 0)
                        {
                            Console.WriteLine("帳密成功連接NAS");
                            return true;
                        }
                        Console.WriteLine($"無帳密連接時，發生錯誤: {result}");
                        throw new Win32Exception(result);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"無帳密連接時，發生錯誤: {ex.Message}");
                return false;
            }
        }
    }
}
