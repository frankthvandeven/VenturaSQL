using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Validation.Validators
{
    public abstract class ValidatorBase
    {
        private ValidationEngine _validationengine;
        private string _validationgroup;
        private string _refersTo;
        private object _refersToObject;

        public ValidatorBase(string validationgroup, string refersTo, object refersToObject)
        {
            _validationgroup = validationgroup;
            _refersTo = refersTo;
            _refersToObject = refersToObject;
        }

        public void SetValidationEngine(ValidationEngine validationengine)
        {
            _validationengine = validationengine;
        }

        public abstract void Validate();

        public void AddInfo(string message)
        {
            _validationengine.AddMessage(_validationgroup, _refersTo, _refersToObject, ValidationMessageKind.Information, message);
        }

        public void AddWarning(string message)
        {
            _validationengine.AddMessage(_validationgroup, _refersTo, _refersToObject, ValidationMessageKind.Warning, message);
        }

        public void AddError(string message)
        {
            _validationengine.AddMessage(_validationgroup, _refersTo, _refersToObject, ValidationMessageKind.Error, message);
        }

    }
}
