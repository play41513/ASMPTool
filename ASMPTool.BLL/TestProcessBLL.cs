using ASMPTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.BLL
{
    public class TestProcessBLL
    {
        private readonly nint _windowHandle;
        private CancellationTokenSource _cts;
        private Task _timeTask;
        public int TaskIndex { get; private set; }
        public TestResultItem ResultItem { get; private set; }

        public TestProcessBLL(nint windowHandle)
        {
            _windowHandle = windowHandle;
            ResultItem = new TestResultItem();
        }

        public void StartProcess()
        {

            Task.Run(ProcessLoop);
        }

        private async Task ProcessLoop()
        {
            string loopStep = "STEP_WAIT";
            while (true)
            {
                switch (loopStep)
                {
                    case "STEP_WAIT":
                        loopStep = "STEP_SCAN_BARCODE";
                        break;
                    case "STEP_SCAN_BARCODE":
                        DLLManagerBLL.PostMessage(_windowHandle, MessageModel.MessageBoxScanBarcodeEnable);
                        loopStep = "STEP_READ_BARCODE";
                        break;
                    case "STEP_READ_BARCODE":
                        if (!string.IsNullOrEmpty(TestResultModel.Instance.ScanBarcodeNumber))
                        {
                            DLLManagerBLL.PostMessage(_windowHandle, MessageModel.MessageInitializedTotalTime);
                            loopStep = "STEP_INIT_TASKS";
                        }
                        break;
                    case "STEP_INIT_TASKS":
                        ResetTestData();
                        loopStep = "STEP_CHECK_TASKS";
                        break;
                    case "STEP_CHECK_TASKS":
                        if (TaskIndex >= INIFileModel.Instance.Tasks.Count)
                        {
                            loopStep = "STEP_PASSED";
                            break;
                        }
                        ExecuteTask();
                        break;
                    case "STEP_PASSED":
                        EndProcess(true);
                        loopStep = "STEP_SCAN_BARCODE";
                        break;
                    case "STEP_FAILED":
                        EndProcess(false);
                        loopStep = "STEP_SCAN_BARCODE";
                        break;
                }
                await Task.Delay(100);
            }
        }

        private void ResetTestData()
        {
            TestResultModel.Instance.Clear();
            TaskIndex = 0;
            ResultItem.Result = "TESTTING";
        }

        private void ExecuteTask()
        {
            var task = INIFileModel.Instance.Tasks[TaskIndex];
            if (task.Enable)
            {
                ResultItem.StartTime = Environment.TickCount;
                StartTimeLoop();
                TaskIndex++;
            }
        }

        public void StartTimeLoop()
        {
            _cts = new CancellationTokenSource();
            _timeTask = Task.Run(TimeLoop, _cts.Token);
        }

        public void StopTimeLoop()
        {
            _cts.Cancel();
        }

        private void TimeLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                ResultItem.SpendTime = (Environment.TickCount - ResultItem.StartTime) / 1000;
                TestResultModel.Instance.ChangeTableContent(TaskIndex, ResultItem);
                Thread.Sleep(100);
            }
        }

        private void EndProcess(bool isPassed)
        {
            DLLManagerBLL.PostMessage(_windowHandle, MessageModel.MessageTerminatedTotalTime);
            ResultItem.Result = isPassed ? "PASSED" : "FAILED";
        }
    }

}
