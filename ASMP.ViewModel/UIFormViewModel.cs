// ASMP.ViewModel/UIFormViewModel.cs

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

        // DataGridView 的資料來源。使用 BindingList<T> 是 WinForms 中實現雙向資料繫結的最佳選擇。
        public BindingList<TestStepViewModel> TestSteps { get; } = [];

        #endregion

        #region Commands

        // 詳解: 命令是 View 觸發 ViewModel 執行操作的標準方式。
        public ICommand BarcodeEnteredCommand { get; }

        #endregion

        #region Events

        // 詳解: 事件是 ViewModel 通知 View 執行某些無法透過資料繫結完成的 UI 操作（如滾動、彈窗）的方式。
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

        private async Task<bool> ExecuteTestStepsAsync(TestResultModel testResult)
        {
            for (int i = 0; i < _testPlan.Tasks.Count; i++)
            {
                var currentTask = _testPlan.Tasks[i];
                var correspondingStepVM = TestSteps[i];

                if (!currentTask.Enable)
                {
                    continue;
                }

                // --- UI 更新與計時器啟動 ---
                _uiSyncContext.Post(_ =>
                {
                    ScrollToRowRequested?.Invoke(i);
                    correspondingStepVM.Result = "TESTTING";
                }, null);
                _uiSync_AppendLog($"--- Start: {currentTask.Name} ---");

                var stepStopwatch = Stopwatch.StartNew();

                var timerCts = new CancellationTokenSource();

                // 每隔 10 毫秒，就更新一次畫面上的 SpendTime。
                var timerTask = Task.Run(async () =>
                {
                    while (!timerCts.Token.IsCancellationRequested)
                    {
                        _uiSyncContext.Post(s =>
                        {
                            // 只有在程式還在跑的時候才更新
                            if (stepStopwatch.IsRunning)
                                correspondingStepVM.SpendTime = stepStopwatch.Elapsed.TotalSeconds.ToString("F2");
                        }, null);
                        await Task.Delay(10); // 等待 10 毫秒
                    }
                });

                bool stepPassed = false;
                string stepDetail = "";

                var executionTask = Task.Run(() =>
                {
                    if (currentTask.FunctionTestType.Contains("MessageWindows.exe"))
                    {
                        return MessageBoxBLL.ExecuteMessageBox(currentTask.FunctionTestType, out stepDetail);
                    }
                    else
                    {
                        return DLLManagerBLL.ExecuteSpecificPlugin(
                            currentTask.FunctionTestType,
                            currentTask.FunctionTestPath,
                            out stepDetail,
                            _ownerHandle,
                            testResult
                        );
                    }
                });

                // 等待測試執行完成
                stepPassed = await executionTask;

                // 測試結束
                stepStopwatch.Stop();

                timerCts.Cancel();
                await timerTask; // 可以選擇性地等待計時器任務完全結束

                var finalStepDetail = stepDetail;

                // 更新最後一次的時間
                _uiSyncContext.Post(_ =>
                {
                    correspondingStepVM.Result = stepPassed ? "PASSED" : "FAILED";
                    correspondingStepVM.SpendTime = stepStopwatch.Elapsed.TotalSeconds.ToString("F2"); 
                    correspondingStepVM.Detail = finalStepDetail;

                    _uiSync_AppendLog(finalStepDetail);
                    _uiSync_AppendLog($"--- End: {currentTask.Name}, Result: {correspondingStepVM.Result} ---");
                }, null);

                // 寫入 Log 檔
                var stepResultItem = new TestResultItem
                {
                    TestItemName = currentTask.Name, 
                    Result = stepPassed ? "PASSED" : "FAILED",
                    SpendTime = stepStopwatch.Elapsed.TotalSeconds,
                    Detail = finalStepDetail
                };
                testResult.StepResults.Add(stepResultItem);

                // 如果測試失敗，則停止整個流程並回傳失敗
                if (!stepPassed)
                {
                    _uiSync_AppendLog($"*** Step Failed. Executing NG recovery steps... ***");

                    // 1. 取得在 INI 檔案中的 NGTest 
                    List<int> ngTestIndices = currentTask.NGTest;

                    // 2. 執行每一個指定的 NG 步驟
                    foreach (int ngTaskIndex in ngTestIndices)
                    {
                        int actualIndex = ngTaskIndex - 1;

                        if (actualIndex >= 0 && actualIndex < _testPlan.Tasks.Count)
                        {
                            var ngTask = _testPlan.Tasks[actualIndex];
                            _uiSync_AppendLog($"--- Executing NG Step: {ngTask.Name} ---");

                            // 執行 NG 任務，不判斷它是否成功，只是執行

                            if (ngTask.FunctionTestType.Contains("MessageWindows.exe"))
                            {
                                MessageBoxBLL.ExecuteMessageBox(ngTask.FunctionTestType, out _);
                            }
                            else
                            {   //不傳入 ownerHwnd
                                DLLManagerBLL.ExecuteSpecificPlugin(
                                    ngTask.FunctionTestType,
                                    ngTask.FunctionTestPath,
                                    out _,
                                    0,
                                    testResult
                                );
                            }
                        }
                    }
                    _uiSync_AppendLog($"*** NG recovery steps finished. Terminating test. ***");
                    return false; // 執行完 NG 步驟後，中斷整個測試流程
                }
                await Task.Delay(10);
            }
            return true; 
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
    }
}