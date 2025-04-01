using ASMPTool.BLL;
using ASMPTool.Model;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;

namespace ASMP.ViewModel
{
    public class UIFormViewModel
    {

        public Thread _thread = null!;
        private static SynchronizationContext? _synchronizationContext;

        private readonly nint mHwd;
        private readonly INIFileBLL _iniFileBLL;
        private readonly ErrorCodeBLL _errorCodeBLL;
        private readonly TestResultItem ResultItem = new();

        public int TaskIndex;
        public double TaskItemStartTime;
        public double TaskItemSpendTime;

        private string previousLoopStep = string.Empty;
        private string TextBoxContent = string.Empty;
        private string TextBoxCount = "0000";

        public UIFormViewModel(nint Handle,string FilePath)
        {
            mHwd = Handle;
            // 取得資料
            _iniFileBLL = new INIFileBLL(FilePath);
            _errorCodeBLL = new ErrorCodeBLL();
            //將INIFileModel.Instance.Tasks裡的資料放置DataModel.Instance.DataTable
            DataTableModel.Instance.DataTable = _iniFileBLL.GetDataTable(DataTableModel.Instance.DataTable);
            //cmd       
            UIFormViewModel._synchronizationContext = SynchronizationContext.Current;
        }
        public void Start()
        {
            // 啟動RunLoop方法
            _thread = new Thread(RunLoop);
            _thread.Start();
        }
        private void RunLoop()
        {
            string LoopStep = "STEP_WAIT";
            bool bSaveLOG_FAIL = false;

            while (true)
            {
                UpdateLoopStepToTextBox(LoopStep);
                switch (LoopStep)
                {
                    case "STEP_WAIT":
                        LoopStep = "STEP_SCAN_BARCODE";
                        break;
                    case "STEP_SCAN_BARCODE":
                        DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageBoxScanBarcodeEnable);                        
                        LoopStep = "STEP_READ_BARCODE";
                        break;
                    case "STEP_READ_BARCODE":
                        if (TestResultModel.Instance.ScanBarcodeNumber != string.Empty)
                        {
                            DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageInitializedTotalTime);
                            LoopStep = "STEP_INIT_TASKS";                          
                        }
                        break;
                    case "STEP_INIT_TASKS":
                        TestResultModel.Instance.Clear();
                        UIFormViewModel.SyncUI("TestResultModel.Instance.TestResult", "TESTTING");
                        UIFormViewModel.SyncUI("TestResultModel.Instance.TestResultColor", "SILVER");
                        TestResultModel.Instance.StartTime = System.Environment.TickCount;

                        TaskIndex = 0;
                        DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageChangeDataGridDisplayRow, TaskIndex);
                        LoopStep = "STEP_CHECK_TASKS";
                        break;
                    case "STEP_CHECK_TASKS":
                        {
                            ResultItem.Result = "";
                            ResultItem.SpendTime = 0;
                            ResultItem.Detail = "";

                            if (TaskIndex >= INIFileModel.Instance.Tasks.Count)
                            {
                                LoopStep = "STEP_PASSED";
                                break;
                            }
                            DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageChangeDataGridDisplayRow, TaskIndex);
                            UpdateStringToTextBox(INIFileModel.Instance.Tasks[TaskIndex].Name);
                            if (INIFileModel.Instance.Tasks[TaskIndex].Enable == true)
                            {
                                //測試項
                                TaskItemStartTime = System.Environment.TickCount;
                                ResultItem.Result = "TESTTING";
                                ResultItem.StartTime = TaskItemStartTime;
                                TestResultModel.Instance.ChangeTableContent(TaskIndex , ResultItem);
                                StartTimeLoop();
                                LoopStep = "STEP_TASKS";
                            }
                            else
                            {
                                TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                                TaskIndex++;
                            }
                            break;
                        }
                    case "STEP_TASKS":
                        if (INIFileModel.Instance.Tasks[TaskIndex].FunctionTest)
                        {
                            bool bl = false;
                            if(INIFileModel.Instance.Tasks[TaskIndex].FunctionTestType.Contains(".dll"))
                            { 
                                bl = DLLManagerBLL.ExecuteSpecificPlugin(
                                    INIFileModel.Instance.Tasks[TaskIndex].FunctionTestType,
                                    INIFileModel.Instance.Tasks[TaskIndex].FunctionTestPath
                                    , out string Content);
                                ResultItem.Detail += Content;
                            }
                            else if (INIFileModel.Instance.Tasks[TaskIndex].FunctionTestType.Contains("MessageWindows.exe"))
                            {
                                bl = MessageBoxBLL.ExecuteMessageBox(
                                    INIFileModel.Instance.Tasks[TaskIndex].FunctionTestType
                                    , out string Content);
                                ResultItem.Detail += Content;
                            }
                            if (bl == false)
                            {
                                ResultItem.Result = "FAILED";
                                ResultItem.SpendTime = (System.Environment.TickCount - (TaskItemStartTime)) / 1000;
                                TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                                LoopStep = "STEP_NG_ITEM";
                            }
                            else
                            {
                                StopTimeLoop();
                                ResultItem.Result = "PASSED";
                                ResultItem.SpendTime = (System.Environment.TickCount - (TaskItemStartTime)) / 1000;
                                ResultItem.Detail += "Result:PASS#";
                                TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                                TaskIndex++;
                                LoopStep = "STEP_CHECK_TASKS";
                            }
                        }
                        else
                        {
                            //尚未有其他種類的測試項目  暫定保留
                            TaskIndex++;
                            LoopStep = "STEP_CHECK_TASKS";
                        }
                        break;
                    case "STEP_FAILED":
                        if(INIFileModel.Instance.Tasks[TaskIndex].FunctionTest)
                        {
                            
                            StopTimeLoop();
                            DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageTerminatedTotalTime);
                            ResultItem.Result = "FAILED";
                            ResultItem.SpendTime = (System.Environment.TickCount - (TaskItemStartTime)) / 1000;
                            ResultItem.Detail += "Result:FAILED#";
                            TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                            TestResultModel.Instance.ErrorCode = _errorCodeBLL.GetErrorCodeKey(ResultItem.Detail);
                            UpdateStringToTextBox("NG :"+ ResultItem.Detail);
                            UpdateStringToTextBox("ErrorCode :" + TestResultModel.Instance.ErrorCode);

