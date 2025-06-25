// ASMPTool.BLL/DLLManagerBLL.cs

using ASMPTool.DAL;
using ASMPTool.Model;
using System;
using System.Text.RegularExpressions; // 為了更安全的解析，可以考慮使用正規表達式

namespace ASMPTool.BLL
{
    public class DLLManagerBLL
    {
        // *** 修改點 1: 新增 TestResultModel 參數 ***
        public static bool ExecuteSpecificPlugin(string dllFile, string iniPath, out string msg, IntPtr ownerHwnd, TestResultModel testResult)
        {
            string result = IDLLManagerDAL.ExecuteSpecificPlugin(dllFile, iniPath, ownerHwnd);
            msg = result;

            // *** 修改點 2: 將 testResult 傳遞給 StringAnalysis ***
            StringAnalysis(result, testResult);

            return !result.Contains("ERROR");
        }

        // *** 修改點 3: 新增 TestResultModel 參數 ***
        public static void StringAnalysis(string result, TestResultModel testResult)
        {
            // 使用正規表達式來安全地解析字串，避免因找不到 '#' 而拋出例外
            Match mac1Match = Regex.Match(result, @"MAC1:(.*?)#");
            if (mac1Match.Success)
            {
                // *** 修改點 4: 使用傳入的 testResult 物件 ***
                testResult.MACNumber1 = mac1Match.Groups[1].Value.Trim().ToUpper();
            }

            Match mac2Match = Regex.Match(result, @"MAC2:(.*?)#");
            if (mac2Match.Success)
            {
                testResult.MACNumber2 = mac2Match.Groups[1].Value.Trim().ToUpper();
            }

            Match mac3Match = Regex.Match(result, @"MAC3:(.*?)#");
            if (mac3Match.Success)
            {
                testResult.MACNumber3 = mac3Match.Groups[1].Value.Trim().ToUpper();
            }

            Match snMatch = Regex.Match(result, @"SN:(.*?)#");
            if (snMatch.Success)
            {
                testResult.SerialNumber = snMatch.Groups[1].Value.Trim().ToUpper();
            }
        }

        // --- 以下方法無需修改 ---
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