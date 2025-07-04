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

        private readonly UIFormViewModel _viewModel;

        public UIForm(LoginInfoModel loginInfo)
        {
            InitializeComponent();

            // --- ViewModel ���إ߻P��Ʒǳ� ---
            // 1. �ھڵn�J��T�A�ǳƴ��խp�e�ɮת����|�C
            string filePath = $@"WorkStationFile\{loginInfo.ProductModel}\{loginInfo.WorkStation}\{loginInfo.Version}";

            // 2. �z�L BLL ���J���խp�e�C
            var iniFileBll = new INIFileBLL(filePath);
            var testPlan = iniFileBll.LoadToModel();

            // 3. �إ� ViewModel�A�ñN�Ҧ��ݭn����ơ]����Handle�B�n�J��T�^�ǤJ�C
            _viewModel = new UIFormViewModel(this.Handle, loginInfo);

            // 4. �]�w�Ҧ������ô���P�ƥ�q�\�C
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
        }

        #region ViewModel Event Handlers (�� ViewModel Ĳ�o�AView ����)

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(UIFormViewModel.IsScanBarcodeVisible):
                        // �ھ� ViewModel �����A�A��ʳ]�w Visible �ݩ�
                        plMessageBox.Visible = _viewModel.IsScanBarcodeVisible;

                        // �p�G�O�n��ܭ��O�A�N����m���B�M�ũM��J���ާ@
                        if (_viewModel.IsScanBarcodeVisible)
                        {
                            plMessageBox.Top = this.Height / 2 - plMessageBox.Height / 2;
                            plMessageBox.Left = this.Width / 2 - plMessageBox.Width / 2;
                            tBoxScanBarcode.Clear();
                            tBoxScanBarcode.Focus();
                            WindowHelper.ForceFocus(this.Handle);
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
                    // �T�O�n�u�ʨ쪺��O�i����
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

        #region UI Event Handlers (�N�ާ@�e���� ViewModel)

        private void tBoxScanBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(tBoxScanBarcode.Text))
            {
                _viewModel.BarcodeEnteredCommand.Execute(tBoxScanBarcode.Text);
                e.Handled = true;
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
            // �ھ� ViewModel ���p��n���C��ӳ]�w�r���C��
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

            // ��� 1: Index (�Ǹ�)
            var indexCol = new DataGridViewTextBoxColumn
            {
                Name = "IndexColumn",
                HeaderText = " ", // �����D��r
                DataPropertyName = "Index", // ������ TestStepViewModel �� "Index" �ݩ�
                Width = 50,
                ReadOnly = true
            };
            dataGridView.Columns.Add(indexCol);

            // ��� 2: TestItem
            var testItemCol = new DataGridViewTextBoxColumn
            {
                Name = "TestItemColumn",
                HeaderText = "Test Item",
                DataPropertyName = "TestItem", 
                Width = 300,
                ReadOnly = true
            };
            dataGridView.Columns.Add(testItemCol);

            // ��� 3: TestStep
            var testStepCol = new DataGridViewTextBoxColumn
            {
                Name = "TestStepColumn",
                HeaderText = "Test Step",
                DataPropertyName = "TestStep", 
                Width = 300,
                ReadOnly = true
            };
            dataGridView.Columns.Add(testStepCol);

            // ��� 4: Result
            var resultCol = new DataGridViewTextBoxColumn
            {
                Name = "Result",
                HeaderText = "Result",
                DataPropertyName = "Result",
                Width = 120,
                ReadOnly = true
            };
            dataGridView.Columns.Add(resultCol);

            // ��� 5: SpendTime
            var spendTimeCol = new DataGridViewTextBoxColumn
            {
                Name = "SpendTimeColumn",
                HeaderText = "Spend Time",
                DataPropertyName = "SpendTime", 
                Width = 120,
                ReadOnly = true
            };
            dataGridView.Columns.Add(spendTimeCol);

            // ��� 6: Detail
            var detailCol = new DataGridViewTextBoxColumn
            {
                Name = "DetailColumn",
                HeaderText = "Detail",
                DataPropertyName = "Detail", // ������ TestStepViewModel �� "Detail" �ݩ� 
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };
            dataGridView.Columns.Add(detailCol);
        }
        private void OnLogMessageAppended(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => OnLogMessageAppended(message));
                return;
            }

            if (message == "CLEAR_LOG")
            {
                textBox.Clear();
            }
            else
            {       
                textBox.AppendText(message);
            }
        }

        #endregion

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }
    }
}