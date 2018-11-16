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
    /// Interaction logic for ucUnitIssues.xaml
    /// </summary>
    public partial class ucUnitIssues : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ReportedIssueProperty = DependencyProperty.Register("ReportedIssue", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty TestResultProperty = DependencyProperty.Register("TestResult", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty AbortInputProperty = DependencyProperty.Register("AbortInput", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty IssueProperty = DependencyProperty.Register("Issue", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty ProblemProperty = DependencyProperty.Register("Problem", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty AdditionalCommentsProperty = DependencyProperty.Register("AdditionalComments", typeof(string), typeof(ucProgressControl), new PropertyMetadata(""));
        public static readonly DependencyProperty PartsReplacedProperty = DependencyProperty.Register("PartsReplaced", typeof(List<MultiplePartsReplaced>), typeof(ucProgressControl), new PropertyMetadata(new List<MultiplePartsReplaced>()));
        #endregion

        #region Properties
        public string ReportedIssue
        {
            get { return (string)GetValue(ReportedIssueProperty); }
            set { SetValue(ReportedIssueProperty, value); }
        }
        public string TestResult
        {
            get => (string)GetValue(TestResultProperty);
            set => SetValue(TestResultProperty, value);
        }
        public string AbortInput
        {
            get => (string)GetValue(AbortInputProperty);
            set => SetValue(AbortInputProperty, value);
        }
        public string Issue
        {
            get => (string)GetValue(IssueProperty);
            set => SetValue(IssueProperty, value);
        }
        public string Item
        {
            get => (string)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }
        public string Problem
        {
            get => (string)GetValue(ProblemProperty);
            set => SetValue(ProblemProperty, value);
        }
        public string AdditionalComments
        {
            get { return (string)GetValue(AdditionalCommentsProperty); }
            set { SetValue(AdditionalCommentsProperty, value); }
        }
        public List<MultiplePartsReplaced> PartsReplaced
        {
            get => (List<MultiplePartsReplaced>)GetValue(PartsReplacedProperty);
            set => SetValue(PartsReplacedProperty, value);
        }
        #endregion

        public ucUnitIssues()
        {
            InitializeComponent();
        }
    }
}
