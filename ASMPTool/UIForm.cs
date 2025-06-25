// ASMPTool/UIForm.cs

using ASMP.ViewModel;
using ASMPTool.BLL;
using ASMPTool.Model;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace ASMPTool
{
    public partial class UIForm : Form
    {
        // 詳解: View 只持有對應的 ViewModel 的一個唯讀實例。
        private readonly UIFormViewModel _viewModel;

        // 詳解: 修改建構函式，它不再需要知道如何載入資料，
        // 而是直接接收從登入畫面傳遞過來的、已經包含所有必要資訊的 LoginInfoModel 物件。
        public UIForm(LoginInfoModel loginInfo)
        {
            InitializeComponent();

            // --- ViewModel 的建立與資料準備 ---
            // 1. 根據登入資訊，準備測試計畫檔案的路徑。
            string filePath = $@"WorkStationFile\{loginInfo.ProductModel}\{loginInfo.WorkStation}\{loginInfo.Version}";

            // 2. 透過 BLL 載入測試計畫。
            var iniFileBll = new INIFileBLL(filePath);
            var testPlan = iniFileBll.LoadToModel();

            // 3. 建立 ViewModel，並將所有需要的資料（視窗Handle、登入資訊）傳入。
            _viewModel = new UIFormViewModel(this.Handle, loginInfo);

            // 4. 設定所有的資料繫結與事件訂閱。
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
        }

        #region ViewModel Event Handlers (由 ViewModel 觸發，View 執行)

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(UIFormViewModel.IsScanBarcodeVisible):
                        // 根據 ViewModel 的狀態，手動設定 Visible 屬性
                        plMessageBox.Visible = _viewModel.IsScanBarcodeVisible;

                        // 如果是要顯示面板，就執行置中、清空和對焦等操作
                        if (_viewModel.IsScanBarcodeVisible)
                        {
                            plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                            plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
                            tBoxScanBarcode.Clear();
                            tBoxScanBarcode.Focus();
                        }
                        break;
                }
            });
        }

        private void OnScrollToRowRequested(int rowIndex)
        {
            this.Invoke(() =>
            {
                if (rowIndex >= 0 && rowIndex < dataGridView.RowCount)
                {
                    // 確保要滾動到的行是可見的
                    dataGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - dataGridView.DisplayedRowCount(true) + 1);
                }
            });
        }

        private void OnShowMessageRequested(string message)
        {
            this.Invoke(() => MessageBox.Show(this, message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            tBoxScanBarcode.Focus();
        }

        #endregion

        #region UI Event Handlers (將操作委派給 ViewModel)

        private void tBoxScanBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(tBoxScanBarcode.Text))
            {
                _viewModel.BarcodeEnteredCommand.Execute(tBoxScanBarcode.Text);
                e.Handled = true; // 避免發出 "叮" 的聲音
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
            // 根據 ViewModel 中計算好的顏色來設定字體顏色
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["Result"].Index)
            {
                var stepVM = _viewModel.TestSteps[e.RowIndex];
                if (stepVM != null && e.CellStyle != null)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    e.CellStyle.ForeColor = stepVM.ResultColor;
                }
            }
        }

        private void UIForm_Resize(object sender, EventArgs e)
        {
            // 視窗大小改變時，重新置中掃描面板
            if (plMessageBox.Visible)
            {
                plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
            }
        }
        private void SetupDataGridViewColumns()
        {
            // 關閉自動產生欄位，我們將完全手動控制
            dataGridView.AutoGenerateColumns = false;
            // 隱藏最左邊的行標頭
            dataGridView.RowHeadersVisible = false;

            // 欄位 1: Index (序號)
            var indexCol = new DataGridViewTextBoxColumn
            {
                Name = "IndexColumn",
                HeaderText = " ", // 欄位標題文字
                DataPropertyName = "Index", // ***對應到 TestStepViewModel 的 "Index" 屬性 ***
                Width = 50,
                ReadOnly = true
            };
            dataGridView.Columns.Add(indexCol);

            // 欄位 2: TestItem
            var testItemCol = new DataGridViewTextBoxColumn
            {
                Name = "TestItemColumn",
                HeaderText = "Test Item",
                DataPropertyName = "TestItem", // *** 對應到 TestStepViewModel 的 "TestItem" 屬性 ***
                Width = 300,
                ReadOnly = true
            };
            dataGridView.Columns.Add(testItemCol);

            // 欄位 3: TestStep
            var testStepCol = new DataGridViewTextBoxColumn
            {
                Name = "TestStepColumn",
                HeaderText = "Test Step",
                DataPropertyName = "TestStep", // *** 對應到 TestStepViewModel 的 "TestStep" 屬性 ***
                Width = 300,
                ReadOnly = true
            };
            dataGridView.Columns.Add(testStepCol);

            // 欄位 4: Result
            var resultCol = new DataGridViewTextBoxColumn
            {
                Name = "Result", // Name要與 CellFormatting 中的判斷一致
                HeaderText = "Result",
                DataPropertyName = "Result", // *** 對應到 TestStepViewModel 的 "Result" 屬性 ***
                Width = 120,
                ReadOnly = true
            };
            dataGridView.Columns.Add(resultCol);

            // 欄位 5: SpendTime
            var spendTimeCol = new DataGridViewTextBoxColumn
            {
                Name = "SpendTimeColumn",
                HeaderText = "Spend Time",
                DataPropertyName = "SpendTime", // *** 對應到 TestStepViewModel 的 "SpendTime" 屬性 ***
                Width = 120,
                ReadOnly = true
            };
            dataGridView.Columns.Add(spendTimeCol);

            // 欄位 6: Detail
            var detailCol = new DataGridViewTextBoxColumn
            {
                Name = "DetailColumn",
                HeaderText = "Detail",
                DataPropertyName = "Detail", // *** 對應到 TestStepViewModel 的 "Detail" 屬性 ***
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, // 自動填滿剩餘寬度
                ReadOnly = true
            };
            dataGridView.Columns.Add(detailCol);
        }
        private void OnLogMessageAppended(string message)
        {
            // 確保在 UI 執行緒上執行
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnLogMessageAppended(message));
                return;
            }

            // 如果收到清空信號，就清空 TextBox
            if (message == "CLEAR_LOG")
            {
                textBox.Clear();
            }
            else
            {
                // 否則，使用 AppendText 高效地附加新日誌
                textBox.AppendText(message);
            }
        }

        #endregion

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            // 1. 將插入符 (游標) 移動到文字的結尾
            textBox.SelectionStart = textBox.Text.Length;
            // 2. 捲動檢視以顯示插入符所在的位置
            textBox.ScrollToCaret();
        }
    }
}