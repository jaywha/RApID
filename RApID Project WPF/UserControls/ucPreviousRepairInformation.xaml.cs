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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucPreviousRepairInformation.xaml
    /// </summary>
    public partial class ucPreviousRepairInformation : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DisplayDataProperty = DependencyProperty.Register("DisplayData", typeof(object), typeof(ucPreviousRepairInformation));
        public static readonly DependencyProperty AcceptButtonCommandProperty = DependencyProperty.Register("AcceptCommand", typeof(RoutedCommand), typeof(ucPreviousRepairInformation));
        public static readonly DependencyProperty CancelButtonCommandProperty = DependencyProperty.Register("CancelCommand", typeof(RoutedCommand), typeof(ucPreviousRepairInformation));
        public static readonly DependencyProperty AcceptCommandParameterProperty = DependencyProperty.Register("AcceptCommandParameter", typeof(object), typeof(ucPreviousRepairInformation));
        public static readonly DependencyProperty CancelCommandParameterProperty = DependencyProperty.Register("CancelCommandParameter", typeof(object), typeof(ucPreviousRepairInformation));
        #endregion

        #region Properties
        public object DisplayData
        {
            get { return (object)GetValue(DisplayDataProperty); }
            set {
                SetValue(DisplayDataProperty, value);
                OnPropertyChanged();
            }
        }

        #region Dialog Buttons
        [Description("The command that the OK button will execute"), Category("Common")]
        public RoutedCommand AcceptCommand
        {
            get { return (RoutedCommand)GetValue(AcceptButtonCommandProperty); }
            set { SetValue(AcceptButtonCommandProperty, value); }
        }
        [Description("The command that the Cancel button will execute"), Category("Common")]
        public RoutedCommand CancelCommand
        {
            get { return (RoutedCommand)GetValue(CancelButtonCommandProperty); }
            set { SetValue(CancelButtonCommandProperty, value); }
        }
        [Description("The CommandParameter of the OK button"), Category("Common")]
        public object AcceptCommandParameter
        {
            get { return GetValue(AcceptCommandParameterProperty); }
            set { SetValue(AcceptCommandParameterProperty, value); }
        }
        [Description("The CommandParameter of the Cancel button"), Category("Common")]
        public object CancelCommandParameter
        {
            get { return GetValue(CancelCommandParameterProperty); }
            set { SetValue(CancelCommandParameterProperty, value); }
        }
        #endregion
        #endregion

        public ucPreviousRepairInformation()
        {
            InitializeComponent();
        }
    }
}
