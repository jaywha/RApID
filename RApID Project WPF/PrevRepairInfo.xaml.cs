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
using System.Data.SqlClient;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for PrevRepairInfo.xaml
    /// </summary>
    public partial class PrevRepairInfo : Window
    {

        /// <summary>
        /// >>>>
        /// THIS IS ONLY USED FOR GETTING PREVIOUS INFORMATION FROM THE DQE/QC FORM
        /// <<<<
        /// </summary>
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        PreviousRepairInformation PRI;
        List<string> lUnitIssues = new List<string>(7); //-This will be used to house the unit issues and will be used to build the unit issue tab control.
        public PrevRepairInfo(PreviousRepairInformation prevInfo)
        {
            InitializeComponent();
            PRI = prevInfo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (PRI.OldDB)
            {
                lUnitIssues = new List<string>(4);
                adjustForm();
                loadPrevInfoOldDB();
            }
            else
            {
                lUnitIssues = new List<string>(7);
                loadPrevInfoNewDB();
            }
        }

        /// <summary>
        /// If pulling data from the old database, adjust the form so that there aren't a lot of excess unused controls.
        /// </summary>
        private void adjustForm()
        {
            Thickness oldPosL = lblDateSubmitted.Margin;
            Thickness oldPosC = txtDateSubmitted.Margin;

            Thickness newPosL = lblDateReceived.Margin;
            Thickness newPosC = txtDateReceived.Margin;

            lblDateSubmitted.Margin = newPosL;
            txtDateSubmitted.Margin = newPosC;

            newPosL = oldPosL;
            newPosC = oldPosC;

            oldPosL = lblPartName.Margin;
            oldPosC = txtPartName.Margin;

            lblPartName.Margin = newPosL;
            txtPartName.Margin = newPosC;

            newPosL = oldPosL;
            newPosC = oldPosC;

            oldPosL = lblPartNumber.Margin;
            oldPosC = txtPartNumber.Margin;

            lblPartNumber.Margin = newPosL;
            txtPartNumber.Margin = newPosC;

            newPosL = oldPosL;
            newPosC = oldPosC;

            oldPosL = lblCommoditySubClass.Margin;
            oldPosC = txtCommSubClass.Margin;

            lblCommoditySubClass.Margin = newPosL;
            txtCommSubClass.Margin = newPosC;

            newPosL = oldPosL;
            newPosC = oldPosC;

            oldPosL = lblSN.Margin;
            oldPosC = txtSN.Margin;

            lblSN.Margin = newPosL;
            txtSN.Margin = newPosC;

            newPosL = oldPosL;
            newPosC = oldPosC;

            lblTOR.Margin = newPosL;
            txtTOR.Margin = newPosC;

            cbxScrap.Margin = txtSWVersion.Margin;

            lblDateReceived.Visibility = txtDateReceived.Visibility = lblSWVersion.Visibility = txtSWVersion.Visibility = lblTOF.Visibility = txtTOF.Visibility = lblHOU.Visibility = txtHOU.Visibility = lblAdditionalComments.Visibility = rtbAdditionalComments.Visibility = Visibility.Hidden;
            lblDateReceived.IsEnabled = txtDateReceived.IsEnabled = lblSWVersion.IsEnabled = txtSWVersion.IsEnabled = lblTOF.IsEnabled = txtTOF.IsEnabled = lblHOU.IsEnabled = txtHOU.IsEnabled = lblAdditionalComments.IsEnabled = rtbAdditionalComments.IsEnabled = false;

            this.Height -= 110;
        }

        private void loadPrevInfoOldDB()
        {
            string query = "SELECT * FROM tblManufacturingTechReport WHERE SerialNumber = '" + PRI.SerialNumber + "' AND Date_Time = '" + PRI.DateSubmitted.ToString() + "' AND Technician = '" + PRI.TechName + "'";

            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        txtTechName.Text = PRI.TechName;
                        txtDateSubmitted.Text = PRI.DateSubmitted.ToString("MM/dd/yyyy hh:mm:ss tt");
                        txtPartName.Text = reader["PartName"].ToString();
                        txtPartNumber.Text = reader["PartNumber"].ToString();
                        if (reader["Scrap"].ToString().Equals("-1"))
                            cbxScrap.IsChecked = true;
                        txtSN.Text = PRI.SerialNumber;
                        txtTOR.Text = reader["FromArea"].ToString();

                        lUnitIssues.Add(reader["TestFailure"].ToString());
                        lUnitIssues.Add(reader["Issue"].ToString());
                        lUnitIssues.Add(reader["Item"].ToString());
                        lUnitIssues.Add(reader["Problem"].ToString());
                    }
                }

                conn.Close();

                query = "SELECT CommodityClass FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text + "'";
                conn.ConnectionString = holder.HummingBirdConnectionString;
                cmd = new SqlCommand(query, conn);
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[0] != DBNull.Value)
                            {
                                txtCommSubClass.Text = reader[0].ToString();
                                break;
                            }
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn != null)
                        conn.Close();
                    MessageBox.Show("There was an issue loading old database information.\nError Message: " + ex.Message, "loadPrevInfoOldDB() -> Comm SubClass", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                buildTabControlOLDDB();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue loading old database information.\nError Message: " + ex.Message, "loadPrevInfoOldDB()");
            }
        }

        private void loadPrevInfoNewDB()
        {
            string query = "SELECT * FROM TechnicianSubmission WHERE SerialNumber = '" + PRI.SerialNumber + "' AND DateSubmitted = '" + PRI.DateSubmitted.ToString() + "' AND Technician = '" + PRI.TechName + "'";
            
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        txtTechName.Text = PRI.TechName;
                        txtDateReceived.Text = Convert.ToDateTime(reader["DateReceived"]).ToString("MM/dd/yyyy");
                        txtDateSubmitted.Text = PRI.DateSubmitted.ToString("MM/dd/yyyy hh:mm:ss tt");
                        txtPartName.Text = reader["PartName"].ToString();
                        txtPartNumber.Text = reader["PartNumber"].ToString();
                        txtCommSubClass.Text = reader["CommoditySubClass"].ToString();
                        txtSWVersion.Text = reader["SoftwareVersion"].ToString();
                        txtSN.Text = PRI.SerialNumber;
                        cbxScrap.IsChecked = Convert.ToBoolean(reader["Scrap"]);
                        txtTOR.Text = reader["TypeOfReturn"].ToString();
                        txtTOF.Text = reader["TypeOfFailure"].ToString();
                        txtHOU.Text = reader["HoursOnUnit"].ToString();

                        if (reader["TechAct1"] != DBNull.Value)
                            txtTechAction1.Text = reader["TechAct1"].ToString();
                        if (reader["TechAct2"] != DBNull.Value)
                            txtTechAction2.Text = reader["TechAct2"].ToString();
                        if (reader["TechAct3"] != DBNull.Value)
                            txtTechAction3.Text = reader["TechAct3"].ToString();

                        lUnitIssues.Add(reader["ReportedIssue"].ToString());
                        lUnitIssues.Add(reader["TestResult"].ToString());
                        lUnitIssues.Add(reader["TestResultAbort"].ToString());
                        lUnitIssues.Add(reader["Cause"].ToString());
                        lUnitIssues.Add(reader["Replacement"].ToString());
                        lUnitIssues.Add(reader["PartsReplaced"].ToString());
                        lUnitIssues.Add(reader["RefDesignator"].ToString());

                        rtbAdditionalComments.AppendText(reader["AdditionalComments"].ToString());
                    }
                }
                conn.Close();
                buildTabControl();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue loading previous repair information.\nError Message: " + ex.Message, "loadPrevInfoNewDB()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Processes the overall list of unit issues into individual unit issues
        /// </summary>
        /// <param name="unitIssue"></param>
        /// <param name="IsPartOrRef">Is this a part replaced or ref designator string?</param>
        /// <returns>Returns a List of strings for each unit issue.</returns>
        private List<string> processUnitIssues(string unitIssue, bool IsPartOrRef)
        {
            var lUnitIssueReturn = new List<string>();
            
            if(IsPartOrRef)
            {
                string[] splitters = { "," };
                string[] sSplit = unitIssue.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                lUnitIssueReturn.AddRange(sSplit);
            }
            else
            {
                string[] splitters = { ", " };
                string[] sSplit = unitIssue.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                lUnitIssueReturn.AddRange(sSplit);
            }
            
            return lUnitIssueReturn;
        }


        /// <summary>
        /// Builds the tab control that houses all of the unit issues for the old database
        /// </summary>
        private void buildTabControlOLDDB()
        {
            var UILayout = new Grid();
            UILayout.Width = 300;
            UILayout.Height = 430;
            UILayout.HorizontalAlignment = HorizontalAlignment.Left;
            UILayout.VerticalAlignment = VerticalAlignment.Top;
            UILayout.Margin = new Thickness(0, 0, 0, 0);

            try
            {

                var lblTestFailure = new Label();
                lblTestFailure.HorizontalAlignment = HorizontalAlignment.Left;
                lblTestFailure.VerticalAlignment = VerticalAlignment.Top;
                lblTestFailure.Content = "Test Failure:";
                lblTestFailure.Margin = new Thickness(10, 10, 0, 0);
                UILayout.Children.Add(lblTestFailure);

                var txtTestFailure = new TextBox();
                txtTestFailure.HorizontalAlignment = HorizontalAlignment.Left;
                txtTestFailure.VerticalAlignment = VerticalAlignment.Top;
                txtTestFailure.Name = "txtTestFailure";
                txtTestFailure.Text = lUnitIssues[0].ToString();
                txtTestFailure.Width = 275;
                txtTestFailure.Height = 23;
                txtTestFailure.Margin = new Thickness(10, 33, 0, 0);
                txtTestFailure.IsReadOnly = true;
                UILayout.Children.Add(txtTestFailure);

                var lblIssue = new Label();
                lblIssue.HorizontalAlignment = HorizontalAlignment.Left;
                lblIssue.VerticalAlignment = VerticalAlignment.Top;
                lblIssue.Content = "Issue:";
                lblIssue.Margin = new Thickness(10, 60, 0, 0);
                UILayout.Children.Add(lblIssue);

                var txtIssue = new TextBox();
                txtIssue.HorizontalAlignment = HorizontalAlignment.Left;
                txtIssue.VerticalAlignment = VerticalAlignment.Top;
                txtIssue.Name = "txtIssue";
                txtIssue.Text = lUnitIssues[1];
                txtIssue.Width = 275;
                txtIssue.Height = 23;
                txtIssue.Margin = new Thickness(10, 83, 0, 0);
                txtIssue.IsReadOnly = true;
                UILayout.Children.Add(txtIssue);

                var lblItem = new Label();
                lblItem.HorizontalAlignment = HorizontalAlignment.Left;
                lblItem.VerticalAlignment = VerticalAlignment.Top;
                lblItem.Content = "Item:";
                lblItem.Margin = new Thickness(10, 110, 0, 0);
                UILayout.Children.Add(lblItem);

                var txtItem = new TextBox();
                txtItem.HorizontalAlignment = HorizontalAlignment.Left;
                txtItem.VerticalAlignment = VerticalAlignment.Top;
                txtItem.Name = "txtTestResultAbort";
                txtItem.Text = lUnitIssues[2];
                txtItem.Width = 275;
                txtItem.Height = 23;
                txtItem.Margin = new Thickness(10, 133, 0, 0);
                txtItem.IsReadOnly = true;
                UILayout.Children.Add(txtItem);

                var lblProblem = new Label();
                lblProblem.HorizontalAlignment = HorizontalAlignment.Left;
                lblProblem.VerticalAlignment = VerticalAlignment.Top;
                lblProblem.Content = "Problem:";
                lblProblem.Margin = new Thickness(10, 160, 0, 0);
                UILayout.Children.Add(lblProblem);

                var txtProblem = new TextBox();
                txtProblem.HorizontalAlignment = HorizontalAlignment.Left;
                txtProblem.VerticalAlignment = VerticalAlignment.Top;
                txtProblem.Name = "txtCause";
                txtProblem.Text = lUnitIssues[3];
                txtProblem.Width = 275;
                txtProblem.Height = 23;
                txtProblem.Margin = new Thickness(10, 183, 0, 0);
                txtProblem.IsReadOnly = true;
                UILayout.Children.Add(txtProblem);

                
            }
            catch { }

            var ti = new TabItem();
            ti.Header = "Unit Issue";
            ti.Content = UILayout;
            
            UnitIssueTabControl.Items.Add(ti);
            UnitIssueTabControl.SelectedIndex = 0;
        }


        /// <summary>
        /// Builds the tab control that houses all of the unit issues for the new database
        /// </summary>
        private void buildTabControl()
        {
            List<string> lIssues = processUnitIssues(lUnitIssues[0], false);
            List<string> lTestResult = processUnitIssues(lUnitIssues[1], false);
            List<string> lTestResultAbort = processUnitIssues(lUnitIssues[2], false);
            List<string> lCause = processUnitIssues(lUnitIssues[3], false);
            List<string> lReplacement = processUnitIssues(lUnitIssues[4], false);

            for (int i = 0; i < lTestResult.Count; i++)
            {
                var UILayout = new Grid();
                UILayout.Width = 300;
                UILayout.Height = 430;
                UILayout.HorizontalAlignment = HorizontalAlignment.Left;
                UILayout.VerticalAlignment = VerticalAlignment.Top;
                UILayout.Margin = new Thickness(0, 0, 0, 0);

                try
                {

                    var lblIssue = new Label();
                    lblIssue.HorizontalAlignment = HorizontalAlignment.Left;
                    lblIssue.VerticalAlignment = VerticalAlignment.Top;
                    lblIssue.Content = "Issue:";
                    lblIssue.Margin = new Thickness(10, 10, 0, 0);
                    UILayout.Children.Add(lblIssue);

                    var txtIssue = new TextBox();
                    txtIssue.HorizontalAlignment = HorizontalAlignment.Left;
                    txtIssue.VerticalAlignment = VerticalAlignment.Top;
                    txtIssue.Name = "txtIssue_" + i.ToString();
                    if (!lIssues[0].Equals("NF"))
                        txtIssue.Text = lIssues[0];
                    txtIssue.Width = 275;
                    txtIssue.Height = 23;
                    txtIssue.Margin = new Thickness(10, 33, 0, 0);
                    txtIssue.IsReadOnly = true;
                    UILayout.Children.Add(txtIssue);

                    var lblTestResult = new Label();
                    lblTestResult.HorizontalAlignment = HorizontalAlignment.Left;
                    lblTestResult.VerticalAlignment = VerticalAlignment.Top;
                    lblTestResult.Content = "Test Result:";
                    lblTestResult.Margin = new Thickness(10, 60, 0, 0);
                    UILayout.Children.Add(lblTestResult);

                    var txtTestResult = new TextBox();
                    txtTestResult.HorizontalAlignment = HorizontalAlignment.Left;
                    txtTestResult.VerticalAlignment = VerticalAlignment.Top;
                    txtTestResult.Name = "txtTestResult_" + i.ToString();
                    if (!lTestResult[i].Equals("NF"))
                        txtTestResult.Text = lTestResult[i];
                    txtTestResult.Width = 275;
                    txtTestResult.Height = 23;
                    txtTestResult.Margin = new Thickness(10, 83, 0, 0);
                    txtTestResult.IsReadOnly = true;
                    UILayout.Children.Add(txtTestResult);

                    var lblTestResultAbort = new Label();
                    lblTestResultAbort.HorizontalAlignment = HorizontalAlignment.Left;
                    lblTestResultAbort.VerticalAlignment = VerticalAlignment.Top;
                    lblTestResultAbort.Content = "Test Result (Abort Input):";
                    lblTestResultAbort.Margin = new Thickness(10, 110, 0, 0);
                    UILayout.Children.Add(lblTestResultAbort);

                    var txtTestResultAbort = new TextBox();
                    txtTestResultAbort.HorizontalAlignment = HorizontalAlignment.Left;
                    txtTestResultAbort.VerticalAlignment = VerticalAlignment.Top;
                    txtTestResultAbort.Name = "txtTestResultAbort_" + i.ToString();
                    if (!lTestResultAbort[i].Equals("NF"))
                        txtTestResultAbort.Text = lTestResultAbort[i];
                    txtTestResultAbort.Width = 275;
                    txtTestResultAbort.Height = 23;
                    txtTestResultAbort.Margin = new Thickness(10, 133, 0, 0);
                    txtTestResultAbort.IsReadOnly = true;
                    UILayout.Children.Add(txtTestResultAbort);

                    var lblCause = new Label();
                    lblCause.HorizontalAlignment = HorizontalAlignment.Left;
                    lblCause.VerticalAlignment = VerticalAlignment.Top;
                    lblCause.Content = "Cause:";
                    lblCause.Margin = new Thickness(10, 160, 0, 0);
                    UILayout.Children.Add(lblCause);

                    var txtCause = new TextBox();
                    txtCause.HorizontalAlignment = HorizontalAlignment.Left;
                    txtCause.VerticalAlignment = VerticalAlignment.Top;
                    txtCause.Name = "txtCause_" + i.ToString();
                    if (!lCause[i].Equals("NF"))
                        txtCause.Text = lCause[i];
                    txtCause.Width = 275;
                    txtCause.Height = 23;
                    txtCause.Margin = new Thickness(10, 183, 0, 0);
                    txtCause.IsReadOnly = true;
                    UILayout.Children.Add(txtCause);

                    var lblReplacement = new Label();
                    lblReplacement.HorizontalAlignment = HorizontalAlignment.Left;
                    lblReplacement.VerticalAlignment = VerticalAlignment.Top;
                    lblReplacement.Content = "Replacement:";
                    lblReplacement.Margin = new Thickness(10, 210, 0, 0);
                    UILayout.Children.Add(lblReplacement);

                    var txtReplacement = new TextBox();
                    txtReplacement.HorizontalAlignment = HorizontalAlignment.Left;
                    txtReplacement.VerticalAlignment = VerticalAlignment.Top;
                    txtReplacement.Name = "txtReplacement_" + i.ToString();
                    if (!lReplacement[i].Equals("NF"))
                        txtReplacement.Text = lReplacement[i];
                    txtReplacement.Width = 275;
                    txtReplacement.Height = 23;
                    txtReplacement.Margin = new Thickness(10, 233, 0, 0);
                    txtReplacement.IsReadOnly = true;
                    UILayout.Children.Add(txtReplacement);

                    var dgMultiPart = new DataGrid();
                    dgMultiPart.HorizontalAlignment = HorizontalAlignment.Left;
                    dgMultiPart.VerticalAlignment = VerticalAlignment.Top;
                    dgMultiPart.Width = 275;
                    dgMultiPart.Height = 100;
                    dgMultiPart.Margin = new Thickness(10, 275, 0, 0);
                    dgMultiPart.IsReadOnly = true;
                    dgMultiPart.Columns.Add(DataGridViewHelper.newColumn("Part Replaced", "PartReplaced"));
                    dgMultiPart.Columns.Add(DataGridViewHelper.newColumn("Ref Designator", "RefDesignator"));

                    var lPartToAdd = new List<string>();
                    var lRefToAdd = new List<string>();

                    string query = "SELECT * FROM TechnicianUnitIssues WHERE ID = '" + PRI.ID + "';";

                    var conn = new SqlConnection(holder.RepairConnectionString);
                    var cmd = new SqlCommand(query, conn);
                    try
                    {
                        conn.Open();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                if(reader["Replacement"].ToString().Equals(lReplacement[i].ToString()))
                                {
                                    if (reader["PartsReplaced"] != DBNull.Value && !string.IsNullOrEmpty(reader["PartsReplaced"].ToString()))
                                        lPartToAdd.Add(reader["PartsReplaced"].ToString());
                                    else lPartToAdd.Add("NF");

                                    if (reader["RefDesignator"] != DBNull.Value && !string.IsNullOrEmpty(reader["RefDesignator"].ToString()))
                                        lRefToAdd.Add(reader["RefDesignator"].ToString());
                                    else lRefToAdd.Add("NF");
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch
                    {
                        if (conn != null)
                            conn.Close();
                    }

                    for (int j = 0; j < lPartToAdd.Count; j++)
                    {
                        if (lPartToAdd[j].Equals("NF"))
                            lPartToAdd[j] = "";
                        if (lRefToAdd[j].Equals("NF"))
                            lRefToAdd[j] = "";

                        dgMultiPart.Items.Add(new MultiplePartsReplaced { PartReplaced = lPartToAdd[j], RefDesignator = lRefToAdd[j] });
                    }
                    
                    UILayout.Children.Add(dgMultiPart);
                }
                catch { }

                var ti = new TabItem();
                ti.Header = "Issue # " + (i + 1).ToString();
                ti.Content = UILayout;
                UnitIssueTabControl.Items.Add(ti);
            }

            UnitIssueTabControl.SelectedIndex = 0;
            lIssues = null;
            lTestResult = null;
            lTestResultAbort = null;
            lCause = null;
            lReplacement = null;
        }
    }
}
