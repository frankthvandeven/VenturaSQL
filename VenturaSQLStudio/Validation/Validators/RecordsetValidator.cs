using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.Validation.Validators
{
    public class RecordsetValidator : ValidatorBase
    {
        public enum RecordsetValidationMode
        {
            Full,
            SqlScriptNotEmptyOnly
        }

        private Project _project;
        private RecordsetItem _recordsetitem;
        private RecordsetValidationMode _validationmode;
        private AdoConnector _connector;

        public RecordsetValidator(Project project, RecordsetItem recordsetitem, RecordsetValidationMode validationmode) : base("Recordset", recordsetitem.ClassName, recordsetitem)
        {
            _project = project;
            _recordsetitem = recordsetitem;
            _validationmode = validationmode;

            _connector = AdoConnectorHelper.Create(project.ProviderInvariantName, null);
        }

        public override void Validate()
        {
            if (SqlScriptIsEmpty() == true)
            {
                AddError("There is no SQL script.");
                return;
            }

            // Check for duplicate Parameter names
            IEnumerable<string> duplicateItems = from x in _recordsetitem.Parameters
                                                 group x by x.Name into grouped
                                                 where grouped.Count() > 1
                                                 select grouped.Key;

            int duplicate_count = 0;

            foreach (string columnname in duplicateItems)
            {
                duplicate_count++;
                AddError($"Duplicate parameter name '{columnname}' detected.");

            }

            if (duplicate_count > 0)
                return; // abort right away, as queryinfo will fail

            QueryInfo queryinfo = QueryInfo.CreateInstance(_recordsetitem);

            if (_validationmode == RecordsetValidationMode.SqlScriptNotEmptyOnly)
                return;

            if (_recordsetitem.ClassName.EndsWith("Recordset") == false)
                AddError($"The name of the Recordset must end with the word 'Recordset' (case-sensitive).");

            // Check each individual parameter.
            foreach (ParameterItem parameteritem in _recordsetitem.Parameters)
                ValidateSingleParameter(parameteritem);

            ValidateQueryInfo(queryinfo);

            int enabled_resultset_count = 0;

            for (int i = 0; i < _recordsetitem.Resultsets.Count; i++)
            {
                ResultsetItem resultsetitem = _recordsetitem.Resultsets[i];

                if (resultsetitem.Enabled == false)
                    break;

                enabled_resultset_count++;
            }

            // Resultsets list length must be at least the number of resultsets returned by Ado.
            if (enabled_resultset_count < queryinfo.ResultSets.Count)
                AddError($"Generator needs {queryinfo.ResultSets.Count} (enabled) resultset definitions but there are only {enabled_resultset_count} definitions present. Open Recordset editor and click the [Collect] button to create (or enable) the missing resultset definitions.");
            else
            {
                for (int i = 0; i < queryinfo.ResultSets.Count; i++)
                {
                    ResultSetInfo resultsetinfo = queryinfo.ResultSets[i];
                    ResultsetItem resultsetitem = _recordsetitem.Resultsets[i];
                    ValidateResultsetItem(i + 1, resultsetitem, resultsetinfo);
                }
            }

            foreach (UDCItem udcitem in _recordsetitem.UserDefinedColumns)
            {
                if (udcitem.ColumnName.Length == 0)
                    AddError("There is an user defined column with an empty name.");
                else if (udcitem.ColumnName.Contains(" ") == true)
                    AddError("There is an user defined column with spaces in the name.");
                else
                {
                    bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(udcitem.ColumnName);

                    if (valididentifier == false)
                    {
                        AddError($"User defined column name '{udcitem.ColumnName}' is not a valid C# identifier.");
                    }
                }
            }

            duplicateItems = from x in _recordsetitem.UserDefinedColumns
                             group x by x.ColumnName into grouped
                             where grouped.Count() > 1
                             select grouped.Key;

            foreach (string columnname in duplicateItems)
                AddError($"Duplicate user defined column name '{columnname }' detected.");

            if (_recordsetitem.RowloadIncremental == true)
                ValidateIncremental(queryinfo);

            int count = _recordsetitem.OutputProjects.Count(z => z.Selected == true);

            if (count == 0)
                AddWarning("No output projects checked.");

        }

        private void ValidateIncremental(QueryInfo queryinfo)
        {
            if (queryinfo.ResultSets.Count != 1)
            {
                string count = queryinfo.ResultSets.Count == 0 ? "no" : queryinfo.ResultSets.Count.ToString();

                AddError("A recordset that supports the incremental loading of rows must have SQL script that produces " +
                    $"exactly 1 resultset, but it produces {count} resultsets instead.");
            }

            if (this.SqlScriptContainsRowOffsetAndRowLimitParameters() == false)
            {
                var pref = MainWindow.ViewModel.CurrentProject.ParameterPrefix;

                AddError($"The recordset supports incremental loading but does not refer to (automatic) parameters '{pref}RowOffset' and '{pref}RowLimit' " +
                    "in its SQL script. The SQL statement doesn't properly support incremental loading.");
            }

        }

        private bool SqlScriptContainsRowOffsetAndRowLimitParameters()
        {
            var script = _recordsetitem.SqlScript;
            var pref = MainWindow.ViewModel.CurrentProject.ParameterPrefix;

            string par_offset = $"{pref}RowOffset";
            string par_limit = $"{pref}RowLimit";

            bool contains_offset = script.IndexOf($"{pref}RowOffset", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool contains_limit = script.IndexOf($"{pref}RowLimit", System.StringComparison.InvariantCultureIgnoreCase) >= 0;

            return (contains_offset == true && contains_limit == true);
        }


        private void ValidateSingleParameter(ParameterItem parameteritem)
        {
            string parameter_prefix = _connector.ParameterPrefix.ToString();

            string csharp_variable_name = parameteritem.Name.Substring(1);

            if (csharp_variable_name.Length == 0)
                AddError("There is a SQL parameter with an empty name.");
            else if (csharp_variable_name.Contains(" ") == true)
                AddError("There is a SQL parameter with spaces in the name.");
            else
            {
                bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(csharp_variable_name);

                if (valididentifier == false)
                    AddError($"SQL parameter '{csharp_variable_name}' is not a valid C# identifier.");
            }

            if (parameteritem.Input == false && parameteritem.Output == false)
                AddError($"Neither Input nor Output selected for parameter {parameteritem.Name}.");

        }

        private void ValidateQueryInfo(QueryInfo queryinfo)
        {
            for (int r = 0; r < queryinfo.ResultSets.Count; r++)
            {
                ColumnArrayBuilder builder = new ColumnArrayBuilder();
                builder.Add(queryinfo.ResultSets[r], null);

                VenturaSchema schema = new VenturaSchema(builder);

                for (int c = 0; c < schema.Count; c++)
                {
                    VenturaColumn column = schema[c];

                    if (column.ColumnName == "") // Column without a name. Normal in expression like SUM(..)
                        AddError($"Column '{(c + 1)}', of result set {r + 1}, has no name. Use the \"AS\" clause to name the column. For example SELECT SUM(Total) AS 'Total'");

                } // column loop

            } // resultset loop

        }

        private void ValidateResultsetItem(int resultset_index, ResultsetItem resultsetitem, ResultSetInfo resultsetinfo)
        {
            if (resultsetitem.ResultsetName.Contains(" ") == true)
                AddError($"Resultset definition {resultset_index} has spaces in the the resultset name.");
            else
            {
                if (resultsetitem.ResultsetName.StartsWith("Resultset") == false)
                    AddError($"The property name for Resultset definition {resultset_index} must start with the word 'Resultset' (case-sensitive).");

                bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(resultsetitem.ResultsetName);

                if (valididentifier == false)
                {
                    AddError($"Resultset definition {resultset_index} name '{resultsetitem.ResultsetName}' is not a valid C# identifier.");
                }
            }

            TableName updateabletablename = resultsetitem.UpdateableTableName;

            if (updateabletablename == null)
                return; // nothing to validate

            TableInfo info = resultsetinfo.Tables.Find(a => a.TableName == resultsetitem.UpdateableTableName);

            if (info == null)
            {
                AddError($"Resultset definition {resultset_index}, selected updateable table {updateabletablename} is not referenced in the SQL statement.");
                return;
            }

            if (info.PrimaryKeys.Count == 0)
            {
                AddError($"Resultset definition {resultset_index}, selected updateable table {updateabletablename} has no primary keys. A Recordset cannot update a table without primary keys.");
                return;
            }

            if (info.MissingPriKeys.Count > 0)
            {
                AddError($"Resultset definition {resultset_index}, all the primary key columns for updateable table {updateabletablename} must be referenced in the SQL statement. The following columns are missing: {ReadableColumnInfoString(info.MissingPriKeys)}.");
                return;
            }

            if (info.MatchingOtherColumns.Count == 0 && info.MissingOtherColumns.Count > 0)
            {
                AddWarning($"Resultset definition {resultset_index}, contains all primary keys for updateable table {updateabletablename}, but no other columns for that table. Is this intentional?");
            }

        }

        private string ReadableColumnInfoString(List<string> list)
        {
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
