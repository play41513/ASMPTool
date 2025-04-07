using ASMP.ViewModel;
using ASMPTool.BLL;
using ASMPTool.Model;
using System;
using System.Data;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ASMPTool
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "IDE0079:SpecifyValidationMode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "IDE1006:SpecifyValidationMode")]
    public partial class UIForm : Form
    {
        private readonly BindingSource bindingSource;
        private readonly BindingSource bindingSource2;
        private readonly System.Windows.Forms.Timer timer;
        private int TotalTimeValue = 0;

        readonly UIFormViewModel _viewModel;
        public UIForm()
        {

            //"WorkStationFile\OE-DI01-H100\A2\1.0.ini"
            InitializeComponent();

            // 創建 BindingSource
            bindingSource = new BindingSource
            {
                DataSource = LoginInfoModel.Instance
            };
            string FilePath = @"WorkStationFile\" + LoginInfoModel.Instance.ProductModel + @"\"
                            + LoginInfoModel.Instance.WorkStation + @"\" + LoginInfoModel.Instance.Version;
            _viewModel = new(this.Handle, FilePath);
            // 將 BindingSource 與控制項綁定

            lbWorkOrder.DataBindings.Add("Text", bindingSource.DataSource, "WorkOrder");
            lbEmployeeID.DataBindings.Add("Text", bindingSource.DataSource, "EmployeeID");
            lbProduct.DataBindings.Add("Text", bindingSource.DataSource, "ProductModel");
            lbWorkStation.DataBindings.Add("Text", bindingSource.DataSource, "WorkStation");
            lbVersion.DataBindings.Add("Text", bindingSource.DataSource, "Version");

            // 創建 BindingSource
            bindingSource2 = new BindingSource
            {
                DataSource = TestResultModel.Instance
            };

            lbResult.DataBindings.Add("Text", bindingSource2.DataSource, "TestResult");
            lbResult.DataBindings.Add("BackColor", bindingSource2.DataSource, "ResultColor");
            textBox.DataBindings.Add("Text", bindingSource2.DataSource, "TextBoxText");


            // 創建 DataGridView
            dataGridView.RowHeadersVisible = false;
            dataGridView.DataSource = DataTableModel.Instance.DataTable;
            dataGridView.Columns[" "].Width = 50;
            dataGridView.Columns["Test Item"].Width = 300;
            dataGridView.Columns["Test Step"].Width = 300;
            dataGridView.Columns["Result"].Width = 120;
            dataGridView.Columns["Spend Time"].Width = 120;
            dataGridView.Columns["Detail"].Width = 2000;
            dataGridView.DataError += (sender, e) =>
            {
                e.ThrowException = false; // 直接抑制錯誤對話框
            };

            //Display time
            timer = new System.Windows.Forms.Timer();
            lbTime.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Interval = 100; // 0.1 秒鐘
            timer.Tick += UpdateTime;
            timer.Start();

            _viewModel.Start();

        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MessageModel.MessageBoxScanBarcodeEnable:
                    TestResultModel.Instance.ScanBarcodeNumber = string.Empty;
                    plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                    plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
                    plMessageBox.Enabled = true;
                    plMessageBox.Visible = true;
                    tBoxScanBarcode.Clear();
                    tBoxScanBarcode.Focus();
                    break;
                case MessageModel.MessageInitializedTotalTime:
                    TotalTimeValue = System.Environment.TickCount;
                    break;
                case MessageModel.MessageTerminatedTotalTime:
                    TotalTimeValue = 0;
                    break;
                case MessageModel.MessageChangeDataGridDisplayRow:
                    if (m.WParam == 0)
                        dataGridView.FirstDisplayedScrollingRowIndex = 0;
                    else if (m.WParam > dataGridView.DisplayedRowCount(true) - 1)
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex = (int)m.WParam - dataGridView.DisplayedRowCount(true) + 2;
                    }
                    break;
                case MessageModel.MessageSaveFileToNAS_FAIL:
                    MessageBox.Show(
                        "  沒有連線到伺服器，傳送LOG失敗! \r\n\r\n  Not connected to the server. LOG transmission failed!"
                        , "Message"
                        , MessageBoxButtons.OK
                        );
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        private void UpdateTime(object? sender, EventArgs e)
        {
            lbTime.Text = DateTime.Now.ToString("HH:mm:ss");
            if (TotalTimeValue != 0)
            {
                lbTotalTime.Text = "Total Time: " + ((System.Environment.TickCount - TotalTimeValue) / 1000.0).ToString("F2");
            }
        }
        private void UIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LoggingBLL.UnmapNetworkDrive(LoginInfoModel.Instance.NAS_IP_Address);
            System.Environment.Exit(0);
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                string value = e.Value?.ToString() ?? string.Empty;
                if (e.CellStyle != null && value != string.Empty)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    e.CellStyle.ForeColor = value switch
                    {
                        " TESTTING" => Color.SteelBlue,
                        " PASSED" => Color.Green,
                        " FAILED" => Color.Red,
                        _ => Color.Black,
                    };
                }
            }
        }

        private void tBoxScanBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                TestResultModel.Instance.ScanBarcodeNumber = tBoxScanBarcode.Text;
                plMessageBox.Visible = false;
                plMessageBox.Enabled = false;
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

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (textBox.Lines.Length == 0) return; // 若沒有內容則不處理

            // 取得最後一行
            string lastLine = textBox.Lines[textBox.Lines.Length - 2];

            // 設定選取範圍到最後一行
            int startIndex = textBox.Text.LastIndexOf(lastLine);
            textBox.Select(startIndex, lastLine.Length);

            // 檢查是否包含 "NG :" 並設定顏色
            if (lastLine.Contains("ErrorCode :"))
            {
                textBox.SelectionColor = Color.Red;
            }
            else
            {
                textBox.SelectionColor = Color.Black;
            }

            // 讓游標滾動到最後
            textBox.ScrollToCaret();
        }

        private void tBoxScanBarcode_TextChanged(object sender, EventArgs e)
        {

        }
    }
}