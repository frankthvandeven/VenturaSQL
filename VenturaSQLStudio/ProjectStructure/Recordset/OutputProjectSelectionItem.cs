using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio {
    public class OutputProjectSelectionItem : ViewModelBase
    {
        private int _arrayindex; // position in list
        private bool _selected;

        private Project _owningproject;

        public OutputProjectSelectionItem(Project owningproject, int arrayindex, bool selected)
        {
            _owningproject = owningproject;
            _arrayindex = arrayindex;
            _selected = selected;

            //MainWindow.CurrentProject.VisualStudioProjects[arrayindex].PropertyChanged += OutputProjectSelectionItem_PropertyChanged;
         }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value)
                    return;

                _selected = value;

                NotifyPropertyChanged("Selected");

                _owningproject?.SetModified();
            }
        }

        public string ProjectFileInfo
        {
            get { return MainWindow.ViewModel.CurrentProject.VisualStudioProjects[_arrayindex].ProjectFileInfo; }
        }

    }
}
