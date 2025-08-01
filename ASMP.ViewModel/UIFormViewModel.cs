﻿// ASMP.ViewModel/UIFormViewModel.cs

using ASMPTool.BLL;
using ASMPTool.Commands;
using ASMPTool.Model;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Input;
using System.Diagnostics; // For Stopwatch

namespace ASMP.ViewModel
{
    public class UIFormViewModel : ViewModelBase
    {
        #region Private Fields

        // --- 核心資料與依賴 ---
        private readonly LoginInfoModel _loginInfo;
        private readonly INIFileModel _testPlan;
        private readonly ErrorCodeBLL _errorCodeBLL;
        private readonly IntPtr _ownerHandle;

        // --- UI 狀態的私有儲存 ---
        private string _overallResult = "WAIT";
        private Color _overallResultColor = Color.Silver;
        private string _totalTestTime = "Total Time: 0.00";
        private bool _isScanBarcodeVisible = false;
        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");

        // --- 流程控制與工具 ---
        private readonly StringBuilder _logBuilder = new();

        private readonly System.Threading.Timer _clockTimer;
        // 用於從背景執行緒安全更新 UI 的同步上下文
        private readonly SynchronizationContext _uiSyncContext;
        // 用於非同步等待使用者輸入條碼的關鍵工具
        private TaskCompletionSource<string> _barcodeTcs = new();

        #endregion

        #region Public Properties (for Data Binding)

        // 這些是給 View (UIForm.cs) 進行資料繫結的公開屬性。
        // 當它們的值在 ViewModel 中改變時，UI 會自動更新。

        // 從登入資訊中傳遞過來的唯讀屬性
        public string WorkOrder => _loginInfo.WorkOrder;
        public string EmployeeID => _loginInfo.EmployeeID;
        public string ProductModel => _loginInfo.ProductModel;
        public string WorkStation => _loginInfo.WorkStation;
        public string Version => _loginInfo.Version;

        // 動態更新的 UI 屬性
        public string CurrentTime { get => _currentTime; private set => SetProperty(ref _currentTime, value); }
        public string TotalTestTime { get => _totalTestTime; private set => SetProperty(ref _totalTestTime, value); }
        public string OverallResult { get => _overallResult; private set => SetProperty(ref _overallResult, value); }
        public Color OverallResultColor { get => _overallResultColor; private set => SetProperty(ref _overallResultColor, value); }
        public bool IsScanBarcodeVisible { get => _isScanBarcodeVisible; private set => SetProperty(ref _isScanBarcodeVisible, value); }

        // DataGridView 的資料來源。使用 BindingList<T>實現雙向資料繫結。
        public BindingList<TestStepViewModel> TestSteps { get; } = [];

        #endregion

        #region Commands

        // 詳解: 命令是 View 觸發 ViewModel 執行操作的標準方式。
        public ICommand BarcodeEnteredCommand { get; }

        #endregion

        #region Events

        // 事件是 ViewModel 通知 View 執行某些無法透過資料繫結完成的 UI 操作（如滾動、彈窗）的方式。
        public event Action<int>? ScrollToRowRequested;
        public event Action<string>? ShowMessageRequested;
        public event Action<string>? LogMessageAppended;

        #endregion

        public UIFormViewModel(IntPtr ownerHandle, LoginInfoModel loginInfo)
        {
            _ownerHandle = ownerHandle;
            _loginInfo = loginInfo;
            _errorCodeBLL = new ErrorCodeBLL();
            _uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            // 載入測試項目
            string filePath = $@"WorkStationFile\{_loginInfo.ProductModel}\{_loginInfo.WorkStation}\{_loginInfo.Version}";
            _testPlan = new INIFileBLL(filePath).LoadToModel(); 

            // 初始化
            BarcodeEnteredCommand = new RelayCommand<string>(OnBarcodeEntered);

            for (int i = 0; i < _testPlan.Tasks.Count; i++)
            {
                // 測試項加入到顯示列表中
                 TestSteps.Add(new TestStepViewModel(i, _testPlan.Tasks[i]));
            }

            // 啟動任務
            _clockTimer = new System.Threading.Timer(OnClockTick, null, 0, 1000);
            Task.Run(MainTestLoop); // 在背景執行緒中啟動主測試流程
        }

        private void OnBarcodeEntered(string? barcode)
        {
            if (!string.IsNullOrEmpty(barcode))
            {
                // 當使用者輸入條碼後，我們透過 TaskCompletionSource 來喚醒正在等待的主迴圈。
                _barcodeTcs.TrySetResult(barcode);
            }
        }

        private void OnClockTick(object? state)
        {
            // 每秒更新時間
            _uiSyncContext.Post(_ => CurrentTime = DateTime.Now.ToString("HH:mm:ss"), null);
        }

