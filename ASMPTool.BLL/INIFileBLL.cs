
using ASMPTool.DAL;
using ASMPTool.Model;
using System.Collections.Generic;

namespace ASMPTool.BLL
{
    public class INIFileBLL
    {
        private readonly string _filePath;

        public INIFileBLL(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// 從 INI 檔案中讀取設定，並將其載入到一個新的 INIFileModel 物件中。
        /// </summary>
        /// <returns>包含測試計畫的INIFileModel。</returns>
        public INIFileModel LoadToModel()
        {
            // 1. 建立Model 物件
            var model = new INIFileModel();
            var tasks = new List<ItemTask>();

            // 2. 迭代每個 section 來讀取資料
            for (int index = 1; ; index++)
            {
                string section = $"item{index}";
                string name = INIFileDAL.ReadString(_filePath, section, "ItemName");

                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                ItemTask task = new()
                {
                    Enable = INIFileDAL.ReadBoolean(_filePath, section, "Enable", false),
                    Name = name,
                    Sync = INIFileDAL.ReadBoolean(_filePath, section, "Sync", false),
                    FunctionTest = INIFileDAL.ReadBoolean(_filePath, section, "FunctionEnable"),
                    FunctionTestType = INIFileDAL.ReadString(_filePath, section, "FunctionType"),
                    FunctionTestPath = INIFileDAL.ReadString(_filePath, section, "FunctionIniPath"),
                    RetryTarget = INIFileDAL.ReadInteger(_filePath, section, "RetryTarget"),
                    NGTest = GetNGTests(section)
                };
                tasks.Add(task);
            }

            // 3. 將讀取到的 tasks 列表賦值給新建立的 model 物件
            model.Tasks = tasks;

            // 4. 回傳這個包含完整資料的 model 物件
            return model;
        }
        public static void SaveToIni(INIFileModel model, string filePath)
        {
            // 如果檔案存在，先刪除，以確保是全新的寫入
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            for (int i = 0; i < model.Tasks.Count; i++)
            {
                string section = $"item{i + 1}";
                var task = model.Tasks[i];

                INIFileDAL.WriteString(filePath, section, "ItemName", task.Name);
                INIFileDAL.WriteString(filePath, section, "Enable", task.Enable ? "1" : "0");
                INIFileDAL.WriteString(filePath, section, "Sync", task.Sync ? "1" : "0");
                INIFileDAL.WriteString(filePath, section, "FunctionEnable", task.FunctionTest ? "1" : "0");
                INIFileDAL.WriteString(filePath, section, "FunctionType", task.FunctionTestType);
                INIFileDAL.WriteString(filePath, section, "FunctionIniPath", task.FunctionTestPath);

                if (task.RetryTarget > 0)
                {
                    INIFileDAL.WriteString(filePath, section, "RetryTarget", task.RetryTarget.ToString());
                }

                for (int j = 0; j < task.NGTest.Count; j++)
                {
                    INIFileDAL.WriteString(filePath, section, $"NGItem{j + 1}", task.NGTest[j].ToString());
                }
            }
        }
        private List<int> GetNGTests(string section)
        {
            List<int> ngTests = [];
            int ngIndex = 1;

            while (true)
            {
                int result = INIFileDAL.ReadInteger(_filePath, section, $"NGItem{ngIndex}");
                if (result != 0)
                {
                    ngTests.Add(result);
                    ngIndex++;
                }
                else
                {
                    break;
                }
            }
            return ngTests;
        }
    }
}