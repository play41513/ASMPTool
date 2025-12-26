
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

            // 1. 寫入標題列
            sb.AppendLine("Enable,ItemName,FunctionEnable,FunctionType,FunctionIniPath,Sync,RetryTarget,NGItem1,NGItem2,NGItem3");

            // 2. 寫入每一列資料
            foreach (var task in model.Tasks)
            {
                var ng1 = task.NGTest.Count > 0 ? task.NGTest[0].ToString() : "";
                var ng2 = task.NGTest.Count > 1 ? task.NGTest[1].ToString() : "";
                var ng3 = task.NGTest.Count > 2 ? task.NGTest[2].ToString() : "";

                // 組合 CSV 行
                string line = $"{(task.Enable ? "1" : "0")}," +
                              $"{task.Name}," +
                              $"{(task.FunctionTest ? "1" : "0")}," +
                              $"{task.FunctionTestType}," +
                              $"{task.FunctionTestPath}," +
                              $"{(task.Sync ? "1" : "0")}," +
                              $"{task.RetryTarget}," +
                              $"{ng1},{ng2},{ng3}";

                sb.AppendLine(line);
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 從 CSV 檔案讀取並轉換為 INIFileModel
        /// </summary>
        public static INIFileModel LoadFromCsv(string filePath)
        {
            var model = new INIFileModel();
            var lines = File.ReadAllLines(filePath);

            // 跳過標題列，從第一行開始讀取
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 簡單的 CSV 分割 (假設內容不含逗號)
                string[] parts = line.Split(',');

                // 確保欄位數量足夠 (至少到 RetryTarget)
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
                    // 處理 NG Items (index 7, 8, 9)
                    NGTest = []
                };
                for (int j = 7; j <= 9; j++)
                {
                    if (parts.Length > j && int.TryParse(parts[j].Trim(), out int ngVal) && ngVal != 0)
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