        // 主測試流程 
        private async Task MainTestLoop()
        {
            while (true)
            {
                var testResult = new TestResultModel();

                // 1. 等待掃描MES條碼
                string barcode = await RequestBarcodeScanAsync();
                testResult.ScanBarcodeNumber = barcode;
                _uiSyncContext.Post(_ => IsScanBarcodeVisible = false, null);

                // 2. 初始化測試
                ResetForNewTest();
                var totalTimeStopwatch = Stopwatch.StartNew();
                var totalTimeTimer = new System.Threading.Timer(
                    _ => _uiSyncContext.Post(s => TotalTestTime = $"Total Time: {totalTimeStopwatch.Elapsed.TotalSeconds:F2}", null),
                    null, 0, 100);

                // 3. 執行所有測試步驟
                bool isPassed = await ExecuteTestStepsAsync(testResult);

                // 4. 測試結束，停止計時並更新最終結果
                totalTimeStopwatch.Stop();
                totalTimeTimer.Dispose();
                _uiSync_UpdateTotalTime(totalTimeStopwatch.Elapsed.TotalSeconds); // 更新最後一次時間

                _uiSyncContext.Post(_ =>
                {
                    OverallResult = isPassed ? "PASS" : "FAIL";
                    OverallResultColor = isPassed ? Color.Green : Color.Red;
                }, null);

                testResult.FinalResult = isPassed ? "PASS" : "FAIL";
                if (!isPassed && string.IsNullOrEmpty(testResult.ErrorCode))
                {
                    testResult.ErrorCode = _errorCodeBLL.GetErrorCodeKey(_logBuilder.ToString()) ?? "E999";
                }

                // 5. 儲存 Log
                if (!LoggingBLL.SaveToCSV(_loginInfo, testResult))
                {
                    _uiSyncContext.Post(_ => ShowMessageRequested?.Invoke("沒有連線到伺服器，傳送LOG失敗!\nNot connected to the server, failed to send log file!"), null);
                }
            }
        }

        private async Task<string> RequestBarcodeScanAsync()
        {
            // 每次請求掃描時，都建立一個新的 TaskCompletionSource
            _barcodeTcs = new TaskCompletionSource<string>();
            // 更新 UI 狀態以顯示掃描面板
            _uiSyncContext.Post(_ => IsScanBarcodeVisible = true, null);
            // 非同步等待，直到 OnBarcodeEntered 被呼叫並設置了結果
            return await _barcodeTcs.Task;
        }

        private void ResetForNewTest()
        {
            _uiSyncContext.Post(_ =>
            {
                LogMessageAppended?.Invoke("CLEAR_LOG"); 

                OverallResult = "TESTTING";
                OverallResultColor = Color.Silver;
                TotalTestTime = "Total Time: 0.00";
                foreach (var step in TestSteps)
                {
                    step.Result = "";
                    step.SpendTime = "";
                    step.Detail = "";
                }
            }, null);
        }

        private void _uiSync_AppendLog(string message)
        {
            // 組合好要顯示的單行訊息
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\r\n";
            _uiSyncContext.Post(_ => LogMessageAppended?.Invoke(formattedMessage), null);
        }

        private void _uiSync_UpdateTotalTime(double seconds)
        {
            _uiSyncContext.Post(_ => TotalTestTime = $"Total Time: {seconds:F2}", null);
        }

