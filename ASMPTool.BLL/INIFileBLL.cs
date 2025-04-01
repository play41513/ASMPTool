using ASMPTool.DAL;
using ASMPTool.Model;
using System.Data;

namespace ASMPTool.BLL
{

    public class INIFileBLL(string filePath)
    {
        /// <summary>
        /// Retrieves a DataTable containing task information from the INI file model.
        /// </summary>
        /// <param name="dataTable">The DataTable to populate with task information.</param>
        /// <returns>The populated DataTable.</returns>
        public DataTable GetDataTable(DataTable dataTable)
        {
            // Load settings from the INI file model before populating the DataTable.
            LoadSettings();

            // Iterate through each task in the INI file model.
            for (int i = 0; i < INIFileModel.Instance.Tasks.Count; i++)
            {
                var task = INIFileModel.Instance.Tasks[i];
                // Check if the task is enabled.
                if (task.Enable)
                {
                    // Add a new row to the DataTable with the task information.
                    // The first column is the task number (i + 1), the second column is empty, and the third column is the task name.
                    dataTable.Rows.Add(i + 1, "", task.Name);
                }
                else
                {
                    // Add a new row to the DataTable with the task information, but only include the task name.
                    dataTable.Rows.Add(i + 1, task.Name);
                }
            }

            // Return the populated DataTable.
            return dataTable;
        }
        public void LoadSettings()
        {

            // 建立Task列表
            List<ItemTask> tasks = [];

            // 迭代每個section
            for (int index = 1; ; index++)
            {
                string section = $"item{index}";
                // 取得section中的資料
                string name = INIFileDAL.ReadString(filePath,section, "ItemName");
                //如果name值為空則跳出迴圈
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }
                // 建立Task物件
                ItemTask task = new()
                {
                    Enable = INIFileDAL.ReadBoolean(filePath, section, "Enable", false),
                    Name = name,
                    FunctionTest = INIFileDAL.ReadBoolean(filePath, section, "FunctionEnable"),
                    FunctionTestType = INIFileDAL.ReadString(filePath, section, "FunctionType"),
                    FunctionTestPath = INIFileDAL.ReadString(filePath, section, "FunctionIniPath"),
                    NGTest = GetNGTests(section)
                };

                // 加入Task列表
                tasks.Add(task);
            }
            INIFileModel.Instance.Tasks = tasks;
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
    }
}