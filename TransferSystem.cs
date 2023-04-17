using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using Zaber.Motion.Ascii;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading;

namespace GrandUnifiedEngine
{
    internal class TransferSystem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected = false;
        public int BaudRate = 115200;
        public string ComPort;

        public Connection ControllerConnectionInstance;
        public Task PositionPollingTask;
        Device PrimaryController;
        Device SecondaryController;
        List<Axis> FoundAxes = new List<Axis>();
        
        double _axisOnePos;
        double _axisTwoPos;
        double _axisThreePos;
        bool _axisOneHome;
        bool _axisTwoHome;
        bool _axisThreeHome;

        public string ZaberOnePositionStr
        {
            get { return String.Format("{0:N2}mm", _axisOnePos); }
        }
        public double ZaberOnePosition
        {
            get { return _axisOnePos; }
            set { _axisOnePos = value; OnPropertyChanged("ZaberOnePosition"); OnPropertyChanged("ZaberOnePositionStr"); }
        }
        public string ZaberTwoPositionStr
        {
            get { return String.Format("{0:N2}mm", _axisTwoPos); }
        }
        public double ZaberTwoPosition
        {
            get { return _axisTwoPos; }
            set { _axisTwoPos = value; OnPropertyChanged("ZaberTwoPosition"); OnPropertyChanged("ZaberTwoPositionStr"); }
        }
        
        public string ZaberThreePositionStr
        {
            get { return String.Format("{0:N2}mm", _axisThreePos); }
        }
        public double ZaberThreePosition
        {
            get { return _axisThreePos; }
            set { _axisThreePos = value; OnPropertyChanged("ZaberThreePosition"); OnPropertyChanged("ZaberThreePositionStr"); }
        }

        public string ZaberOneHomeStr
        {
            get { return _axisOneHome ? "Homed" : "Needs Homing"; }
        }
        public string ZaberTwoHomeStr
        {
            get { return _axisTwoHome ? "Homed" : "Needs Homing"; }
        }
        public string ZaberThreeHomeStr
        {
            get { return _axisThreeHome ? "Homed" : "Needs Homing"; }
        }

        public bool ZaberOneHome
        {
            get { return _axisOneHome; }
            set { _axisOneHome = value; OnPropertyChanged("ZaberOneHome"); OnPropertyChanged("ZaberOneHomeStr"); }
        }

        public bool ZaberTwoHome
        {
            get { return _axisTwoHome; }
            set { _axisTwoHome = value; OnPropertyChanged("ZaberTwoHome"); OnPropertyChanged("ZaberTwoHomeStr"); }
        }

        public bool ZaberThreeHome
        {
            get { return _axisThreeHome; }
            set { _axisThreeHome = value; OnPropertyChanged("ZaberThreeHome"); OnPropertyChanged("ZaberThreeHomeStr"); }
        }

        private bool _srasReady;
        private bool _srasClearToLoad;
        private bool _srasCompleted;
        private bool _srasError;
        private bool _gripperActive;
        private bool _r3dReadyToLoad;
        private bool _r3dReadyToSras;
        private bool _r3dIsOkay;

