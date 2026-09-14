using ASMPTool.BLL;
using ASMPTool.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASMPTool
{
    public partial class frmMttSelection : Form
    {
        private readonly List<Tuple<string, int>> _testItemHeaders;
        private readonly LoginInfoModel _loginInfo;
        public INIFileModel ModifiedTestPlan { get; private set; }
        private bool _isUpdatingFromCode = false;
        public List<bool> EnabledStates { get; private set; } = [];
        public int LoopCount { get; private set; } = 1;
        public frmMttSelection(List<Tuple<string, int>> testItemHeaders, INIFileModel currentTestPlan, LoginInfoModel loginInfo)
        {
            _testItemHeaders = testItemHeaders;

            ModifiedTestPlan = currentTestPlan.Clone();
            _loginInfo = loginInfo;
            InitializeComponent();
            PopulateMainList();

            // 訂閱事件
            this.checkedListBoxItems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxItems_ItemCheck);
            this.checkedListBoxItems.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxItems_SelectedIndexChanged);
            this.checkedListBoxSubItems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxSubItems_ItemCheck);
        }
        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            _isUpdatingFromCode = true;

            try
            {
                // 清除左側主列表的所有勾選
                for (int i = 0; i < checkedListBoxItems.Items.Count; i++)
                {
                    checkedListBoxItems.SetItemChecked(i, false);
                }

                // 將 Model 中「所有任務」都設為停用
                foreach (var task in ModifiedTestPlan.Tasks)
                {
                    task.Enable = false;
                }

                checkedListBoxSubItems.Items.Clear();
                checkedListBoxSubItems.Enabled = false;
            }
            finally
            {
                _isUpdatingFromCode = false;
            }
        }
        private void PopulateMainList()
        {
            _isUpdatingFromCode = true;
            checkedListBoxItems.Items.Clear();
            foreach (var header in _testItemHeaders)
            {
                int firstStepIndex = header.Item2 + 1;
                bool isGroupEnabled = false;

                if (firstStepIndex < ModifiedTestPlan.Tasks.Count)
                {
                    int nextHeaderIndex = _testItemHeaders.FirstOrDefault(h => h.Item2 > header.Item2)?.Item2 ?? ModifiedTestPlan.Tasks.Count;
                    for (int i = firstStepIndex; i < nextHeaderIndex; i++)
                    {
                        if (ModifiedTestPlan.Tasks[i].Enable)
                        {
                            isGroupEnabled = true;
                            break;
                        }
                    }
                }
                checkedListBoxItems.Items.Add(header.Item1, isGroupEnabled);
            }
            _isUpdatingFromCode = false;

            if (checkedListBoxItems.Items.Count > 0)
            {
                checkedListBoxItems.SelectedIndex = 0;
            }
        }

        private void PopulateSubList(int headerIndex)
        {
            _isUpdatingFromCode = true;
            checkedListBoxSubItems.Items.Clear();

            if (headerIndex < 0 || headerIndex >= _testItemHeaders.Count)
            {
                _isUpdatingFromCode = false;
                return;
            }

            var header = _testItemHeaders[headerIndex];
            int startIndex = header.Item2 + 1;
            int endIndex = (headerIndex + 1 < _testItemHeaders.Count) ? _testItemHeaders[headerIndex + 1].Item2 : ModifiedTestPlan.Tasks.Count;

            for (int i = startIndex; i < endIndex; i++)
            {
                var task = ModifiedTestPlan.Tasks[i];
                checkedListBoxSubItems.Items.Add(task.Name, task.Enable);
            }
            _isUpdatingFromCode = false;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            LoopCount = chkLoopTest.Checked ? (int)numLoopCount.Value : 1;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void chkLoopTest_CheckedChanged(object sender, EventArgs e)
        {
            numLoopCount.Enabled = chkLoopTest.Checked;
        }

        private void checkedListBoxItems_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int selectedIndex = checkedListBoxItems.SelectedIndex;
            if (selectedIndex != -1)
            {
                bool isParentChecked = checkedListBoxItems.GetItemChecked(selectedIndex);
                checkedListBoxSubItems.Enabled = isParentChecked;
                PopulateSubList(selectedIndex);
            }
        }

        private void checkedListBoxItems_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isUpdatingFromCode) return;
            _isUpdatingFromCode = true;

            bool isBeingChecked = (e.NewValue == CheckState.Checked);
            int headerIndex = e.Index;

            // 更新 Model 中的資料
            var header = _testItemHeaders[headerIndex];
            int startIndex = header.Item2 + 1;
            int endIndex = (headerIndex + 1 < _testItemHeaders.Count) ? _testItemHeaders[headerIndex + 1].Item2 : ModifiedTestPlan.Tasks.Count;
            for (int i = startIndex; i < endIndex; i++)
            {
                ModifiedTestPlan.Tasks[i].Enable = isBeingChecked;
            }

            // 如果當前選中的就是正在操作的項目，則同步更新右側UI
            if (checkedListBoxItems.SelectedIndex == headerIndex)
            {
                PopulateSubList(headerIndex);
                checkedListBoxSubItems.Enabled = isBeingChecked;
            }

            _isUpdatingFromCode = false;
        }

        private void checkedListBoxSubItems_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isUpdatingFromCode) return;

            int mainIndex = checkedListBoxItems.SelectedIndex;
            if (mainIndex == -1) return;

            // 1. 更新 Model 中的子項目狀態
            int taskIndex = _testItemHeaders[mainIndex].Item2 + 1 + e.Index;
            if (taskIndex < ModifiedTestPlan.Tasks.Count)
            {
                ModifiedTestPlan.Tasks[taskIndex].Enable = (e.NewValue == CheckState.Checked);
            }

            // 2. 根據所有子項目的狀態，決定主項目的狀態
            bool anySubItemChecked = false;
            for (int i = 0; i < checkedListBoxSubItems.Items.Count; i++)
            {
                // 因為 ItemCheck 事件發生在實際狀態改變前，所以要特別處理正在被改變的項目
                bool currentItemIsChecked = (i == e.Index) ? (e.NewValue == CheckState.Checked) : checkedListBoxSubItems.GetItemChecked(i);
                if (currentItemIsChecked)
                {
                    anySubItemChecked = true;
                    break;
                }
            }

            // 3. 更新主項目的勾選狀態，並防止遞迴
            _isUpdatingFromCode = true;
            checkedListBoxItems.SetItemChecked(mainIndex, anySubItemChecked);
            checkedListBoxSubItems.Enabled = anySubItemChecked; // 如果所有子項都取消了，禁用右邊列表
            _isUpdatingFromCode = false;
        }

        private void picSave_Click(object sender, EventArgs e)
        {
            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "WorkStationFile", _loginInfo.ProductModel, _loginInfo.WorkStation);
            Directory.CreateDirectory(initialDirectory); // 確保路徑存在
            saveFileDialog.InitialDirectory = initialDirectory;
            saveFileDialog.FileName = "MyTestSetting.ini";
            saveFileDialog.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 呼叫 BLL 的儲存方法
                    INIFileBLL.SaveToIni(ModifiedTestPlan, saveFileDialog.FileName);
                    MessageBox.Show("設定已成功儲存!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"儲存失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "WorkStationFile", _loginInfo.ProductModel, _loginInfo.WorkStation);
            if (!Directory.Exists(initialDirectory)) Directory.CreateDirectory(initialDirectory);

            SaveFileDialog sfd = new()
            {
                InitialDirectory = initialDirectory,
                FileName = "TestPlanExport.csv",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 使用新增加的 SaveToCsv 方法
                    INIFileBLL.SaveToCsv(ModifiedTestPlan, sfd.FileName);
                    MessageBox.Show("CSV 匯出成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"匯出失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            // 1. 開啟檔案選擇器讓使用者選 CSV
            OpenFileDialog ofd = new();
            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "WorkStationFile", _loginInfo.ProductModel, _loginInfo.WorkStation);
            if (!Directory.Exists(initialDirectory)) Directory.CreateDirectory(initialDirectory);

            ofd.InitialDirectory = initialDirectory;
            ofd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            ofd.Title = "請選擇要轉換的 CSV 檔案";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 2. 讀取 CSV 內容到臨時 Model (不替換 ModifiedTestPlan)
                    INIFileModel tempModel = INIFileBLL.LoadFromCsv(ofd.FileName);

                    // 3. 準備存檔對話框 (另存新檔)
                    SaveFileDialog sfd = new()
                    {
                        // 設定預設路徑為「該 CSV 的資料夾」
                        InitialDirectory = Path.GetDirectoryName(ofd.FileName),

                        // 設定預設檔名同名，只是副檔名改為 ini
                        FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + ".ini",

                        Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*",
                        Title = "另存為 INI 設定檔"
                    };

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // 4. 將臨時 Model 存為 INI
                        INIFileBLL.SaveToIni(tempModel, sfd.FileName);
                        MessageBox.Show($"轉換成功！\n已儲存至: {sfd.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"轉換失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
