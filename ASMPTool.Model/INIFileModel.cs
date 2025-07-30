
namespace ASMPTool.Model
{
    public class ItemTask
    {
        public bool Enable { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Sync { get; set; }
        public bool FunctionTest { get; set; }
        public string FunctionTestType { get; set; } = string.Empty;
        public string FunctionTestPath { get; set; } = string.Empty;
        public List<int> NGTest { get; set; } = new();
    }

    public class INIFileModel
    {
        public List<ItemTask> Tasks { get; set; } = new();
    }
}