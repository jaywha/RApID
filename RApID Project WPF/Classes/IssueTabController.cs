using Dragablz;
using RApID_Project_WPF.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RApID_Project_WPF.Classes
{
    [Obsolete("Old legacy class; please don't use unless fixed.")]
    public class IssueTabClient : IInterTabClient
    {
        public IssueTabClient() { }

        public List<NewTabHost<Window>> OpenIssues = new List<NewTabHost<Window>>();

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            try
            {
                if (OpenIssues.Count > 0)
                    return OpenIssues.Where(tab => tab.Container.Content == (ucUnitIssue)(UnitIssueModel)source.SelectedContent).Single();
                else
                    OpenIssues.Add(new NewTabHost<Window>(new Window() { Content = (ucUnitIssue)(UnitIssueModel)source.SelectedContent }, source));

                return OpenIssues.Last();
            } catch(Exception e) {
                csExceptionLogger.csExceptionLogger.Write("RApID_IssueTabClient-GetNewHost", e);
                return new NewTabHost<Window>(new Window() { Content = (ucUnitIssue)(UnitIssueModel) source.SelectedContent }, source);
            }
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            if(tabControl.Items.Count == 1)
            {
                //TODO: Dynamically call Snackbar Notificaitons.
                MessageBox.Show("Can't remove the last tab.");
                return TabEmptiedResponse.DoNothing;
            } else if (MessageBox.Show($"Are you sure you want to remove the {(tabControl.SelectedItem as TabItem)?.Header.ToString() ?? "current"} tab?", "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                return TabEmptiedResponse.CloseWindowOrLayoutBranch;
            } else {
                //NOTE: Do nothing on no && when last tab... issue?
                return TabEmptiedResponse.DoNothing;
            }
        }
    }
}
