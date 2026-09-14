
using ASMPTool.DAL;
using System.Text;
using System.IO;
using ASMPTool.Model;
using System.Collections.Generic;

namespace ASMPTool.BLL
{
    public class INIFileBLL(string filePath)
    {
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
                string name = INIFileDAL.ReadString(filePath, section, "ItemName");

                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                ItemTask task = new()
                {
                    Enable = INIFileDAL.ReadBoolean(filePath, section, "Enable", false),
                    Name = name,
                    Sync = INIFileDAL.ReadBoolean(filePath, section, "Sync", false),
                    FunctionTest = INIFileDAL.ReadBoolean(filePath, section, "FunctionEnable"),
                    FunctionTestType = INIFileDAL.ReadString(filePath, section, "FunctionType"),
                    FunctionTestPath = INIFileDAL.ReadString(filePath, section, "FunctionIniPath"),
                    RetryTarget = INIFileDAL.ReadInteger(filePath, section, "RetryTarget"),
                    NGTest = GetNGTests(section),
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

                // 1. 手動寫入 Section
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
                int result = INIFileDAL.ReadInteger(filePath, section, $"NGItem{ngIndex}");
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
        /// <summary>
        /// 將 Model 匯出為 CSV 格式
        /// </summary>
        public static void SaveToCsv(INIFileModel model, string filePath)
        {
            StringBuilder sb = new();

            //找出最大 NG 項目數量，決定需要多少個 NGItem 欄位
            int maxNgCount = model.Tasks.Count != 0 ? model.Tasks.Max(t => t.NGTest.Count) : 0;

            List<string> headers = ["Enable", "ItemName", "FunctionEnable", "FunctionType", "FunctionIniPath", "Sync", "RetryTarget"];
            for (int i = 1; i <= maxNgCount; i++)
            {
                headers.Add($"NGItem{i}");
            }
            sb.AppendLine(string.Join(",", headers));

            foreach (var task in model.Tasks)
            {
                List<string> fields =
                [
                    task.Enable ? "1" : "0",
                    task.Name,
                    task.FunctionTest ? "1" : "0",
                    task.FunctionTestType,
                    task.FunctionTestPath,
                    task.Sync ? "1" : "0",
                    task.RetryTarget.ToString()
                ];
                foreach (var ng in task.NGTest)
                {
                    fields.Add(ng.ToString());
                }
                // 如果該列 NG 數量不足 maxNgCount，補空白欄位
                for (int i = task.NGTest.Count; i < maxNgCount; i++)
                {
                    fields.Add("");
                }

                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 從 CSV 檔案讀取並轉換為 INIFileModel
        /// </summary>
        public static INIFileModel LoadFromCsv(string filePath)
        {
            var model = new INIFileModel();
            if (!File.Exists(filePath)) return model;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(',');

                if (parts.Length < 7) continue;

                var task = new ItemTask
                {
                    Enable = parts[0].Trim() == "1",
                    Name = parts[1].Trim(),
                    FunctionTest = parts[2].Trim() == "1",
                    FunctionTestType = parts[3].Trim(),
                    FunctionTestPath = parts[4].Trim(),
                    Sync = parts[5].Trim() == "1",
                    RetryTarget = int.TryParse(parts[6].Trim(), out int rt) ? rt : 0,
                    NGTest = []
                };

                for (int j = 7; j < parts.Length; j++)
                {
                    string val = parts[j].Trim();
                    if (!string.IsNullOrEmpty(val) && int.TryParse(val, out int ngVal) && ngVal != 0)
                    {
                        task.NGTest.Add(ngVal);
                    }
                }

                model.Tasks.Add(task);
            }
            return model;
        }
    }
}