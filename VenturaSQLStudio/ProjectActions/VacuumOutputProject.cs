using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VenturaSQLStudio.RecordsetGenerator;

namespace VenturaSQLStudio.ProjectActions
{
    internal class VacuumOutputProject
    {
        private Project _project;
        private VisualStudio_Projectfile_Modifier _modifier;
        private int _modifier_index;
        private List<RootItem.FolderListItem> _pathlist;
        private List<string> _expected_cs_files;

        internal VacuumOutputProject(Project project, VisualStudio_Projectfile_Modifier modifier, int modifier_index, List<RootItem.FolderListItem> pathlist)
        {
            _project = project;
            _modifier = modifier;
            _modifier_index = modifier_index;
            _pathlist = pathlist;
        }

        internal void Vacuum()
        {
            List<string> root_folders = new List<string>();

            _expected_cs_files = new List<string>();

            // We find all folders in the root.
            foreach (ITreeViewItem tvi in _project.FolderStructure.Children)
                if (tvi.ItemKind == TreeViewModelKind.FolderItem)
                {
                    root_folders.Add(tvi.Name + @"\");
                    ScanProjectFolder((FolderItem)tvi);
                }

            _modifier.Vacuum(_expected_cs_files, root_folders);
        }

        private void ScanProjectFolder(FolderItem folder)
        {
            string partial_path = folder.CalculatePath();
            string target_folder = Path.Combine(_modifier.ProjectFileFolder, partial_path);

            List<string> expected_files = new List<string>();
            List<string> expected_folders = new List<string>();

            foreach (ITreeViewItem tvi in folder.Children)
            {
                if (tvi.ItemKind == TreeViewModelKind.RecordsetItem)
                {
                    RecordsetItem recordset = (RecordsetItem)tvi;

                    if (recordset.Enabled == true && recordset.OutputProjects[_modifier_index].Selected == true)
                    {
                        string file_name = recordset.ClassName + ".cs";

                        expected_files.Add(file_name);

                        _expected_cs_files.Add(Path.Combine(partial_path, file_name));
                    }
                }
                else if (tvi.ItemKind == TreeViewModelKind.FolderItem)
                {
                    expected_folders.Add(tvi.Name);
                }
            }

            if (Directory.Exists(target_folder) == true)
                VacuumFolder(target_folder, expected_files, expected_folders);

            foreach (ITreeViewItem tvi in folder.Children)
            {
                if (tvi.ItemKind == TreeViewModelKind.FolderItem)
                    ScanProjectFolder((FolderItem)tvi);
            }

        }

        // This operates on physical folders and files.
        private void VacuumFolder(string target_folder, List<string> expected_files, List<string> expected_folders)
        {
            string[] files = Directory.GetFiles(target_folder);
            string[] folders = Directory.GetDirectories(target_folder);

            foreach (string full_path in files)
            {
                string file_name = Path.GetFileName(full_path);

                if (expected_files.Any(a => a.ToLower() == file_name.ToLower()) == false)
                    File.Delete(full_path);
            }

            foreach (string full_folder in folders)
            {
                string folder_name = Path.GetFileName(full_folder);

                if (expected_folders.Any(a => a.ToLower() == folder_name.ToLower()) == false)
                    Directory.Delete(full_folder, true);
            }

        }

    }
}
