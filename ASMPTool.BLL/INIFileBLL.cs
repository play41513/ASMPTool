
using ASMPTool.DAL;
using System.Text;
using System.IO;
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
            StringBuilder sb = new();

            for (int i = 0; i < model.Tasks.Count; i++)
            {
                var task = model.Tasks[i];
                string sectionName = $"item{i + 1}";

                // 1. 手動寫入 Section (注意要加中括號)
                sb.AppendLine($"[{sectionName}]");

                // 2. 手動寫入 Key=Value
                sb.AppendLine($"ItemName={task.Name}");
                sb.AppendLine($"Enable={(task.Enable ? "1" : "0")}");
                sb.AppendLine($"Sync={(task.Sync ? "1" : "0")}");
                sb.AppendLine($"FunctionEnable={(task.FunctionTest ? "1" : "0")}");
                sb.AppendLine($"FunctionType={task.FunctionTestType}");
                sb.AppendLine($"FunctionIniPath={task.FunctionTestPath}");

                if (task.RetryTarget > 0)
                {
                    sb.AppendLine($"RetryTarget={task.RetryTarget}");
                }

                for (int j = 0; j < task.NGTest.Count; j++)
                {
                    sb.AppendLine($"NGItem{j + 1}={task.NGTest[j]}");
                }

                // 每個 Section 之間空一行，方便閱讀
                sb.AppendLine();
            }

            // --- 寫入檔案 ---
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
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