using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMPTool;
using ASMPTool.BLL;
using ASMPTool.Model;

#pragma warning disable IDE1006
namespace ASMPTool
{
    public partial class frmLogin : Form
    {
        readonly bool ConnectSWToolServer = true;
        protected Point MousePt; // 紀錄移動前和移動後的滑鼠座標
        protected bool canMove = false; // 紀錄表單可否被拖曳
        protected int LeftVar = 0, TopVar = 0; // 紀錄form的移動量

        string SWToolFileDir = "";


        public frmLogin()
        {
            InitializeComponent();
            ReadRecordFile();
            if (!ConnectSWToolServer)
                pictureBoxConnect.Visible = false;
            Task task = Task.Run(() => CheckDirectoryExists());
        }

        private void plFrmLoginTop_MouseDown(object sender, MouseEventArgs e)
        {
            // 設定滑鼠移動前的座標
            MousePt = new Point(e.X, e.Y);
            canMove = true; // 如果按下滑鼠左鍵時 可以移動表單
        }

        private void plFrmLoginTop_MouseMove(object sender, MouseEventArgs e)
        {
            // 拖曳form
            if (canMove)
            {
                this.Left += e.X - MousePt.X;
                this.Top += e.Y - MousePt.Y;
            }
        }

        private void plFrmLoginTop_MouseUp(object sender, MouseEventArgs e)
        {
            canMove = false; // 如果放開滑鼠左鍵時 暫停移動表單
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (ComBoxProductModel.Text == "" || ComboBoxWorkStation.Text == ""
                || TBoxWorkOrder.Text == "" || TBoxEmployeeID.Text == ""
                || ComboBoxVersion.Text == "")
            {
                MessageBox.Show("(!) 請填入完整檔案 Please enter complete information.");
            }
            else
            {
                if (ConnectSWToolServer)
                {
                    string dir = SWToolFileDir + "\\" + ComBoxProductModel.Text + "\\" + ComboBoxWorkStation.Text; ;
                    if (SWToolFileDir .Contains("192.168.20.1"))
                    {
                        if (LoggingBLL.MapNetworkDrive("\\\\192.168.20.1\\swtool\\logs", "user", "user1234"))
                        {
                            if (!Directory.Exists(dir))
                            {
                                //建立資料夾
                                Directory.CreateDirectory(dir);
                            }
                            LoginInfoModel.Instance.NAS_IP_Address = SWToolFileDir;
                            LoggingBLL.UnmapNetworkDrive("\\\\192.168.20.1\\swtool\\logs");
                        }                           
                        
                    }
                    else if(SWToolFileDir.Contains("192.168.14.23"))
                    {
                        if (!Directory.Exists(dir))
                        {
                            //建立資料夾
                            Directory.CreateDirectory(dir);
                        }
                        LoginInfoModel.Instance.NAS_IP_Address = SWToolFileDir;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("  沒有連線伺服器，是否繼續? \r\n\r\n  No server connection. Continue?", "Confirm", MessageBoxButtons.YesNo);
                        if (result != DialogResult.Yes)
                            return;
                    }
                }
               
                LoginInfoModel.Instance.WorkOrder = TBoxWorkOrder.Text;
                LoginInfoModel.Instance.EmployeeID = TBoxEmployeeID.Text;
                LoginInfoModel.Instance.ProductModel = ComBoxProductModel.Text;
                LoginInfoModel.Instance.WorkStation = ComboBoxWorkStation.Text;
                LoginInfoModel.Instance.Version = ComboBoxVersion.Text;
                SaveRecordFile();
                UIForm FrmMain = new();
                FrmMain.Shown += (s, args) =>
                {
                    // 在主表單載入完成後，關閉Loading表單
                    //loadingForm.Close();
                };
                FrmMain.Show();
                this.Hide();
            }
        }

        private void frmLogin_Shown(object sender, EventArgs e)
        {
            TBoxWorkOrder.Focus();
            TBoxWorkOrder.SelectAll();
        }

