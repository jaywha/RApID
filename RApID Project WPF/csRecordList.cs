using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// <see cref="Record"/> model controller for <see cref="frmGlobalSearch"/>'s data grid.
    /// </summary>
    public class RecordList : RangeObservableCollection<Record>
    {
        /// <summary>
        /// Entry point for <see cref="frmGlobalSearch.ToggleButtonsEnabled(bool)"/> method.
        /// </summary>
        public Action<bool> ToggleButtonControls;

        /// <summary>
        /// Entry point for the <see cref="frmGlobalSearch.ToggleFiltersEnabled(bool)"/> method.
        /// </summary>
        public Action<bool> ToggleFilterControls;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RecordList() : base() { }

        /// <summary>
        /// Gets the current list of records from the TechnicianSubmission Table, possibly based off of filters.
        /// </summary>
        internal async Task GetData(CancellationToken cancelation, Label lbl, ProgressBar prog, Dispatcher UIThread, string _query = "",
            string lblMsg ="", string notifyTitle = "", string notifyMsg = "")
        {
            var recs = new List<Record>();
            if (string.IsNullOrEmpty(_query)) _query = $"SELECT * FROM [Repair].[dbo].[TechnicianSubmission]";

            int numRecs = 0;

            await Task.Factory.StartNew(() =>
            {
                using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    #region SqlDataReader
                    using (var reader = new SqlCommand(_query, conn).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] rowVals = new object[reader.VisibleFieldCount];
                            reader.GetValues(rowVals);
                            Record new_record = rowVals;

                            recs.Add(new_record);
                            numRecs++;
                        }

                        UIThread.Invoke(() => AddRange(recs), DispatcherPriority.Background);
                    }
                    #endregion
                }

                lbl.Dispatcher.Invoke(() =>
                {
                    lbl.Content = !string.IsNullOrEmpty(lblMsg) ? lblMsg :  "Loading Complete!";
                    lbl.Visibility = Visibility.Collapsed;
                });

                prog.Dispatcher.Invoke(() =>
                    prog.Visibility = Visibility.Collapsed
                );

                UIThread.BeginInvoke(new Action(() => ToggleButtonControls.Invoke(true)));
                UIThread.BeginInvoke(new Action(() => ToggleFilterControls.Invoke(true)));

                MainWindow.Notify.Dispatcher.Invoke(() =>
                    MainWindow.Notify.ShowBalloonTip(!string.IsNullOrEmpty(notifyTitle) ? notifyTitle : "RApID - Global Search Complete!", 
                    !string.IsNullOrEmpty(notifyMsg) ? notifyMsg : ("Global Search window has completed loading all of" +
                    " the records currently in the database."), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info)
                );

                Console.WriteLine("[INFO]: Number of rows in data grid (" + numRecs + ").");
            }, cancelation);
        }
    }

    /// <summary>
    /// Should help with too many event listeners
    /// <para/>
    /// https://peteohanlon.wordpress.com/2008/10/22/bulk-loading-in-observablecollection/
    /// </summary>
    /// <typeparam name="T">Typically <see cref="Record"/></typeparam>
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("[ROC.AddRange]: List to add was null");

            _suppressNotification = true;

            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>
    /// Model for a row in the TechnicianSubmission Table.
    /// </summary>
    public class Record : INotifyPropertyChanged
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
        #endregion

        public static implicit operator Record(object[] vals)
        {
            int vindex = 0;

            var newRecord = new Record();

            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ID = int.TryParse(vals[vindex]?.ToString(), out int id_res) ? id_res : 0; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Technician = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.DateReceived = DateTime.TryParse(vals[vindex]?.ToString(), out DateTime drec_res) ? drec_res : DateTime.Now.AddYears(-100); vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.PartName = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.PartNumber = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.CommoditySubClass = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Quantity = int.TryParse(vals[vindex]?.ToString(), out int q_res) ? q_res : 0; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.SoftwareVersion = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Scrap = vals[vindex]?.ToString().Equals("1") ?? false; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TypeOfReturn = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TypeOfFailure = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.HoursOnUnit = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ReportedIssue = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TestResult = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TestResultAbort = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Cause = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Replacement = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.PartsReplaced = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.RefDesignator = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.AdditionalComments = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.CustomerNumber = int.TryParse(vals[vindex]?.ToString(), out int cust_res) ? cust_res : 0; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.SerialNumber = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.DateSubmitted = DateTime.TryParse(vals[vindex]?.ToString(), out DateTime dsub_res) ? dsub_res : DateTime.Now.AddYears(-50); vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.SubmissionStatus = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Quality = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.RP = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TechAct1 = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TechAct2 = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TechAct3 = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.QCDQEComments = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.OrderNumber = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ProblemCode1 = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ProblemCode2 = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.RepairCode = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.TechComments = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.LineNumber = decimal.TryParse(vals[vindex]?.ToString(), out decimal line_res) ? line_res : 0.0m; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ProcessedFlag = char.TryParse(vals[vindex]?.ToString(), out char c_res) ? c_res : ' '; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.ProcessedDateTime = DateTime.TryParse(vals[vindex]?.ToString(), out DateTime dpro_res) ? dpro_res : DateTime.Now.AddYears(-25); vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Series = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.FromArea = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.SaveID = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.QCDQEDateSubmitted = DateTime.TryParse(vals[vindex]?.ToString(), out DateTime ddqe_res) ? ddqe_res : DateTime.Now.AddYears(-5); vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Issue = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Item = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.Problem = vals[vindex]?.ToString() ?? ""; vindex++;
            if (vals[vindex] == null || vals[vindex] == DBNull.Value) { vals[vindex] = default; }
            newRecord.LogID = long.TryParse(vals[vindex]?.ToString(), out long id_log) ? id_log : 0L;

            return newRecord;
        }
    }
}
