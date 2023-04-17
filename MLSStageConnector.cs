using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Thorlabs.MotionControl.DeviceSupport;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;
using System.Threading;

namespace GrandUnifiedEngine
{
    class MLSStageConnector : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _sn;
        private bool _isConnected;
        private decimal _currentXPos;
        private decimal _currentYPos;
        private decimal _currentXVel;
        private decimal _currentYVel;
         
        public string SerialNumber
        {
            get => _sn;
            set
            {
                _sn = value; OnPropertyChanged();
            }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value; OnPropertyChanged(); 
            }
        }
        public decimal CurrentXVel
        {
            get => _currentXVel;
            set
            { _currentXVel = value; OnPropertyChanged(); }
        }
        public decimal CurrentYVel
        {
            get => _currentYVel;
            set
            {
                _currentYVel = value; OnPropertyChanged();
            }
        }

        public decimal CurrentXPos
        {
            get => _currentXPos;
            set
            {
                _currentXPos = value; OnPropertyChanged(); 
            }
        }

        public decimal CurrentYPos
        {
            get => _currentYPos;
            set
            {
                _currentYPos = value; OnPropertyChanged();
            }
        }

        public BenchtopBrushlessMotor BBDMotorController;
        public BrushlessMotorChannel MLSXAxis;
        public BrushlessMotorChannel MLSYAxis;

        public MLSStageConnector()
        {

        }

        public void ToggleConnection()
        {
            if(IsConnected == false)
            {
                IsConnected = true;
                DeviceManagerCLI.Initialize();
                DeviceManagerCLI.BuildDeviceList();
                if(SerialNumber != null)
                {
                    BBDMotorController = BenchtopBrushlessMotor.CreateBenchtopBrushlessMotor(SerialNumber);
                    BBDMotorController.Connect(SerialNumber);
                    MLSXAxis = BBDMotorController.GetChannel(1);
                    MLSYAxis = BBDMotorController.GetChannel(2);
                    MLSXAxis.EnableDevice();
                    MLSYAxis.EnableDevice();
                    Thread.Sleep(100);
                    MLSXAxis.Home(5000);
                    MLSYAxis.Home(5000);
                }

            } else if (IsConnected == true)
            {
                BBDMotorController.DisconnectTidyUp();
                IsConnected = false;
            }
        }

        public void CommandMove(decimal _tgtX, decimal _tgtY)
        {

        }

        public void PositionUpdateWorker()
        {

        }

        public void ToggleUpdateWorkerTask()
        {

        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
