using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class frmSplashScreen : Window
    {
        EricStabileLibrary.SplashScreenVM ssvm = new EricStabileLibrary.SplashScreenVM();

        public frmSplashScreen()
        {
            InitializeComponent();
            DataContext = ssvm;
        }
    }
}