        public bool SRASReady
        {
            get => _srasReady;
            set
            {
                _srasReady = value; OnPropertyChanged();
            }
        }
        public bool SRASClearToLoad
        {
            get => _srasClearToLoad;
            set
            {
                _srasClearToLoad = value; OnPropertyChanged();
            }
        }
        public bool SRASCompleted
        {
            get => _srasCompleted;
            set
            {
                _srasCompleted = value; OnPropertyChanged();   
            }
        }
        public bool SRASError
        {
            get => _srasError;
            set
            {
                _srasError = value; OnPropertyChanged();
            }
        }
        public bool GripperActive
        {
            get => _gripperActive;
            set
            {
                _gripperActive = value; OnPropertyChanged();
            }
        }
        public bool R3DReadyToLoad
        {
            get => _r3dReadyToLoad;
            set {  _r3dReadyToLoad = value; OnPropertyChanged(); }
        }
        public bool R3DReadyToSRAS
        {
            get => _r3dReadyToSras;
            set
            {
                _r3dReadyToSras = value; OnPropertyChanged();
            }
        }
        public bool R3DIsOkay
        {
            get => _r3dIsOkay;
            set { _r3dIsOkay = value; OnPropertyChanged(); }
        }
        public TransferSystem() {
            // Constructor constructor what's your functor?
        }
        public List<string> enumeratePorts()
        {
            List<string> _displayPorts = new List<string>();
            // Stolen from stackoverflow question 2837985
            using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
            {
                // Super ugmo way to get serial port information from the windows registry
                foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                {
                    Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                    if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                        continue; // Skip all devices except device class "PORTS"

                    String s_Caption = i_Inst.GetPropertyValue("Caption").ToString();
                    String s_Manufact = i_Inst.GetPropertyValue("Manufacturer").ToString();
                    String s_DeviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                    String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + s_DeviceID + "\\Device Parameters";
                    String s_PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();

                    int s32_Pos = s_Caption.IndexOf(" (COM");
                    if (s32_Pos > 0) // remove COM port from description
                        s_Caption = s_Caption.Substring(0, s32_Pos);
                    string s_tmpName = s_DeviceID + " - " + s_PortName + " " + s_Caption;
                    if(s_DeviceID.IndexOf("VID") > 0)
                    {
                        // Extract out the Product and Vendor IDs for use in the output list
                        string s_PID = s_DeviceID.Substring(s_DeviceID.IndexOf("PID") + 4, 4);
                        string s_VID = s_DeviceID.Substring(s_DeviceID.IndexOf("VID") + 4, 4);
                        Debug.WriteLine("PID: " + s_PID + "\tVID: " + s_VID);
                        string _prettyPort = s_PortName + ": [" + s_VID + ":" + s_PID + "]";
                        if(s_VID.Equals("2939"))
                        {
                            _prettyPort = _prettyPort + ": Zaber ";
                            if(s_PID.Equals("495B"))
                            {
                                _prettyPort = _prettyPort + "X-MCB2";
                                
                            }
                        } 
                        
                        _displayPorts.Add(_prettyPort);
                    }
                    
                }
            }
            return _displayPorts;
        }
        public void ToggleGrip()
        {
            if(GripperActive == true)
            {
                this.SecondaryController.IO.SetDigitalOutput(1, false);
                GripperActive = false;
            } else
            {
                this.SecondaryController.IO.SetDigitalOutput(1, true);
                GripperActive = true;
            }
        }
        public void ToggleConnection(string _cPort)
        {
            if(this.IsConnected == false)
            {
                this.ControllerConnectionInstance = Zaber.Motion.Ascii.Connection.OpenSerialPort(_cPort, this.BaudRate, false);
                
                this.IsConnected = true;
                Debug.WriteLine("Transfer System Connected.");
                this.PrimaryController = this.ControllerConnectionInstance.GetDevice(1);
                this.PrimaryController.Identify();
                this.SecondaryController = this.ControllerConnectionInstance.GetDevice(2);
                this.SecondaryController.Identify();
                this.FoundAxes.Add(this.PrimaryController.GetAxis(1));
                this.FoundAxes.Add(this.PrimaryController.GetAxis(2));
                this.FoundAxes.Add(this.SecondaryController.GetAxis(1));
                this.PositionPollingTask = Task.Run(this.PositionUpdateWorker);
                foreach(Axis stage in this.FoundAxes)
                {
                    if(!stage.IsHomed())
                    {
                        stage.HomeAsync();
                        Debug.WriteLine("Homing Axis!");
                    } else
                    {
                        Debug.WriteLine("Axis is homed!");
                    }
                }

            } else if (this.IsConnected == true)
            {
                this.IsConnected = false;
                Thread.Sleep(50);
                this.ControllerConnectionInstance.Close();
                this.ControllerConnectionInstance?.Dispose();
                this.FoundAxes?.Clear();
                Debug.WriteLine("Transfer System Disconnected.");
            }
        }
        public void PositionUpdateWorker()
        {
            while(IsConnected)
            {
                this.UpdateStatus();
                Thread.Sleep(100);
            }
        }
        public void UpdateStatus()
        {
            // Query the motion controller for its current state and assign to the 
            // bound properties
            if(IsConnected == true)
            {
            ZaberOnePosition = this.FoundAxes[0].GetPosition(Zaber.Motion.Units.Length_Millimetres);
            ZaberTwoPosition = this.FoundAxes[1].GetPosition(Zaber.Motion.Units.Length_Millimetres);
            ZaberThreePosition = this.FoundAxes[2].GetPosition(Zaber.Motion.Units.Length_Millimetres);
            ZaberOneHome = this.FoundAxes[0].IsHomed();
            ZaberTwoHome = this.FoundAxes[1].IsHomed();
            ZaberThreeHome = this.FoundAxes[2].IsHomed();
            SRASReady = this.PrimaryController.IO.GetDigitalOutput(1);
            SRASClearToLoad = this.PrimaryController.IO.GetDigitalOutput(2);
            SRASCompleted = this.PrimaryController.IO.GetDigitalOutput(3);
            SRASError = this.PrimaryController.IO.GetDigitalOutput(4);
            GripperActive = this.SecondaryController.IO.GetDigitalOutput(1);
            R3DReadyToLoad = this.PrimaryController.IO.GetDigitalInput(1);
            R3DReadyToSRAS = this.PrimaryController.IO.GetDigitalInput(2);
            R3DIsOkay = this.PrimaryController.IO.GetDigitalInput(3);
            }

        }
        public void MoveAxis(int axisNo, decimal absPosition)
        {
            FoundAxes[axisNo].MoveAbsolute((double)absPosition, 
                Zaber.Motion.Units.Length_Millimetres);
            return;
        }
        public void SetOutputs(bool _o1, bool _o2, bool _o3, bool _o4, bool _grip)
        {
            bool[] outputarray = { _o1, _o2, _o3, _o4 };
            this.PrimaryController.IO.SetAllDigitalOutputs(outputarray);
            this.SecondaryController.IO.SetDigitalOutput(1, _grip);
        }
        public int InternalState
        {
            get => _intState;
            set
            {
                _intState = value; OnPropertyChanged();
            }
        }
        private int _intState = 0;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    
}
