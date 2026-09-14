// ASMPTool/frmLogin.cs

using ASMP.ViewModel;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#pragma warning disable IDE0079
#pragma warning disable IDE1006


namespace ASMPTool
{
    public partial class frmLogin : Form
    {
        private readonly LoginViewModel _viewModel;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public frmLogin()
        {
            InitializeComponent();
            this.CreateHandle();
            _viewModel = new LoginViewModel();
            SetupBindingsAndEvents();

            AddDebugMenuOption();
        }

        private void SetupBindingsAndEvents()
        {
            // --- Data Bindings ---
            // 將 UI 控制項的屬性「綁定」到 ViewModel 的公開屬性。
            // `DataSourceUpdateMode.OnPropertyChanged` 讓輸入框的任何變更都會立即更新到 ViewModel。
            TBoxWorkOrder.DataBindings.Add("Text", _viewModel, nameof(_viewModel.WorkOrder), false, DataSourceUpdateMode.OnPropertyChanged);
            TBoxEmployeeID.DataBindings.Add("Text", _viewModel, nameof(_viewModel.EmployeeID), false, DataSourceUpdateMode.OnPropertyChanged);

            ComBoxProductModel.DataBindings.Add("Text", _viewModel, nameof(_viewModel.ProductModel), false, DataSourceUpdateMode.OnPropertyChanged);
            ComboBoxWorkStation.DataBindings.Add("Text", _viewModel, nameof(_viewModel.WorkStation), false, DataSourceUpdateMode.OnPropertyChanged);
            ComboBoxVersion.DataBindings.Add("Text", _viewModel, nameof(_viewModel.Version), false, DataSourceUpdateMode.OnPropertyChanged);

            // --- Event Subscriptions ---
            // View 訂閱ViewModel 的事件，當 ViewModel 觸發事件時，View 會執行對應的操作。
            _viewModel.LoginSuccessful += OnLoginSuccessful;
            _viewModel.RequestConfirmation += OnRequestConfirmation;

            // 訂閱 ViewModel 的 PropertyChanged 事件，用於處理無法直接綁定的 UI 更新。
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            // 訂閱集合變化事件來手動刷新 ComboBox(Winform必要的操作)。
            _viewModel.ProductModels.CollectionChanged += (s, e) => { RefreshComboBoxDataSource(ComBoxProductModel, _viewModel.ProductModels); };
            _viewModel.WorkStations.CollectionChanged += (s, e) => { RefreshComboBoxDataSource(ComboBoxWorkStation, _viewModel.WorkStations); };
            _viewModel.Versions.CollectionChanged += (s, e) => { RefreshComboBoxDataSource(ComboBoxVersion, _viewModel.Versions); };

            btnLogin.Enabled = _viewModel.LoginCommand.CanExecute(null);
            if (btnLogin.Enabled)
                btnLogin.BackColor = Color.White;
            else
                btnLogin.BackColor = Color.Gray;

            UpdateNasConnectionIcon();
        }
        // 當 ViewModel 的 PropertyChanged 事件被觸發時執行
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            // 使用 this.Invoke 確保所有 UI 更新都在 UI 執行緒上執行。
            this.Invoke(() =>
            {
                if (this.IsDisposed) return;
                switch (e.PropertyName)
                {
                    case nameof(LoginViewModel.IsConnected):
                        UpdateNasConnectionIcon();
                        break;

                    // 當任何一個會影響登入按鈕狀態的屬性發生變化時，都重新評估按鈕狀態
                    case nameof(LoginViewModel.WorkOrder):
                    case nameof(LoginViewModel.EmployeeID):
                    case nameof(LoginViewModel.ProductModel):
                    case nameof(LoginViewModel.WorkStation):
                    case nameof(LoginViewModel.Version):
                        btnLogin.Enabled = _viewModel.LoginCommand.CanExecute(null);
                        if (btnLogin.Enabled) btnLogin.BackColor = Color.White;
                        else btnLogin.BackColor = Color.Gray;
                        break;
                }
            });
        }
        private void UpdateNasConnectionIcon()
        {
            try
            {
                string iconName = _viewModel.IsConnected ? "connect.png" : "disconnect.png";
                string iconPath = Path.Combine(Application.StartupPath, "icon", iconName);

                if (File.Exists(iconPath))
                {
                    using var fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
                    pictureBoxConnect.Image = Image.FromStream(fs);
                }
                else
                {
                    pictureBoxConnect.Image = Properties.Resources.disconnect;
                }
            }
            catch
            {
                pictureBoxConnect.Image = Properties.Resources.disconnect;
            }
        }
        private const int CB_SETMINVISIBLE = 0x1701;


