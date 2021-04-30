using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VenturaSQLStudio.Validation.Validators;

namespace VenturaSQLStudio.Validation
{
    public class ValidationEngine
    {
        //public event Action<string> LogOutputEvent;

        private List<ValidatorBase> _validatorslist;

        private ObservableCollection<ValidationListItem> _validationmessages;

        private int _informationcount;
        private int _warningcount;
        private int _errorcount;

        public ValidationEngine()
        {
            _informationcount = 0;
            _warningcount = 0;
            _errorcount = 0;

            _validatorslist = new List<ValidatorBase>();
            _validationmessages = new ObservableCollection<ValidationListItem>();
        }

        public void AddValidator(ValidatorBase validator)
        {
            validator.SetValidationEngine(this);

            _validatorslist.Add(validator);
        }


        public bool Exec()
        {
            //AddMessage("", "", null, ValidationMessageKind.Information, $"Validation started on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}.");

            foreach (ValidatorBase validator in _validatorslist)
            {
                try
                {
                    validator.Validate();
                }
                catch (Exception ex)
                {
                    validator.AddError(ex.Message);
                }

            }

            //if (_errorcount == 0)
            //    AddMessage("", "", null, ValidationMessageKind.Information, $"Validation completed with no errors and {_warningcount} warning(s).");
            //else
            //    AddMessage("", "", null, ValidationMessageKind.Error, $"Validation failed with {_errorcount} error(s) and {_warningcount} warning(s).");

            if (_errorcount > 0)
                return false;

            return true;
        }

        public void AddMessage(string messagegroup, string refersTo, object refersToObject, ValidationMessageKind messagekind, string message)
        {
            _validationmessages.Add(new ValidationListItem(messagegroup, refersTo, refersToObject, messagekind, message));

            if (messagekind == ValidationMessageKind.Information)
                _informationcount++;
            else if (messagekind == ValidationMessageKind.Warning)
                _warningcount++;
            else if (messagekind == ValidationMessageKind.Error)
                _errorcount++;

        }

        public ObservableCollection<ValidationListItem> ValidationMessages
        {
            get { return _validationmessages; }
        }

        public int InformationCount
        {
            get { return _informationcount; }
        }

        public int WarningCount
        {
            get { return _warningcount; }
        }
        public int ErrorCount
        {
            get { return _errorcount; }
        }

        public void ShowMessagesWindow()
        {
            ValidationWindow window = new ValidationWindow(this);
            window.ShowDialog();
        }

    }

    public enum ValidationMessageKind
    {
        Information = 0,
        Warning = 10,
        Error = 20
    }



}
