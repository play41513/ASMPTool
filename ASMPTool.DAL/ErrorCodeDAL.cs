using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.DAL
{
    public class ErrorCodeDAL
    {
        public static Dictionary<string, string> ReadErrorCodes(string filePath)
        {
            var errorCodes = new Dictionary<string, string>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Error code file not found", filePath);
            }

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.Split(',');
                    if (columns.Length == 2)
                    {
                        var errorCode = columns[0].Trim();
                        var description = columns[1].Trim();
                        errorCodes.Add(errorCode, description);
                    }
                }
            }

            return errorCodes;
        }
    }
}
