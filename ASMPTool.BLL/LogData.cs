using ASMPTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.BLL
{
    public class LogDisplayInfo
    {
        public string Text { get; set; } = string.Empty; // 用於 "CLEAR_LOG" 等特殊命令
        public bool IsFail { get; set; } = false;

        // 結構化日誌資料
        public string Timestamp { get; set; } = string.Empty;
        public int PassCount { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class LogData
    {
        private static readonly string _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.csv");

        public static (List<LogDisplayInfo> logs, int passCount) Load(LoginInfoModel loginInfo)
        {
            var logs = new List<LogDisplayInfo>();
            int passCount = 0;

            if (!File.Exists(_logFilePath))
            {
                return (logs, passCount);
            }

            var lines = File.ReadAllLines(_logFilePath);
            if (lines.Length < 2) return (logs, passCount);

            string header = lines[0];
            string expectedHeader = $"{loginInfo.WorkOrder},{loginInfo.ProductModel},{loginInfo.WorkStation},{loginInfo.Version}";
            if (header != expectedHeader)
            {
                File.Delete(_logFilePath);
                return (logs, passCount);
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length == 6)
                {
                    string result = parts[3];
                    if (int.TryParse(parts[1], out int count))
                    {
                        passCount = count;
                    }

                    logs.Add(new LogDisplayInfo
                    {
                        Timestamp = parts[0],
                        PassCount = passCount,
                        Barcode = parts[2],
                        Result = result,
                        ErrorCode = parts[4],
                        ErrorMessage = parts[5],
                        IsFail = result == "FAIL"
                    });
                }
            }

            return (logs, passCount);
        }

        public static void Append(LoginInfoModel loginInfo, LogDisplayInfo log)
        {
            if (!File.Exists(_logFilePath) || File.ReadLines(_logFilePath).FirstOrDefault() != $"{loginInfo.WorkOrder},{loginInfo.ProductModel},{loginInfo.WorkStation},{loginInfo.Version}")
            {
                File.WriteAllText(_logFilePath, $"{loginInfo.WorkOrder},{loginInfo.ProductModel},{loginInfo.WorkStation},{loginInfo.Version}\r\n");
            }

            // 使用結構化資料建立 CSV 行
            string csvLine = $"{log.Timestamp},{log.PassCount},{log.Barcode},{log.Result},{log.ErrorCode},{log.ErrorMessage}";
            File.AppendAllText(_logFilePath, csvLine + "\r\n");
        }
    }
}
