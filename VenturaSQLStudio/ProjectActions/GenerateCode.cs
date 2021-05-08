using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.RecordsetGenerator;

namespace VenturaSQLStudio.ProjectActions
{
    internal class GenerateCode
    {
        internal event Action<string> LogOutputEvent;

        private Project _project;

        private VisualStudio_Projectfile_Modifier[] _vs_modifiers;

        private int _files_written;
        private Exception _exception;

        internal GenerateCode(Project project)
        {
            _vs_modifiers = new VisualStudio_Projectfile_Modifier[project.VisualStudioProjects.Count];

            _project = project;

            _files_written = 0;
            _exception = null;

        }

        internal void Exec(DateTime timestamp)
        {

            try
            {
                AdoConnector connector = AdoConnectorHelper.Create(_project.ProviderInvariantName, _project.MacroConnectionString);

                using (DbConnection connection = connector.OpenConnection())
                {
                    //Log($"Code generation started on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}.");

                    for (int i = 0; i < _project.VisualStudioProjects.Count; i++)
                    {
                        VisualStudioProjectItem vs_project = _project.VisualStudioProjects[i];

                        if (vs_project.ProjectEnabled)
                        {
                            string base_path = Path.GetDirectoryName(MainWindow.ViewModel.FileName);
                            string absolute_path_to_file = StudioGeneral.GetAbsolutePath(base_path, vs_project.OutputProjectFilename);
                            _vs_modifiers[i] = new VisualStudio_Projectfile_Modifier(absolute_path_to_file, LogOutputEvent);
                            _vs_modifiers[i].Load();
                        }
                    }

                    List<RootItem.FolderListItem> pathlist = _project.FolderStructure.GetFolderList();

                    // Vacuum the Visual Studio projects.
                    Log("Vacuuming C# projects.");


                    for (int i = 0; i < _vs_modifiers.Length; i++)
                    {
                        var modifier = _vs_modifiers[i];

                        if (modifier != null)
                        {
                            VacuumOutputProject vacuumer = new VacuumOutputProject(_project, modifier, i, pathlist);
                            vacuumer.Vacuum();
                        }
                    }

                    Log("Vacuuming complete.");

                    // RecordsetItems only...
                    foreach (RootItem.FolderListItem pathlistitem in pathlist)
                        foreach (RecordsetItem projectitem in pathlistitem.FolderItem.Children.Where(child => child.ItemKind == TreeViewModelKind.RecordsetItem))
                        {
                            if (projectitem.Enabled == true)
                                ExecRecordsetItem(connection, projectitem, pathlistitem.OutputFolder, timestamp);
                        }

                    // Save all open and updated project files.
                    for (int i = 0; i < _project.VisualStudioProjects.Count; i++)
                    {
                        if (_vs_modifiers[i] != null)
                            _vs_modifiers[i].SaveAndCloseProjectFile();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
                Log($"Code generation aborted with an internal error. The error message is: {ex.Message}");
            }

        }

        private void ExecRecordsetItem(DbConnection connection, RecordsetItem recordsetitem, string outputfolder, DateTime timestamp)
        {
            int files2generate = 0;

            for (int i = 0; i < _project.VisualStudioProjects.Count; i++)
            {
                if (_project.VisualStudioProjects[i].ProjectEnabled && recordsetitem.OutputProjects[i].Selected)
                    files2generate++;
            }

            if (files2generate == 0)
                Log($"No output C# project file(s) selected for recordset {recordsetitem.ClassName}. Nothing to do.");
            else
            {
                if (files2generate == 1)
                    Log($"Generating recordset {recordsetitem.ClassName} in 1 C# output project.");
                else
                    Log($"Generating recordset {recordsetitem.ClassName} in {files2generate} C# output projects.");

                MasterTemplate master_template = new MasterTemplate(connection, /*_project,*/ recordsetitem, timestamp);

                master_template.PreGenerate();

                for (int i = 0; i < _project.VisualStudioProjects.Count; i++)
                {
                    VisualStudioProjectItem vs_project = _project.VisualStudioProjects[i];

                    if (vs_project.ProjectEnabled && recordsetitem.OutputProjects[i].Selected)
                        Write2File(_vs_modifiers[i],
                                   recordsetitem,
                                   master_template,
                                   outputfolder,
                                   vs_project.GenerateDirectAdoConnectionCode,
                                   TargetPlatform2Enum(vs_project.TargetPlatform));
                }

                master_template.PostGenerate();
            }

        }

        private VenturaSqlPlatform TargetPlatform2Enum(string platformstring)
        {
            VenturaSqlPlatform s = (VenturaSqlPlatform)Enum.Parse(typeof(VenturaSqlPlatform), platformstring);
            return s;
        }

        private void Write2File(VisualStudio_Projectfile_Modifier vspm, RecordsetItem recordsetitem, MasterTemplate template, string outputfolder, bool ado_direct, VenturaSqlPlatform generatortarget)
        {
            string namespace_name = vspm.RelativePath2Namespace(outputfolder);

            string generatedcode = template.GenerateCSharp(namespace_name, ado_direct, generatortarget);

            string targetfolder = Path.Combine(vspm.ProjectFileFolder, outputfolder);
            string targetfilename = recordsetitem.ClassName + ".cs";
            string targetfullpath = Path.Combine(targetfolder, targetfilename);

            if (Directory.Exists(targetfolder) == false)
                Directory.CreateDirectory(targetfolder);

            FileStream stream = new FileStream(targetfullpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            stream.SetLength(0);
            byte[] utf8 = Encoding.UTF8.GetBytes(generatedcode);
            stream.Write(utf8, 0, utf8.Length);
            stream.Close();

            vspm.AddCSharpFileToProject(outputfolder, targetfilename);

            //Log($"   Generated C# file {targetfullpath} ({namespace_name}.{recordsetitem.ClassName}).");

            _files_written++;
        }

        internal int FilesWritten
        {
            get { return _files_written; }
        }

        internal Exception Exception
        {
            get { return _exception; }
        }

        // Entity framework optimization:
        // https://www.simple-talk.com/dotnet/.net-tools/entity-framework-performance-and-what-you-can-do-about-it/?utm_source=simpletalk&utm_medium=pubemail&utm_campaign=dotnetnewsletter&utm_content=entityframeworkperformance&utm_term=dotnetnewsletter20160111

        private void Log(string text)
        {
            if (LogOutputEvent != null)
                LogOutputEvent(text);
        }


    }

    internal class Aborted : Exception
    {

        internal Aborted() : base("Process Aborted Exception")
        {
        }

        internal Aborted(string message) : base(message)
        {
        }

        internal Aborted(string message, Exception innerException) : base(message, innerException)
        {
        }

    }


}
