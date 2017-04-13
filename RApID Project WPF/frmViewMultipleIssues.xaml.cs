using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmViewMultipleIssues.xaml
    /// </summary>
    /// 
    public partial class frmViewMultipleIssues : Window
    {
        StaticVars sVars = StaticVars.StaticVarsInstance();
        public frmViewMultipleIssues()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initForm();
            buildDGV();
        }
        
        private void initForm()
        {
            //int _xLoc = Convert.ToInt32((this.Width/2) - (btnRemoveSelectedItem.Width + (btnRemoveSelectedItem.Width / 2)) - 2);
            //int _yLoc = Convert.ToInt32(btnRemoveSelectedItem.Margin.Top);
            //btnEditSelectedItem.Margin = new Thickness(_xLoc, _yLoc, 0, 0);
            
            //_xLoc = Convert.ToInt32((this.Width / 2) - (btnRemoveSelectedItem.Width / 2));
            //btnRemoveSelectedItem.Margin = new Thickness(_xLoc, _yLoc, 0, 0);

            //_xLoc = Convert.ToInt32((this.Width / 2) + (btnRemoveSelectedItem.Width / 2) + 2);
            //btnExit.Margin = new Thickness(_xLoc, _yLoc, 0, 0);
        }

        private void buildDGV()
        {
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Test Result", "TestResult"));
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Test Result (Abort)", "TestResultAbort"));
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Cause", "Cause"));
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Replacement", "Replacement"));
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Parts Replaced", "PartsReplaced"));
            //dgvIssueList.Columns.Add(DataGridViewHelper.newColumn("Ref Designator", "RefDesignator"));

            //fillDGV();
        }

        private void fillDGV()
        {
            //dgvIssueList.Items.Clear();

            //for (int i = 0; i < sVars.lMultipleIssues.Count; i++)
            //{
            //    MultipleIssues mi = new MultipleIssues();
            //    mi.ID = sVars.lMultipleIssues[i].ID;
            //    mi.TestResult = sVars.lMultipleIssues[i].TestResult;
            //    mi.TestResultAbort = sVars.lMultipleIssues[i].TestResultAbort;
            //    mi.Cause = sVars.lMultipleIssues[i].Cause;
            //    mi.Replacement = sVars.lMultipleIssues[i].Replacement;

            //    string sPR = String.Empty;
            //    string sRD = String.Empty;

            //    for (int j = 0; j < sVars.lMultipleIssues[i].MultiPartsReplaced.Count; j++)
            //    {
            //        sPR += sVars.lMultipleIssues[i].MultiPartsReplaced[j].PartReplaced + ",";
            //        sRD += sVars.lMultipleIssues[i].MultiPartsReplaced[j].RefDesignator + ",";
            //    }

            //    sPR = sPR.TrimEnd(',');
            //    sRD = sRD.TrimEnd(',');

            //    mi.PartsReplaced = sPR;
            //    mi.RefDesignator = sRD;

            //    dgvIssueList.Items.Add(mi);
            //}
        }

        private void btnRemoveSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            //if (dgvIssueList.SelectedItem != null)
            //{
            //    sVars.lMultipleIssues.Remove((MultipleIssues)dgvIssueList.SelectedItem);
            //}
            //fillDGV();
        }

        private void dgvIssueList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEditSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            //if(dgvIssueList.SelectedItem != null)
            //{
            //    MultipleIssues mi = (MultipleIssues)dgvIssueList.SelectedItem;
            //    StaticVars.SelectedIssue = Convert.ToInt32(mi.ID);
            //    this.Close();
            //}
        }
    }
}
