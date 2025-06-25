// ASMPTool.BLL/LoggingBLL.cs

using ASMPTool.DAL;
using ASMPTool.Model;
using System;
using System.IO;

namespace ASMPTool.BLL
{
    public class LoggingBLL
    {
        /// <summary>
        /// 儲存測試日誌，包含本地儲存和NAS儲存。
        /// </summary>
        /// <param name="loginInfo">包含登入使用者和產品資訊的模型。</param>
        /// <param name="testResult">包含測試結果的模型。</param>
        /// <param name="testPlan">包含測試項目的模型。</param>
        /// <returns>如果寫入 NAS 成功或無需寫入，則為 true。</returns>
        public static bool SaveToCSV(LoginInfoModel loginInfo, TestResultModel testResult)
        {
            // Debug 模式：輸入0則不儲存
            if (testResult.ScanBarcodeNumber == "0")
                return true;

            string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string localLogFolder = @"C:\log";
            Directory.CreateDirectory(localLogFolder);
            string csvPath = Path.Combine(localLogFolder, $"{date}_{loginInfo.WorkStation}.csv");

            // --- 組合 Log 字串 ---
            string logString = "";

            foreach (var stepResult in testResult.StepResults)
            {
                logString += $"{{{stepResult.TestItemName}__{stepResult.Result}__{stepResult.SpendTime:F2}__{stepResult.Detail}}}";
            }
            string finalResult = testResult.FinalResult;
            if (finalResult == "TESTTING")
            {
                finalResult = (logString.Contains("FAIL") || logString.Contains("ERROR")) ? "FAIL" : "PASS";
            }

            // --- 寫入本地 ---
            string[] headers = ["ProductName", "EmployeeID", "Version", "Barcode", "UnitNumber", "Date", "Result", "ErrorCode", "WorkOrder", "SN", "MAC1", "MAC2", "MAC3", "LOG"];
            using (StreamWriter writer = new(csvPath))
            {
                writer.WriteLine(string.Join(",", headers));
                writer.WriteLine($"{loginInfo.ProductModel},{loginInfo.EmployeeID}," +
                                 $"{loginInfo.Version},{testResult.ScanBarcodeNumber}," +
                                 $"{testResult.UnitNumber},{date}," +
                                 $"{finalResult},{testResult.ErrorCode}," +
                                 $"{loginInfo.WorkOrder},{testResult.SerialNumber}," +
                                 $"{testResult.MACNumber1},{testResult.MACNumber2}," +
                                 $"{testResult.MACNumber3},{logString}");
            }

            // --- 寫入 NAS  ---
            return WriteToNas(loginInfo, testResult, finalResult, logString, date);
        }

        /// <summary>
        /// 將日誌資訊寫入 NAS。
        /// </summary>
        private static bool WriteToNas(LoginInfoModel loginInfo, TestResultModel testResult, string finalResult, string logString, string date)
        {
            try
            {
                // 確保 NAS 的 LOG 路徑可用
                if (!NasConnectionDAL.CheckLogPathConnection(loginInfo.NAS_IP_Address))
                {
                    return false;
                }

                string csvFolder = Path.Combine(loginInfo.NAS_IP_Address, loginInfo.ProductModel, loginInfo.WorkStation);
                Directory.CreateDirectory(csvFolder);

                string csvPath = Path.Combine(csvFolder, $"{date}_{loginInfo.WorkStation}.csv");

                string[] headers = ["ProductName", "EmployeeID", "Version", "Barcode", "UnitNumber", "Date", "Result", "ErrorCode", "WorkOrder", "SN", "MAC1", "MAC2", "MAC3", "LOG"];
                using (StreamWriter writer = new(csvPath))
                {
                    writer.WriteLine(string.Join(",", headers));
                    writer.WriteLine($"{loginInfo.ProductModel},{loginInfo.EmployeeID}," +
                                     $"{loginInfo.Version},{testResult.ScanBarcodeNumber}," +
                                     $"{testResult.UnitNumber},{date}," +
                                     $"{finalResult},{testResult.ErrorCode}," +
                                     $"{loginInfo.WorkOrder},{testResult.SerialNumber}," +
                                     $"{testResult.MACNumber1},{testResult.MACNumber2}," +
                                     $"{testResult.MACNumber3},{logString}");
                }

                Console.WriteLine("CSV 檔案成功寫入 NAS：" + csvPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"寫入 NAS LOG 時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 檢查與 NAS 的基本連線狀態。
        /// </summary>
        /// <returns>如果可以連線到預設路徑，則為 true。</returns>
        public static bool CheckNasConnection()
        {
            return NasConnectionDAL.CheckLogPathConnection();
        }
    }
}