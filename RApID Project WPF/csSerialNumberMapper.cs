using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace RApID_Project_WPF
{
    public class csSerialNumberMapper : IDisposable
    {
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

        /// <summary>
        /// The schematic path
        /// </summary>
        private const string SchemaPath = @"\\joi\EU\application\EngDocumentation\Design\Electrical\";

        /*
                /// <summary>
                /// The local path
                /// </summary>
                private string _localPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\RetestDocs\";
        */

        /// <summary>
        /// Boolean for the Ice Flasher case
        /// </summary>
        public bool BIsIce;

        /// <summary>
        /// Boolean for the NMEA 2K Case
        /// </summary>
        public bool BIsNMEA;

        /// <summary>
        /// Gets the component number for the scanned barcode.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        /// <returns>True if data is found | False if no data is found</returns>
        public bool GetData(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Equals("\0")) return false; // disconnected scanners emit \0 on reconnect
            BarcodeNumber = barcode;
            var date = DateTime.Parse($"{BarcodeNumber.Substring(2, 2)}-{BarcodeNumber.Substring(4, 2)}-{BarcodeNumber.Substring(0, 2)}");
            var success = $"Board SN [{BarcodeNumber}] on date {date} with PN {PartNumber} & WO# {WorkNumber}";
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
                                                     "WHERE(RTRIM(LTRIM([ItemNumber])) = RTRIM(LTRIM(@PN)) OR RTRIM(LTRIM([WONumber])) = RTRIM(LTRIM(@WO)) " +
                                                     "OR RTRIM(LTRIM([ComponentNumber])) = RTRIM(LTRIM(@CN)))" +
                                                     "AND ComponentDescription LIKE '%P%C%B%' " +
                                                     "AND ComponentLineNumber IN (2.0, 5.0, 6.0)", hummingConn); searchWoList.Parameters.AddWithValue("@PN", PartNumber);
            searchWoList.Parameters.AddWithValue("@WO", WorkNumber);
            searchWoList.Parameters.AddWithValue("@CN", ComponentNumber);
            var searchWoListBackup = new SqlCommand("SELECT TOP(1) [ComponentNumber],[ItemNumber] FROM [HummingBird].[dbo].[WorkOrderPartsListBackup] " +
                                                     "WHERE(RTRIM(LTRIM([ItemNumber])) = RTRIM(LTRIM(@PN)) OR RTRIM(LTRIM([WONumber])) = RTRIM(LTRIM(@WO)) " +
                                                     "OR RTRIM(LTRIM([ComponentNumber])) = RTRIM(LTRIM(@CN)))" +
                                                     "AND ComponentDescription LIKE '%P%C%B%' " +
                                                     "AND ComponentLineNumber IN (2.0, 5.0, 6.0)", hummingConn); searchWoListBackup.Parameters.AddWithValue("@PN", PartNumber);
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
        public Tuple<string, bool> FindFile(string ext)
        {
            var filename = "";
            var found = false;

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

            // if (!Directory.Exists(LOCAL_PATH + ComponentNumber)) Directory.CreateDirectory(LOCAL_PATH + ComponentNumber);

            /*if (File.Exists(Path.Combine(LOCAL_PATH + ComponentNumber) + "\\" + PartNumber + "-PCB-Assembly"+ext))
            {
                filename = Path.Combine(LOCAL_PATH + ComponentNumber) + "\\" + PartNumber + "-PCB-Assembly"+ext;
                found = true;
            }
            else
            {*/
            foreach (var dir in Directory.GetDirectories(SchemaPath, @"*" + ComponentNumber + @"*", SearchOption.TopDirectoryOnly))
            {
                var dirstatus = $"Using {dir} as parent directory...";
                var subdirstatus = $"Using {0} as direct child...";
                Console.WriteLine(dirstatus);

                if (ext.Equals(".pdf"))
                {
                    if (Directory.GetFiles(dir, PartNumber + "*ASSY*").Length > 0)
                    {
                        FirebasePdf = Directory.GetFiles(dir, PartNumber + "*ASSY*")[0];
                    }

                    filename = dir;
                    found = true;
                    if (!string.IsNullOrEmpty(FirebasePdf)) break; // stop from processing further...
                }
                else if (ext.Equals(".xls"))
                {
                    #region Search for XLS File (in parent DIR)
                    foreach (var file in Directory.GetFiles(dir, "*" + PartNumber + "*xls*"))
                    {
                        if (string.IsNullOrEmpty(file)) { }
                        else
                        {
                            filename = file;
                            found = true;
                            break; // from file loop
                        }
                    }

                    if (!found)
                    {
                        foreach (var file in Directory.GetFiles(dir, "*" + ComponentNumber + "*xls*"))
                        {
                            if (string.IsNullOrEmpty(file)) { }
                            else
                            {
                                filename = file;
                                found = true;
                                break; // from file loop
                            }
                        }
                    }
                    #endregion
                    if (found) break; // from dir preindex
                }


                foreach (string subdir in Directory.GetDirectories(dir, @"*" + PartNumber + @"*"))
                {
                    Console.WriteLine(subdirstatus, subdir.Remove(0, dir.Length + 1));
                    if (string.IsNullOrEmpty(subdir)) { }
                    else
                    {
                        if (ext.Equals(".pdf") && string.IsNullOrEmpty(FirebasePdf))
                        {
                            foreach (var file in Directory.GetFiles(subdir, ComponentNumber + "*ASSY*"))
                            {
                                if (string.IsNullOrEmpty(file)) { }
                                else
                                {
                                    FirebasePdf = file;
                                    break; // from file loop
                                }
                            }
                        }
                        else
                        {
                            #region Search for XLS File (in subdirs)
                            foreach (var file in Directory.GetFiles(subdir, "*" + PartNumber + "*xls*"))
                            {
                                if (string.IsNullOrEmpty(file)) { }
                                else
                                {
                                    filename = file;
                                    found = true;
                                    break; // from file loop
                                }
                            }

                            if (!found)
                            {
                                foreach (var file in Directory.GetFiles(subdir, "*" + ComponentNumber + "*xls*"))
                                {
                                    if (string.IsNullOrEmpty(file)) { }
                                    else
                                    {
                                        filename = file;
                                        found = true;
                                        break; // from file loop
                                    }
                                }
                            }
                            #endregion
                        }

                        if (found && (ext.Equals("pdf") || !string.IsNullOrEmpty(FirebasePdf))) break; // from subdir loop
                    }
                }
                if (found) break; // from dir loop
            }
            //}

            return new Tuple<string, bool>(filename, found);
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="T:System.NotImplementedException"></exception>
        public void Dispose() => _mapperInstance = null;
    }
}
