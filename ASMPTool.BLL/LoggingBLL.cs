// ASMPTool.BLL/LoggingBLL.cs

using ASMPTool.DAL;
using ASMPTool.Model;
using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;

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

            string date = DateTime.Now.ToString("yyMMdd_HHmmss_fff");
            string localLogFolder = @"C:\log";
            Directory.CreateDirectory(localLogFolder);


            // --- 組合 Log 字串 ---
            string jsonLogString = ConvertTestResultToJson(testResult);

            // --- 判斷最終結果 ---
            string finalResult = testResult.FinalResult;
            if (finalResult == "TESTTING")
            {
                // 現在可以直接判斷 jsonLogString 是否包含 "FAIL" 或 "ERROR"
                finalResult = (jsonLogString.Contains("\"Result\": \"FAIL\"") || jsonLogString.Contains("\"Result\": \"ERROR\"")) ? "FAIL" : "PASS";
            }

            // --- 寫入本地 ---
            string[] headers = ["ProductName", "EmployeeID", "Version", "Barcode", "UnitNumber", "Date", "Result", "ErrorCode", "WorkOrder", "SN", "MAC1", "MAC2", "MAC3", "LOG"];
            string csvPath = Path.Combine(localLogFolder, $"{date}_{loginInfo.WorkStation}[Result_{finalResult}].csv");
            // 建立一個符合 CSV 規範的安全字串，可以放在LOG欄位裡
            string csvSafeLogString = $"\"{jsonLogString.Replace("\"", "\"\"")}\"";
            using (StreamWriter writer = new(csvPath))
            {
                writer.WriteLine(string.Join(",", headers));
                writer.WriteLine($"{loginInfo.ProductModel},{loginInfo.EmployeeID}," +
                                 $"{loginInfo.Version},{testResult.ScanBarcodeNumber}," +
                                 $"{testResult.UnitNumber},{date}," +
                                 $"{finalResult},{testResult.ErrorCode}," +
                                 $"{loginInfo.WorkOrder},{testResult.SerialNumber}," +
                                 $"{testResult.MACNumber1},{testResult.MACNumber2}," +
                                 $"{testResult.MACNumber3},{csvSafeLogString}");
            }

            // --- 寫入 NAS  ---
            return WriteToNas(loginInfo, testResult, finalResult, csvSafeLogString, date);
        }

        /// <summary>
        /// 將日誌資訊寫入 NAS。
        /// </summary>
        private static bool WriteToNas(LoginInfoModel loginInfo, TestResultModel testResult, string finalResult, string logString, string date)
        {
            try
            {
                // 確保 NAS 的 LOG 路徑可用
                if (!Directory.Exists(loginInfo.NAS_IP_Address))
                {
                    Console.WriteLine($"NAS LOG 路徑 '{loginInfo.NAS_IP_Address}' 無法存取。");
                    return false;
                }

                string csvFolder = Path.Combine(loginInfo.NAS_IP_Address, loginInfo.ProductModel, loginInfo.WorkStation);
                Directory.CreateDirectory(csvFolder);

                string csvPath = Path.Combine(csvFolder, $"{date}_{loginInfo.WorkStation}[Result_{finalResult}].csv");

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
        public static bool CheckNasConnection(string dnsPath, string ipPath)
        {
            return NasConnectionDAL.CheckNasConnection(dnsPath, ipPath);
        }
        /// <summary>
        /// 將 TestResult 的所有步驟轉換為 JSON 格式的 Log 字串
        /// </summary>
        /// <param name="testResult">包含測試結果的物件</param>
        /// <returns>格式化後的 JSON 字串</returns>
        public static string ConvertTestResultToJson(TestResultModel testResult)
        {
            // 建立一個列表，用來存放所有步驟的 LogEntry 物件
            var logEntries = new List<LogEntry>();

            foreach (var stepResult in testResult.StepResults)
            {
                // --- 解析 Detail 字串 ---
                string errorCode = stepResult.Result; // 預設值
                JsonElement dataElement = JsonDocument.Parse("{}").RootElement.Clone(); // 預設為空的 JSON 物件 {}

                // 嘗試從 Detail 中提取 ErrorCode
                Match logMatch = Regex.Match(stepResult.Detail, @"LOG:(.*?)#");
                if (logMatch.Success)
                {
                    errorCode = logMatch.Groups[1].Value.Trim();
                }

                // 嘗試從 Detail 中提取 DATA 的 JSON 部分
                Match dataMatch = Regex.Match(stepResult.Detail, @"DATA:({.*?}|\[.*?\])#");
                if (dataMatch.Success)
                {
                    string jsonString = dataMatch.Groups[1].Value;
                    try
                    {
                        // 解析 JSON 字串並將其根元素 Clone 出來儲存
                        // .Clone() 很重要，因為 JsonDocument 在 using 結束後會被釋放
                        using (JsonDocument doc = JsonDocument.Parse(jsonString))
                        {
                            dataElement = doc.RootElement.Clone();
                        }
                    }
                    catch (JsonException)
                    {
                        // 如果 DATA 後面的字串不是合法的 JSON，則保持 data 為空物件
                        // 可以在此處加入 log 記錄錯誤
                    }
                }

                // --- 組合 LogEntry 物件 ---
                var logEntry = new LogEntry
                {
                    TestStep = stepResult.TestItemName,
                    Result = stepResult.Result,
                    SpendTime = Math.Round(stepResult.SpendTime, 2), // 確保小數點位數
                    ErrorCode = errorCode,
                    Data = dataElement
                };

                logEntries.Add(logEntry);
            }

            // --- 序列化 ---
            // 設定序列化選項，使其輸出排版美觀的 JSON (方便除錯)
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // 如果 JSON 包含中文，建議加上這行以正確編碼
                // Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };

            // 將整個列表序列化成一個 JSON 陣列字串
            return JsonSerializer.Serialize(logEntries, options);
        }
    }
}