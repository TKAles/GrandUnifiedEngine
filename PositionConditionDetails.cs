using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace GrandUnifiedEngine
{
    public class PositionConditionDetails : IEditableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        struct PositionData
        {
            internal string pFriendlyName;
            internal decimal xPos;
            internal decimal yPos;
            internal decimal zPos;
            internal bool iOne;
            internal bool iTwo;
            internal bool iThree;
            internal bool iFour;
            internal bool oOne;
            internal bool oTwo;
            internal bool oThree;
            internal bool oFour;
            internal bool magGrip;
            internal bool safeMLS;
            internal bool loadMLS;
        }
        PositionData currentPosData;
        PositionData backupPosData;

        bool _isBeingEdited = false;

        public string PositionFriendlyName
        {
            get => this.currentPosData.pFriendlyName;

            set { currentPosData.pFriendlyName = value; OnPropertyChanged(); }
        }
        public decimal XPos
        {
            get => this.currentPosData.xPos;
            set { this.currentPosData.xPos = value; OnPropertyChanged(); }
        }
        public decimal YPos
        {
            get => this.currentPosData.yPos;
            set { currentPosData.yPos = value; OnPropertyChanged(); }
        }
        public decimal ZPos
        {
            get => this.currentPosData.zPos;
            set { currentPosData.zPos = value; OnPropertyChanged(); }
        }

        public bool InputOne
        {
            get => this.currentPosData.iOne;
            set { currentPosData.iOne = value; OnPropertyChanged(); }
        }
        public bool InputTwo
        {
            get => this.currentPosData.iTwo;
            set { currentPosData.iTwo = value; OnPropertyChanged(); }
        }
        public bool InputThree
        {
            get => this.currentPosData.iThree;
            set { currentPosData.iThree = value; OnPropertyChanged(); }
        }
        public bool InputFour
        {
            get => this.currentPosData.iFour;
            set { currentPosData.iFour = value; OnPropertyChanged(); }
        }

        public bool OutputOne
        {
            get => this.currentPosData.oOne;
            set { currentPosData.oOne = value; OnPropertyChanged(); }
        }
        public bool OutputTwo
        {
            get => this.currentPosData.oTwo;
            set { currentPosData.oTwo = value; OnPropertyChanged(); }
        }
        public bool OutputThree
        {
            get => this.currentPosData.oThree;
            set { currentPosData.oThree = value; OnPropertyChanged(); }
        }
        public bool OutputFour
        {
            get => this.currentPosData.oFour;
            set { currentPosData.oFour = value; OnPropertyChanged(); }
        }

        public bool GripOn
        {
            get => currentPosData.magGrip;
            set { currentPosData.magGrip = value; OnPropertyChanged(); }
        }
        public bool SafeMLS
        {
            get => currentPosData.safeMLS;
            set
            {
                currentPosData.safeMLS = value; OnPropertyChanged();
            }
        }

        public bool LoadMLS
        {
            get => currentPosData.loadMLS;
            set { currentPosData.loadMLS = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            Debug.WriteLine("OnPropertyChanged called for property " + name);
        }

        public PositionConditionDetails() {

        }

        void IEditableObject.BeginEdit()
        {
            if(!_isBeingEdited)
            {
                _isBeingEdited = true;
                this.backupPosData = this.currentPosData;
            }

        }

        void IEditableObject.EndEdit()
        { 
            if(_isBeingEdited)
            {
                _isBeingEdited = false;
                backupPosData = new PositionData();
            }
        } 

        void IEditableObject.CancelEdit()
        {
            if(_isBeingEdited)
            {
                this.currentPosData = backupPosData;
                _isBeingEdited = false;
            }
        }

    }
}
