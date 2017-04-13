/*
 * csClassHelper.cs
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RApID_Project_WPF
{
    class DataGridViewHelper
    {
        /// <summary>
        /// Helps quickly add new columns to a DataView
        /// </summary>
        /// <param name="_h">Header</param>
        /// <param name="_b">Binding</param>
        /// <returns>DataGridTextColumn</returns>
        public static System.Windows.Controls.DataGridTextColumn newColumn(string _header, string _binding)
        {
            System.Windows.Controls.DataGridTextColumn newCol = new System.Windows.Controls.DataGridTextColumn();
            newCol.Header = _header;
            newCol.Binding = new System.Windows.Data.Binding(_binding);
            return newCol;
        }
    }

    /// <summary>
    /// DGVAOI is needed to add new row 'items' to dgvAOI. (It's stupid. I know.)
    /// </summary>
    class DGVAOI
    {
        public string Shift { get; set; }
        public string SystemID { get; set; }
        public string Inspector { get; set; }
        public string Assy { get; set; }
        public DateTime IDate { get; set; }
        public string SN { get; set; }
        public string RefID { get; set; }
        public string DefectCode { get; set; }
        public string PartTotal { get; set; }
        public string RecType { get; set; }
        public string Reworked { get; set; }
        public string PN { get; set; }
        public string BrdFail { get; set; }
    }

    /// <summary>
    /// Defect codes for dgvDefectCodes
    /// </summary>
    class DGVDEFECTCODES
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Holds the values of the Test/Results from EOL/PRE/POST tests
    /// </summary>
    class EOLLISTVIEW
    {
        public string Test { get; set; }
        public string Value { get; set; }
    }

    public class DGVPARTNUMNAMEITEM
    {
        public bool PartNumberSelected { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string PartSeries { get; set; }
    }

    public class OrderNumberInformation
    {
        public string OrderNumber { get; set; }
        public string RPNumber { get; set; }
        public double LineNumber { get; set; }
        public string CustomerNumber { get; set; }
    }

    /// <summary>
    /// Used when multiple RP numbers are found.
    /// </summary>
    public class DGVMULTIPLERP
    {
        public string RPNumber { get; set; }
        public double LineNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public CustomerInformation CustInfo { get; set; }
    }

    /// <summary>
    /// MultipleIssues will be used to help with storing multiple issues related to the unit.
    /// </summary>
    public class RepairMultipleIssues
    {
        public int ID { get; set; }
        public string ReportedIssue { get; set; }
        public string TestResult { get; set; }
        public string TestResultAbort { get; set; }
        public string Cause { get; set; }
        public string Replacement { get; set; }
        public List<MultiplePartsReplaced> MultiPartsReplaced { get; set; }
        public MultiplePartsReplaced SinglePartReplaced { get; set; }
    }

    /// <summary>
    /// Class object for a Full Unit Issue within Production
    /// </summary>
    public class ProductionMultipleUnitIssues
    {
        public int ID { get; set; }
        public string ReportedIssue { get; set; }
        public string TestResult { get; set; }
        public string TestResultAbort { get; set; }
        public string Issue { get; set; }
        public string Item { get; set; }
        public string Problem { get; set; }
        public List<MultiplePartsReplaced> MultiPartsReplaced { get; set; }
    }

    /// <summary>
    /// Class object for Parts Replaced.
    /// </summary>
    public class MultiplePartsReplaced
    {
        public string PartReplaced { get; set; }
        public string RefDesignator { get; set; }
        public string PartsReplacedPartDescription { get; set; }
    }

    /// <summary>
    /// This is used to assist the dgPrevRepairInfo with holding the previously entered repair information
    /// If the information is coming from the old database, the OldDB flag will be set to true.
    /// </summary>
    public class PreviousRepairInformation
    {
        public int ID { get; set; }
        public string TechName { get; set; }
        public DateTime DateSubmitted { get; set; }
        public bool OldDB { get; set; }
        public string SerialNumber { get; set; }
    }

    #region Repair Tech Codes
    public class PC1
    {
        public string CodeName { get; set; }
        public string ReturnCode { get; set; }
    }

    public class PC2
    {
        public string CodeName { get; set; }
        public string ReturnCode { get; set; }
    }

    public class PC3
    {
        public string CodeName { get; set; }
        public string ReturnCode { get; set; }
    }

    public class EndUse
    {
        public string CodeName { get; set; }
        public string ReturnCode { get; set; }
    }

    public class JDEReturnCodes
    {
        public string JDECodeName { get; set; }
        public int JDEReturnCode { get; set; }
    }
    #endregion

    public class IssueItemProblemCombinations
    {
        public string Issue { get; set; }
        public string Item { get; set; }
        public string Problem { get; set; }
    }

    public class CustomerInformation
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddy1 { get; set; }
        public string CustomerAddy2 { get; set; }
        public string CustomerAddy3 { get; set; }
        public string CustomerAddy4 { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCountryCode { get; set; }
    }

    public class QCDQETechComments
    {
        public string FullTechList { get; set; }
        public string FullTechComments { get; set; }
    }

    /// <summary>
    /// Holds the information when a unit is searched in the QC/DQE Page.
    /// </summary>
    public class QCDQEPageLoad
    {
        public int ID { get; set; }
        public string Technician { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public string PartSeries { get; set; }
        public string CommoditySubClass { get; set; }
        public string SoftwareVersion { get; set; }
        public string Quantity { get; set; }
        public DateTime DateReceived { get; set; }
        public string SerialNumber { get; set; }
        public string TypeOfReturn { get; set; }
        public string TypeOfFailure { get; set; }
        public string HoursOnUnit { get; set; }
        public List<RepairMultipleIssues> UnitIssues { get; set; }
        public string AdditionalComments { get; set; }
        public string TechAction1 { get; set; }
        public string TechAction2 { get; set; }
        public string TechAction3 { get; set; }
        public CustomerInformation CustomerInformation { get; set; }
        public QCDQETechComments QCandDQEComments { get; set; }
        public bool LoadSuccessful { get; set; }
    }

    /// <summary>
    /// The data to be submitted within the QC/DQE form.
    /// </summary>
    public class QCDQESubmitData
    {
        public string SubmitQuery { get; set; }
        public string QualityTech { get; set; }
        public string QCDQEComments { get; set; }
        public string SubmissionStatus { get; set; }
        public DateTime QCDQEDateSubmit { get; set; }
    }
}
