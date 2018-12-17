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
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e) => unitIssue.ReadOnly = !unitIssue.ReadOnly;

        private void btnMutate_Click(object sender, RoutedEventArgs e)
        {
            if (unitIssue.ReadOnly)
            {
                //From: https://stackoverflow.com/a/1344258/7476183
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var stringChars = new char[8];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                unitIssue.ReportedIssue = new string(stringChars);
            }
        }
    }
}
