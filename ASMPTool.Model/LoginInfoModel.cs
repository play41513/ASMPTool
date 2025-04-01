

using System.ComponentModel;

namespace ASMPTool.Model
{
    public class LoginInfoModel : INotifyPropertyChanged
    {
        private static LoginInfoModel? _instance;
        private string WorkOrderValue { get; set; } = string.Empty;
        private string EmployeeIDValue { get; set; } = string.Empty;
        private string ProductModelValue { get; set; } = string.Empty;
        private string WorkStationValue { get; set; } = string.Empty;
        private string VersionValue { get; set; } = string.Empty;
        private string NAS_IP_AddressValue { get; set; } = string.Empty;



        public static LoginInfoModel Instance
        {
            get
            {
                _instance ??= new LoginInfoModel();
                return _instance;
            }
        }
        public string WorkOrder
        {
            get { return WorkOrderValue; }
            set 
            { 
                WorkOrderValue = value; 
                OnPropertyChanged(nameof(WorkOrder)); 
            }
        }

        public string EmployeeID
        {
            get { return EmployeeIDValue; }
            set 
            { 
                EmployeeIDValue = value; 
                OnPropertyChanged(nameof(EmployeeID)); 
            }
        }
        public string ProductModel
        {
            get { return ProductModelValue; }
            set
            {
                ProductModelValue = value;
                OnPropertyChanged(nameof(ProductModel));
            }
        }
        public string WorkStation
        {
            get { return WorkStationValue; }
            set
            {
                WorkStationValue = value;
                OnPropertyChanged(nameof(WorkStation));
            }
        }
        public string Version
        {
            get { return VersionValue; }
            set
            {
                VersionValue = value;
                OnPropertyChanged(nameof(Version));
            }
        }

        public string NAS_IP_Address
        {
            get { return NAS_IP_AddressValue; }
            set
            {
                NAS_IP_AddressValue = value;
                OnPropertyChanged(nameof(NAS_IP_Address));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}