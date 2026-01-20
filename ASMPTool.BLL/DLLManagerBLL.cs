// ASMPTool.BLL/DLLManagerBLL.cs

using ASMPTool.DAL;
using ASMPTool.Model;
using System.Text.Json;
using System;
using System.Text.RegularExpressions;


namespace ASMPTool.BLL
{
    public class DLLManagerBLL
    {
        public static bool ExecuteSpecificPlugin(string dllFile, string iniPath, out string msg, IntPtr ownerHwnd, TestResultModel testResult, bool isRetry = false)
        {
            string dllName = Path.GetFileName(dllFile);
            Console.WriteLine($"[DLLManagerBLL] 準備呼叫 DLL: {dllName} (Retry={isRetry})");

            string result = IDLLManagerDAL.ExecuteSpecificPlugin(dllFile, iniPath, ownerHwnd, isRetry);
            msg = result;
            Console.WriteLine($"[DLLManagerBLL] {dllName} 回傳結果: {result}");

            StringAnalysis(result, testResult);
            bool isSuccess = !result.Contains("ERROR");
            if (!isSuccess)
            {
                Console.WriteLine($"[DLLManagerBLL] {dllName} 判定失敗 (包含 ERROR)"); // [Log]
            }
            return !result.Contains("ERROR");
        }

        public static void StringAnalysis(string result, TestResultModel testResult)
        {
            // 提取 DATA: 後面的 JSON 物件
            Match dataMatch = Regex.Match(result, @"DATA:({.*?})#");

            if (dataMatch.Success)
            {
                string jsonString = dataMatch.Groups[1].Value;
                Console.WriteLine($"[DLLManagerBLL] 抓取到 JSON 資料: {jsonString}");

                try
                {
                    // 解析提取出來的 JSON 字串
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    // 嘗試讀取每個 key 的值，這樣即使某個 key 不存在也不會出錯
                    if (root.TryGetProperty("MAC1", out JsonElement mac1Element))
                    {
                        testResult.MACNumber1 = mac1Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        Console.WriteLine($"[DLLManagerBLL] 解析 MAC1: {testResult.MACNumber1}");
                    }

                    if (root.TryGetProperty("MAC2", out JsonElement mac2Element))
                    {
                        testResult.MACNumber2 = mac2Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        Console.WriteLine($"[DLLManagerBLL] 解析 MAC2: {testResult.MACNumber1}");
                    }

                    if (root.TryGetProperty("MAC3", out JsonElement mac3Element))
                    {
                        testResult.MACNumber3 = mac3Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        Console.WriteLine($"[DLLManagerBLL] 解析 MAC3: {testResult.MACNumber1}");
                    }

                    if (root.TryGetProperty("SN", out JsonElement snElement))
                    {
                        testResult.SerialNumber = snElement.GetString()?.Trim().ToUpper() ?? string.Empty;
                        Console.WriteLine($"[DLLManagerBLL] 解析 SN: {testResult.SerialNumber}");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[DLLManagerBLL] JSON 解析失敗: {ex.Message}");
                }
            }
            else
            {
                if (!result.StartsWith("LOG:ERROR"))
                {
                    Console.WriteLine($"[DLLManagerBLL] 注意: 回傳字串中未發現 'DATA:{{...}}#' 格式");
                }
            }
        }
        public static void ReleaseDll(string dllFile)
        {
            Console.WriteLine($"[DLLManagerBLL] 釋放 DLL: {Path.GetFileName(dllFile)}");
            IDLLManagerDAL.ReleaseDll(dllFile);
        }

        public static void ReleaseAllDlls()
        {
            Console.WriteLine("[DLLManagerBLL] 釋放所有 DLL");
            IDLLManagerDAL.ReleaseAllDlls();
        }

        public static void PostMessage(nint mhwd, int msg, int parm = 0)
        {
            IDLLManagerDAL.PosMessage(mhwd, msg, parm);
        }
    }
}