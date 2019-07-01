using ExcelDataReader;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
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

        /// <summary>
        /// Boolean for the Ethernet Cases
        /// </summary>
        public bool BIsEthernet;

        /// <summary>
        /// Output string for temporary schematic PDFs
        /// </summary>
        public string OUT_STR = $"Temp<TYPE>-<PN>-<CN>.pdf";
        #endregion

        /// <summary>
        /// Prints a simple message with info about this mapper.
        /// </summary>
        /// <returns>A preformatted message -- used in debugging.</returns>
        public string Success() => $"Board SN [{BarcodeNumber}] from date {DateTime.Now} with PN {PartNumber} & WO# {WorkNumber}";

        /// <summary>
        /// Gets the component number for the scanned barcode.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        /// <returns>True if data is found | False if no data is found</returns>
        public bool GetData(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Equals("\0")) return false; // disconnected scanners emit \0 on reconnect
            BarcodeNumber = barcode;

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();

                using(var cmd = new SqlCommand("spFindWOPN", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SCANNED", barcode);
                    using(var reader = cmd.ExecuteReader())
                    {
                        while(reader.NextResult())
                        {
                            if (reader.GetName(0).Equals("WorkOrder") && reader.Read()) {
                                WorkNumber = reader["WorkOrder"].ToString().Trim();
                                PartNumber = reader["PartNumber"].ToString().Trim();
                                ComponentNumber =
                                    string.IsNullOrWhiteSpace(reader["ComponentNumber"].ToString())
                                    ? reader["ComponentNumberBackup"].ToString().Trim()
                                    : reader["ComponentNumber"].ToString().Trim();
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
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
            #region Old Code
            var filename = "";
            var found = false;

            Console.WriteLine($"Running find... using data package {AsDataPackage()}");

            if (BIsEthernet && ext.Equals(".xls"))
            {
                filename = @"\\joi\EU\application\EngDocumentation\Design\Electrical\408208-3 REV C (AS ETH 5PS)\408206-1_(AS ETH 5PS)_408208-3_C.xls";
                ShowSuccessMessage(filename);
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
                            ShowSuccessMessage(filename);
                            break;
                        }
                    }
                }));
                if (!found)
                {
                    #region Tech Form Code
                    /*
                     var finalOutPath = OUT_STR.Replace("<PN>", PartNumber).Replace("<CN>", ComponentNumber);

                    switch (ext) {
                        default:
                        case ".xls":
                        case ".xlsm":
                        case ".xlsx":
                            string bom = "";
                            (bom,_,_) = frmBoardAliases.FindFilesFor(PartNumber);
                            return new Tuple<string, bool>(bom, !string.IsNullOrWhiteSpace(bom));
                        case ".pdf":
                            //TODO: Change to a listbox of Schematic files
                            finalOutPath = finalOutPath.Replace("<TYPE>", "ASSY");
                            string top = "";
                            string bottom = "";
                            (_,top,bottom) = frmBoardAliases.FindFilesFor(PartNumber);

                            // https://stackoverflow.com/a/808699/7476183
                            using (var outPdf = new PdfDocument())
                            {
                                if (string.IsNullOrWhiteSpace(top)) {
                                    return new Tuple<string, bool>("Nothing doing.", false);
                                }

                                var one = PdfReader.Open(top, PdfDocumentOpenMode.Import);
                                CopyPages(one, outPdf);

                                if (!string.IsNullOrWhiteSpace(bottom))
                                {
                                    var two = PdfReader.Open(bottom, PdfDocumentOpenMode.Import);
                                    CopyPages(two, outPdf);
                                }

                                outPdf.Save(finalOutPath);
                            }

                            void CopyPages(PdfDocument from, PdfDocument to)
                            {
                                for (int i = 0; i < from.PageCount; i++)
                                {
                                    to.AddPage(from.Pages[i]);
                                }
                            }

                            return new Tuple<string, bool>(finalOutPath, true);
                    }*/
                    #endregion
                }
            } catch(Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("BarcodeMapper_FindFileError",e);
            }

            return new Tuple<string, bool>(filename, found);
            
            #endregion
        }

        private void ShowSuccessMessage(string filename)
        {
            void OpenDirectory(object sender, RoutedEventArgs e)
                                => System.Diagnostics.Process.Start(filename.Substring(0, filename.LastIndexOf('\\')));

            MainWindow.Notify.Dispatcher.Invoke(() => {
                MainWindow.Notify.TrayBalloonTipClicked += OpenDirectory;
                MainWindow.Notify.TrayBalloonTipClosed += delegate {
                    MainWindow.Notify.TrayBalloonTipClicked -= OpenDirectory;
                };
                MainWindow.Notify.ShowBalloonTip($"BOM Parts Pulled for [{PartNumber}]",
                $"The file is stored here: {filename}", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            });
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
