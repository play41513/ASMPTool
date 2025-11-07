// ASMPTool/UIForm.cs

using ASMP.ViewModel;
using ASMPTool.BLL;
using ASMPTool.Model;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ASMPTool
{
    public partial class UIForm : Form
    {

        private readonly UIFormViewModel _viewModel;
        private readonly LoginInfoModel _loginInfo;

        public UIForm(LoginInfoModel loginInfo)
        {
            InitializeComponent();
            _loginInfo = loginInfo;
            _viewModel = new UIFormViewModel(this.Handle, loginInfo);
            SetupBindingsAndEvents();
        }

        private void SetupBindingsAndEvents()
        {
            // --- 資料繫結 (Data Bindings) ---
            // 將 UI 控制項的屬性與 ViewModel 的屬性一一對應。
            // 當 ViewModel 的屬性改變時，UI 會自動更新。

            // 底部資訊標籤
            lbWorkOrder.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.WorkOrder));
            lbEmployeeID.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.EmployeeID));
            lbProduct.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.ProductModel));
            lbWorkStation.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.WorkStation));
            lbVersion.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.Version));
            lbTime.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.CurrentTime));

            // 測試結果區域
            lbResult.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.OverallResult));
            lbResult.DataBindings.Add("BackColor", _viewModel, nameof(UIFormViewModel.OverallResultColor));
            lbTotalTime.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.TotalTestTime));
            lbLoopStatus.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.LoopStatus));

            // DataGridView 的資料來源設定
            // 將 DataGridView 的 DataSource 指向 ViewModel 中的 BindingList<TestStepViewModel>。
            SetupDataGridViewColumns();
            dataGridView.DataSource = _viewModel.TestSteps;

            // --- 事件訂閱 (Event Subscriptions) ---
            // 訂閱 ViewModel 的事件，以便在 ViewModel 需要時，執行 View 才能做的操作。
            _viewModel.ScrollToRowRequested += OnScrollToRowRequested;
            _viewModel.ShowMessageRequested += OnShowMessageRequested;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.LogMessageAppended += OnLogMessageAppended;
            _viewModel.ShowMttSelectionDialogRequested += OnShowMttSelectionDialogRequested;
        }

        #region ViewModel Event Handlers

        private MttSettings? OnShowMttSelectionDialogRequested(List<Tuple<string, int>> headers, INIFileModel testPlan)
        {
            using (var mttForm = new frmMttSelection(headers, testPlan, _loginInfo))
            {
                if (mttForm.ShowDialog(this) == DialogResult.OK)
                {
                    return new MttSettings
                    {
                        ModifiedTestPlan = mttForm.ModifiedTestPlan,
                        LoopCount = mttForm.LoopCount
                    };
                }
            }
            return null;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.Invoke(() =>
            {
                if (e.PropertyName == nameof(UIFormViewModel.IsScanBarcodeVisible))
                {
                    // 根據 ViewModel 的狀態，手動設定 Visible 屬性
                    plMessageBox.Visible = _viewModel.IsScanBarcodeVisible;
                    
                    // 如果是要顯示面板，就執行置中、清空和對焦等操作
                    if (_viewModel.IsScanBarcodeVisible)
                    {
                        plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                        plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
                        tBoxScanBarcode.Clear();
                        WindowHelper.ForceFocus(this.Handle);
                        tBoxScanBarcode.Focus();
                    }
                }
            });
        }

        private void OnScrollToRowRequested(int rowIndex)
        {
            this.Invoke(() =>
            {
                if (rowIndex >= 0 && rowIndex < dataGridView.RowCount)
                {
                    int firstDisplayed = dataGridView.FirstDisplayedScrollingRowIndex;
                    int displayedCount = dataGridView.DisplayedRowCount(true);
                    if (rowIndex < firstDisplayed || rowIndex >= firstDisplayed + displayedCount)
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex);
                    }
                }
            });
        }

        private void OnShowMessageRequested(string message)
        {
            this.Invoke(() => MessageBox.Show(this, message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            tBoxScanBarcode.Focus();
        }

        private void OnLogMessageAppended(LogDisplayInfo logInfo)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnLogMessageAppended(logInfo));
                return;
            }

            if (logInfo.Text == "CLEAR_LOG")
            {
                textBox.Clear();
            }
            else
            {
                if(logInfo.Barcode == null || logInfo.Barcode == "0") 
                    return;
                // 產生帶有標題的格式化字串          
                string displayText = $"{logInfo.PassCount.ToString("D5")} |" +
                                     $"{logInfo.Timestamp}| " +
                                     $"[Barcode :{logInfo.Barcode}] " +
                                     $"[Result :{logInfo.Result}] ";
                if (logInfo.Result != "PASS")
                {
                    displayText += $"[ErrorCode :{logInfo.ErrorCode}] " +
                                     $"[Detail :{logInfo.ErrorMessage}]";
                }
                displayText += "\r\n";

                textBox.SelectionStart = textBox.TextLength;
                textBox.SelectionLength = 0;
                textBox.SelectionColor = logInfo.IsFail ? Color.Red : Color.Black;
                textBox.AppendText(displayText);
                textBox.SelectionColor = textBox.ForeColor; // 還原預設顏色
            }
        }

        #endregion

        #region UI Event Handlers
        private void restartExplorerMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 尋找所有名為 "explorer" 的處理程序
                Process[] explorerProcesses = Process.GetProcessesByName("explorer");
                foreach (Process process in explorerProcesses)
                {
                    // 強制結束處理程序
                    process.Kill();
                }
                // Windows 通常會自動偵測到 explorer.exe 被關閉並重新啟動它
                MessageBox.Show(this, "Windows 檔案總管已成功重啟。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"重啟檔案總管時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void openDiagramMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 從 ViewModel 取得 ProductModel 和 WorkStation 來組合路徑
                string relativePath = Path.Combine(
                    "ItemParameter",
                    _viewModel.ProductModel,
                    _viewModel.WorkStation,
                    "EquipmentDiagram.jpg"
                );
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                if (File.Exists(fullPath))
                {
                    // 使用 Process.Start 開啟檔案，並設定 UseShellExecute = true
                    // 這樣會使用系統預設的圖片檢視器開啟
                    Process.Start(new ProcessStartInfo(fullPath)
                    {
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(this, $"找不到示意圖檔案：\n{fullPath}", "檔案不存在", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"開啟檔案時發生錯誤：\n{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void tBoxScanBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                string inputText = tBoxScanBarcode.Text.Trim();
                if (inputText.Equals("MTT", StringComparison.OrdinalIgnoreCase))
                {
                    _viewModel.EnterMttModeCommand.Execute(null);
                }
                else if (!string.IsNullOrWhiteSpace(inputText))
                {
                    _viewModel.BarcodeEnteredCommand.Execute(inputText);
                }
            }
        }

        private void UIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }

        #endregion

        #region Pure UI Logic

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["Result"]?.Index)
            {
                if (_viewModel.TestSteps.Count > e.RowIndex)
                {
                    var stepVM = _viewModel.TestSteps[e.RowIndex];
                    if (stepVM != null && e.CellStyle != null)
                    {
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        e.CellStyle.ForeColor = stepVM.ResultColor;
                    }
                }
            }
        }

        private void UIForm_Resize(object sender, EventArgs e)
        {
            if (plMessageBox.Visible)
            {
                plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
            }
        }

        private void SetupDataGridViewColumns()
        {
            dataGridView.AutoGenerateColumns = false;
            // 隱藏最左邊的行標頭
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "IndexColumn", HeaderText = " ", DataPropertyName = "Index", Width = 50, ReadOnly = true });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TestItemColumn", HeaderText = "Test Item", DataPropertyName = "TestItem", Width = 300, ReadOnly = true });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TestStepColumn", HeaderText = "Test Step", DataPropertyName = "TestStep", Width = 300, ReadOnly = true });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Result", HeaderText = "Result", DataPropertyName = "Result", Width = 120, ReadOnly = true });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "SpendTimeColumn", HeaderText = "Spend Time", DataPropertyName = "SpendTime", Width = 120, ReadOnly = true });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DetailColumn", HeaderText = "Detail", DataPropertyName = "Detail", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }

        private void UIForm_Load(object sender, EventArgs e) { }

        private void lbResult_DoubleClick(object sender, EventArgs e)
        {
            plMessageBox.Visible = !plMessageBox.Visible;
            plMessageBox.Enabled = !plMessageBox.Enabled;
            if(plMessageBox.Visible)
                tBoxScanBarcode.Focus();
        }

        #endregion
    }
}