                            LoopStep = LoggingBLL.SaveToCSV() ? "STEP_WAIT" : "STEP_RETRY_SAVE_RESULT";

                            UIFormViewModel.SyncUI("TestResultModel.Instance.TestResult", "FAIL");
                            UIFormViewModel.SyncUI("TestResultModel.Instance.TestResultColor", "RED");                  
                        }
                        break;
                    case "STEP_PASSED":
                        {
                            DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageTerminatedTotalTime);

                            LoopStep = LoggingBLL.SaveToCSV() ? "STEP_WAIT" : "STEP_RETRY_SAVE_RESULT";

                            UIFormViewModel.SyncUI("TestResultModel.Instance.TestResult", "PASS");
                            UIFormViewModel.SyncUI("TestResultModel.Instance.TestResultColor", "GREEN");                            
                            break;
                        }
                    case "STEP_NG_ITEM":
                        for (int i = 0; i < INIFileModel.Instance.Tasks[TaskIndex].NGTest.Count; i++)
                        {
                            int NG_TaskIndex = INIFileModel.Instance.Tasks[TaskIndex].NGTest[i]-1;
                            if (INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTest)
                            {   //直接執行NG後的項目 不判斷PASS、FAIL
                                if (INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTestType.Contains(".dll"))
                                {
                                    DLLManagerBLL.ExecuteSpecificPlugin(
                                        INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTestType,
                                        INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTestPath
                                        , out _);
                                }
                                else if (INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTestType.Contains("MessageWindows.exe"))
                                {
                                    MessageBoxBLL.ExecuteMessageBox(
                                        INIFileModel.Instance.Tasks[NG_TaskIndex].FunctionTestType
                                        , out _);
                                }                                
                            }
                        }
                        LoopStep = "STEP_FAILED";
                        break;
                    case "STEP_RETRY_SAVE_RESULT":
                        if (bSaveLOG_FAIL == false)
                        {
                            bSaveLOG_FAIL = true;
                            DLLManagerBLL.PostMessage(mHwd, MessageModel.MessageSaveFileToNAS_FAIL);
                        }
                        LoopStep = "STEP_WAIT";
                        break;
                }
                
                Thread.Sleep(100);
            }
        }
        public static void SyncUI(string Parm,string Value)
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext?.Post(state =>
                {
                    // 這裡可以執行UI操作
                    switch(Parm)
                    {
                        case "TestResultModel.Instance.TestResult":
                            TestResultModel.Instance.TestResult = Value;
                            break;
                        case "TestResultModel.Instance.TestResultColor":
                            TestResultModel.Instance.ResultColor = Value switch
                            {
                                "RED" => Color.Red,
                                "GREEN" => Color.Green,
                                _ => Color.Silver,
                            };
                            break;
                        case "TestResultModel.Instance.TextBoxText":
                            TestResultModel.Instance.TextBoxText = Value;
                            break;
                    }
                }, null);
            }
        }

        private CancellationTokenSource? _cts;
        /// <summary>
        /// Starts the time loop, which updates the spend time of the result item at regular intervals.
        /// </summary>
        public void StartTimeLoop()
        {
            // Create a new cancellation token source to control the time loop
            _cts = new CancellationTokenSource()!;
            // Start the time loop task, passing the cancellation token
            Task.Run(() => TimeLoop(), _cts.Token);
        }

        /// <summary>
        /// Stops the time loop.
        /// </summary>
        public void StopTimeLoop()
        {
            // Cancel the time loop by cancelling the token
            _cts?.Cancel();
        }

        /// <summary>
        /// The time loop, which updates the spend time of the result item at regular intervals.
        /// </summary>
        private void TimeLoop()
        {
            // Continue the loop until cancellation is requested
            if (_cts != null)
            {
                while (!_cts.IsCancellationRequested)
                {
                    // Calculate the spend time of the result item
                    ResultItem.SpendTime = (System.Environment.TickCount - (TaskItemStartTime)) / 1000;

                    // Update the table content with the new spend time
                    TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                    // Sleep for 100ms before updating again
                    Thread.Sleep(100);
                }
            }
        }
        private void UpdateLoopStepToTextBox(string loopStep)
        {
            if (previousLoopStep != loopStep)
            {
                previousLoopStep = loopStep;
                if (loopStep == "STEP_INIT_TASKS")
                {
                    TextBoxContent = "";
                    TextBoxCount = "0000";
                }
                else
                {
                    if (loopStep != "STEP_WAIT"
                        && loopStep != "STEP_SCAN_BARCODE"
                        && loopStep != "STEP_READ_BARCODE")
                    {
                        TextBoxCount = (int.Parse(TextBoxCount) + 1).ToString("D4");
                        TextBoxContent += TextBoxCount + " " + loopStep + "\r\n";
                        UIFormViewModel.SyncUI("TestResultModel.Instance.TextBoxText", TextBoxContent);
                    }
                }
            }
        }
        private void UpdateStringToTextBox(string text)
        {
            {
                TextBoxCount = (int.Parse(TextBoxCount) + 1).ToString("D4");
                TextBoxContent += TextBoxCount + " " + text + "\r\n";
                UIFormViewModel.SyncUI("TestResultModel.Instance.TextBoxText", TextBoxContent);
            }
        }
    }
}
