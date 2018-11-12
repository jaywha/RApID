using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// <see cref="Record"/> model controller for <see cref="frmGlobalSearch"/>'s data grid.
    /// </summary>
    public class RecordList : ObservableCollection<Record>
    {
        /// <summary>
        /// Delegate record submission to asynchronously update data grid parallel to UI thread.
        /// </summary>
        /// <param name="records">List of records to process.</param>
        private delegate void PushPapers(params Record[] records);

        /// <summary>
        /// Default constructor
        /// </summary>
        public RecordList() : base() { }

        /// <summary>
        /// Implementation of the delegated method <see cref="PushPapers"/>.
        /// </summary>
        /// <param name="records">List of records to file.</param>
        private void FilePapers(params Record[] records) 
            => Array.ForEach(records, rec => Add(rec));

        /// <summary>
        /// Gets the current list of records from the TechnicianSubmission Table.
        /// </summary>
        internal async Task GetData(Label lbl, Dispatcher UIThread)
        {
            var recs = new List<Record>();
            var numRows = 0;

            await Task.Factory.StartNew(() =>
            {
                using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    using (var reader = new SqlCommand("SELECT * FROM [Repair].[dbo].[TechnicianSubmission]", conn).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] rowVals = new object[reader.VisibleFieldCount];
                            var dum_num = reader.GetValues(rowVals);

                            recs.Add((Record)rowVals);
                            UIThread.Invoke(() => lbl.Content = $"Loading... ({++numRows}) loaded so far{new string('.', numRows % 3)}");
                        }
                    }
                }
            });

            UIThread.Invoke(() => lbl.Content = $"Loading Complete! Adding rows to user table... \n\n\nMay take a few minutes.");
            System.Threading.Thread.Sleep(250);

            // Put all the records into the acutal data grid
            await UIThread.BeginInvoke(DispatcherPriority.ApplicationIdle, new PushPapers(FilePapers), recs.ToArray());
        }
    }

    /// <summary>
    /// Model for a row in the TechnicianSubmission Table.
    /// </summary>
    public class Record : INotifyPropertyChanged, IEditableObject
    {
        #region Fields
        public int _ID;
        public string _Technician;
        public DateTime _DateReceived;
        public string _PartName;
        public string _PartNumber;
        public string _CommoditySubClass;
        public int _Quantity;
        public string _SoftwareVersion;
        public bool _Scrap;
        public string _TypeOfReturn;
        public string _TypeOfFailure;
        public string _HoursOnUnit;
        public string _ReportedIssue;
        public string _TestResult;
        public string _TestResultAbort;
        public string _Cause;
        public string _Replacement;
        public string _PartsReplaced;
        public string _RefDesignator;
        public string _AdditionalComments;
        public int _CustomerNumber;
        public string _SerialNumber;
        public DateTime _DateSubmitted;
        public string _SubmissionStatus;
        public string _Quality;
        public string _RP;
        public string _TechAct1;
        public string _TechAct2;
        public string _TechAct3;
        public string _QCDQEComments;
        public string _OrderNumber;
        public string _ProblemCode1;
        public string _ProblemCode2;
        public string _RepairCode;
        public string _TechComments;
        public decimal _LineNumber;
        public char _ProcessedFlag;
        public DateTime _ProcessedDateTime;
        public string _Series;
        public string _FromArea;
        public string _SaveID;
        public DateTime _QCDQEDateSubmitted;
        public string _Issue;
        public string _Item;
        public string _Problem;
        public long _LogID;
        #endregion

        #region Properties
        public int ID
        {
            get => _ID;
            set
            {
                _ID = value;
                OnPropertyChanged();
            }
        }
        public string Technician
        {
            get => _Technician;
            set
            {
                _Technician = value;
                OnPropertyChanged();
            }
        }
        public DateTime DateReceived
        {
            get => _DateReceived;
            set
            {
                _DateReceived = value;
                OnPropertyChanged();
            }
        }
        public string PartName
        {
            get => _PartName;
            set
            {
                _PartName = value;
                OnPropertyChanged();
            }
        }
        public string PartNumber
        {
            get => _PartNumber;
            set
            {
                _PartNumber = value;
                OnPropertyChanged();
            }
        }
        public string CommoditySubClass
        {
            get => _CommoditySubClass;
            set
            {
                _CommoditySubClass = value;
                OnPropertyChanged();
            }
        }
        public int Quantity
        {
            get => _Quantity;
            set
            {
                _Quantity = value;
                OnPropertyChanged();
            }
        }
        public string SoftwareVersion
        {
            get => _SoftwareVersion;
            set
            {
                _SoftwareVersion = value;
                OnPropertyChanged();
            }
        }
        public bool Scrap
        {
            get => _Scrap;
            set
            {
                _Scrap = value;
                OnPropertyChanged();
            }
        }
        public string TypeOfReturn
        {
            get => _TypeOfReturn;
            set
            {
                _TypeOfReturn = value;
                OnPropertyChanged();
            }
        }
        public string TypeOfFailure
        {
            get => _TypeOfFailure;
            set
            {
                _TypeOfFailure = value;
                OnPropertyChanged();
            }
        }
        public string HoursOnUnit
        {
            get => _HoursOnUnit;
            set
            {
                _HoursOnUnit = value;
                OnPropertyChanged();
            }
        }
        public string ReportedIssue
        {
            get => _ReportedIssue;
            set
            {
                _ReportedIssue = value;
                OnPropertyChanged();
            }
        }
        public string TestResult
        {
            get => _TestResult;
            set
            {
                _TestResult = value;
                OnPropertyChanged();
            }
        }
        public string TestResultAbort
        {
            get => _TestResultAbort;
            set
            {
                _TestResultAbort = value;
                OnPropertyChanged();
            }
        }
        public string Cause
        {
            get => _Cause;
            set
            {
                _Cause = value;
                OnPropertyChanged();
            }
        }
        public string Replacement
        {
            get => _Replacement;
            set
            {
                _Replacement = value;
                OnPropertyChanged();
            }
        }
        public string PartsReplaced
        {
            get => _PartsReplaced;
            set
            {
                _PartsReplaced = value;
                OnPropertyChanged();
            }
        }
        public string RefDesignator
        {
            get => _RefDesignator;
            set
            {
                _RefDesignator = value;
                OnPropertyChanged();
            }
        }
        public string AdditionalComments
        {
            get => _AdditionalComments;
            set
            {
                _AdditionalComments = value;
                OnPropertyChanged();
            }
        }
        public int CustomerNumber
        {
            get => _CustomerNumber;
            set
            {
                _CustomerNumber = value;
                OnPropertyChanged();
            }
        }
        public string SerialNumber
        {
            get => _SerialNumber;
            set
            {
                _SerialNumber = value;
                OnPropertyChanged();
            }
        }
        public DateTime DateSubmitted
        {
            get => _DateSubmitted;
            set
            {
                _DateSubmitted = value;
                OnPropertyChanged();
            }
        }
        public string SubmissionStatus
        {
            get => _SubmissionStatus;
            set
            {
                _SubmissionStatus = value;
                OnPropertyChanged();
            }
        }
        public string Quality
        {
            get => _Quality;
            set
            {
                _Quality = value;
                OnPropertyChanged();
            }
        }
        public string RP
        {
            get => _RP;
            set
            {
                _RP = value;
                OnPropertyChanged();
            }
        }
        public string TechAct1
        {
            get => _TechAct1;
            set
            {
                _TechAct1 = value;
                OnPropertyChanged();
            }
        }
        public string TechAct2
        {
            get => _TechAct2;
            set
            {
                _TechAct2 = value;
                OnPropertyChanged();
            }
        }
        public string TechAct3
        {
            get => _TechAct3;
            set
            {
                _TechAct3 = value;
                OnPropertyChanged();
            }
        }
        public string QCDQEComments
        {
            get => _QCDQEComments;
            set
            {
                _QCDQEComments = value;
                OnPropertyChanged();
            }
        }
        public string OrderNumber
        {
            get => _OrderNumber;
            set
            {
                _OrderNumber = value;
                OnPropertyChanged();
            }
        }
        public string ProblemCode1
        {
            get => _ProblemCode1;
            set
            {
                _ProblemCode1 = value;
                OnPropertyChanged();
            }
        }
        public string ProblemCode2
        {
            get => _ProblemCode2;
            set
            {
                _ProblemCode2 = value;
                OnPropertyChanged();
            }
        }
        public string RepairCode
        {
            get => _RepairCode;
            set
            {
                _RepairCode = value;
                OnPropertyChanged();
            }
        }
        public string TechComments
        {
            get => _TechComments;
            set
            {
                _TechComments = value;
                OnPropertyChanged();
            }
        }
        public decimal LineNumber
        {
            get => _LineNumber;
            set
            {
                _LineNumber = value;
                OnPropertyChanged();
            }
        }
        public char ProcessedFlag
        {
            get => _ProcessedFlag;
            set
            {
                _ProcessedFlag = value;
                OnPropertyChanged();
            }
        }
        public DateTime ProcessedDateTime
        {
            get => _ProcessedDateTime;
            set
            {
                _ProcessedDateTime = value;
                OnPropertyChanged();
            }
        }
        public string Series
        {
            get => _Series;
            set
            {
                _Series = value;
                OnPropertyChanged();
            }
        }
        public string FromArea
        {
            get => _FromArea;
            set
            {
                _FromArea = value;
                OnPropertyChanged();
            }
        }
        public string SaveID
        {
            get => _SaveID;
            set
            {
                _SaveID = value;
                OnPropertyChanged();
            }
        }
        public DateTime QCDQEDateSubmitted
        {
            get => _QCDQEDateSubmitted;
            set
            {
                _ = value;
                OnPropertyChanged();
            }
        }
        public string Issue
        {
            get => _Issue;
            set
            {
                _Issue = value;
                OnPropertyChanged();
            }
        }
        public string Item
        {
            get => _Item;
            set
            {
                _Item = value;
                OnPropertyChanged();
            }
        }
        public string Problem
        {
            get => _Problem;
            set
            {
                _Problem = value;
                OnPropertyChanged();
            }
        }
        public long LogID
        {
            get => _LogID;
            set
            {
                _LogID = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Interface Inheritance
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private Record temp_record = null;
        private bool IsEditing = false;

        public void BeginEdit()
        {
            if (!IsEditing)
            {
                temp_record = this.MemberwiseClone() as Record;
                IsEditing = true;
            }
        }

        public void CancelEdit()
        {
            if (IsEditing)
            {
                this.ID = temp_record.ID;
                this.Technician = temp_record.Technician;
                this.DateReceived = temp_record.DateReceived;
                this.PartName = temp_record.PartName;
                this.PartNumber = temp_record.PartNumber;
                this.CommoditySubClass = temp_record.CommoditySubClass;
                this.Quantity = temp_record.Quantity;
                this.SoftwareVersion = temp_record.SoftwareVersion;
                this.Scrap = temp_record.Scrap;
                this.TypeOfReturn = temp_record.TypeOfReturn;
                this.TypeOfFailure = temp_record.TypeOfFailure;
                this.HoursOnUnit = temp_record.HoursOnUnit;
                this.ReportedIssue = temp_record.ReportedIssue;
                this.TestResult = temp_record.TestResult;
                this.TestResultAbort = temp_record.TestResultAbort;
                this.Cause = temp_record.Cause;
                this.Replacement = temp_record.Replacement;
                this.PartsReplaced = temp_record.PartsReplaced;
                this.RefDesignator = temp_record.RefDesignator;
                this.AdditionalComments = temp_record.AdditionalComments;
                this.CustomerNumber = temp_record.CustomerNumber;
                this.SerialNumber = temp_record.SerialNumber;
                this.DateSubmitted = temp_record.DateSubmitted;
                this.SubmissionStatus = temp_record.SubmissionStatus;
                this.Quality = temp_record.Quality;
                this.RP = temp_record.RP;
                this.TechAct1 = temp_record.TechAct1;
                this.TechAct2 = temp_record.TechAct2;
                this.TechAct3 = temp_record.TechAct3;
                this.QCDQEComments = temp_record.QCDQEComments;
                this.OrderNumber = temp_record.OrderNumber;
                this.ProblemCode1 = temp_record.ProblemCode1;
                this.ProblemCode2 = temp_record.ProblemCode2;
                this.RepairCode = temp_record.RepairCode;
                this.TechComments = temp_record.TechComments;
                this.LineNumber = temp_record.LineNumber;
                this.ProcessedFlag = temp_record.ProcessedFlag;
                this.ProcessedDateTime = temp_record.ProcessedDateTime;
                this.Series = temp_record.Series;
                this.FromArea = temp_record.FromArea;
                this.SaveID = temp_record.SaveID;
                this.QCDQEDateSubmitted = temp_record.QCDQEDateSubmitted;
                this.Issue = temp_record.Issue;
                this.Item = temp_record.Item;
                this.Problem = temp_record.Problem;
                this.LogID = temp_record.LogID;
                IsEditing = false;
            }
        }

        public void EndEdit()
        {
            if (IsEditing)
            {
                temp_record = null;
                IsEditing = false;
            }
        }
        #endregion

        public static explicit operator Record(object[] vals)
        {
            int index = 0;
            foreach(var val in vals)
            {
                if(val == DBNull.Value)
                {
                    vals[index] = default;
                } index++;
            }

            var newRecord = new Record();

            newRecord.ID = int.TryParse(vals[0]?.ToString(), out int id_res) ? id_res : 0;
            newRecord.Technician = vals[1]?.ToString() ?? "";
            newRecord.DateReceived = DateTime.TryParse(vals[2]?.ToString(), out DateTime drec_res) ? drec_res : DateTime.Now.AddYears(-100);
            newRecord.PartName = vals[3]?.ToString() ?? "";
            newRecord.PartNumber = vals[4]?.ToString() ?? "";
            newRecord.CommoditySubClass = vals[5]?.ToString() ?? "";
            newRecord.Quantity = int.TryParse(vals[6]?.ToString(), out int q_res) ? q_res : 0;
            newRecord.SoftwareVersion = vals[7]?.ToString() ?? "";
            newRecord.Scrap = vals[8]?.ToString().Equals("1") ?? false;
            newRecord.TypeOfReturn = vals[9]?.ToString() ?? "";
            newRecord.TypeOfFailure = vals[10]?.ToString() ?? "";
            newRecord.HoursOnUnit = vals[11]?.ToString() ?? "";
            newRecord.ReportedIssue = vals[12]?.ToString() ?? "";
            newRecord.TestResult = vals[13]?.ToString() ?? "";
            newRecord.TestResultAbort = vals[14]?.ToString() ?? "";
            newRecord.Cause = vals[15]?.ToString() ?? "";
            newRecord.Replacement = vals[16]?.ToString() ?? "";
            newRecord.PartsReplaced = vals[17]?.ToString() ?? "";
            newRecord.RefDesignator = vals[18]?.ToString() ?? "";
            newRecord.AdditionalComments = vals[19]?.ToString() ?? "";
            newRecord.CustomerNumber = int.TryParse(vals[20]?.ToString(), out int cust_res) ? cust_res : 0;
            newRecord.SerialNumber = vals[21]?.ToString() ?? "";
            newRecord.DateSubmitted = DateTime.TryParse(vals[22]?.ToString(), out DateTime dsub_res) ? drec_res : DateTime.Now.AddYears(-50);
            newRecord.SubmissionStatus = vals[23]?.ToString() ?? "";
            newRecord.Quality = vals[24]?.ToString() ?? "";
            newRecord.RP = vals[25]?.ToString() ?? "";
            newRecord.TechAct1 = vals[26]?.ToString() ?? "";
            newRecord.TechAct2 = vals[27]?.ToString() ?? "";
            newRecord.TechAct3 = vals[28]?.ToString() ?? "";
            newRecord.QCDQEComments = vals[29]?.ToString() ?? "";
            newRecord.OrderNumber = vals[30]?.ToString() ?? "";
            newRecord.ProblemCode1 = vals[31]?.ToString() ?? "";
            newRecord.ProblemCode2 = vals[32]?.ToString() ?? "";
            newRecord.RepairCode = vals[33]?.ToString() ?? "";
            newRecord.TechComments = vals[34]?.ToString() ?? "";
            newRecord.LineNumber = decimal.TryParse(vals[35]?.ToString(), out decimal line_res) ? line_res : 0.0m;
            newRecord.ProcessedFlag = char.TryParse(vals[36]?.ToString(), out char c_res) ? c_res : ' ';
            newRecord.ProcessedDateTime = DateTime.TryParse(vals[37]?.ToString(), out DateTime dpro_res) ? dpro_res : DateTime.Now.AddYears(-25);
            newRecord.Series = vals[38]?.ToString() ?? "";
            newRecord.FromArea = vals[39]?.ToString() ?? "";
            newRecord.SaveID = vals[40]?.ToString() ?? "";
            newRecord.QCDQEDateSubmitted = DateTime.TryParse(vals[41]?.ToString(), out DateTime ddqe_res) ? ddqe_res : DateTime.Now.AddYears(-5);
            newRecord.Issue = vals[42]?.ToString() ?? "";
            newRecord.Item = vals[43]?.ToString() ?? "";
            newRecord.Problem = vals[44]?.ToString() ?? "";
            newRecord.LogID = long.TryParse(vals[45]?.ToString(), out long id_log) ? id_log : 0L;

            return newRecord;
        }
    }
}
