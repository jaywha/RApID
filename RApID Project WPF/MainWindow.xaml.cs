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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StaticVars sVars = StaticVars.StaticVarsInstance();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int xLoc = Convert.ToInt32((Width / 2) - (lblTitle.Width / 2));
            lblTitle.Margin = new Thickness(xLoc, 10, 0, 0);
            xLoc = Convert.ToInt32((Width / 2) - (lblTitleDef.Width / 2));
            lblTitleDef.Margin = new Thickness(xLoc, lblTitleDef.Margin.Top, 0, 0);
            sVars.initStaticVars();

            var s = ((decimal)sVars.VersionControl.CurrentVersion).ToString();
            Title = "RApID: v." + s;
        }

        private void btnClicks(object sender, RoutedEventArgs e)
        {
            if (((Control)sender).Name.ToString().Equals("btnRework"))
            {
                Hide();
                frmProduction fpr = new frmProduction();
                fpr.Owner = this;
                fpr.ShowDialog();
                Show();
                Activate();
            }
            else if(((Control)sender).Name.ToString().Equals("btnRepair"))
            {
                Hide();
                Repair rpr = new Repair(false);
                rpr.Owner = this;
                rpr.ShowDialog();
                Show();
                Activate();
            }
            else if (((Control)sender).Name.ToString().Equals("btnReportViewer"))
            {
                System.Diagnostics.Process.Start(Properties.Settings.Default.DefaultReportManagerLink);
            }
            else if (((Control)sender).Name.ToString().Equals("btnQCDQE"))
            {
                Hide();
                frmQCDQE fQC = new frmQCDQE();
                fQC.Owner = this;
                fQC.ShowDialog();
                Show();
                Activate();
            }
            else if (((Control)sender).Name.ToString().Equals("btnSettings"))
            {
                Hide();
                frmSettings fSettings = new frmSettings();
                fSettings.Owner = this;
                fSettings.ShowDialog();
                Show();
                Activate();
            }
        }
    }
}
