﻿using System;
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

        private void btnToggle_Click(object sender, RoutedEventArgs e) => unitIssue.ReadOnly = !unitIssue.ReadOnly;

        private void btnMutate_Click(object sender, RoutedEventArgs e)
        {
            //From: https://stackoverflow.com/a/1344258/7476183
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var buttonTag = (sender as Button).Tag;
            switch (buttonTag) {
                case "ReportedIssue":
                    unitIssue.ReportedIssue = new string(stringChars);
                    break;
                case "TestResult":
                    unitIssue.TestResult = new string(stringChars);
                    break;
                case "TestResultAbort":
                    unitIssue.AbortResult = new string(stringChars);
                    break;
                case "Cause":
                    unitIssue.Cause = new string(stringChars);
                    break;
                case "Replacement":
                    unitIssue.Replacement = new string(stringChars);
                    break;
                case "Issue":
                    unitIssue.Issue = new string(stringChars);
                    break;
                case "Item":
                    unitIssue.Item = new string(stringChars);
                    break;
                case "Problem":
                    unitIssue.Problem = new string(stringChars);
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

        private void btnMutateForm_Click(object sender, RoutedEventArgs e) => unitIssue.IsRepairForm = !unitIssue.IsRepairForm;

        private void unitIssue_AddPartReplaced(object sender, RoutedEventArgs e)
        {
            unitIssue.dgMultipleParts.Items.Add(
                      new MultiplePartsReplaced()
                      {
                          RefDesignator = unitIssue.cmbxRefDesignator.SelectedValue.ToString(),
                          PartReplaced = unitIssue.cmbxPartNumber.SelectedValue.ToString(),
                          PartsReplacedPartDescription = frmProduction.getPartReplacedPartDescription(unitIssue.cmbxPartNumber.SelectedValue.ToString())
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
                                    MessageBox.Show("Couldn't find the barcode's entry in the database.\nPlease enter information manually.", 
                                        "Soft Error - BOM Lookup", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    }
}