        private async Task<bool> ExecuteTestStepsAsync(TestResultModel testResult)
        {
            var allGroups = new List<List<(ItemTask task, int index)>>();
            List<(ItemTask task, int index)>? currentGroup = null;

            for (int i = 0; i < _testPlan.Tasks.Count; i++)
            {
                var task = _testPlan.Tasks[i];
                if (!task.Enable)
                {
                    if (currentGroup != null) { allGroups.Add(currentGroup); }
                    currentGroup = [(task, i)];
                }
                else
                {
                    if (currentGroup == null) { currentGroup = []; }
                    currentGroup.Add((task, i));
                }
            }
            if (currentGroup != null && currentGroup.Any()) { allGroups.Add(currentGroup); }

            var parallelExecutionList = new List<List<(ItemTask task, int index)>>();

            foreach (var group in allGroups)
            {
                bool isSyncGroup = group.First().task.Sync;

                if (isSyncGroup)
                {
                    parallelExecutionList.Add(group);
                }
                else // 遇到循序組
                {
                    // 步驟 1: 處理之前收集的所有並行任務
                    if (parallelExecutionList.Any())
                    {
                        _uiSync_AppendLog($"--- Starting Parallel Block ({parallelExecutionList.Count} groups) ---");
                        var parallelTasks = new List<Task<bool>>();
                        bool isFirstParallelGroup = true; // 標記是否為第一個並行組

                        foreach (var parallelGroup in parallelExecutionList)
                        {
                            // 只有第一個並行組，才會被授予捲動權限 (canRequestScroll: true)
                            parallelTasks.Add(ExecuteSingleGroupAsync(parallelGroup, testResult, canRequestScroll: isFirstParallelGroup));
                            isFirstParallelGroup = false; // 後續的組都設為 false
                        }

                        bool[] results = await Task.WhenAll(parallelTasks);
                        _uiSync_AppendLog($"--- Parallel Block Finished ---");

                        parallelExecutionList.Clear();
                        if (results.Any(r => !r)) return false;
                    }

                    // 步驟 2: 處理當前的循序任務
                    // 循序任務組，永遠擁有捲動權限
                    bool sequentialResult = await ExecuteSingleGroupAsync(group, testResult, canRequestScroll: true);
                    if (!sequentialResult) return false;
                }
            }

            // 步驟 3: 處理最後剩下的一批並行任務
            if (parallelExecutionList.Any())
            {
                _uiSync_AppendLog($"--- Starting Final Parallel Block ({parallelExecutionList.Count} groups) ---");
                var parallelTasks = new List<Task<bool>>();
                bool isFirstParallelGroup = true;

                foreach (var parallelGroup in parallelExecutionList)
                {
                    // 只有第一個並行組被授予捲動權限 
                    parallelTasks.Add(ExecuteSingleGroupAsync(parallelGroup, testResult, canRequestScroll: isFirstParallelGroup));
                    isFirstParallelGroup = false;
                }

                bool[] results = await Task.WhenAll(parallelTasks);
                _uiSync_AppendLog($"--- Final Parallel Block Finished ---");
                if (results.Any(r => !r)) return false;
            }

            return true;
        }
        private async Task<bool> ExecuteSingleGroupAsync(List<(ItemTask task, int index)> group, TestResultModel testResult, bool canRequestScroll)
        {
            foreach (var (task, index) in group)
            {
                if (!task.Enable) continue;

                var correspondingStepVM = TestSteps[index];
                if (canRequestScroll)
                {
                    _uiSyncContext.Post(_ => ScrollToRowRequested?.Invoke(index), null);
                }
                _uiSyncContext.Post(_ =>
                {
                    correspondingStepVM.Result = "TESTTING";
                }, null);
                _uiSync_AppendLog($"--- Start: {task.Name} ---");

                var stepStopwatch = Stopwatch.StartNew();
                var timerCts = new CancellationTokenSource();

                var timerTask = Task.Run(async () =>
                {
                    while (!timerCts.Token.IsCancellationRequested)
                    {
                        _uiSyncContext.Post(s =>
                        {
                            if (stepStopwatch.IsRunning)
                                correspondingStepVM.SpendTime = stepStopwatch.Elapsed.TotalSeconds.ToString("F2");
                        }, null);
                        await Task.Delay(100, timerCts.Token); 
                    }
                }, timerCts.Token);


                string stepDetail = "";
                bool stepPassed = false;

                var executionTask = Task.Run(() =>
                {
                    if (task.FunctionTestType.Contains("MessageWindows.exe"))
                    {
                        return MessageBoxBLL.ExecuteMessageBox(task.FunctionTestType, out stepDetail);
                    }
                    else
                    {
                        return DLLManagerBLL.ExecuteSpecificPlugin(
                            task.FunctionTestType,
                            task.FunctionTestPath,
                            out stepDetail,
                            _ownerHandle,
                            testResult 
                        );
                    }
                });

                stepPassed = await executionTask;


                stepStopwatch.Stop();
                timerCts.Cancel(); 

                var stepResultItem = new TestResultItem
                {
                    TestItemName = task.Name,
                    Result = stepPassed ? "PASSED" : "FAILED",
                    SpendTime = stepStopwatch.Elapsed.TotalSeconds,
                    Detail = stepDetail
                };

                testResult.StepResults.Add(stepResultItem);

                _uiSyncContext.Post(_ =>
                {
                    correspondingStepVM.Result = stepResultItem.Result;
                    correspondingStepVM.SpendTime = stepResultItem.SpendTime.ToString("F2"); 
                    correspondingStepVM.Detail = stepResultItem.Detail;

                    _uiSync_AppendLog(stepResultItem.Detail);
                    _uiSync_AppendLog($"--- End: {task.Name}, Result: {correspondingStepVM.Result} ---");
                }, null);

                if (!stepPassed)
                {
                    _uiSync_AppendLog($"*** Step Failed. Executing NG recovery steps... ***");

                    foreach (int ngTaskIndex in task.NGTest)
                    {
                        int actualIndex = ngTaskIndex - 1; 

                        if (actualIndex >= 0 && actualIndex < _testPlan.Tasks.Count)
                        {
                            var ngTask = _testPlan.Tasks[actualIndex];
                            _uiSync_AppendLog($"--- Executing NG Step: {ngTask.Name} ---");

                            if (ngTask.FunctionTestType.Contains("MessageWindows.exe"))
                            {
                                MessageBoxBLL.ExecuteMessageBox(ngTask.FunctionTestType, out _);
                            }
                            else
                            {
                                //不判定PASS FAIL ，不傳入 ownerHwnd
                                DLLManagerBLL.ExecuteSpecificPlugin(ngTask.FunctionTestType, ngTask.FunctionTestPath, out _, 0, testResult);
                            }
                        }
                    }
                    _uiSync_AppendLog($"*** NG recovery steps finished. Terminating group. ***");

                    return false;
                }
            }
            return true;
        }
    }
}