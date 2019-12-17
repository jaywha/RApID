using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RApID_Project_WPF.Classes
{
    [DataContract]
    public class BOMInfoModel : INotifyPropertyChanged {
        [DataMember]
        public string FullAssemblyName { get; set; } = string.Empty;
        [DataMember]
        public string ReferenceDesignator { get; set; } = string.Empty;
        [DataMember]
        public string PartNumber { get; set; } = string.Empty;
        [DataMember]
        public string PartDescription { get; set; } = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
