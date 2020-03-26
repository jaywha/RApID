using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EricStabileLibrary;

namespace RApID_Project_WPF
{
    public class StaticVars
    {
        public csVersionControl VersionControl;
        private static StaticVars StaticVars_Instance = null;
        public static StaticVars StaticVarsInstance()
        {
            if (StaticVars_Instance == null)
                StaticVars_Instance = new StaticVars();
            return StaticVars_Instance;
        }

        private StaticVars()
        {
            // Empty private consturctor for singleton façade
        }

        public void initStaticVars()
        {
            LogHandler = new csLogging(System.Environment.UserName, DateTime.Now, Properties.Settings.Default.LogWriteLocation);
            if (VersionControl == null)
            {
                VersionControl = new csVersionControl();
            }
        }

        public void resetStaticVars()
        {
            SelectedIssue = -1;
            SelectedPartNumberPartName = new DGVPARTNUMNAMEITEM { PartNumber = "", PartName = "", PartSeries = "" };
            SelectedRPNumber = new DGVMULTIPLERP { RPNumber = "", LineNumber = -1, CustInfo = new CustomerInformation()};
            LogHandler.resetLog();
        }

        public csLogging LogHandler;
        public static int SelectedIssue = -1;
        public DGVPARTNUMNAMEITEM SelectedPartNumberPartName { get; set; }
            = new DGVPARTNUMNAMEITEM { PartNumber = "", PartName = "", PartSeries = "" };

        public DGVMULTIPLERP SelectedRPNumber { get; set; }
            = new DGVMULTIPLERP { RPNumber = "", LineNumber = -1, CustInfo = new CustomerInformation() };

        public MULTIPLEBOM SelectedBOMFile { get; set; }
            = new MULTIPLEBOM { Filename = "BOMFile", Notes = "Nope!" };

        internal void disposeStaticVars()
        {
            LogHandler = null;
            VersionControl = null;
        }
    }
}
