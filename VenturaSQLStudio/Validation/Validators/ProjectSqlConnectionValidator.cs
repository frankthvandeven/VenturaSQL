using System;
using System.Data.Common;
using VenturaSQL;

namespace VenturaSQLStudio.Validation.Validators
{
    public class ProjectSqlConnectionValidator : ValidatorBase
    {
        private Project _project;

        public ProjectSqlConnectionValidator(Project project) : base("ProjectSql", project.MacroConnectionString, project)
        {
            _project = project;
        }

        public override void Validate()
        {

            if (_project.MacroConnectionString.Trim().Length == 0)
                this.AddError("The connection string is empty");
            else
            {
                try
                {
                    /* there is a connectstring, so check it against the database server */
                    AdoConnector connector = AdoConnectorHelper.Create(_project.ProviderInvariantName, _project.MacroConnectionString);
                    DbConnection connection = connector.OpenConnection();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    this.AddError("Sql connection string failed. " + ex.Message);
                }

            }


        }
    }
}
