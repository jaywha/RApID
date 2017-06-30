using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;



namespace TEST_eFuseLinuxCommand
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SerialPort sp = new SerialPort("COM2", 38400))
            {
                sp.Open();
                vSleepTime(1000);

                string testCommand = "#rGPSI\r\n"; //gpsi //ipfi
                bool bGpsConnected = false;
                string InputData = "";
                while (!bGpsConnected)
                {
                    sp.Write(testCommand);
                    Log("GPS not ready");
                    InputData = sp.ReadExisting();
                    if (InputData != String.Empty)
                    {
                        if (InputData != "0\n\r")
                        {
                            bGpsConnected = true;
                        }
                    }
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(500);
                }

                WriteSerialData(sp, "#lcat < /dev/ttyO1 > file1.txt &" + Environment.NewLine);
                vSleepTime(5000);
                sReadComData(sp);

                WriteSerialData(sp, "#lecho -ne \"\\xb5\\x62\\x0a\\x0d\\x00\\x00\\x17\\x4f\" > /dev/ttyO1" + Environment.NewLine);
                vSleepTime(5000);
                sReadComData(sp);
                WriteSerialData(sp, "#lecho -ne \"\\xb5\\x62\\x0a\\x0d\\x00\\x00\\x17\\x4f\" > /dev/ttyO1" + Environment.NewLine);
                vSleepTime(5000);
                sReadComData(sp);
                //WriteSerialData(sp, "#lecho -ne \"\\xb5\\x62\\x0a\\x0d\\x00\\x00\\x17\\x4f\" > /dev/ttyO1" + Environment.NewLine);
                //vSleepTime(5000);
                //sReadComData(sp);
                //WriteSerialData(sp, "#lecho -ne \"\\xb5\\x62\\x0a\\x0d\\x00\\x00\\x17\\x4f\" > /dev/ttyO1" + Environment.NewLine);
                //vSleepTime(5000);
                //sReadComData(sp);

                WriteSerialData(sp, "#lpkill -9 -f cat" + Environment.NewLine);
                vSleepTime(5000);
                sReadComData(sp);

                WriteSerialData(sp, "#lhexdump -v -e '1/1 \"%02x \" ' file1.txt > /dev/ttyO3" + Environment.NewLine); // > file2.txt
                vSleepTime(5000);
                sReadComData(sp);
            }

        }

        public void WriteSerialData(SerialPort port, String text)
        {
            try
            {
                port.WriteTimeout = 1000;

                port.Write(text);

                Log("Serial Write: " + text.Replace("\r\n", ""));
            }
            catch (Exception ex)
            {
            }
        }

        public string sReadComData(SerialPort port)
        {
            try
            {
                port.ReadTimeout = 1000;
                string data = port.ReadExisting();

                Log("Serial Read: " + data.Replace("\r\n", "") + "\n\n");

                return data;
            }
            catch
            {
                return "";
            }
        }

        public void Log(string text)
        {
            this.richTextBox1.Invoke(new EventHandler(delegate
            {
                if (!string.IsNullOrEmpty(text) & !text.Equals("\r\n"))
                {
                    richTextBox1.ReadOnly = false;
                    richTextBox1.AppendText(text + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                    System.Windows.Forms.Application.DoEvents();
                    richTextBox1.ReadOnly = true;
                }
            }));
        }

        public void vSleepTime(int iTimeToSleep)
        {
            DateTime dtWaitTime = DateTime.Now.AddMilliseconds(iTimeToSleep);
            while (DateTime.Now <= dtWaitTime)
            { }
        }
    }
}
