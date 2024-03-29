﻿using System;
using System.Data;
using System.Data.Common;
using System.Text;
using VenturaSQL;

namespace VenturaSQLStudio
{
    public class ParameterItem : ViewModelBase
    {
        private string _name = "";

        private string _fulltypename = "System.Int32";

        private bool _input = true;
        private bool _output = false;

        private string _designvalue = "";

        private bool _set_dbtype = false;
        private bool _set_length = false;
        private bool _set_precision = false;
        private bool _set_scale = false;

        private string _dbtype_string = "DbType.Int32";
        private int _length = 0;
        private byte _precision = 0;
        private byte _scale = 0;

        private Project _owningproject;

        public ParameterItem(Project owningproject)
        {
            _owningproject = owningproject;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;

                _name = value;

                NotifyPropertyChanged("Name");

                _owningproject?.SetModified();
            }
        }

        public string DbTypeString
        {
            get { return _dbtype_string; }
            set
            {
                if (_dbtype_string == value)
                    return;

                _dbtype_string = value;

                _owningproject?.SetModified();

                NotifyPropertyChanged("DbTypeString");
                NotifyPropertyChanged("ShortDbTypeString");
            }
        }

        public string ShortDbTypeString
        {
            get
            {
                int index = _dbtype_string.LastIndexOf('.');

                string txt = _dbtype_string.Substring(index + 1);

                return txt;
            }
        }


        public string FullTypename
        {
            get { return _fulltypename; }
            set
            {
                if (_fulltypename == value)
                    return;

                _fulltypename = value;

                _owningproject?.SetModified();

                NotifyPropertyChanged("FullTypename");
                NotifyPropertyChanged("FullTypenameInfo");
            }
        }

        /// <summary>
        /// For ListView
        /// </summary>
        public string FullTypenameInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                int index = _fulltypename.LastIndexOf('.');

                sb.Append(_fulltypename.Substring(index + 1));

                if (TypeTools.TryConvertToCSharpTypeName(_fulltypename, out string csharp_name))
                    sb.Append(" (" + csharp_name + ")");

