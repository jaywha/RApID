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

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ucProgressControl : UserControl
    {
        private DependencyProperty _lblText = DependencyProperty.Register("LabelText", typeof(string), typeof(ucProgressControl), new PropertyMetadata("Loading..."));
        private DependencyProperty _pbar = DependencyProperty.Register("Marquee", typeof(bool), typeof(ucProgressControl));

        public string LabelText
        {
            get => (string)GetValue(_lblText);
            set => SetValue(_lblText, value);
        }

        public bool Marquee
        {
            get => (bool)GetValue(_pbar);
            set => SetValue(_pbar, value);
        }

        public static ProgressBar progBar;
        public static Label lblDesc;

        public ucProgressControl()
        {
            InitializeComponent();

            progBar = progData;
            lblDesc = lblLoadingIndicator;
        }
    }    
}
