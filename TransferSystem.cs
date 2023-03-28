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

namespace GrandUnifiedEngine
{
    internal class TransferSystem
    {
        public bool IsConnected = false;
        public int BaudRate = 115200;
        public string ComPort;

        public Connection ControllerConnectionInstance;
        public Task PositionPollingTask;
        Device PrimaryController;
        Device SecondaryController;
        List<Axis> FoundAxes = new List<Axis>();
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

        public void ToggleConnection(string _cPort)
        {
            if(this.IsConnected == false)
            {
                this.ControllerConnectionInstance = Zaber.Motion.Ascii.Connection.OpenSerialPort(_cPort, this.BaudRate, false);
                this.IsConnected = true;
                Debug.WriteLine("Transfer System Connected.");
                this.PrimaryController = this.ControllerConnectionInstance.GetDevice(1);
                this.PrimaryController.Identify();
                //this.SecondaryController = this.ControllerConnectionInstance.GetDevice(2);
                this.FoundAxes.Add(this.PrimaryController.GetAxis(1));
                
                Debug.WriteLine(this.FoundAxes[0].Identity.ToString());

            } else if (this.IsConnected == true)
            {
                this.ControllerConnectionInstance.Close();
                this.IsConnected = false;
                Debug.WriteLine("Transfer System Disconnected.");
            }
        }

        protected void PositionWorker()
        {

        }
    }
    
}
