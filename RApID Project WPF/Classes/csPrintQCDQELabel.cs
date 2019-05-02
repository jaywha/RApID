/*
 * csPrintQCDQELabel.cs: Handles the printing of the QC/DQE Labels.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Drawing;


namespace RApID_Project_WPF
{
    class csPrintQCDQELabel
    {
        private PrintDocument docToPrint = new PrintDocument();
        private PrintPreviewDialog docToPreview = new PrintPreviewDialog();
        private PrintDialog docDialog = new PrintDialog();

        BarcodeLib.Barcode bcLib = new BarcodeLib.Barcode();
        BarcodeLib.TYPE bcType128 = BarcodeLib.TYPE.CODE128;

        private string sSentTo;
        private string sSentBy;
        private string sTimeSent;
        private string sBarcode;

        public csPrintQCDQELabel(string _sentTo, string _sentBy, string _timeSent, string _barcode)
        {
            docToPrint.PrintPage += new PrintPageEventHandler(docToPrint_PrintPage);
            sSentTo = _sentTo;
            sSentBy = _sentBy;
            sTimeSent = _timeSent;
            sBarcode = _barcode;
        }

        public void PrintLabel()
        {
            try
            {
                docToPrint.PrinterSettings.Copies = 1;
                buildLabel();
                docToPrint.PrinterSettings.PrinterName = Properties.Settings.Default.PrinterToUse;
                docToPrint.Print();
            }
            catch { }
        }

        public void PrintPreview()
        {
            try
            {
                docToPrint.PrinterSettings.Copies = 1;
                buildLabel();
                docToPrint.PrinterSettings.PrinterName = Properties.Settings.Default.PrinterToUse;
                docToPreview.ShowDialog();
            }
            catch { }
        }

        public void buildLabel()
        {
            docToPrint.DefaultPageSettings.PaperSize = new PaperSize("QCDQE", 200, 150);
            docToPrint.DefaultPageSettings.Landscape = false;
            docToPrint.PrinterSettings.PrinterName = Properties.Settings.Default.PrinterToUse;
            docToPreview.Document = docToPrint;
        }

        private void docToPrint_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                var LabelFont = new Font("Arial", 9, FontStyle.Bold);

                var blackBrush = new SolidBrush(Color.Black);
                var whiteBrush = new SolidBrush(Color.White);

                var blackPen = new Pen(Color.Black);
                var whitePen = new Pen(Color.White);

                var centerString = new StringFormat();
                centerString.Alignment = StringAlignment.Center;
                centerString.LineAlignment = StringAlignment.Center;

                int xOffset = Properties.Settings.Default.PrinterXOffset;
                int yOffset = Properties.Settings.Default.PrinterYOffset;
                int pWidth = docToPrint.DefaultPageSettings.PaperSize.Width;
                int pHeight = docToPrint.DefaultPageSettings.PaperSize.Height;

                //-Label Outline-//
                var labelRect = new Rectangle(xOffset, yOffset, pWidth, pHeight);
               // e.Graphics.DrawRectangle(blackPen, labelRect);

                //-Sent To...-//
                var sentToRect = new Rectangle(xOffset, yOffset, pWidth, 15);
                e.Graphics.DrawString("Sent To: " + sSentTo, LabelFont, blackBrush, sentToRect, centerString);

                //-Sent By...-//
                var sendByRect = new Rectangle(xOffset, yOffset, pWidth, 55);
                e.Graphics.DrawString("Sent By: " + sSentBy, LabelFont, blackBrush, sendByRect, centerString);

                //-Date and Time-//
                var dateTimeRect = new Rectangle(xOffset, yOffset, pWidth, 95);
                e.Graphics.DrawString(sTimeSent, LabelFont, blackBrush, dateTimeRect, centerString);

                //-Barcode-//
                var barcodeRect = new Rectangle(xOffset + 20, yOffset + 90, pWidth - 50, 40);
                bcLib.RawData = sBarcode;
                e.Graphics.DrawImage(bcLib.Encode(bcType128, Color.Black, Color.White), barcodeRect);

                //-Barcode Text-//
                var bcodeText = new Rectangle(xOffset, yOffset, pWidth, pHeight);
                e.Graphics.DrawString(sBarcode, LabelFont, blackBrush, bcodeText, centerString);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
