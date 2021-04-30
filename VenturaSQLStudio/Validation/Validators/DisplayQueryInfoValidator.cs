using System.Collections.Generic;
using System.IO;
using System.Text;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.Validation.Validators
{
    /// <summary>
    /// This Validator does not validate at all. It just adds QueryInfo information to the validation window.
    /// </summary>
    public class DisplayQueryInfoValidator : ValidatorBase
    {
        private RecordsetItem _recordsetitem;

        public DisplayQueryInfoValidator(RecordsetItem recordsetitem) : base("QueryInfo", recordsetitem.ClassName, recordsetitem)
        {
            _recordsetitem = recordsetitem;
        }

        public override void Validate()
        {
            if (_recordsetitem.UpdateableTableName.Length == 0)
                return; // nothing to validate

            if (SqlScriptIsEmpty() == true)
                return;

            SqlConnection connection = null;
            QueryInfo queryinfo = null;
            
            try
            {
                connection = Database.OpenSQLConnection(new Connector("temp", ConnectorMode.Ado, MainWindow.CurrentProject.ConnectString));
                
                queryinfo = new QXXXueryInfo(connection, _recordsetitem.SqlScript);

                TableInfo info = queryinfo.TableInfo.Find(a => a.FullTableName == _recordsetitem.UpdateableTableName);

                if (info == null)
                    return;

                AddInfo($"PrimaryKeys in {_recordsetitem.UpdateableTableName}: {ReadableColumnInfoString(info.PrimaryKeys)}");
                AddInfo($"OtherColumns in {_recordsetitem.UpdateableTableName}: {ReadableColumnInfoString(info.OtherColumns)}");
                AddInfo($"MatchingPriKeys in SQL statement: {ReadableColumnInfoString(info.MatchingPriKeys)}");
                AddInfo($"MissingPriKeys in SQL statement: {ReadableColumnInfoString(info.MissingPriKeys)}");
                AddInfo($"MatchingOtherColumns in SQL statement: {ReadableColumnInfoString(info.MatchingOtherColumns)}");
                AddInfo($"MissingOtherColumns in SQL statement: {ReadableColumnInfoString(info.MissingOtherColumns)}");

                connection.Close();
            }
            finally
            {
                General.SmartClose(connection);
            }

        }

        private string ReadableColumnInfoString(List<string> list)
        {
            if (list.Count == 0)
                return "(none)";

            StringBuilder sb = new StringBuilder();

            int count = 0;

            foreach (string name in list)
            {
                count++;

                if (count > 1 && count < list.Count)
                    sb.Append(", ");
                else if (count == list.Count && list.Count > 1)
                    sb.Append(" and ");

                sb.Append(name); ;
            }

            return sb.ToString();
        }

        private bool SqlScriptIsEmpty()
        {
            StringReader strReader = new StringReader(_recordsetitem.SqlScript);
            int characters = 0;

            while (true)
            {
                string line = strReader.ReadLine();

                if (line == null) break;

                characters += line.Trim().Length;
            }

            return (characters == 0);
        }


    }
}
