using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GrandUnifiedEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TransferSystem TestTransferSystem;
        List<String> PortsFound;  
        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("Main Window Loaded");
            this.TestTransferSystem = new TransferSystem();
            this.PortsFound = new List<String>();
            this.PortComboBox.ItemsSource = this.PortsFound;
            this.PortsFound.Add("Press Scan Ports to get list.");
            this.PortComboBox.SelectedIndex = 0;

        }

        private void ScanPorts_Click(object sender, RoutedEventArgs e)
        {
            this.PortsFound = this.TestTransferSystem.enumeratePorts();
            this.PortComboBox.ItemsSource = this.PortsFound;
            this.PortComboBox.SelectedIndex = 0;
        }

        private void ToggleZaberConnection_Click(object sender, RoutedEventArgs e)
        {
            var _uglyPort = PortComboBox.SelectedItem.ToString().Substring(0,4);
            Debug.WriteLine("_uglyPort: '" +  _uglyPort + "'");
            TestTransferSystem.ToggleConnection(_uglyPort);
            Debug.WriteLine("Connection State: " + TestTransferSystem.IsConnected.ToString());
            if(TestTransferSystem.IsConnected)
            {
                ToggleZaberConnection.Content = "Disconnect";
            } else
            {
                ToggleZaberConnection.Content = "Connect";
            }
        }
    }
}
