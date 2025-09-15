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
        public static bool ExecuteSpecificPlugin(string dllFile, string iniPath, out string msg, IntPtr ownerHwnd, TestResultModel testResult)
        {
            string result = IDLLManagerDAL.ExecuteSpecificPlugin(dllFile, iniPath, ownerHwnd);
            msg = result;

            StringAnalysis(result, testResult);

            return !result.Contains("ERROR");
        }

        public static void StringAnalysis(string result, TestResultModel testResult)
        {
            // 提取 DATA: 後面的 JSON 物件
            Match dataMatch = Regex.Match(result, @"DATA:({.*?})#");

            if (dataMatch.Success)
            {
                string jsonString = dataMatch.Groups[1].Value;

                try
                {
                    // 解析提取出來的 JSON 字串
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        JsonElement root = doc.RootElement;

                        // 嘗試讀取每個 key 的值，這樣即使某個 key 不存在也不會出錯
                        if (root.TryGetProperty("MAC1", out JsonElement mac1Element))
                        {
                            testResult.MACNumber1 = mac1Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        }

                        if (root.TryGetProperty("MAC2", out JsonElement mac2Element))
                        {
                            testResult.MACNumber2 = mac2Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        }

                        if (root.TryGetProperty("MAC3", out JsonElement mac3Element))
                        {
                            testResult.MACNumber3 = mac3Element.GetString()?.Trim().ToUpper() ?? string.Empty;
                        }

                        if (root.TryGetProperty("SN", out JsonElement snElement))
                        {
                            testResult.SerialNumber = snElement.GetString()?.Trim().ToUpper() ?? string.Empty;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parsing failed: {ex.Message}");
                }
            }
        }
        public static void ReleaseDll(string dllFile)
        {
            IDLLManagerDAL.ReleaseDll(dllFile);
        }

        public static void ReleaseAllDlls()
        {
            IDLLManagerDAL.ReleaseAllDlls();
        }

        public static void PostMessage(nint mhwd, int msg, int parm = 0)
        {
            IDLLManagerDAL.PosMessage(mhwd, msg, parm);
        }
    }
}