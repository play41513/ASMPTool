using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMPTool.Model
{
    public class TestResultItem
    {
        public string Result { get; set; } = string.Empty;
        public double SpendTime { get; set; }
        public string Detail { get; set; } = string.Empty;
        public double StartTime { get; set; }
    }
    public class TestResultModel : INotifyPropertyChanged
    {


        private static TestResultModel? _instance;
        private List<TestResultItem> _results = [];

        private string TestResultValue { get; set; } = "WAIT";
        private Color ResultColorValue { get; set; }
        private string TextBoxTextValue { get; set; } = "";

        private double StartTimeValue { get; set; }
        private string ErrorCodeValue { get; set; } = string.Empty;
        private string UnitNumberValue { get; set; } = string.Empty;
        private string ScanBarcodeNumberValue { get; set; } = string.Empty;
        private string SerialNumberValue { get; set; } = string.Empty;
        private string MACNumberValue { get; set; } = string.Empty;
        private string MACNumberValue2 { get; set; } = string.Empty;
        private string MACNumberValue3 { get; set; } = string.Empty;

        public static TestResultModel Instance
        {
            get
            {
                _instance ??= new TestResultModel();
                return _instance;
            }
        }

        public void ChangeTableContent(int index, TestResultItem item)
        {
            TestResultItem ResultItem = new()
            {
                StartTime = item.StartTime,
                SpendTime = item.SpendTime,
                Result = item.Result,
                Detail = item.Detail
            };

            if (index >= 0 && index >= _results.Count)
                _results.Add(ResultItem);
            else
                _results[index] = ResultItem;
            OnPropertyChangedEx(nameof(TableContent), index);
        }
 
        public List<TestResultItem> TableContent
        {
            get { return _results; }
            set 
            { 
                _results = value;
                OnPropertyChangedEx(nameof(TableContent),0);
            }
        }

        public double StartTime
        {
            get { return StartTimeValue; }
            set 
            { 
                StartTimeValue = value;
                OnPropertyChangedEx(nameof(StartTime),0);
            }
        }
        public string TestResult
        {
            get { return TestResultValue; }
            set 
            { 
                TestResultValue = value;
                OnPropertyChanged(nameof(TestResult));
            }
        }
        public Color ResultColor
        {
            get { return ResultColorValue; }
            set
            {
                ResultColorValue = value;
                OnPropertyChanged(nameof(ResultColor));
            }
        }
        public string TextBoxText
        {
            get { return TextBoxTextValue; }
            set
            {
                TextBoxTextValue = value;
                OnPropertyChanged(nameof(TextBoxText));
            }
        }

        public string UnitNumber
        {
            get { return UnitNumberValue; }
            set
            {
                UnitNumberValue = value;
                OnPropertyChanged(nameof(UnitNumber));
            }
        }
        public string ScanBarcodeNumber
        {
            get { return ScanBarcodeNumberValue; }
            set
            {
                ScanBarcodeNumberValue = value;
                OnPropertyChanged(nameof(ScanBarcodeNumber));
            }
        }

        public string ErrorCode
        {
            get { return ErrorCodeValue; }
            set
            {
                ErrorCodeValue = value;
                OnPropertyChangedEx(nameof(ErrorCode),0);
            }
        }
        public string SerialNumber
        {
            get { return SerialNumberValue; }
            set
            {
                SerialNumberValue = value;
                OnPropertyChangedEx(nameof(SerialNumber),0);
            }
        }
        public string MACNumber1
        {
            get { return MACNumberValue; }
            set
            {
                MACNumberValue = value;
                OnPropertyChangedEx(nameof(MACNumber1),0);
            }
        }
        public string MACNumber2
        {
            get { return MACNumberValue2; }
            set
            {
                MACNumberValue2 = value;
                OnPropertyChangedEx(nameof(MACNumber2),0);
            }
        }
        public string MACNumber3
        {
            get { return MACNumberValue3; }
            set
            {
                MACNumberValue3 = value;
                OnPropertyChangedEx(nameof(MACNumber3),0);
            }
        }

        public void Clear()
        {
            _results.Clear();
            OnPropertyChangedEx("Clear", 0);
        }




        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // Invoke the PropertyChanged event if it has been subscribed to
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when a property value changes, with an additional index parameter.
        /// </summary>
        public delegate void PropertyChangedEventHandlerEx(object sender, PropertyChangedEventArgsEx e);

        /// <summary>
        /// Occurs when a property value changes, with an additional index parameter.
        /// </summary>
        public event PropertyChangedEventHandlerEx? PropertyChangedEx;

        /// <summary>
        /// Raises the PropertyChangedEx event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="index">The index of the property that changed.</param>
        protected virtual void OnPropertyChangedEx(string propertyName, int index)
        {
            // Invoke the PropertyChangedEx event if it has been subscribed to
            PropertyChangedEx?.Invoke(this, new PropertyChangedEventArgsEx(propertyName, index));
        }

        /// <summary>
        /// Provides data for the PropertyChangedEx event.
        /// </summary>
        public class PropertyChangedEventArgsEx : PropertyChangedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the PropertyChangedEventArgsEx class.
            /// </summary>
            /// <param name="propertyName">The name of the property that changed.</param>
            /// <param name="index">The index of the property that changed.</param>
            public PropertyChangedEventArgsEx(string propertyName, int index) : base(propertyName)
            {
                // Initialize the Index property
                Index = index;
            }

            /// <summary>
            /// Gets the index of the property that changed.
            /// </summary>
            public int Index { get; set; }
        }
    }
}
