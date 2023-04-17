using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml.Serialization;

namespace GrandUnifiedEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TransferSystem TestTransferSystem;
        MLSStageConnector MLSSystemStage;
        List<String> PortsFound;
        ObservableCollection<PositionConditionDetails> MovementPositions = new();
        XmlSerializer serializer = new XmlSerializer(
            typeof(ObservableCollection<PositionConditionDetails>));
        readonly Binding ZXPosition = new("ZaberOnePositionStr");
        readonly Binding ZYPosition = new("ZaberTwoPositionStr");
        readonly Binding ZZPosition = new("ZaberThreePositionStr");
        readonly Binding ZXHome = new("ZaberOneHomeStr");
        readonly Binding ZYHome = new("ZaberTwoHomeStr");
        readonly Binding ZZHome = new("ZaberThreeHomeStr");
        readonly Binding bSR = new("SRASReady");
        readonly Binding bSCTL = new("SRASClearToLoad");
        readonly Binding bSComplete = new("SRASCompleted");
        readonly Binding bSERR = new("SRASError");
        readonly Binding Grip = new("GripperActive");
        readonly Binding R3DRTL = new("R3DReadyToLoad");
        readonly Binding R3DRTS = new("R3DReadyToSRAS");
        readonly Binding R3DOK = new("R3DIsOkay");
        readonly Binding IntSysState = new("InternalState");
        public bool SystemIsActive = false;

        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("Main Window Loaded");
            this.TestTransferSystem = new TransferSystem();
            this.PortsFound = new List<String>();
            this.PortComboBox.ItemsSource = this.PortsFound;
            this.PortsFound.Add("Press Scan Ports to get list.");
            this.PortComboBox.SelectedIndex = 0;
            ZXPosition.Source = TestTransferSystem;
            ZYPosition.Source = TestTransferSystem;
            ZZPosition.Source = TestTransferSystem;
            ZXHome.Source = TestTransferSystem;
            ZYHome.Source = TestTransferSystem;
            ZZHome.Source = TestTransferSystem;
            bSR.Source = TestTransferSystem;
            bSCTL.Source = TestTransferSystem;
            bSComplete.Source = TestTransferSystem;
            bSERR.Source = TestTransferSystem;
            Grip.Source = TestTransferSystem;
            R3DRTL.Source = TestTransferSystem;
            R3DRTS.Source = TestTransferSystem;
            R3DOK.Source = TestTransferSystem;
            IntSysState.Source = TestTransferSystem;
            this.AxisOnePosition.SetBinding(Label.ContentProperty, ZXPosition);
            this.AxisTwoPosition.SetBinding(Label.ContentProperty, ZYPosition);
            this.AxisThreePosition.SetBinding(Label.ContentProperty, ZZPosition);
            this.AxisOneHomed.SetBinding(Label.ContentProperty, ZXHome);
            this.AxisTwoHomed.SetBinding (Label.ContentProperty, ZYHome);
            this.AxisThreeHomed.SetBinding(Label.ContentProperty, ZZHome);
            this.bSRASReady.SetBinding(Label.ContentProperty, bSR);
            this.bClearToLoadSRAS.SetBinding(Label.ContentProperty, bSCTL);
            this.bSRASDone.SetBinding(Label.ContentProperty, bSComplete);
            this.bSRASError.SetBinding(Label.ContentProperty, bSERR);
            this.bReadyToLoadSRAS.SetBinding(Label.ContentProperty, R3DRTL);
            this.bReadyToSRAS.SetBinding(Label.ContentProperty, R3DRTS);
            this.bR3DOK.SetBinding(Label.ContentProperty, R3DOK);
            this.MagGrip.SetBinding(Label.ContentProperty, Grip);
            this.PositionDataGrid.SetBinding(DataGrid.SelectedIndexProperty, IntSysState);

            this.PositionDataGrid.ItemsSource = MovementPositions;

            /*
            this.PositionDataGrid.Columns[0].Header = "Friendly Name";
            this.PositionDataGrid.Columns[1].Header = "X [mm]";
            this.PositionDataGrid.Columns[2].Header = "Y [mm]";
            this.PositionDataGrid.Columns[3].Header = "Z [mm]";
            this.PositionDataGrid.Columns[4].Header = "SRASCTL";
            this.PositionDataGrid.Columns[5].Header = "SRASDONE";
            this.PositionDataGrid.Columns[6].Header = "SRASERR";
            this.PositionDataGrid.Columns[7].Header = "Unused";
            this.PositionDataGrid.Columns[8].Header = "R3DRTL";
            this.PositionDataGrid.Columns[9].Header = "R3DRTS";
            this.PositionDataGrid.Columns[10].Header = "R3DOK";
            this.PositionDataGrid.Columns[11].Header = "Unused";
            */ 
        }

        private void ScanPorts_Click(object sender, RoutedEventArgs e)
        {            this.PortsFound = this.TestTransferSystem.enumeratePorts();
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
                TestTransferSystem.UpdateStatus();
            } else
            {
                ToggleZaberConnection.Content = "Connect";
            }
        }

        private void ThorlabsButton_Click(object sender, RoutedEventArgs e)
        {
            MLSSystemStage = new MLSStageConnector();
            if(MLSSystemStage.IsConnected == false)
            {
                MLSSystemStage.SerialNumber = ThorlabsSN.Text;
                MLSSystemStage.ToggleConnection();
                ThorlabsButton.Content = "Disconnect MLS";
            } else if (MLSSystemStage.IsConnected == true)
            {
                MLSSystemStage.ToggleConnection();
                ThorlabsButton.Content = "Connect MLS";
            }
        }

        private void AddPositionButton_Click(object sender, RoutedEventArgs e)
        {
            this.MovementPositions.Add(new());
            this.MovementPositions[this.MovementPositions.Count - 1].PositionFriendlyName = "Add a friendly name here";
            this.PositionDataGrid.UpdateLayout();
        }

        private void RemovePositionButton_Click(object sender, RoutedEventArgs e)
        {
            var _selectedPosition = this.PositionDataGrid.SelectedIndex;
            if (_selectedPosition < 0)
            {
                MessageBox.Show("You must select a row to remove", "Removal Error");
            } else
            {
                this.MovementPositions.RemoveAt(_selectedPosition);
                //this.PositionDataGrid.UpdateLayout();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check for the existence of the file that contains the positions
            // of the transfer system. Otherwise use a blank observablecollection.

            if(File.Exists("positions.xml"))
            {
                Debug.WriteLine("Found existing positions.xml file. Loading entries.");

                using (StreamReader posreader = new StreamReader("positions.xml"))
                {
                    this.MovementPositions = serializer.Deserialize(posreader) 
                                        as ObservableCollection<PositionConditionDetails>;
                    this.PositionDataGrid.ItemsSource = this.MovementPositions;
                    this.PositionDataGrid.UpdateLayout();    
                }
            } else
            {
                this.MovementPositions = new ObservableCollection<PositionConditionDetails>();
            }
        }

        private void SaveToXMLButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter poswriter = new StreamWriter("positions.xml"))
            {
                serializer.Serialize(poswriter, MovementPositions);
            }
        }

        private void EnableControl_Click(object sender, RoutedEventArgs e)
        {
            bool cycleHasFinished = false;
            if(SystemIsActive == false)
            {
                SystemIsActive = true;
                TestTransferSystem.InternalState = 0;
                EnableControl.Content = "SYSTEM ACTIVE. Click to Disable.";
                Task.Run(() =>
                {
                    Debug.WriteLine("Spawning System Control Thread...");

                    if (TestTransferSystem.IsConnected)
                    {
                        // Make sure we are actually connected and homed before
                        // making any movements and such
                        while (SystemIsActive)
                        {
                            // Flag to let the system know it needs to move backwards now
                            if(TestTransferSystem.InternalState == (MovementPositions.Count))
                            { 
                                cycleHasFinished = true;
                            }
                            if(!cycleHasFinished)
                            {
                                Debug.WriteLine("Cycle is advancing to stage " + TestTransferSystem.InternalState
                                    + ".");
                                var targetPosition = MovementPositions[TestTransferSystem.InternalState];
                                // Check if the input conditions are satisfied
                                var inputsReady = false;
                                while(inputsReady == false)
                                {
                                    if (SystemIsActive == false)
                                    { return; }
                                    // Get the inputs and see if the match the required input positions
                                    bool i1 = targetPosition.InputOne == TestTransferSystem.R3DReadyToLoad;
                                    bool i2 = targetPosition.InputTwo == TestTransferSystem.R3DReadyToSRAS;
                                    bool i3 = targetPosition.InputThree == TestTransferSystem.R3DIsOkay;
                                    inputsReady = (i1 && i2 && i3) == true;
                                    Thread.Sleep(100);
                                }
                                if (SystemIsActive == false)
                                {
                                    // Check to see if the enable button has been toggled off
                                    return;
                                }
                                var _res = MessageBox.Show("Inputs to the controller have been satisfied. Move?",
                                    "Move Request", MessageBoxButton.YesNo);
                                // Check if the back slide is where it should be
                                if ((decimal)TestTransferSystem.ZaberOnePosition != targetPosition.XPos)
                                {
                                    TestTransferSystem.MoveAxis(0, targetPosition.XPos);
                                    while((decimal)Math.Round(TestTransferSystem.ZaberOnePosition, 2) !=
                                    Math.Round(targetPosition.XPos, 2))
                                    {
                                        Thread.Sleep(100);
                                        Debug.WriteLine("Target X: {0}, Actual: {0}", (decimal)Math.Round(TestTransferSystem.ZaberOnePosition, 2),
                                            Math.Round(targetPosition.XPos, 2));
                                        Debug.WriteLine("Waiting for move to complete.");

                                    }
                                }
                                // Check if the SRAS->Back slide is where it should be
                                if ((decimal)TestTransferSystem.ZaberTwoPosition != targetPosition.YPos)
                                {
                                    TestTransferSystem.MoveAxis(1, targetPosition.YPos);
                                    while ((decimal)Math.Round(TestTransferSystem.ZaberTwoPosition, 2) !=
                                        Math.Round(targetPosition.YPos, 2))
                                    {
                                        Thread.Sleep(100);
                                        Debug.WriteLine("Target Y: {0}, Actual: {0}", (decimal)Math.Round(TestTransferSystem.ZaberTwoPosition, 2),
                                            Math.Round(targetPosition.YPos,2));
                                        Debug.WriteLine("Waiting for move to complete.");
                                    }
                                }
                                // Check if the plunge axis where it should be
                                if ((decimal)Math.Round(TestTransferSystem.ZaberThreePosition,2)  != 
                                    Math.Round(targetPosition.ZPos, 2))
                                {
                                    TestTransferSystem.MoveAxis(2, targetPosition.ZPos);
                                    Debug.WriteLine("Target Z: {0}, Actual: {0}", (decimal)TestTransferSystem.ZaberThreePosition,
                                        targetPosition.ZPos);
                                }
                                Thread.Sleep(100);  // 100 ms sleep 
                                // Set the outputs 
                                TestTransferSystem.SetOutputs(targetPosition.OutputOne, targetPosition.OutputTwo,
                                    targetPosition.OutputThree, targetPosition.OutputFour, targetPosition.GripOn);
                                
                                // Take another nap
                                Thread.Sleep(100);
                                TestTransferSystem.InternalState++;
                            }
                            Thread.Sleep(100);
                        }
                }
                });
            } else
            {
                SystemIsActive = false;
                EnableControl.Content = "Enable Transfer System";
            }
        }

        private void ForceNextStep_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TeachPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if(TestTransferSystem.IsConnected)
            {
                var _x = Math.Round(TestTransferSystem.ZaberOnePosition, 2);
                var _y = Math.Round(TestTransferSystem.ZaberTwoPosition, 2);
                var _z = Math.Round(TestTransferSystem.ZaberThreePosition, 2);

                var _selectedIndex = PositionDataGrid.SelectedIndex;
                if (_selectedIndex < 0)
                {
                    MessageBox.Show("You must select a row to teach.",
                        "Position Teach Error");
                } else
                {
                    var _ox = Math.Round(MovementPositions[_selectedIndex].XPos, 2);
                    var _oy = Math.Round(MovementPositions[_selectedIndex].YPos, 2);   
                    var _oz = Math.Round(MovementPositions[_selectedIndex].ZPos, 2);

                    var _retval = MessageBox.Show("Do you want to replace the positions" +
                        "in row " + _selectedIndex + 1 + "?\nOld Values:\tNew Values:\n" +
                        "x: " + _ox + "\t " + _x + "\ny: " + _oy + "\t " + _y +
                        "\nz: " + _oz + "\t " + _z, "Teach Position " + _selectedIndex + "?",
                        MessageBoxButton.YesNoCancel);
                    if(_retval == MessageBoxResult.Yes )
                    {
                        MovementPositions[_selectedIndex].XPos = (decimal)_x;
                        MovementPositions[_selectedIndex].YPos = (decimal)_y;
                        MovementPositions[_selectedIndex].ZPos = (decimal)_z;
                        Debug.WriteLine("Overwrote Position " + _selectedIndex +
                            "with x: " + _x + "\ty: " + _y + "\tz" + _z + ".");
                    } else if(_retval == MessageBoxResult.No )
                    {
                        // Do nothing.
                        Debug.WriteLine("User elected to NOT overwrite position #" +
                            _selectedIndex);
                    }
                }
            }
        }

        private void ToggleMag_Click(object sender, RoutedEventArgs e)
        {
            TestTransferSystem.ToggleGrip();
        }
    }
}