                return sb.ToString();
            }
        }

        public bool Input
        {
            get { return _input; }
            set
            {
                if (_input == value)
                    return;

                _input = value;

                NotifyPropertyChanged("Input");

                _owningproject?.SetModified();
            }
        }

        public bool Output
        {
            get { return _output; }
            set
            {
                if (_output == value)
                    return;

                _output = value;

                NotifyPropertyChanged("Output");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Used by the code-generator
        /// </summary>
        public VenturaSqlColumn AsVenturaSqlColumn()
        {
            if (_name.Length == 0)
                throw new VenturaSqlException("Name can not be empty when calling AsVenturaSqlColumn.");

            VenturaSqlColumn column = new VenturaSqlColumn(_name, _fulltypename, true);
            column.DbType = TypeTools.StringToEnum<DbType>(_dbtype_string);
            column.Input = _input;
            column.Output = _output;

            // At this point ColumnSize has the default value 0.

            if (_set_length == true)
                column.ColumnSize = _length;

            if (_set_precision == true)
                column.NumericPrecision = _precision;

            if (_set_scale == true)
                column.NumericScale = _scale;

            return column;
        }

        public ParameterDirection AsAdoDirection
        {
            get
            {
                if (_input == true && _output == true)
                    return ParameterDirection.InputOutput;

                if (_input == false && _output == true)
                    return ParameterDirection.Output;

                return ParameterDirection.Input;
            }
        }

        public string DesignValue
        {
            get { return _designvalue; }
            set
            {
                if (_designvalue == value)
                    return;

                _designvalue = value;

                NotifyPropertyChanged("DesignValue");

                _owningproject?.SetModified();
            }
        }

        public DbParameter CreateDesignValueDbParameter(AdoConnector connector)
        {
            DbParameter db_parameter = connector.CreateParameter(_name);
            Type type = TypeTools.GetType(_fulltypename);
            string dv = _designvalue.Trim();

            /*
             *   Setup the DbParameter.
             * 
             *   Did not set db_parameter.Direction. I assume it is always direction input by default.
             */

            db_parameter.DbType = TypeTools.StringToEnum<DbType>(_dbtype_string);

            if (_set_length == true)
                db_parameter.Size = _length;

            if (_set_precision == true)
                db_parameter.Precision = _precision;

            if (_set_scale == true)
                db_parameter.Scale = _scale;

            /*
             *   Set the Value by parsing the DesignValue string
             */

            if (dv == "" || dv == "null")
            {
                db_parameter.Value = DBNull.Value;
                return db_parameter;
            }

            if (type == typeof(Boolean))
            {
                if (dv == "1")
                    db_parameter.Value = (bool)true;
                else if (dv == "0")
                    db_parameter.Value = (bool)false;
                else
                    db_parameter.Value = Boolean.Parse(dv);  // Convert.ChangeType(_designvalue, type);
            }
            else if (type == typeof(Byte))
                db_parameter.Value = Byte.Parse(dv);
            else if (type == typeof(DateTime))
                db_parameter.Value = DateTime.Parse(dv);
            else if (type == typeof(Decimal))
                db_parameter.Value = Decimal.Parse(dv);
            else if (type == typeof(Single))
                db_parameter.Value = Single.Parse(dv);
            else if (type == typeof(Double))
                db_parameter.Value = Double.Parse(dv);
            else if (type == typeof(Int16))
                db_parameter.Value = Int16.Parse(dv);
            else if (type == typeof(Int32))
                db_parameter.Value = Int32.Parse(dv);
            else if (type == typeof(Int64))
                db_parameter.Value = Int64.Parse(dv);
            else if (type == typeof(String))
                db_parameter.Value = dv;
            else if (type == typeof(Guid))
                db_parameter.Value = Guid.Parse(dv);
            else if (type == typeof(byte[]))
                db_parameter.Value = DBNull.Value;
            else if (type == typeof(Object))
                db_parameter.Value = DBNull.Value;
            else if (type == typeof(TimeSpan))
                db_parameter.Value = TimeSpan.Parse(dv);
            else if (type == typeof(DateTimeOffset))
                db_parameter.Value = DateTimeOffset.Parse(dv);
            else
                throw new InvalidOperationException($"CreateDesignValueDbParameter doesn't know how to convert string to {type.FullName}. Please contact support.");

            return db_parameter;
        }

        public bool SetDbType
        {
            get { return _set_dbtype; }
            set
            {
                if (_set_dbtype == value)
                    return;

                _set_dbtype = value;

                NotifyPropertyChanged("SetDbType");

                _owningproject?.SetModified();
            }
        }

        public bool SetLength
        {
            get { return _set_length; }
            set
            {
                if (_set_length == value)
                    return;

                _set_length = value;

                NotifyPropertyChanged("SetLength");

                _owningproject?.SetModified();
            }
        }

        public bool SetPrecision
        {
            get { return _set_precision; }
            set
            {
                if (_set_precision == value)
                    return;

                _set_precision = value;

                NotifyPropertyChanged("SetPrecision");

                _owningproject?.SetModified();
            }
        }
        public bool SetScale
        {
            get { return _set_scale; }
            set
            {
                if (_set_scale == value)
                    return;

                _set_scale = value;

                NotifyPropertyChanged("SetScale");

                _owningproject?.SetModified();
            }
        }


        public int Length
        {
            get { return _length; }
            set
            {
                if (_length == value)
                    return;

                _length = value;

                NotifyPropertyChanged("Length");

                _owningproject?.SetModified();
            }
        }

        public byte Precision
        {
            get { return _precision; }
            set
            {
                if (_precision == value)
                    return;

                _precision = value;

                NotifyPropertyChanged("Precision");

                _owningproject?.SetModified();
            }
        }
        public byte Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value)
                    return;

                _scale = value;

                NotifyPropertyChanged("Scale");

                _owningproject?.SetModified();
            }
        }

        public ParameterItem Clone()
        {
            ParameterItem temp = new ParameterItem(_owningproject);
            temp.Name = this.Name;
            temp.DbTypeString = this.DbTypeString;
            temp.FullTypename = this.FullTypename;
            temp.Input = this.Input;
            temp.Output = this.Output;
            temp.DesignValue = this.DesignValue;
            temp.SetDbType = this.SetDbType;
            temp.SetLength = this.SetLength;
            temp.SetPrecision = this.SetPrecision;
            temp.SetScale = this.SetScale;
            temp.Length = this.Length;
            temp.Precision = this.Precision;
            temp.Scale = this.Scale;
            return temp;
        }

    }
}
