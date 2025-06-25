// ASMPTool.Model/TestResultItem.cs
namespace ASMPTool.Model
{
    // 單個測試項目的結果
    public class TestResultItem
    {
        public string TestItemName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public double SpendTime { get; set; }
        public string Detail { get; set; } = string.Empty;
    }
}
// ASMPTool.Model/TestResultModel.cs
namespace ASMPTool.Model
{
    // 整體測試結果的資料容器
    public class TestResultModel
    {
        // 儲存每個測試步驟的結果
        public List<TestResultItem> StepResults { get; } = new();

        // 登入與測試過程中的其他資訊
        public string ScanBarcodeNumber { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string MACNumber1 { get; set; } = string.Empty;
        public string MACNumber2 { get; set; } = string.Empty;
        public string MACNumber3 { get; set; } = string.Empty;

        // 最終的測試結果相關資訊
        public string FinalResult { get; set; } = "WAIT";
        public string ErrorCode { get; set; } = string.Empty;
    }
}