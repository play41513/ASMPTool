﻿// ASMP.ViewModel/LoginViewModel.cs

using ASMPTool.BLL;
using ASMPTool.Commands;
using ASMPTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ASMP.ViewModel
{
    // 自訂 EventArgs 傳遞登入資訊
    public class LoginEventArgs : EventArgs
    {
        public LoginInfoModel LoginInfo { get; }
        public LoginEventArgs(LoginInfoModel loginInfo) { LoginInfo = loginInfo; }
    }

    public class LoginViewModel : ViewModelBase
    {
        #region Private Fields
        // 這些是私有欄位，儲存屬性的實際值
        private string _workOrder = "";
        private string _employeeID = "";
        private string _productModel = "";
        private string _workStation = "";
        private string _version = "";

        // 用於快取從網路讀取的完整列表，避免重複讀取，提高效能
        private readonly List<string> _allProductModels = [];
        private readonly List<string> _allWorkStations = [];
        private readonly List<string> _allVersions = [];

        private bool _isConnected = false;
        private string _nasRootPath = Directory.GetCurrentDirectory();

        private readonly CancellationTokenSource _cts;
        private readonly Task _nasCheckTask;
        #endregion

        #region Public Properties (for Data Binding)
        // 這是公開屬性，View 會綁定到這裡。
        // 當 set 被呼叫時，它會使用 ViewModelBase 的 SetProperty 方法。
        // SetProperty 會更新私有欄位、觸發 PropertyChanged 事件通知 UI，並回傳是否真的有變更。
        public string WorkOrder
        {   // 如果值有變更，就通知 LoginCommand 重新評估 CanExecute
            get => _workOrder;
            set { if (SetProperty(ref _workOrder, value)) LoginCommand.RaiseCanExecuteChanged(); }
        }

        public string EmployeeID
        {
            get => _employeeID;
            set { if (SetProperty(ref _employeeID, value)) LoginCommand.RaiseCanExecuteChanged(); }
        }

        public string ProductModel
        {
            get => _productModel;
            set
            {
                if (SetProperty(ref _productModel, value))
                {   // 當產品型號改變時，清空後面的選項，確保資料一致性
                    WorkStation = string.Empty;
                    WorkStations.Clear();
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string WorkStation
        {
            get => _workStation;
            set
            {
                if (SetProperty(ref _workStation, value))
                {
                    Version = string.Empty;
                    Versions.Clear();
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string Version
        {
            get => _version;
            set { if (SetProperty(ref _version, value)) LoginCommand.RaiseCanExecuteChanged(); }
        }



        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }



        // 這些是 ComboBox 選項列表的資料來源。
        // 使用 ObservableCollection 的好處是，當它的內容改變時(Add, Clear)，會自動通知 UI 更新。
        public ObservableCollection<string> ProductModels { get; } = [];
        public ObservableCollection<string> WorkStations { get; } = [];
        public ObservableCollection<string> Versions { get; } = [];
        #endregion

        #region Commands
        // 命令是 View 和 ViewModel 之間互動的主要橋梁，用於觸發 ViewModel 中的操作。
        public RelayCommand LoginCommand { get; }
        public ICommand LoadItemsCommand { get; }
        public ICommand FilterItemsCommand { get; } // 篩選命令
        #endregion

        #region Events
        // 事件用於 ViewModel 向 View 發送「通知」，例如「登入成功了，請切換頁面」。
        public event EventHandler<LoginEventArgs>? LoginSuccessful;
        public event Func<string,bool>? RequestConfirmation;
        #endregion

        public LoginViewModel()
        {   // 將命令與其對應的執行方法和判斷方法關聯起來。
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            LoadItemsCommand = new RelayCommand<string>(LoadComboBoxItems);
            FilterItemsCommand = new RelayCommand<string>(FilterComboBoxItems);

            ReadRecordFile();

            _cts = new CancellationTokenSource();
            _nasCheckTask = Task.Run(() => CheckNasConnectionLoop(_cts.Token));
        }

        #region Command Methods
        // CanLogin 方法決定 LoginCommand 是否可執行，直接影響登入按鈕的 Enabled 狀態。
        private bool CanLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(WorkOrder) &&
                   !string.IsNullOrWhiteSpace(EmployeeID) &&
                   !string.IsNullOrWhiteSpace(ProductModel) &&
                   !string.IsNullOrWhiteSpace(WorkStation) &&
                   !string.IsNullOrWhiteSpace(Version);
        }

        // OnLogin 方法是 LoginCommand 的實際執行內容。
        private void OnLogin(object? parameter)
        {
            if (!IsConnected)
            {
                // 觸發事件，讓 View 跳出確認對話框
                if (RequestConfirmation?.Invoke("沒有連線伺服器，是否繼續?\nNo connection to server, continue?")
                    == false)
                    return;
            }

            // 1. 建立新的 Model 實例來承載資料
            var loginInfo = new LoginInfoModel
            {
                WorkOrder = this.WorkOrder,
                EmployeeID = this.EmployeeID,
                ProductModel = this.ProductModel,
                WorkStation = this.WorkStation,
                Version = this.Version
            };

            // 2. 執行業務邏輯
            if (IsConnected)
            {
                HandleNASOperations(@"\\swtool\swtool\tools\ASMPTool", loginInfo);
            }
            SaveRecordFile();

            // 3. 觸發登入成功事件，並傳遞 loginInfo
            LoginSuccessful?.Invoke(this, new LoginEventArgs(loginInfo));
        }

        private void LoadComboBoxItems(string? comboBoxName)
        {
            if (string.IsNullOrEmpty(comboBoxName)) return;

            string basePath = Path.Combine(_nasRootPath, "WorkStationFile");
            try
            {
                // 因為此方法由UI事件觸發，已經在UI執行緒上，
                // 所以可以直接修改 ObservableCollection，無需 Dispatcher。
                if (comboBoxName.Contains("ProductModel"))
                {
                    if (!Directory.Exists(basePath)) return;
                    var items = Directory.GetDirectories(basePath).Select(Path.GetFileName).Where(s => s != null).Cast<string>();

                    ProductModels.Clear();
                    _allProductModels.Clear();
                    foreach (var item in items)
                    {
                        _allProductModels.Add(item);
                        ProductModels.Add(item);
                    }
                }
                else if (comboBoxName.Contains("WorkStation"))
                {
                    string productPath = Path.Combine(basePath, ProductModel);
                    if (string.IsNullOrEmpty(ProductModel) || !Directory.Exists(productPath)) return;
                    var items = Directory.GetDirectories(productPath).Select(Path.GetFileName).Where(s => s != null).Cast<string>();

                    _allWorkStations.Clear();
                    WorkStations.Clear();
                    foreach (var item in items)
                    {
                        _allWorkStations.Add(item);
                        WorkStations.Add(item);
                    }
                }
                else if (comboBoxName.Contains("Version"))
                {
                    string stationPath = Path.Combine(basePath, ProductModel, WorkStation);
                    if (string.IsNullOrEmpty(WorkStation) || !Directory.Exists(stationPath)) return;
                    var items = Directory.GetFiles(stationPath).Select(Path.GetFileName).Where(s => s != null).Cast<string>();

                    _allVersions.Clear();
                    Versions.Clear();
                    foreach (var item in items)
                    {
                        _allVersions.Add(item);
                        Versions.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ComboBox items: {ex.Message}");
            }
        }
        #endregion

        #region Private Helper Methods
        private void ReadRecordFile()
        {
            const string configFile = "ASMPTool.ini";
            if (!File.Exists(configFile)) return;
            var lines = File.ReadAllLines(configFile);
            foreach (var line in lines)
            {
                var columns = line.Split(',');
                if (columns.Length != 2) continue;
                string key = columns[0].Trim(), value = columns[1].Trim();
                if (key.Contains("WorkOrder")) WorkOrder = value;
                else if (key.Contains("EmployeeID")) EmployeeID = value;
                else if (key.Contains("ProductModel")) ProductModel = value;
                else if (key.Contains("WorkStation")) WorkStation = value;
                else if (key.Contains("Version")) Version = value;
            }
        }

        private void SaveRecordFile()
        {
            File.WriteAllLines("ASMPTool.ini",
            [
                $"WorkOrder,{WorkOrder}",
                $"EmployeeID,{EmployeeID}",
                $"ProductModel,{ProductModel}",
                $"WorkStation,{WorkStation}",
                $"Version,{Version}"
            ]);
        }

        private void HandleNASOperations(string toolPath, LoginInfoModel loginInfo)
        {
            LoggingBLL.CheckNasConnection();
            string logDir = Path.Combine(@"\\swtool\swtool\logs", loginInfo.ProductModel, loginInfo.WorkStation);
            Directory.CreateDirectory(logDir);

            loginInfo.NAS_IP_Address = @"\\swtool\swtool\logs";

            string destinationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            // 1: 複製主工具目錄，依 checksum 判斷，且遞迴時要排除特殊目錄。
            CopyFilesRecursively(toolPath, destinationPath, loginInfo, forceCopy: false, excludeSpecialFolders: true);

            // 2: 複製DLL檔，依 checksum 判斷。如果需要複製，則其下所有內容都會被複製。
            string dllSource = toolPath + @"\dll";
            string dllDestination = destinationPath + @"\dll";
            CopyFilesRecursively(dllSource, dllDestination, loginInfo, forceCopy: false, excludeSpecialFolders: false);

            // 3: 複製站點設定檔，強制覆蓋。
            string wsFileSource = Path.Combine(toolPath, "WorkStationFile", loginInfo.ProductModel, loginInfo.WorkStation);
            string wsFileDestination = Path.Combine(destinationPath, "WorkStationFile", loginInfo.ProductModel, loginInfo.WorkStation);
            CopyFilesRecursively(wsFileSource, wsFileDestination, loginInfo, forceCopy: true, excludeSpecialFolders: false);

            // 4: 複製參數檔，依 checksum 判斷。如果需要複製，則其下所有內容都會被複製。
            string itemParamSource = Path.Combine(toolPath, "ItemParameter", loginInfo.ProductModel, loginInfo.WorkStation);
            string itemParamDestination = Path.Combine(destinationPath, "ItemParameter", loginInfo.ProductModel, loginInfo.WorkStation);
            CopyFilesRecursively(itemParamSource, itemParamDestination, loginInfo, forceCopy: false, excludeSpecialFolders: false);

        }
        private void CopyFilesRecursively(string sourcePath, string destinationPath, LoginInfoModel loginInfo, bool forceCopy, bool excludeSpecialFolders)
        {
            DirectoryInfo dir = new(sourcePath);
            if (!dir.Exists) return;

            Directory.CreateDirectory(destinationPath);
            FileInfo[] files = dir.GetFiles();

            // --- Checksum 判斷邏輯 ---
            bool skipCopy = false;
            FileInfo? sourceChecksumFile = files.FirstOrDefault(f => f.Name.StartsWith("checksum") && f.Extension.Equals(".ini", StringComparison.OrdinalIgnoreCase));

            if (sourceChecksumFile != null)
            {
                // 如果來源有 checksum 檔案，就檢查目的地是否存在同名檔案。
                string destChecksumPath = Path.Combine(destinationPath, sourceChecksumFile.Name);
                if (File.Exists(destChecksumPath))
                {
                    // 如果存在，則認為檔案版本相同，跳過複製。
                    skipCopy = true;
                }
            }
            // 如果沒有 checksum 檔案，預設行為是「不跳過」，也就是要複製。
            // 'forceCopy' 參數擁有最高優先級，可以覆蓋 checksum 的判斷結果。
            if (forceCopy)
            {
                skipCopy = false;
            }

            // --- 檔案複製邏輯 ---
            if (!skipCopy)
            {
                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.CopyTo(Path.Combine(destinationPath, file.Name), true);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Could not copy file '{file.Name}' because it is in use. Skipping. Error: {ex.Message}");
                    }
                }
            }

            // --- 子目錄遞迴邏輯 ---
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                // 根據 excludeSpecialFolders 參數，決定是否要跳過特殊目錄。
                if (excludeSpecialFolders && (subdir.Name.Contains("ItemParameter")
                    || subdir.Name.Contains("WorkStationFile") || subdir.Name.Contains("dll")))
                {
                    continue; // 跳過此子目錄，不進行遞迴
                }

                string temppath = Path.Combine(destinationPath, subdir.Name);

                // 在遞迴呼叫時，我們需要決定子目錄是否也應該被強制複製。
                // 如果父目錄被強制複製(forceCopy=true)，或父目錄因checksum不符而需要複製(!skipCopy)，那麼子目錄也應該被完整複製。
                bool recursiveForceCopy = forceCopy || !skipCopy;
                if (recursiveForceCopy)
                    CopyFilesRecursively(subdir.FullName, temppath, loginInfo, recursiveForceCopy, excludeSpecialFolders);
            }
        }

        private async Task CheckNasConnectionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool connected = LoggingBLL.CheckNasConnection();
                if (IsConnected != connected) IsConnected = connected;
                _nasRootPath = IsConnected ? @"\\swtool\swtool\tools\ASMPTool" : Directory.GetCurrentDirectory();
                await Task.Delay(1000, token);
            }
        }

        public void OnViewClosing()
        {
            _cts.Cancel();
        }
        #endregion
        private void FilterComboBoxItems(string? filterPayload)
        {
            if (string.IsNullOrEmpty(filterPayload)) return;

            var parts = filterPayload.Split('|'); // 我們用 | 來分隔 ComboBox 名稱和關鍵字
            if (parts.Length != 2) return;

            string comboBoxName = parts[0];
            string keyword = parts[1];

            if (comboBoxName.Contains("ProductModel"))
            {
                var filteredItems = _allProductModels.Where(p => p.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                ProductModels.Clear();
                foreach (var item in filteredItems) ProductModels.Add(item);
            }
        }
    }
}