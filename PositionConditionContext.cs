using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrandUnifiedEngine
{
    class PositionConditionContext : INotifyPropertyChanged
    {
        PositionConditionDetails _editablePosition;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<PositionConditionDetails> SystemPositions { get; set; }
        public PositionConditionDetails EditablePosition
        {
            get { return _editablePosition; }
            set { _editablePosition = value; OnPropertyChanged(); }
        }

        public PositionConditionContext() {
            SystemPositions = new ObservableCollection<PositionConditionDetails>();
            EditablePosition = new PositionConditionDetails();
        }

        public void AddPosition()
        {
            SystemPositions.Add(EditablePosition);
            EditablePosition = new PositionConditionDetails();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
