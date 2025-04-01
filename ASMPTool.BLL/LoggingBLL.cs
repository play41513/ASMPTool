using ASMPTool.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.BLL
{
    public class LoggingBLL
    {
        public static bool SaveToCSV()
        {
            // CSV檔案路徑
            string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string csvPath = @"C:\log\"+ date + LoginInfoModel.Instance.WorkStation+".csv";
            if (!Directory.Exists(@"C:\log"))
                Directory.CreateDirectory(@"C:\log");

            string[] headers = [ "ProductName","EmployeeID", "Version", "Barcode", "UnitNumber", "Date",
                                "Result","ErrorCode","WorkOrder","SN","MAC1","MAC2","MAC3","LOG" ]; // CSV檔案標題
            // 寫入標題
            using StreamWriter writer = new(csvPath);
            writer.WriteLine(string.Join(",", headers)); 
            // 寫入資料                                             
            string logString = "";

            for (int i = 0; i < TestResultModel.Instance.TableContent.Count; i++)
            {
                if (TestResultModel.Instance.TableContent[i].Result != "")
                {
                    logString += "{"+ INIFileModel.Instance.Tasks[i].Name + "__"
                        + TestResultModel.Instance.TableContent[i].Result + "__"
                        + TestResultModel.Instance.TableContent[i].SpendTime + "__"                        
                        + TestResultModel.Instance.TableContent[i].Detail + "}";
                }
            }
            string result = TestResultModel.Instance.TestResult;
            if (result == "TESTTING")//有時TestResultModel.Instance.TestResult未更新
            {
                if(logString.Contains("FAIL") || logString.Contains("ERROR"))
                    result = "FAILED";
                else
                    result = "PASSED";
            }
            writer.WriteLine($"{LoginInfoModel.Instance.ProductModel},{LoginInfoModel.Instance.EmployeeID}" +
                $",{LoginInfoModel.Instance.Version},{TestResultModel.Instance.ScanBarcodeNumber}" +
                $",{TestResultModel.Instance.UnitNumber},{date}" +
                $",{result},{TestResultModel.Instance.ErrorCode}" +
                $",{LoginInfoModel.Instance.WorkOrder},{TestResultModel.Instance.SerialNumber}" +
                $",{TestResultModel.Instance.MACNumber1},{TestResultModel.Instance.MACNumber2}" +
                $",{TestResultModel.Instance.MACNumber3},{logString}");

            writer.Close();

            return SaveToSWTool(result, logString);
        }
        public static bool SaveToSWTool(string result,string logString)
        {
            string networkPath = LoginInfoModel.Instance.NAS_IP_Address;
            string username = "user";
            string password = "user1234";
            bool isMapped = false; // 用於記錄是否成功掛載
            bool bPassed = false;
            try
            {
                // 掛載網路磁碟
                isMapped = MapNetworkDrive(networkPath, username, password);
                if (!isMapped)
                {
                    Console.WriteLine("無法掛載網路磁碟，寫入取消！");
                    return false;
                }

                // CSV檔案存放路徑
                string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string csvFolder = Path.Combine(networkPath, LoginInfoModel.Instance.ProductModel, LoginInfoModel.Instance.WorkStation);

                // 檢查目標資料夾是否存在，不存在則建立
                if (!Directory.Exists(csvFolder))
                    Directory.CreateDirectory(csvFolder);

                // 建立 CSV 完整路徑
                string csvPath = Path.Combine(csvFolder, $"{date}_{LoginInfoModel.Instance.WorkStation}.csv");

                // CSV 標題
                string[] headers = [ "ProductName", "EmployeeID", "Version", "Barcode", "UnitNumber", "Date",
                        "Result", "ErrorCode", "WorkOrder", "SN", "MAC1", "MAC2", "MAC3", "LOG" ];

                // 開始寫入 CSV
                using (StreamWriter writer = new(csvPath))
                {
                    writer.WriteLine(string.Join(",", headers));
                    writer.WriteLine($"{LoginInfoModel.Instance.ProductModel},{LoginInfoModel.Instance.EmployeeID}" +
                        $",{LoginInfoModel.Instance.Version},{TestResultModel.Instance.ScanBarcodeNumber}" +
                        $",{TestResultModel.Instance.UnitNumber},{date}" +
                        $",{result},{TestResultModel.Instance.ErrorCode}" +
                        $",{LoginInfoModel.Instance.WorkOrder},{TestResultModel.Instance.SerialNumber}" +
                        $",{TestResultModel.Instance.MACNumber1},{TestResultModel.Instance.MACNumber2}" +
                        $",{TestResultModel.Instance.MACNumber3},{logString}");
                }

                Console.WriteLine("CSV 檔案寫入成功：" + csvPath);
                bPassed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("寫入 CSV 失敗：" + ex.Message);
                bPassed = false;
            }
            finally
            {
                // 如果掛載成功，則解除網路磁碟
                if (isMapped)
                {
                    UnmapNetworkDrive(networkPath);
                    Console.WriteLine("已解除網路磁碟掛載：" + networkPath);
                }               
            }
            return bPassed;
        }
        public static bool MapNetworkDrive(string networkPath, string username, string password)
        {
            if (networkPath.Contains(@"192.168.20.1"))
            {
                try
                {
                    ProcessStartInfo psi = new("net")
                    {
                        Arguments = $"use {networkPath} /user:{username} {password}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process? process = Process.Start(psi);
                    process?.WaitForExit();
                    return process?.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("掛載失敗：" + ex.Message);
                    return false;
                }
            }
            else
                return true;
        }
        public static void UnmapNetworkDrive(string networkPath)
        {
            if (networkPath.Contains(@"192.168.20.1"))
            {
                try
                {
                    ProcessStartInfo psi = new("net")
                    {
                        Arguments = $"use {networkPath} /delete /yes",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process? process = Process.Start(psi);
                    process?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("解除掛載失敗：" + ex.Message);
                }
            }
            else
                return;
        }
    }
}
