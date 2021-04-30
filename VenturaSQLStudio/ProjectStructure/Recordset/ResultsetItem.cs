using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio {
    public class ResultsetItem : ViewModelBase
    {
        private int _resultsetnumber;
        private bool _enabled = true;
        private string _resultsetname;
        private TableName _updateableTableName;

        private ObservableCollection<ReferencedTableItem> _referenced_tables_list;

        private Project _owningproject;

        public ResultsetItem(Project owningproject, int resultsetnumber, string resultsetname)
        {
            _owningproject = owningproject;

            _resultsetnumber = resultsetnumber; // The first one has number 1.
            _resultsetname = resultsetname;
            _updateableTableName = null;

            _referenced_tables_list = new ObservableCollection<ReferencedTableItem>();

            _referenced_tables_list.Add(new ReferencedTableItem
            {
                DataObject = null,
                Invalid = false
            });

        }

        public ResultsetItem Clone()
        {
            ResultsetItem temp = new ResultsetItem(_owningproject, _resultsetnumber, _resultsetname);
            temp.Enabled = this.Enabled;
            temp.UpdateableTableName = _updateableTableName;

            foreach (ReferencedTableItem cb_item in _referenced_tables_list)
                if (cb_item.DataObject != null)
                    temp.ReferencedTablesList.Add(cb_item.Clone());

            return temp;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                NotifyPropertyChanged("Enabled");
                NotifyPropertyChanged("TextDecorationForListview");

                _owningproject?.SetModified();
            }
        }

        public TextDecorationCollection TextDecorationForListview
        {
            get
            {
                if (_enabled == true)
                    return null;
                else
                    return TextDecorations.Strikethrough;
            }
        }


        public string ResultsetInternalName
        {
            get
            {
                return $"Resultset {_resultsetnumber}";
            }
        }

        /// <summary>
        /// This name + "Recordset" = full recordsetname, in multiple resultset recordset.
        /// </summary>
        public string ResultsetName
        {
            get { return _resultsetname; }
            set
            {
                if (_resultsetname == value)
                    return;

                _resultsetname = value;

                NotifyPropertyChanged("ResultsetName");
                NotifyPropertyChanged("DisplayName");

                _owningproject?.SetModified();
            }
        }

        public int ResultsetNumber
        {
            get { return _resultsetnumber; }
        }

        /// <summary>
        /// The name to display in the dropdown combobox (data binding).
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (_resultsetname.Trim().Length == 0)
                    return $"Resultset {_resultsetnumber}";
                else
                    return $"Resultset {_resultsetnumber} ({_resultsetname })";
            }
        }


        /// <summary>
        /// This is the dropdown-combobox. The selecteditem is the UpdateableTableName.
        /// </summary>
        public ObservableCollection<ReferencedTableItem> ReferencedTablesList
        {
            get { return _referenced_tables_list; }
        }

        public string GetDefaultName()
        {
            return $"Resultset{_resultsetnumber}";
        }

        public void GenerateReferencedTablesList(ResultSetInfo resultsetinfo)
        {
            Action action = () =>
            {
                // Clear all except the first item and the updateabletablename.
                // The first item is "", meaning read-only. That item is always in the list.
                for (int x = _referenced_tables_list.Count - 1; x > 0; x--)
                {
                    if (_referenced_tables_list[x].DataObject != _updateableTableName)
                        _referenced_tables_list.RemoveAt(x);
                }

                bool updateabletable_present = _referenced_tables_list.Count(z => z.DataObject == _updateableTableName) != 0;

                if (updateabletable_present == false)
                {
                    _referenced_tables_list.Add(new ReferencedTableItem
                    {
                        DataObject = _updateableTableName,
                        Invalid = false
                    });
                }

                if (resultsetinfo == null) // There is no list of table names from the database.
                    return;

                foreach (TableInfo tableinfo in resultsetinfo.Tables)
                {
                    ReferencedTableItem cb_item_found = _referenced_tables_list.FirstOrDefault(z => z.DataObject == tableinfo.TableName);

                    if (cb_item_found == null)
                    {
                        _referenced_tables_list.Add(new ReferencedTableItem
                        {
                            DataObject = tableinfo.TableName,
                            Invalid = false
                        });
                    }
                }

                // Mark any table name that is not referred to in the sql-script-resultset as invalid.
                for (int i = 1; i < _referenced_tables_list.Count; i++)
                {
                    ReferencedTableItem cbitem = _referenced_tables_list[i];

                    TableInfo found = resultsetinfo.Tables.Find(a => a.TableName == cbitem.DataObject);

                    cbitem.Invalid = (found == null);
                }


            }; // end of Action block

            Application.Current.Dispatcher.Invoke(action);
        }

        public TableName UpdateableTableName
        {
            get { return _updateableTableName; }
            set
            {
                if (_updateableTableName == value)
                    return;

                _updateableTableName = value;

                NotifyPropertyChanged("UpdateableTableName");
                NotifyPropertyChanged("UpdateableTableNameAsRTI");

                _owningproject?.SetModified();
            }
        }

        public ReferencedTableItem UpdateableTableNameAsRTI
        {
            get
            {
                ReferencedTableItem rti = _referenced_tables_list.FirstOrDefault(a => a.DataObject == _updateableTableName);

                if (rti is null)
                    throw new Exception("rti null");

                return rti;
            }
            set
            {
                if (value == null)
                {
                    this.UpdateableTableName = null;
                    return;
                }


                TableName tn = value.DataObject;

                this.UpdateableTableName = tn;
            }
        }

    }
}
