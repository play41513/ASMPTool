// ASMPTool/UIForm.cs

using ASMP.ViewModel;
using ASMPTool.BLL;
using ASMPTool.Model;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Collections.Generic;

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
            // --- ���ô�� (Data Bindings) ---
            // �N UI ������ݩʻP ViewModel ���ݩʤ@�@�����C
            // �� ViewModel ���ݩʧ��ܮɡAUI �|�۰ʧ�s�C

            // ������T����
            lbWorkOrder.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.WorkOrder));
            lbEmployeeID.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.EmployeeID));
            lbProduct.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.ProductModel));
            lbWorkStation.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.WorkStation));
            lbVersion.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.Version));
            lbTime.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.CurrentTime));

            // ���յ��G�ϰ�
            lbResult.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.OverallResult));
            lbResult.DataBindings.Add("BackColor", _viewModel, nameof(UIFormViewModel.OverallResultColor));
            lbTotalTime.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.TotalTestTime));
            lbLoopStatus.DataBindings.Add("Text", _viewModel, nameof(UIFormViewModel.LoopStatus));

            // DataGridView ����ƨӷ��]�w
            // �N DataGridView �� DataSource ���V ViewModel ���� BindingList<TestStepViewModel>�C
            SetupDataGridViewColumns();
            dataGridView.DataSource = _viewModel.TestSteps;

            // --- �ƥ�q�\ (Event Subscriptions) ---
            // �q�\ ViewModel ���ƥ�A�H�K�b ViewModel �ݭn�ɡA���� View �~�వ���ާ@�C
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
                    // �ھ� ViewModel �����A�A��ʳ]�w Visible �ݩ�
                    plMessageBox.Visible = _viewModel.IsScanBarcodeVisible;
                    
                    // �p�G�O�n��ܭ��O�A�N����m���B�M�ũM��J���ާ@
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
                // ���ͱa�����D���榡�Ʀr��          
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
                textBox.SelectionColor = textBox.ForeColor; // �٭�w�]�C��
            }
        }

        #endregion

        #region UI Event Handlers

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
            // ���ó̥��䪺����Y
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