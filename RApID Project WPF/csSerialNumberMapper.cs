using ExcelDataReader;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TriggersTools.IO.Windows;

namespace RApID_Project_WPF
{
    public class csSerialNumberMapper : IDisposable
    {
        #region Singleton Instance
        /// <summary>
        /// The mapper instance
        /// </summary>
        private static csSerialNumberMapper _mapperInstance;
        /// <summary>
        /// Prevents a default instance of the <see cref="csSerialNumberMapper"/> class from being created.
        /// </summary>
        private csSerialNumberMapper() { }
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static csSerialNumberMapper Instance => _mapperInstance ?? new csSerialNumberMapper();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the barcode number.
        /// </summary>
        /// <value>
        /// The barcode number.
        /// </value>
        private string BarcodeNumber { get; set; } = "";

        /// <summary>
        /// Gets or sets the part number.
        /// </summary>
        /// <value>
        /// The part number.
        /// </value>
        private string PartNumber { get; set; } = "";

        /// <summary>
        /// Gets or sets the work number.
        /// </summary>
        /// <value>
        /// The work number.
        /// </value>
        private string WorkNumber { get; set; } = "";

        /// <summary>
        /// Gets or sets the component number.
        /// </summary>
        /// <value>
        /// The component number.
        /// </value>
        public string ComponentNumber { get; private set; } = "";

        /// <summary>
        /// Gets or sets the Firebase PDF =>
        ///     The current schematic.
        /// </summary>
        /// <value>
        /// The firebase PDF.
        /// </value>
        public string FirebasePdf { get; private set; } = "";
        #endregion

        #region Fields
        /// <summary>
        /// The schematic path
        /// </summary>
        private const string SchemaPath = @"L:\EngDocumentation\Design\Electrical\";

        /// <summary>
        /// Boolean for the Ice Flasher case
        /// </summary>
        public bool BIsIce;

        /// <summary>
        /// Boolean for the NMEA 2K Case
        /// </summary>
        public bool BIsNMEA;
        #endregion

        /// <summary>
        /// Gets the component number for the scanned barcode.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        /// <returns>True if data is found | False if no data is found</returns>
        public bool GetData(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Equals("\0")) return false; // disconnected scanners emit \0 on reconnect
            BarcodeNumber = barcode;
            DateTime date = DateTime.Now;
            try
            {
                date = DateTime.Parse($"{BarcodeNumber.Substring(2, 2)}-{BarcodeNumber.Substring(4, 2)}-{BarcodeNumber.Substring(0, 2)}");
            } catch(FormatException fe)
            {
                csExceptionLogger.csExceptionLogger.Write("GetData_UnusualDate", fe);

                try
                {
                    date = DateTime.Parse($"{BarcodeNumber.Substring(5, 2)}-{BarcodeNumber.Substring(7, 2)}-{BarcodeNumber.Substring(9, 2)}");
                }
                catch (Exception e)
                {
                    csExceptionLogger.csExceptionLogger.Write("GetData_NoDateAtAll", e);
                    return false;
                }
            }

            var success = $"Board SN [{BarcodeNumber}] from date {date} with PN {PartNumber} & WO# {WorkNumber}";
            const string errmsg = "There was an error during AOI/EOL SQL Execution.\nPlease inform EEPT.";

