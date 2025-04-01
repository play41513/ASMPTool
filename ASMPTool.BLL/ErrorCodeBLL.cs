using ASMPTool.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.BLL
{
    public class ErrorCodeBLL
    {
        private readonly Dictionary<string, string> _errorCodes;

        public ErrorCodeBLL()
        {
            _errorCodes = ErrorCodeDAL.ReadErrorCodes("errorcode.csv");
        }

        public string GetErrorCodeKey(string description)
        {
            return _errorCodes.FirstOrDefault(x => description.Contains(x.Value)).Key;
        }
    }
}
