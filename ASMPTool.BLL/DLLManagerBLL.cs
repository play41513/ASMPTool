using ASMPTool.DAL;
using ASMPTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.BLL
{
    public class DLLManagerBLL
    {
        public static bool ExecuteSpecificPlugin(string dllFile, string iniPath,out string msg)
        {
            string result = IDLLManagerDAL.ExecuteSpecificPlugin(dllFile, iniPath);
            msg = string.Empty;
            if (result != "")
                msg = result;
            StringAnalysis(result);
            if (result.Contains("ERROR"))
                return false;

            return true;
        }
        public static void StringAnalysis(string result)
        {
            if (result.Contains("MAC1:"))
            {
                string temp = result;
                temp = temp.Remove(0, temp.IndexOf("MAC1:"));
                temp = temp[5..temp.IndexOf('#')].ToUpper();
                TestResultModel.Instance.MACNumber1 = temp;
            }
            if (result.Contains("MAC2:"))
            {
                string temp = result;
                temp = temp.Remove(0, temp.IndexOf("MAC2:"));
                temp = temp[5..temp.IndexOf('#')].ToUpper();
                TestResultModel.Instance.MACNumber2 = temp;
            }
            if (result.Contains("MAC3:"))
            {
                string temp = result;
                temp = temp.Remove(0, temp.IndexOf("MAC3:"));
                temp = temp[5..temp.IndexOf('#')].ToUpper();
                TestResultModel.Instance.MACNumber3 = temp;
            }
            if (result.Contains("SN:"))
            {
                string temp = result;
                temp = temp.Remove(0, temp.IndexOf("SN:"));
                temp = temp[5..temp.IndexOf('#')].ToUpper();
                TestResultModel.Instance.SerialNumber = temp;
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
