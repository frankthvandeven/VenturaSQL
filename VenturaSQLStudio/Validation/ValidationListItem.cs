namespace VenturaSQLStudio.Validation
{
    public class ValidationListItem
    {
        private string _messagegroup;
        private string _refersTo;
        private object _refersToObject;
        private ValidationMessageKind _messagekind;
        private string _message;

        public ValidationListItem(string messagegroup, string refersTo, object refersToObject, ValidationMessageKind messagekind, string message)
        {
            _messagegroup = messagegroup;
            _refersTo = refersTo;
            _refersToObject = refersToObject;
            _messagekind = messagekind;
            _message = message;
        }

        public string Group
        {
            get { return _messagegroup; }
        }

        /// <summary>
        /// What user-visible ID refers this message to. In the case of a Recordset this would be the Recordset.ClassName.
        /// </summary>
        public string RefersTo
        {
            get { return _refersTo; }
        }

        /// <summary>
        /// What (alive) object does this message refer to? In the case of a Recordset this would be a RecordsetItem reference.
        /// </summary>
        public object RefersToObject
        {
            get { return _refersToObject; }
        }

        public ValidationMessageKind Kind
        {
            get { return _messagekind; }
        }

        public string KindAsImage
        {
            get
            {
                if (_messagekind == ValidationMessageKind.Information)
                    return "/VenturaSQLStudio;component/Assets/validation_info.png";
                else if (_messagekind == ValidationMessageKind.Warning)
                    return "/VenturaSQLStudio;component/Assets/validation_warning.png";
                else
                    return "/VenturaSQLStudio;component/Assets/validation_error.png";
            }
        }

        public string Message
        {
            get { return _message; }
        }

    }
}
