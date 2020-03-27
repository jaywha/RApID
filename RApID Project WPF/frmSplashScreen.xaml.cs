using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmSplashScreen.xaml
    /// </summary>
    public partial class frmSplashScreen : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private string _splashText = "Loading...";

        public string SplashText
        {
            get { return _splashText; }
            set
            {
                if (_splashText != value)
                {
                    _splashText = value;
                    OnPropertyChanged();
                }
            }
        }

        public frmSplashScreen()
        {
            InitializeComponent();
        }
    }
}
