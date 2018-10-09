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
            SelPartNumPartName = new DGVPARTNUMNAMEITEM { PartNumber = "", PartName = "", PartSeries = "" };
            SelRPNumber = new DGVMULTIPLERP { RPNumber = "", LineNumber = -1, CustInfo = new CustomerInformation()};
            LogHandler.resetLog();
        }

        public csLogging LogHandler;
        public static int SelectedIssue = -1;

        /// <summary>
        /// SelectedPartNumberPartName is used to help with the selected part number and part name of a scanned in unit.
        /// Since this item is accessed across multiple forms, I decided to store it here so it can be accessed/modified when needed.
        /// </summary>
        private DGVPARTNUMNAMEITEM SelPartNumPartName = new DGVPARTNUMNAMEITEM { PartNumber = "", PartName = "", PartSeries = "" };
        public DGVPARTNUMNAMEITEM SelectedPartNumberPartName
        {
            get { return SelPartNumPartName; }
            set { SelPartNumPartName = value; }
        }

        private DGVMULTIPLERP SelRPNumber = new DGVMULTIPLERP { RPNumber = "", LineNumber = -1, CustInfo = new CustomerInformation() };
        public DGVMULTIPLERP SelectedRPNumber
        {
            get { return SelRPNumber; }
            set { SelRPNumber = value; }
        }
    }
}
