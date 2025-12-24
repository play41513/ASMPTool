
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
    //用於從 View 回傳 MTT 設定的資料結構
    public class MttSettings
    {
        public INIFileModel ModifiedTestPlan { get; set; } = new INIFileModel();
        public int LoopCount { get; set; } = 1;
    }
    // 用於控制流程跳轉的例外
    public class RetryException : Exception
    {
        public int TargetIndex { get; }
        public RetryException(int targetIndex) { TargetIndex = targetIndex; }
    }
    public class UIFormViewModel : ViewModelBase
    {
        #region Private Fields
        // --- 錯誤資訊的私有儲存 ---
        private readonly object _firstErrorLock = new();
        private string? _firstErrorDetail = null;
        // --- 核心資料與依賴 ---
        private readonly LoginInfoModel _loginInfo;
        private readonly INIFileModel _originalTestPlan;
        private INIFileModel _testPlan;
        private readonly ErrorCodeBLL _errorCodeBLL;
        private readonly IntPtr _ownerHandle;

        // --- UI 狀態的私有儲存 ---
        private string _overallResult = "WAIT";
        private Color _overallResultColor = Color.Silver;
        private string _totalTestTime = "Total Time: 0.00";
        private bool _isScanBarcodeVisible = false;
        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
        private string _loopStatus = ""; 
        private int _loopCount = 1;
        private bool _nasWarningShown = false;
        private int _passCount;


        private readonly System.Threading.Timer _clockTimer;
        private readonly System.Threading.Timer _uploadTimer;
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
        public string LoopStatus { get => _loopStatus; private set => SetProperty(ref _loopStatus, value); }

        // DataGridView 的資料來源。使用 BindingList<T>實現雙向資料繫結。
        public BindingList<TestStepViewModel> TestSteps { get; } = [];

        #endregion

        #region Commands

        // 詳解: 命令是 View 觸發 ViewModel 執行操作的標準方式。
        public ICommand BarcodeEnteredCommand { get; }
        public ICommand EnterMttModeCommand { get; }

        #endregion

        #region Events

        // 事件是 ViewModel 通知 View 執行某些無法透過資料繫結完成的 UI 操作（如滾動、彈窗）的方式。
        public event Action<int>? ScrollToRowRequested;
        public event Action<string>? ShowMessageRequested;
        public event Action<LogDisplayInfo>? LogMessageAppended;
        public event Func<List<Tuple<string, int>>, INIFileModel, MttSettings?>? ShowMttSelectionDialogRequested;
        // 詢問使用者是否重測 (回傳 true 表示重測)
        public event Func<string, bool>? ConfirmRetryRequested;

        #endregion

        public UIFormViewModel(IntPtr ownerHandle, LoginInfoModel loginInfo)
        {
            _ownerHandle = ownerHandle;
            _loginInfo = loginInfo;
            _errorCodeBLL = new ErrorCodeBLL();
            _uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            // 載入測試項目
            string filePath = $@"WorkStationFile\{_loginInfo.ProductModel}\{_loginInfo.WorkStation}\{_loginInfo.Version}";
            _originalTestPlan = new INIFileBLL(filePath).LoadToModel();
            _testPlan = new INIFileBLL(filePath).LoadToModel();

            // 初始化
            BarcodeEnteredCommand = new RelayCommand<string>(OnBarcodeEntered);
            EnterMttModeCommand = new RelayCommand(OnEnterMttMode);
            LoadLocalLogs();

            for (int i = 0; i < _testPlan.Tasks.Count; i++)
            {
                // 測試項加入到顯示列表中
                TestSteps.Add(new TestStepViewModel(i, _testPlan.Tasks[i]));
            }

            _clockTimer = new System.Threading.Timer(OnClockTick, null, 0, 1000);
            _uploadTimer = new System.Threading.Timer(OnUploadTimerTick, null, 5000, 30000);
            Task.Run(MainTestLoop);
        }
        private void OnUploadTimerTick(object? state)
        {
            // 在背景執行緒執行上傳檢查
            LoggingBLL.UploadPendingLogs(_loginInfo);
        }
        private void LoadLocalLogs()
        {
            var (logs, passCount) = LogData.Load(_loginInfo);
            _passCount = passCount;
            _uiSyncContext.Post(_ =>
            {
                LogMessageAppended?.Invoke(new LogDisplayInfo { Text = "CLEAR_LOG" });
                foreach (var log in logs)
                {
                    LogMessageAppended?.Invoke(log);
                }
            }, null);
        }

        private void OnEnterMttMode(object? parameter)
        {
            _barcodeTcs.TrySetCanceled();
            _uiSyncContext.Post(_ => IsScanBarcodeVisible = false, null);

            _uiSyncContext.Post(_ =>
            {
                var testItemHeaders = new List<Tuple<string, int>>();
                for (int i = 0; i < _originalTestPlan.Tasks.Count; i++)
                {
                    if (!_originalTestPlan.Tasks[i].Enable)
                    {
                        testItemHeaders.Add(new Tuple<string, int>(_originalTestPlan.Tasks[i].Name, i));
                    }
                }

                MttSettings? settings = ShowMttSelectionDialogRequested?.Invoke(testItemHeaders, _testPlan);

                if (settings != null)
                {
                    _loopCount = settings.LoopCount;
                    _testPlan = settings.ModifiedTestPlan;
                    ResetForNewTest("WAIT");
                    TestSteps.Clear();
                    for (int i = 0; i < _testPlan.Tasks.Count; i++)
                    {
                        TestSteps.Add(new TestStepViewModel(i, _testPlan.Tasks[i]));
                    }
                }
            }, null);
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
                string barcode = "";
                try
                {
                    // 1. 等待掃描MES條碼
                    barcode = await RequestBarcodeScanAsync();
                }
                catch (TaskCanceledException)
                {
                    continue;
                }
                _uiSyncContext.Post(_ => LoopStatus = "", null);
                _uiSyncContext.Post(_ => IsScanBarcodeVisible = false, null);
                bool overallSuccess = true;
                _firstErrorDetail = null;
                for (int currentLoop = 1; currentLoop <= _loopCount; currentLoop++)
                {
                    var testResult = new TestResultModel { ScanBarcodeNumber = barcode };
                    var logBuilder = new StringBuilder();
                    // 更新UI顯示當前循環次數
                    if (_loopCount > 1)
                    {
                        _uiSyncContext.Post(_ => LoopStatus = $"Loop: {currentLoop} / {_loopCount}", null);
                    }
                    // 2. 初始化測試
                    ResetForNewTest("TESTTING");
                    var totalTimeStopwatch = Stopwatch.StartNew();
                    var totalTimeTimer = new System.Threading.Timer(
                        _ => _uiSyncContext.Post(s => TotalTestTime = $"Total Time: {totalTimeStopwatch.Elapsed.TotalSeconds:F2}", null),
                        null, 0, 100);

                    // 3. 執行所有測試步驟
                    bool isPassed = await ExecuteTestStepsAsync(testResult, logBuilder);

                    // 4. 測試結束，停止計時並更新最終結果
                    totalTimeStopwatch.Stop();
                    totalTimeTimer.Dispose();
                    _uiSync_UpdateTotalTime(totalTimeStopwatch.Elapsed.TotalSeconds);

                    if (currentLoop == _loopCount || !isPassed)
                    {
                        _uiSyncContext.Post(_ =>
                        {
                            OverallResult = isPassed ? "PASS" : "FAIL";
                            OverallResultColor = isPassed ? Color.Green : Color.Red;
                        }, null);
                    }
                    string finalResultStr = isPassed ? "PASS" : "FAIL";
                    string errorCode = "";
                    string errorMsg = "";

                    if (isPassed)
                    {
                        if(barcode != "0")
                            _passCount++; // 只有 PASS 才增加計數
                    }
                    else
                    {
                        string logForErrorCode = _firstErrorDetail ?? logBuilder.ToString();
                        errorCode = _errorCodeBLL.GetErrorCodeKey(logBuilder.ToString()) ?? "E999";
                        errorMsg = _errorCodeBLL.GetErrorDescription(errorCode);
                    }

                    var logInfo = new LogDisplayInfo
                    {
                        Timestamp = DateTime.Now.ToString("MM:dd_HH:mm:ss"),
                        PassCount = _passCount,
                        Barcode = barcode,
                        Result = finalResultStr,
                        ErrorCode = errorCode,
                        ErrorMessage = errorMsg,
                        IsFail = !isPassed
                    };

                    _uiSyncContext.Post(_ => LogMessageAppended?.Invoke(logInfo), null);
                    LogData.Append(_loginInfo, logInfo);


                    testResult.FinalResult = finalResultStr;
                    testResult.ErrorCode = errorCode;

                    if (!LoggingBLL.SaveToCSV(_loginInfo, testResult) && !_nasWarningShown)
                    {
                        _uiSyncContext.Post(_ => ShowMessageRequested?.Invoke("沒有連線到伺服器，傳送LOG失敗!\nNot connected to the server, failed to send log file!"), null);
                        _nasWarningShown = true;
                    }

                    if (!isPassed)
                    {
                        overallSuccess = false;
                        if (_loopCount > 1)
                        {
                            _uiSyncContext.Post(_ => LoopStatus = $"FAIL at Loop: {currentLoop} / {_loopCount}", null);
                        }
                        break;
                    }
                    if (currentLoop < _loopCount)
                    {
                        await Task.Delay(500);
                    }
                }
                _uiSyncContext.Post(_ =>
                {
                    OverallResult = overallSuccess ? "PASS" : "FAIL";
                    OverallResultColor = overallSuccess ? Color.Green : Color.Red;
                }, null);
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

        private void ResetForNewTest(string? message = null)
        {
            _uiSyncContext.Post(_ =>
            {
                //LogMessageAppended?.Invoke(new LogDisplayInfo { Text = "CLEAR_LOG" });

                OverallResult = message;
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
        private void _uiSync_UpdateTotalTime(double seconds)
        {
            _uiSyncContext.Post(_ => TotalTestTime = $"Total Time: {seconds:F2}", null);
        }

        private async Task<bool> ExecuteTestStepsAsync(TestResultModel testResult, StringBuilder logBuilder)
        {
            try
            {
                var allGroups = new List<List<(ItemTask task, int index)>>();
                List<(ItemTask task, int index)>? currentGroup = null;

                for (int i = 0; i < _testPlan.Tasks.Count; i++)
                {
                    var task = _testPlan.Tasks[i];

                    bool shouldBreakGroup = false;

                    if (currentGroup == null)
                    {
                        shouldBreakGroup = true;
                    }
                    else
                    {
                        var lastTask = currentGroup.Last().task;

                        if (lastTask.Enable != task.Enable) shouldBreakGroup = true;
                        else if (lastTask.Sync != task.Sync) shouldBreakGroup = true;
                        else if (task.Enable && task.Sync) shouldBreakGroup = true;
                    }

                    if (shouldBreakGroup)
                    {
                        if (currentGroup != null) allGroups.Add(currentGroup);
                        currentGroup = new List<(ItemTask task, int index)>();
                    }

                    currentGroup.Add((task, i));
                }
                if (currentGroup != null && currentGroup.Any()) { allGroups.Add(currentGroup); }

                var parallelExecutionList = new List<List<(ItemTask task, int index)>>();
                int minTaskIndexToRun = -1;
            ReExecuteLabel:

                for (int i = 0; i < allGroups.Count; i++)
                {
                    var group = allGroups[i];

                    if (group.Count <= 1 && !group.First().task.Enable) continue;
                    if (!group.First().task.Enable && !group.Skip(1).Any(t => t.task.Enable)) continue;

                    try
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
                                var parallelTasks = new List<Task<bool>>();
                                bool isFirstParallelGroup = true;

                                foreach (var parallelGroup in parallelExecutionList)
                                {
                                    parallelTasks.Add(ExecuteSingleGroupAsync(parallelGroup, testResult, logBuilder, canRequestScroll: isFirstParallelGroup, minTaskIndexToRun));
                                    isFirstParallelGroup = false;
                                }

                                bool[] results = await Task.WhenAll(parallelTasks);
                                parallelExecutionList.Clear();
                                if (results.Any(r => !r)) return false;
                            }

                            // 步驟 2: 處理當前的循序任務
                            bool sequentialResult = await ExecuteSingleGroupAsync(group, testResult, logBuilder, canRequestScroll: true, minTaskIndexToRun);
                            if (!sequentialResult) return false;
                        }
                    }
                    catch (RetryException ex)
                    {
                        // 捕捉重測例外
                        int targetTaskIndex = ex.TargetIndex;

                        minTaskIndexToRun = targetTaskIndex;
                        // 1. 清除 UI 狀態 (從 Target 到 Current 的所有步驟)
                        int currentMaxIndex = group.Max(t => t.index);

                        _uiSyncContext.Post(_ =>
                        {
                            for (int k = targetTaskIndex; k <= currentMaxIndex; k++)
                            {
                                if (k < TestSteps.Count)
                                {
                                    TestSteps[k].Result = "";
                                    TestSteps[k].SpendTime = "";
                                    TestSteps[k].Detail = "";
                                }
                            }
                        }, null);

                        // 2. 清除 TestResultModel 中的紀錄 (移除重測範圍內的結果)
                        for (int r = testResult.StepResults.Count - 1; r >= 0; r--)
                        {
                            var resultItemName = testResult.StepResults[r].TestItemName;
                            var taskIndex = _testPlan.Tasks.FindIndex(t => t.Name == resultItemName);
                            if (taskIndex >= targetTaskIndex)
                            {
                                testResult.StepResults.RemoveAt(r);
                            }
                        }

                        // 3. 清除並行清單，避免狀態殘留
                        parallelExecutionList.Clear();
                        // 4.重置 i
                        i = -1;
                        continue;
                    }
                }

                if (parallelExecutionList.Any())
                {
                    try
                    {
                        var parallelTasks = new List<Task<bool>>();
                        bool isFirstParallelGroup = true;

                        foreach (var parallelGroup in parallelExecutionList)
                        {
                            parallelTasks.Add(ExecuteSingleGroupAsync(parallelGroup, testResult, logBuilder, canRequestScroll: isFirstParallelGroup, minTaskIndexToRun));
                            isFirstParallelGroup = false;
                        }

                        bool[] results = await Task.WhenAll(parallelTasks);
                        if (results.Any(r => !r)) return false;
                    }
                    catch (RetryException ex)
                    {
                        int targetTaskIndex = ex.TargetIndex;
                        minTaskIndexToRun = targetTaskIndex; // 設定過濾條件

                        // 1. 清除 UI (清除從目標到最後的所有項目)
                        _uiSyncContext.Post(_ =>
                        {
                            for (int k = targetTaskIndex; k < TestSteps.Count; k++)
                            {
                                TestSteps[k].Result = "";
                                TestSteps[k].SpendTime = "";
                                TestSteps[k].Detail = "";
                            }
                        }, null);

                        // 2. 清除 Result Model
                        for (int r = testResult.StepResults.Count - 1; r >= 0; r--)
                        {
                            var resultItemName = testResult.StepResults[r].TestItemName;
                            var taskIndex = _testPlan.Tasks.FindIndex(t => t.Name == resultItemName);
                            if (taskIndex >= targetTaskIndex) testResult.StepResults.RemoveAt(r);
                        }

                        // 3. 清除並行暫存
                        parallelExecutionList.Clear();
                        goto ReExecuteLabel;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // 發生未預期的崩潰 (Crash)
                string errorMsg = $"CRITICAL SYSTEM ERROR: {ex.Message}\nStack Trace: {ex.StackTrace}";
                logBuilder.AppendLine(errorMsg);

                // 記錄第一筆錯誤資訊，確保 UI 顯示錯誤代碼
                lock (_firstErrorLock)
                {
                    if (_firstErrorDetail == null)
                    {
                        _firstErrorDetail = errorMsg;
                    }
                }

                // 嘗試顯示錯誤訊息給操作員 (使用 Invoke 確保 UI 執行緒安全)
                _uiSyncContext.Post(_ => ShowMessageRequested?.Invoke($"系統發生嚴重錯誤，測試終止。\n{ex.Message}"), null);

                return false; // 回傳失敗，讓 MainTestLoop 處理後續 Log 存檔
            }
        }

        private async Task<bool> ExecuteSingleGroupAsync(List<(ItemTask task, int index)> group, TestResultModel testResult, StringBuilder logBuilder, bool canRequestScroll, int minTaskIndex = -1)
        {
            foreach (var (task, index) in group)
            {
                if (minTaskIndex != -1 && index < minTaskIndex)
                {
                    continue;
                }
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
                        bool isRetry = (minTaskIndex != -1);
                        return DLLManagerBLL.ExecuteSpecificPlugin(
                            task.FunctionTestType,
                            task.FunctionTestPath,
                            out stepDetail,
                            _ownerHandle,
                            testResult,
                            isRetry
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
                logBuilder.AppendLine(stepResultItem.Detail);
                _uiSyncContext.Post(_ =>
                {
                    correspondingStepVM.Result = stepResultItem.Result;
                    correspondingStepVM.SpendTime = stepResultItem.SpendTime.ToString("F2");
                    correspondingStepVM.Detail = stepResultItem.Detail;    
                }, null);

                if (!stepPassed)
                {
                    lock (_firstErrorLock)
                    {
                        if (_firstErrorDetail == null)
                        {
                            _firstErrorDetail = stepResultItem.Detail;
                        }
                    }
                    foreach (int ngTaskIndex in task.NGTest)
                    {
                        int actualIndex = ngTaskIndex - 1;

                        if (actualIndex >= 0 && actualIndex < _testPlan.Tasks.Count)
                        {
                            var ngTask = _testPlan.Tasks[actualIndex];

                            if (ngTask.FunctionTestType.Contains("MessageWindows.exe"))
                            {
                                MessageBoxBLL.ExecuteMessageBox(ngTask.FunctionTestType, out _);
                            }
                            else
                            {
                                //不判定PASS FAIL ，不傳入 ownerHwnd
                                DLLManagerBLL.ExecuteSpecificPlugin(ngTask.FunctionTestType, ngTask.FunctionTestPath, out _, IntPtr.Zero, testResult, false);
                            }
                        }
                    }
                    // 檢查是否需要重測
                    if (task.RetryTarget > 0)
                    {
                        // 確保不在 UI 執行緒阻塞，使用 Invoke 呼叫 UI 層顯示對話框
                        bool userWantsRetry = false;

                        // 透過事件詢問 View
                        if (ConfirmRetryRequested != null)
                        {
                            string message = $"項目 [{task.Name}] 失敗 \n" +
                             $"設定要求跳回項目 {task.RetryTarget} 重測\n\n" +
                             $"是否執行重測？\n" +
                             $"(是: 重測 / 否: 失敗)"+
                             $"\n\n\nItem [{task.Name}] Failed.\nRetry requested from item {task.RetryTarget}.\n\nDo you want to retry?\n(Y: Retry / N: FAIL)";

                            userWantsRetry = ConfirmRetryRequested.Invoke(message);
                        }

                        if (userWantsRetry)
                        {
                            // 拋出例外，攜帶目標索引 (轉換為 0-based)
                            throw new RetryException(task.RetryTarget - 1);
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}