using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASMPTool.Model
{
    public class CommandModel : INotifyPropertyChanged
    {
        private string ?_commandOutput;

        public string ?CommandOutput
        {
            get { return _commandOutput; }
            set
            {
                _commandOutput = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler ?PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
