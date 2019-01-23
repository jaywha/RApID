using RApID_Project_WPF.UserControls;
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
using System.Windows.Threading;

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

            snkbrNotificationTray.MessageQueue.Enqueue($"{(sender as ComboBox).Name} changed vaule to {(sender as ComboBox).SelectedValue} !");
            Console.WriteLine($"DropDownEvent triggered at {DateTime.Now:hh:mm:ss tt}");
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
                using (var mapper = csSerialNumberMapper.Instance)
                {
                    await Task.Factory.StartNew(new Action(() => // in new task
                    {
                        var mapOps = Dispatcher.BeginInvoke(new Action(async () => // perform dispatched UI actions
                        {
                            Console.WriteLine("(TestWindow.xaml.cs) ==> Mapper successfully started...");
                            if (!mapper.GetData(txtSerialNumber.Text))
                            {
#if DEBUG
                                throw new InvalidOperationException("Couldn't find data for this barcode!");
#else
                                    snkbrNotificationTray.MessageQueue.Enqueue("Couldn't find the barcode's entry in the database.\nPlease enter information manually.");
                                    Console.WriteLine("Soft Error - BOM Lookup");
#endif
                            }
                            else
                            {
                                var result = await mapper.FindFileAsync(".xls");
                                csCrossClassInteraction.DoExcelOperations(result.Item1, progMapper,
                                new Tuple<Control, Control>(unitIssue.cmbxRefDesignator, unitIssue.cmbxPartNumber));

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
    }
}
