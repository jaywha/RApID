using RApID_Project_WPF.UserControls;
using SNMapperLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> RefDes = new ObservableCollection<string>();
        public ObservableCollection<string> PartNum = new ObservableCollection<string>();

        private TextBox _partNumberTBOX;
        public TextBox PartNumberTBOX
        {
            get => _partNumberTBOX;
            private set {
                _partNumberTBOX = value;
                OnPropertyChanged();
            }
        }

        private string _refNumber = string.Empty;
        public string RefNumber
        {
            get => _refNumber;
            set {
                _refNumber = value;
                OnPropertyChanged();
            }
        }

        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set
            {
                _partNumber = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public TestWindow()
        {
            InitializeComponent();
            tcUnitIssues.AddTabItem(unitIssue.Copy(), "Modelled Tab");

            unitIssue.cmbxRefDesignator.ItemsSource = RefDes;
            unitIssue.cmbxPartNumber.ItemsSource = PartNum;
            PartNumberTBOX = txtPartNumber;
        }

        private void btnUCSwitch_Click(object sender, RoutedEventArgs e)
        {
            tcUnitIssues.Visibility = (tcUnitIssues.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            unitIssue.Visibility = (unitIssue.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnMutateForm_Click(object sender, RoutedEventArgs e)
        {
            tcUnitIssues.IsRepair = !tcUnitIssues.IsRepair;
            unitIssue.IsRepairForm = !unitIssue.IsRepairForm;
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            tcUnitIssues.ReadOnly = !tcUnitIssues.ReadOnly;
            unitIssue.ReadOnly = !unitIssue.ReadOnly;
        }

        private void btnMutate_Click(object sender, RoutedEventArgs e)
        {
            var buttonTag = (sender as Button).Tag;
            switch (buttonTag) {
                case "ReportedIssue":
                    unitIssue.ReportedIssue = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "TestResult":
                    unitIssue.TestResult = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "TestResultAbort":
                    unitIssue.AbortResult = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "Cause":
                    unitIssue.Cause = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "Replacement":
                    unitIssue.Replacement = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "Issue":
                    unitIssue.Issue = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "Item":
                    unitIssue.Item = csCrossClassInteraction.GenerateRandomString();
                    break;
                case "Problem":
                    unitIssue.Problem = csCrossClassInteraction.GenerateRandomString();
                    break;
                default:
                    Console.WriteLine($"Tag {buttonTag} wasn't found => Nothing to do here...");
                    break;
            }
            
        }

        private void unitIssue_DropDownEvent(object sender, EventArgs e)
        {

            MessageBox.Show($"{(sender as ComboBox).Name} changed vaule to {(sender as ComboBox).SelectedValue} !", 
                $"DropDownEvent triggered at {DateTime.Now:hh:mm:ss tt}",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void unitIssue_AddPartReplaced(object sender, RoutedEventArgs e)
        {
            unitIssue.dgMultipleParts.Items.Add(
                      new MultiplePartsReplaced()
                      {
                          RefDesignator = unitIssue.cmbxRefDesignator.SelectedValue.ToString(),
                          PartReplaced = unitIssue.cmbxPartNumber.SelectedValue.ToString(),
                          PartsReplacedPartDescription = csCrossClassInteraction.GetPartReplacedPartDescription(unitIssue.cmbxPartNumber.SelectedValue.ToString())
                      });

            unitIssue.cmbxPartNumber.SelectedIndex = -1;
            unitIssue.cmbxRefDesignator.SelectedIndex = -1;
        }

        public bool BOMFileActive = false;

        private async void MapRefDesToPartNum()
        {
            try
            {
                using (csSerialNumberMapper mapper = csSerialNumberMapper.Instance)
                {
                    await Task.Factory.StartNew(new Action(() => // in new task
                    {
                        DispatcherOperation mapOps = Dispatcher.BeginInvoke(new Action(async () => // perform dispatched UI actions
                        {
                            Console.WriteLine("(TestWindow.xaml.cs) ==> Mapper successfully started...");
                            if (!mapper.GetData(txtSerialNumber.Text))
                            {
                                    MessageBox.Show("Couldn't find the barcode's entry in the database.\nPlease enter information manually.", 
                                        "Soft Error - BOM Lookup", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                MessageBox.Show(mapper.Success(), "DEBUG", MessageBoxButton.OK, MessageBoxImage.Information);
                                (string filename, bool found) result = await mapper.FindFileAsync(".xls");
                                csCrossClassInteraction.DoExcelOperations(result.filename, progMapper, RefDes, PartNum);

                                csCrossClassInteraction.MapperSuccessMessage(result.filename, mapper.PartNumber);

                                BOMFileActive = true;
                            }
                        }), DispatcherPriority.Background);
                        mapOps.Completed += delegate {
                            txtSerialNumber.Dispatcher.Invoke(() => txtSerialNumber.IsEnabled = true);
                        };
                    }));
                }
            }
            catch (InvalidOperationException ioe)
            {
                csExceptionLogger.csExceptionLogger.Write("BadBarcode-MapRefDesToPartNum", ioe);
                return;
            }
        }

        private void txtSerialNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter))
            {
                (sender as TextBox).IsEnabled = false;
                MapRefDesToPartNum();
            }
        }

        private void btnContextSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (unitIssue.Visibility != Visibility.Visible)
            {
                tcUnitIssues.Visibility = Visibility.Collapsed;
                unitIssue.Visibility = Visibility.Visible;
            } else
            {
                tcUnitIssues.Visibility = Visibility.Visible;
                unitIssue.Visibility = Visibility.Collapsed;
            }
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e) => Mailman.SendEmail("", "", new WhiningException("Testing Email - <yay>"));
    }
}
