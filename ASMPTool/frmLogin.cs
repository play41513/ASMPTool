using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMPTool;
using ASMPTool.BLL;
using ASMPTool.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

#pragma warning disable IDE1006
namespace ASMPTool
{
    public partial class frmLogin : Form
    {

        protected Point MousePt; // 紀錄移動前和移動後的滑鼠座標
        protected bool canMove = false; // 紀錄表單可否被拖曳
        protected int LeftVar = 0, TopVar = 0; // 紀錄form的移動量

        string SWToolFileDir = "";
        string NasWorkStationFileDir = Directory.GetCurrentDirectory();
        static bool ConnectSWToolServer = false;


        public frmLogin()
        {
            InitializeComponent();
            ReadRecordFile();
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
                LoginInfoModel.Instance.WorkOrder = TBoxWorkOrder.Text;
                LoginInfoModel.Instance.EmployeeID = TBoxEmployeeID.Text;
                LoginInfoModel.Instance.ProductModel = ComBoxProductModel.Text;
                LoginInfoModel.Instance.WorkStation = ComboBoxWorkStation.Text;
                LoginInfoModel.Instance.Version = ComboBoxVersion.Text;

                if (ConnectSWToolServer)
                {
                    //建立Server Log資料夾
                    if (SWToolFileDir.Contains("192.168.20.1"))
                    {
                        HandleNASOperations("192.168.20.1", @"\\192.168.20.1\swtool\logs", @"\\192.168.20.1\swtool\tools\ASMPTool");
                    }
                    else if (SWToolFileDir.Contains("192.168.14.23"))
                    {
                        HandleNASOperations("192.168.14.23", @"\\192.168.14.23\swtool\logs", @"\\192.168.14.23\swtool\tools\ASMPTool");
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("  沒有連線伺服器，是否繼續? \r\n\r\n  No server connection. Continue?", "Confirm", MessageBoxButtons.YesNo);
                    if (result != DialogResult.Yes)
                        return;
                }

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
            string workStationFileDir = Path.Combine(NasWorkStationFileDir, "WorkStationFile");
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
            string workStationFileDir = Path.Combine(NasWorkStationFileDir, "WorkStationFile");

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
            if (comboBox.Name.Contains("WorkStation"))
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
                if (LoggingBLL.DirectoryExistsWithTimeout("\\\\192.168.14.23\\swtool\\logs", 1000))
                {
                    SWToolFileDir = "\\\\192.168.14.23\\swtool\\logs";
                    NasWorkStationFileDir = "\\\\192.168.14.23\\swtool\\tools\\ASMPTool";
                    pictureBoxConnect.Load("icon\\connect.png");
                    ConnectSWToolServer = true;
                }
                else
                {
                    if (LoggingBLL.MapNetworkDrive("\\\\192.168.20.1\\swtool\\logs", "user", "user1234"))
                    {
                        SWToolFileDir = "\\\\192.168.20.1\\swtool\\logs";
                        NasWorkStationFileDir = "\\\\192.168.20.1\\swtool\\tools\\ASMPTool";
                        pictureBoxConnect.Load("icon\\connect.png");
                        ConnectSWToolServer = true;
                    }
                    else
                    {
                        pictureBoxConnect.Load("icon\\disconnect.png");
                        SWToolFileDir = "";
                        ConnectSWToolServer = false;
                    }
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
        private static void CopyFilesRecursively(string sourcePath, string destinationPath, bool forceCopy = false)
        {
            // 取得來源資料夾的子資料夾
            DirectoryInfo dir = new DirectoryInfo(sourcePath);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "來源資料夾不存在或找不到: "
                    + sourcePath);
            }

            // 取得來源資料夾的檔案
            FileInfo[] files = dir.GetFiles();

            // 如果目的資料夾不存在，建立它
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // 檢查來源資料夾是否有名稱前綴為 "checksum" 的 ini 檔案
            FileInfo? sourceChecksumFile = files.FirstOrDefault(
                    f => f.Name.StartsWith("checksum") && f.Extension.Equals(".ini", StringComparison.OrdinalIgnoreCase)
                    );

            // 如果來源資料夾有 checksum 檔案，檢查目的資料夾是否有相同名稱的 checksum 檔案
            bool skipCopy = false;
            if (sourceChecksumFile != null)
            {
                string destinationChecksumPath = Path.Combine(destinationPath, sourceChecksumFile.Name);
                if (File.Exists(destinationChecksumPath))
                {
                    skipCopy = true;
                }
            }
            else if (!sourcePath.Contains("ASMPTool\\WorkStationFile"))
                skipCopy = true;
            if (forceCopy) skipCopy = false;

            if (!skipCopy)
            {
                // 複製檔案到目的資料夾
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destinationPath, file.Name);
                    file.CopyTo(temppath, true); // 覆蓋既有檔案
                }
            }

            // 複製子資料夾和其內容到目的資料夾
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                if (subdir.Name != "ItemParameter" && subdir.Name != "WorkStationFile")
                {

                    string temppath = Path.Combine(destinationPath, subdir.Name);
                    if (temppath.Contains("ItemParameter\\" + LoginInfoModel.Instance.ProductModel + "\\" + LoginInfoModel.Instance.WorkStation + "\\")
                        && !skipCopy)
                        CopyFilesRecursively(subdir.FullName, temppath, true);
                    else
                        CopyFilesRecursively(subdir.FullName, temppath);
                }
            }
        }
        private void HandleNASOperations(string nasIp, string logPath, string toolPath)
        {
            if (LoggingBLL.MapNetworkDrive(logPath, "user", "user1234"))
            {
                string dir = Path.Combine(SWToolFileDir, ComBoxProductModel.Text, ComboBoxWorkStation.Text);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                LoginInfoModel.Instance.NAS_IP_Address = SWToolFileDir;

                // Copy essential NAS files to the program folder
                string destinationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                CopyFilesRecursively(toolPath, destinationPath);

                string itemParamSource = Path.Combine(toolPath, "ItemParameter", ComBoxProductModel.Text, ComboBoxWorkStation.Text);
                string itemParamDestination = Path.Combine(destinationPath, "ItemParameter", ComBoxProductModel.Text, ComboBoxWorkStation.Text);
                CopyFilesRecursively(itemParamSource, itemParamDestination);

                itemParamSource = Path.Combine(toolPath, "WorkStationFile", ComBoxProductModel.Text, ComboBoxWorkStation.Text);
                itemParamDestination = Path.Combine(destinationPath, "WorkStationFile", ComBoxProductModel.Text, ComboBoxWorkStation.Text);
                CopyFilesRecursively(itemParamSource, itemParamDestination);
            }
        }

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ConnectSWToolServer)
            {
                LoggingBLL.UnmapNetworkDrive(SWToolFileDir);
            }
        }
    }
}
