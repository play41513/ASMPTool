
using ASMPTool.Model;
using System.Drawing;

namespace ASMP.ViewModel
{
    public class TestStepViewModel : ViewModelBase
    {
        private string _result = string.Empty;
        private string _spendTime = string.Empty;
        private string _detail = string.Empty;
        private Color _resultColor = Color.Black;

        public string Index { get; }
        public string TestItem { get; }
        public string TestStep { get; }

        public string Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public string SpendTime
        {
            get => _spendTime;
            set => SetProperty(ref _spendTime, value);
        }

        public string Detail
        {
            get => _detail;
            set => SetProperty(ref _detail, value);
        }

        // 用於 DataGridView 儲存格變色的屬性
        public Color ResultColor
        {
            get
            {
                return Result switch
                {
                    "TESTTING" => Color.SteelBlue,
                    "PASSED" => Color.Green,
                    "FAILED" => Color.Red,
                    _ => Color.Black,
                };
            }
        }

        public TestStepViewModel(int index, ItemTask task)
        {
            Index = (index + 1).ToString();

            TestItem = task.Enable ? "" : task.Name;
            TestStep = task.Enable ? task.Name : "";
        }
    }
}