            var hummingConn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString);
            var yesConn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().YesDBConnectionString);

            hummingConn.Open();
            yesConn.Open();

            #region Get AOI and EOL Data
            var findWo = new SqlCommand("SELECT TOP(1) [WO],[Assy] FROM [YesDB].[dbo].[SPC_Data] " +
                                                   "WHERE [SN] = @SCANNED AND [WO] IS NOT NULL AND DATALENGTH([WO]) > 0 AND [WO] <> ' '", yesConn);
            findWo.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@SCANNED",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Input,
                Value = barcode
            });


            var findPn = new SqlCommand("SELECT TOP(1) [PartNumber] FROM [HummingBird].[dbo].[tblEOL] " +
                                                   "WHERE [PCBSerial] = @SCANNED AND [PartNumber] IS NOT NULL AND DATALENGTH([PartNumber]) > 0 AND [PartNumber] <> ' '", hummingConn);
            findPn.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@SCANNED",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Input,
                Value = barcode
            });

            try
            {
                var woreader = findWo.ExecuteReader();
                var pnreader = findPn.ExecuteReader();

                if (woreader.Read() && woreader["WO"] != DBNull.Value)
                    WorkNumber = woreader["WO"].ToString().Trim();
                if (pnreader.Read() && pnreader["PartNumber"] != DBNull.Value)
                    PartNumber = pnreader["PartNumber"].ToString().Trim();

                var assemblyInAoi = "";

                try
                {
                    if (woreader.Read() && woreader["Assy"] != DBNull.Value)
                        assemblyInAoi = woreader["Assy"].ToString();
                }
                catch (InvalidOperationException ioe)
                {
                    csExceptionLogger.csExceptionLogger.Write("AssemblyAOI-OnEmpty", ioe);
                }

                // In case board never went to EOL, grab ComponentNumber to get PN later
                if (string.IsNullOrEmpty(PartNumber) && !string.IsNullOrEmpty(assemblyInAoi)) ComponentNumber = assemblyInAoi.Substring(assemblyInAoi.LastIndexOf("REV", StringComparison.Ordinal) - 9, 8).Trim().Replace('_', '-');

                woreader.Close();
                pnreader.Close();
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("RetestVerifier-csBarcodeMapper_GetData-BadSQL_AOIEOL", e);
                MessageBox.Show(errmsg, @"BadSQL Data on WO/PN", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                findWo.Dispose();
                findPn.Dispose();
            }
            #endregion

            success = $"Board SN [{BarcodeNumber}] on date {date} with PN {PartNumber} & WO# {WorkNumber}";
            Console.WriteLine(success);

            // if both are null, then barcode not found!
            if (string.IsNullOrEmpty(PartNumber) && string.IsNullOrEmpty(WorkNumber)) { return false; }

            if (PartNumber.Equals("407026-1") || PartNumber.Equals("407025-1")) //ICE Flasher cases
            {
                BIsIce = true;
                ComponentNumber = "407028-6";
                return true;
            }

            if (PartNumber.Equals("408396-1") || PartNumber.Equals("408390-1")) // NMEA-2k Cases
            {
                BIsNMEA = true;
                ComponentNumber = "408398-1";
                return true;
            }

            #region Find ComponentNumber (or PartNumber)
            var searchWoList = new SqlCommand("SELECT TOP(1) [ComponentNumber],[ItemNumber] FROM [HummingBird].[dbo].[WorkOrderPartsList] " +
                                                     "WHERE (RTRIM(LTRIM([ItemNumber])) = RTRIM(LTRIM(@PN)) " +
                                                     "OR RTRIM(LTRIM([ComponentNumber])) = RTRIM(LTRIM(@CN)))" +
                                                     "AND ComponentDescription LIKE 'P%C%B%' ", hummingConn); searchWoList.Parameters.AddWithValue("@PN", PartNumber);
            searchWoList.Parameters.AddWithValue("@WO", WorkNumber);
            searchWoList.Parameters.AddWithValue("@CN", ComponentNumber);
            var searchWoListBackup = new SqlCommand("SELECT TOP(1) [ComponentNumber],[ItemNumber] FROM [HummingBird].[dbo].[WorkOrderPartsListBackup] " +
                                                     "WHERE (RTRIM(LTRIM([ItemNumber])) = RTRIM(LTRIM(@PN)) " +
                                                     "OR RTRIM(LTRIM([ComponentNumber])) = RTRIM(LTRIM(@CN)))" +
                                                     "AND ComponentDescription LIKE 'P%C%B%' ", hummingConn); searchWoListBackup.Parameters.AddWithValue("@PN", PartNumber);
            searchWoListBackup.Parameters.AddWithValue("@WO", WorkNumber);
            searchWoListBackup.Parameters.AddWithValue("@CN", ComponentNumber);

            try
            {
                using (var wolistReader = searchWoList.ExecuteReader())
                {
                    var targetNumber = string.IsNullOrEmpty(PartNumber) ? "ItemNumber" : "ComponentNumber";
                    var resultNumber = "";

                    if (wolistReader.Read() && wolistReader[targetNumber] != DBNull.Value)
                        resultNumber = wolistReader[targetNumber].ToString().Trim();
                    else
                    {
                        wolistReader.Close();
                        using (var backupReader = searchWoListBackup.ExecuteReader())
                            if (backupReader.Read() && backupReader[targetNumber] != DBNull.Value)
                                resultNumber = backupReader[targetNumber].ToString().Trim();
                            else return false; //no Number Set!
                    }

                    if (string.IsNullOrEmpty(PartNumber))
                    {
                        PartNumber = resultNumber;
                    }
                    else ComponentNumber = resultNumber;
                }
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("RetestVerifier-csBarcodeMapper_GetData-BadSQL_WOLists", e);
                MessageBox.Show(errmsg, @"BadSQL Data on WO Backup", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                searchWoList.Dispose();
                searchWoListBackup.Dispose();
            }
            #endregion

            hummingConn.Close();
            yesConn.Close();

            return true;
        }

        /// <summary>
        /// Finds the file.
        /// </summary>
        /// <param name="ext">The file extension of the target</param>
        /// <returns>A Tuple consisting of the filename and <c>true</c> if it was found</returns>
        [Obsolete("Legacy Extension method. Use FindFileAsync.")]
        public Tuple<string, bool> FindFile(string ext)
        {
            return FindFileAsync(ext).Result;
        }

        /// <summary>
        /// Finds the file.
        /// </summary>
        /// <param name="ext">The file extension of the target</param>
        /// <returns>A Task resulting in a Tuple consisting of the filename and <c>true</c> if it was found</returns>        
        public async Task<Tuple<string, bool>> FindFileAsync(string ext)
        {
            //TODO: Build a list of possible excel files for ops to test each one for JUKI tab
            var filename = "";
            var found = false;

            Console.WriteLine($"Running find... using data package {AsDataPackage()}");

            if (BIsIce && ext.Equals(".pdf"))
            {
                filename = @"\\joi\EU\application\EngDocumentation\Design\Electrical\407028-6 REV F (ICE FLASHER)\";
                return new Tuple<string, bool>(filename, true);
            }
            if (BIsIce && ext.Equals(".xls"))
            {
                filename = @"\\joi\EU\application\EngDocumentation\Design\Electrical\407028-6 REV F (ICE FLASHER)\407026-1_(ICE 35)_407028-6_F.xls";
                return new Tuple<string, bool>(filename, true);
            }

            try
            {
                await Task.Factory.StartNew(new Action(() =>
                {
                    foreach (var file in FileFind.EnumerateFiles(Directory.GetDirectories(SchemaPath, $@"*{ComponentNumber}*", SearchOption.TopDirectoryOnly).Last(),
                        $@"*{PartNumber}*{ext}*", SearchOrder.AllDirectories).Where(f => !f.Contains("Archive")))
                    {
                        var dirstatus = $"Checking {file} as possible file...";
                        Console.WriteLine(dirstatus);

                        if (ext.Equals(".pdf"))
                        {
                            if (file.Length > 0 && file.Contains("ASSY"))
                            {
                                FirebasePdf = file;
                                filename = file;
                                found = true;
                            }
                        }
                        else if (ext.Equals(".xls"))
                        {
                            if (file.Length > 0)
                            {
                                //Ensure file has JUKI sheet
                                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                                {
                                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                                    {
                                        DateTime start = DateTime.Now;
                                        while (reader.NextResult() && reader.Name != null)
                                        {
                                            found = reader.Name.Equals("JUKI");
                                            if (found || start.AddSeconds(5) == DateTime.Now) break;
                                        }
                                    }
                                }

                                if (found) filename = file;
                            }
                        }

                        if (found) {
                            RoutedEventHandler OpenDirectory = delegate {
                                System.Diagnostics.Process.Start(filename.Substring(0, filename.LastIndexOf('\\')));
                            };

                            MainWindow.Notify.Dispatcher.Invoke(() => {
                                MainWindow.Notify.TrayBalloonTipClicked += OpenDirectory;
                                MainWindow.Notify.TrayBalloonTipClosed += delegate {
                                    MainWindow.Notify.TrayBalloonTipClicked -= OpenDirectory;
                                };
                                MainWindow.Notify.ShowBalloonTip($"BOM Parts Pulled for [{PartNumber}]",
                                $"The file is stored here: {filename}", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                            });
                            break;
                        }
                    }
                }));
            } catch(Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("BarcodeMapper_FindFileError",e);
            }

            return new Tuple<string, bool>(filename, found);
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="T:System.NotImplementedException"></exception>
        public void Dispose() => _mapperInstance = null;

        /// <summary>
        /// Helper function to get all property values as an aggregated <see cref="string"/>.
        /// </summary>
        /// <returns>A preformatted string of values.</returns>
        public string AsDataPackage() => "{\n" +
                $"\tBarcode Number: {BarcodeNumber}\n" +
                $"\tPart Number: {PartNumber}\n" +
                $"\tComponent Number: {ComponentNumber}\n" +
                $"\tWork Order Number: {WorkNumber}\n" +
                "}\n";
    }
}
