using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Validation.Validators
{
    public class ProjectSettingsValidator : ValidatorBase
    {
        private Project _project;

        public ProjectSettingsValidator(Project project) : base("Project", "", project)
        {
            _project = project;

        }

        public override void Validate()
        {
            int enabledcount = _project.VisualStudioProjects.Count(z => z.ProjectEnabled == true);

            if (enabledcount == 0)
                AddError("All C# output projects in this project are disabled. Nothing to do.");

            for (int i = 0; i < _project.VisualStudioProjects.Count; i++)
            {
                VisualStudioProjectItem vs_projectitem = _project.VisualStudioProjects[i];

                VerifyProjectFilename(i + 1, vs_projectitem.ProjectEnabled, vs_projectitem.OutputProjectFilename);
            }

        }

        private void VerifyProjectFilename(int number, bool enabled, string relative_filename)
        {
            if (enabled == false)
                return;

            if (relative_filename != relative_filename.Trim())
            {
                AddError($"The project file for project {number} contains leading or trailing spaces.");
                return;
            }

            if (relative_filename.Length == 0)
            {
                AddError($"Output project {number} is enabled, but there is no project file specified.");
                return;
            }

            string base_path = Path.GetDirectoryName(MainWindow.ViewModel.FileName);
            string absolute_path_to_file = StudioGeneral.GetAbsolutePath(base_path, relative_filename);
            string folder = Path.GetDirectoryName(absolute_path_to_file);

            if (Directory.Exists(folder) == false)
                AddError($"Folder for selected output project {number} does not exist. {folder}");

            if (File.Exists(absolute_path_to_file) == false)
                AddError($"Specified output project {number} does not exist. {absolute_path_to_file}");
        }

    }
}
