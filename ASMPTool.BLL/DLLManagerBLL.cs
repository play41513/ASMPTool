// ASMPTool.BLL/DLLManagerBLL.cs

using ASMPTool.DAL;
using ASMPTool.Model;
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
            Match mac1Match = Regex.Match(result, @"MAC1:(.*?)#");
            if (mac1Match.Success)
            {
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