using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASMPTool.Model
{
    public class LogEntry
    {
        [JsonPropertyName("TestStep")]
        public string? TestStep { get; set; }

        [JsonPropertyName("Result")]
        public string? Result { get; set; }

        [JsonPropertyName("SpendTime")]
        public double? SpendTime { get; set; }

        [JsonPropertyName("ErrorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