        private void TBoxWorkOrder_MouseClick(object sender, MouseEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }
        private void TBoxWorkOrder_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb != null && e.KeyChar == 13)
            {
                if (tb.Name.Contains("WorkOrder"))
                    TBoxEmployeeID.Focus();
                else if (tb.Name.Contains("EmployeeID"))
                    btnLogin.PerformClick();
            }
        }
        private void ComBoxProductModel_DropDown(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.Items.Clear();
            //列舉程式當前資料夾下的WorkStationFile資料夾下的所有資料夾
            string currentDir = Directory.GetCurrentDirectory();
            string workStationFileDir = Path.Combine(currentDir, "WorkStationFile");
            if (comboBox.Name.Contains("WorkStation") || comboBox.Name.Contains("Version"))
            {
                if (ComBoxProductModel.Text == string.Empty)
                {
                    comboBox.Enabled = true;
                    return;
                }
                workStationFileDir = Path.Combine(workStationFileDir, ComBoxProductModel.Text);
            }
            if (comboBox.Name.Contains("Version"))
                workStationFileDir = Path.Combine(workStationFileDir, ComboBoxWorkStation.Text);

            if (Directory.Exists(workStationFileDir))
            {
                string[] subDirs;
                if (comboBox.Name.Contains("Version"))
                    subDirs = Directory.GetFiles(workStationFileDir);
                else
                    subDirs = Directory.GetDirectories(workStationFileDir);

                foreach (string subDir in subDirs)
                {
                    string dirName = Path.GetFileName(subDir);
                    comboBox.Items.Add(dirName);
                }
            }
            else
            {
                Console.WriteLine("WorkStationFile 資料夾不存在。");
            }
        }

        private void ComBoxProductModel_TextUpdate(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string searchKeyword = comboBox.Text;

            comboBox.Items.Clear();

            // 列舉程式當前資料夾下的WorkStationFile資料夾下的所有資料夾
            string currentDir = Directory.GetCurrentDirectory();
            string workStationFileDir = Path.Combine(currentDir, "WorkStationFile");

            if (Directory.Exists(workStationFileDir))
            {
                string[] subDirs;
                if (comboBox.Name.Contains("Version"))
                    subDirs = Directory.GetFiles(workStationFileDir);
                else
                    subDirs = Directory.GetDirectories(workStationFileDir);

                foreach (string subDir in subDirs)
                {
                    string dirName = Path.GetFileName(subDir);

                    if (dirName.Contains(searchKeyword))
                    {
                        comboBox.Items.Add(dirName);
                    }
                }
            }
            else
            {
                Console.WriteLine("WorkStationFile 資料夾不存在。");
            }
            comboBox.DroppedDown = true;
            comboBox.Text = searchKeyword;
            comboBox.SelectionStart = comboBox.Text.Length;
            Cursor = Cursors.Default;
        }

        private void ComBoxProductModel_TextChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Name.Contains("ProductModel"))
            {
                ComboBoxWorkStation.Items.Clear();
                ComboBoxVersion.Items.Clear();
                ComboBoxWorkStation.Text = string.Empty;
                ComboBoxVersion.Text = string.Empty;
            }
            if(comboBox.Name.Contains("WorkStation"))
            {
                ComboBoxVersion.Items.Clear();
                ComboBoxVersion.Text = string.Empty;
            }
        }

        private void TimerConnect_Tick(object sender, EventArgs e)
        {

        }
        private void CheckDirectoryExists()
        {
            while (true)
            {
                if (!Directory.Exists("\\\\192.168.14.23\\swtool\\logs"))
                {
                    if (!CanAccessNetworkFolder("\\\\192.168.20.1\\swtool\\logs","user","user1234"))
                    {
                        pictureBoxConnect.Load("icon\\disconnect.png");
                        SWToolFileDir = "";
                    }
                    else
                    {
                        SWToolFileDir = "\\\\192.168.20.1\\swtool\\logs";
                        pictureBoxConnect.Load("icon\\connect.png");

                    }
                }
                else
                {
                    SWToolFileDir = "\\\\192.168.14.23\\swtool\\logs";
                    pictureBoxConnect.Load("icon\\connect.png");

                }
                // 等待 1 秒鐘
                Thread.Sleep(1000);
            }
        }
        private void ReadRecordFile()
        {
            if (!File.Exists("ASMPTool.ini"))
                return;
            using var reader = new StreamReader("ASMPTool.ini");
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var columns = line.Split(',');
                if (columns.Length == 2)
                {
                    if (columns[0].Trim().Contains("WorkOrder"))
                        TBoxWorkOrder.Text = columns[1].Trim();
                    else if (columns[0].Trim().Contains("EmployeeID"))
                        TBoxEmployeeID.Text = columns[1].Trim();
                    else if (columns[0].Trim().Contains("ProductModel"))
                        ComBoxProductModel.Text = columns[1].Trim();
                    else if (columns[0].Trim().Contains("WorkStation"))
                        ComboBoxWorkStation.Text = columns[1].Trim();
                    else if (columns[0].Trim().Contains("Version"))
                        ComboBoxVersion.Text = columns[1].Trim();
                }
            }
        }
        private void SaveRecordFile()
        {
            using var writer = new StreamWriter("ASMPTool.ini");
            writer.WriteLine($"WorkOrder,{TBoxWorkOrder.Text}");
            writer.WriteLine($"EmployeeID,{TBoxEmployeeID.Text}");
            writer.WriteLine($"ProductModel,{ComBoxProductModel.Text}");
            writer.WriteLine($"WorkStation,{ComboBoxWorkStation.Text}");
            writer.WriteLine($"Version,{ComboBoxVersion.Text}");
        }
        public static bool CanAccessNetworkFolder(string networkPath, string username, string password)
        {
            bool isConnected = false;
            try
            {
                // 掛載網路磁碟
                isConnected = LoggingBLL.MapNetworkDrive(networkPath, username, password);

                // 確認資料夾是否存在
                return Directory.Exists(networkPath);
            }
            catch
            {
                return false;
            }
            finally
            {
                // 解除掛載
                if (isConnected)
                    LoggingBLL.UnmapNetworkDrive(networkPath);
            }
        }
    }
}