        private void RefreshComboBoxDataSource(ComboBox comboBox, object dataSource)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => RefreshComboBoxDataSource(comboBox, dataSource));
                return;
            }
            comboBox.DataSource = null;
            comboBox.DataSource = dataSource;

            if (comboBox.IsHandleCreated)
            {
                SendMessage(comboBox.Handle, CB_SETMINVISIBLE, 8, 0);
            }
        }

        #region ViewModel Event Handlers (由 ViewModel 觸發)
        private void OnLoginSuccessful(object? sender, LoginEventArgs e)
        {
            UIForm FrmMain = new(e.LoginInfo);
            FrmMain.Show();
            this.Hide();
        }
        private void AddDebugMenuOption()
        {
            // 假設你新增的元件名稱為 contextMenuStrip1
            if (contextMenuStrip1 != null)
            {
                contextMenuStrip1.Items.Clear(); // 清除預設項目

                var debugMenuItem = new ToolStripMenuItem("開啟除錯視窗 (Debug Console)");
                debugMenuItem.Click += (s, e) =>
                {
                    // 呼叫 DebugConsoleForm 單例
                    DebugConsoleForm.Instance.Show();
                    DebugConsoleForm.Instance.BringToFront();
                };

                contextMenuStrip1.Items.Add(debugMenuItem);
            }
        }
        private bool OnRequestConfirmation(string message)
        {
            var result = MessageBox.Show(this, message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }
        #endregion

        #region UI Event Handlers (將操作委派給 ViewModel)
        private void btnLogin_Click(object sender, EventArgs e)
        {
            _viewModel.LoginCommand.Execute(null);
        }

        private void ComboBox_DropDown(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                _viewModel.LoadItemsCommand.Execute(comboBox.Name);
            }
        }

        private void TBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (sender == TBoxWorkOrder)
                {
                    TBoxEmployeeID.Focus();
                }
                else if (sender == TBoxEmployeeID)
                {
                    if (_viewModel.LoginCommand.CanExecute(null))
                    {
                        _viewModel.LoginCommand.Execute(null);
                    }
                }
                e.Handled = true; // 防止發出 "叮" 的聲音
            }
        }

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            _viewModel.OnViewClosing();
        }
        #endregion

        #region Pure UI Logic
        private Point _mousePt;
        private bool _canMove = false;

        private void plFrmLoginTop_MouseDown(object sender, MouseEventArgs e) { _mousePt = e.Location; _canMove = true; }
        private void plFrmLoginTop_MouseMove(object sender, MouseEventArgs e) { if (_canMove) { this.Location = new Point(this.Left + e.X - _mousePt.X, this.Top + e.Y - _mousePt.Y); } }
        private void plFrmLoginTop_MouseUp(object sender, MouseEventArgs e) { _canMove = false; }
        private void btnClose_Click(object sender, EventArgs e) { this.Close(); }
        private void TBox_MouseClick(object sender, MouseEventArgs e) { if (sender is TextBox tb) tb.SelectAll(); }
        #endregion

        private void ComBoxProductModel_TextUpdate(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                // 組合ComboBox名稱|關鍵字 並執行
                string payload = $"{comboBox.Name}|{comboBox.Text}";
                _viewModel.FilterItemsCommand.Execute(payload);

                //  UI 操作，讓下拉選單保持開啟且游標在最後
                comboBox.DroppedDown = true;
                comboBox.Cursor = Cursors.Default;
                comboBox.SelectionStart = comboBox.Text.Length;
            }
        }
    }
}