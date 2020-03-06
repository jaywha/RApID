using RetestVerifierAppWPF.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public ObservableCollection<BoardModel> Boards { get; } = new ObservableCollection<BoardModel>();

        private SerialPort _mbs = new SerialPort();
        public SerialPort MainBarcodeScanner {
            get => _mbs;
            set {
                _mbs = value;
                if(_mbs != null)
                    _mbs.DataReceived += BarcodeSerial_DataReceived;
                OnPropertyChanged();
            }
        }

        public BatchWindow()
        {
            InitializeComponent();
            if(MainBarcodeScanner == null || string.IsNullOrEmpty(MainBarcodeScanner.PortName))
            {
                txtBuffer.Visibility = Visibility.Visible;
            }
        }

        private void BarcodeSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var secondLater = DateTime.Now.AddSeconds(1.0);

            var boardNumber = "";
            while (DateTime.Now < secondLater)
                boardNumber += ((SerialPort)sender).ReadExisting().Trim();
            
            Dispatcher.Invoke(()=> Boards.Add(boardNumber));
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void WndBatch_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainBarcodeScanner.DataReceived -= BarcodeSerial_DataReceived;
        }

        private void mnuiDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedBoard = lvBoards.SelectedItem as BoardModel;

            Boards.Remove(selectedBoard);
        }
    }
}
