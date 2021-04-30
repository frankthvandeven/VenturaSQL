using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio {
    internal class MasterTemplate
    {
        private const string CRLF = "\r\n";
        private const string TAB = "\t";
        private const string QUOTE = "\"";

        private DbConnection _sqlconnection;
        private RecordsetItem _recordsetItem;
        private DateTime _timestamp;
        private QueryInfo _queryinfo;


        internal MasterTemplate(DbConnection connection, RecordsetItem recordsetItem, DateTime timestamp)
        {
            _sqlconnection = connection;
            _recordsetItem = recordsetItem;
            _timestamp = timestamp;
            _queryinfo = QueryInfo.CreateInstance(recordsetItem);


        }

        /// <summary>
        /// Pregenerate() must be called (once) before calling Generate(). Generate() can be called multiple times.
        /// This method executes the Sql script and stores the Schema returned by ADO.NET.
        /// Also it verifies that all primary keys are present in the Schema returned. This is necessary for the generated Recordset
        /// to be updateable.
        /// </summary>
        internal void PreGenerate()
        {
        }

        internal string GenerateCSharp(string namespace_name, bool AdoDirectSwitch, VenturaPlatform generatortarget)
        {
            StringBuilder sb = new StringBuilder(20000);

            sb.Append("/*" + CRLF);

            sb.Append(TAB + $"Project file: \"{MainWindow.ViewModel.FileName}\"" + CRLF);

            sb.Append(TAB + $"Target platform: {generatortarget}" + CRLF);
            sb.Append(TAB + $"Generator version: {MainWindow.ViewModel.VenturaVersion.ToString(3)}" + CRLF);
            sb.Append(TAB + $"Generated on: {_timestamp.ToLongDateString()} at {_timestamp.ToLongTimeString()}" + CRLF);

            if (_recordsetItem.ImplementDatabinding == true && _recordsetItem.Resultsets.Count > 0)
                sb.Append(TAB + "At the bottom of this file you find a template for extending Recordsets with calculated columns for XAML data binding." + CRLF);

            sb.Append("*/" + CRLF);
            sb.Append("using VenturaSQL;" + CRLF);
            sb.Append("using System;" + CRLF);
            sb.Append("using System.Threading.Tasks;" + CRLF);

            if (_recordsetItem.Resultsets.Count > 0 || _recordsetItem.Parameters.Count > 0)
                sb.Append("using System.Data;" + CRLF);

            if (_recordsetItem.ImplementDatabinding == true && _recordsetItem.Resultsets.Count > 0)
                sb.Append("using System.ComponentModel;" + CRLF);

            sb.Append(CRLF);

            sb.Append("namespace " + namespace_name + CRLF + "{" + CRLF);

            if (_queryinfo.ResultSets.Count == 1) /* Generate a Recordset with Exec/Param/SqlScript/SaveChanges inside it */
            {
                LoaderTemplate loadertemplate = new LoaderTemplate(LoaderTemplateMode.InjectInsideRecordset, _recordsetItem, _timestamp, _queryinfo);
                ResultSetInfo info = _queryinfo.ResultSets[0];
                ResultsetItem item = _recordsetItem.Resultsets[0];
                RecordsetTemplate recordsettemplate = new RecordsetTemplate(_recordsetItem, item, info, _queryinfo);

                string resultset_typename = _recordsetItem.ClassName;
                string record_typename = $"{StudioGeneral.NewStripLast9(_recordsetItem.ClassName)}Record";

                recordsettemplate.GenerateCSharp(sb, AdoDirectSwitch, generatortarget, loadertemplate, resultset_typename, record_typename);
            }
            else
            {
                LoaderTemplate loadertemplate = new LoaderTemplate(LoaderTemplateMode.StandAlone, _recordsetItem, _timestamp, _queryinfo);
                loadertemplate.StandAloneLoaderClass(sb, AdoDirectSwitch, generatortarget);
            }

            sb.Append("}" + CRLF); // end of namespace

            if (_recordsetItem.ImplementDatabinding == true && _recordsetItem.Resultsets.Count > 0)
            {
                // GENERATE RECORDSET AND RECORD EXTENSION TEMPLATES
                sb.Append(CRLF);
                sb.Append("// The following commented out code is a template for implementing calculated columns." + CRLF);
                sb.Append("//" + CRLF);
                sb.Append("// How to guide: https://docs.sysdev.nl/CalculatedColumns.html" + CRLF);
                sb.Append(CRLF);
                sb.Append("/*" + CRLF);
                sb.Append("namespace " + namespace_name + CRLF + "{" + CRLF);

                if (_queryinfo.ResultSets.Count == 1)
                {
                    string record_typename = StudioGeneral.NewStripLast9(_recordsetItem.ClassName) + "Record";
                    GenerateRecordTemplate("", sb, record_typename);
                }
                else
                {
                    sb.Append(TAB + $"public partial class {_recordsetItem.ClassName}" + CRLF);
                    sb.Append(TAB + "{" + CRLF);

                    for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
                    {
                        string record_typename = $"Multi{_recordsetItem.Resultsets[i].ResultsetName}Record";
                        GenerateRecordTemplate(TAB, sb, record_typename);
                    }

                    sb.Append(TAB + "}" + CRLF);
                }

                sb.Append("}" + CRLF); // end of namespace
                sb.Append("*/" + CRLF);
            }

            return sb.ToString();
        }

        private void GenerateRecordTemplate(string PRE, StringBuilder sb, string record_classname)
        {
            sb.Append(PRE + TAB + $"public partial class {record_classname}" + CRLF);
            sb.Append(PRE + TAB + "{" + CRLF);

            sb.Append(PRE + TAB + TAB + "partial void OnAfterPropertyChanged(string propertyName)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "if (propertyName == \"FirstName\" || propertyName == \"LastName\")" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + "this.OnPropertyChanged(\"FirstNameLastName\");" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + "public string FirstNameLastName" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "get" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + "return this.FirstName + \" \" + this.LastName;" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF);

            sb.Append(PRE + TAB + "}" + CRLF + CRLF);
        }

        internal void PostGenerate()
        {
        }

    }
}
