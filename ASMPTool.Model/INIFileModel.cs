
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
        public List<int> NGTest { get; set; } = [];

        public int RetryTarget { get; set; } = 0;
        public bool PostTask { get; set; } = false;
        public ItemTask Clone()
        {
            return new ItemTask
            {
                Enable = this.Enable,
                Name = this.Name,
                Sync = this.Sync,
                FunctionTest = this.FunctionTest,
                FunctionTestType = this.FunctionTestType,
                FunctionTestPath = this.FunctionTestPath,
                NGTest = new List<int>(this.NGTest),
                RetryTarget = this.RetryTarget,
                PostTask = this.PostTask
            };
        }
    }

    public class INIFileModel
    {
        public List<ItemTask> Tasks { get; set; } = [];

        public INIFileModel Clone()
        {
            var newModel = new INIFileModel
            {
                Tasks = this.Tasks.Select(t => t.Clone()).ToList()
            };
            return newModel;
        }
    }
}