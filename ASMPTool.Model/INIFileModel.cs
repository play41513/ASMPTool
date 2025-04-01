using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.Model
{
    public class ItemTask
    {
        //是否為測試項
        public bool Enable { get; set; }
        //項目名稱
        public string Name { get; set; } = string.Empty;


        //是否為功能測試
        public bool FunctionTest { get; set; }
        //功能測試類型
        public string FunctionTestType { get; set; } = string.Empty;
        //功能測試參數檔路徑
        public string FunctionTestPath { get; set; } = string.Empty;

        //NG後要執行的測試項
        public List<int> NGTest { get; set; } = [];
    }
    public class INIFileModel
    {
        private static INIFileModel? _instance;
        private List<ItemTask> _tasks = [];

        public static INIFileModel Instance
        {
            get
            {
                _instance ??= new INIFileModel();
                return _instance;
            }
        }

        public List<ItemTask> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }
    }
}
