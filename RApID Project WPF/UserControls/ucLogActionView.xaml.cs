using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using EricStabileLibrary;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucLogActionView.xaml
    /// </summary>
    public partial class ucLogActionView : UserControl, INotifyPropertyChanged
    {
        public csLog LogToView;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private string _techName;
        public string TechName
        {
            get =>_techName;
            set
            {
                _techName = value;
                OnPropertyChanged();
            }
        }

        private string _createDate;
        public string CreateDate
        {
            get => _createDate;
            set
            {
                _createDate = value;
                OnPropertyChanged();
            }
        }

        private string _submitDate;
        public string SubmitDate
        {
            get => _submitDate;
            set
            {
                _submitDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Shows the set of actions related to a <see cref="csLog"/> held in <see cref="LogToView"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown when <see cref="LogToView"/> wasn't set during instantiation.</exception>
        public ucLogActionView()
        {
            InitializeComponent();            
        }

        public void InitView()
        {
            TechName = LogToView.Tech;
            CreateDate = LogToView.LogCreationTime.ToShortDateString();
            SubmitDate = LogToView.LogSubmitTime.ToShortDateString();

            rtbLogBox.Document.Blocks.Clear();
            csDisplayLog.DisplayLog(rtbLogBox, csLogging.LogState.NONE, LogToView, false);
        }

    }
}
