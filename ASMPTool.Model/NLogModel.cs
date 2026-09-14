using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.Model
{
    public class NLogModel(string message, string logLevel, Exception? exception = null)
    {
        public string Message { get; set; } = message;
        public string LogLevel { get; set; } = logLevel;
        public Exception? Exception { get; set; } = exception;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
