using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.AutoCreate
{
    internal class RunAutoCreate
    {
        private Project _project;

        internal RunAutoCreate(Project project)
        {
            _project = project;
        }

        internal bool Exec(bool visually_select_folder)
        {
            FolderItem folder = _project.FolderStructure.FetchOrCreateFolderItem(_project.AutoCreateSettings.Folder);

            if (visually_select_folder == true)
            {
                // clear the selection
                _project.FolderStructure.UnselectAll();

                folder.IsSelected = true; // Select the folder
                folder.ExpandBubbleUp(); // Make sure the root is expanded too
            }

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                InnerExec(folder);
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Creating recordsets...", action);

            if (result.Error != null)
            {
                Error("Auto Create recordsets was aborted:\n\n" + result.Error.Message);
                return false;
            }

            return true;
        }

        private const string CLASS_SUMMARY = "Recordset item automatically created by VenturaSQL Studio.";

        private void InnerExec(FolderItem folder)
        {
            RecordsetAutoCreator rsa = new RecordsetAutoCreator(_project);

            List<RecordsetAutoCreator.AutoCreateRecordset> list = rsa.CreateRecordsets(); // here an Exception can happen

            bool is_equal = SuperCompare(list, folder);

            if (is_equal == true)
                return;

            void dispatch_action1()
            {
                // Close all the tab pages for the recordsets to be deleted.
                foreach (var tvi in folder.Children)
                {
                    if (tvi is RecordsetItem)
                    {
                        string uniqueid = $"RS {tvi.GetHashCode()}";
                        MainWindow window = (MainWindow)Application.Current.MainWindow;
                        window.CloseTab(uniqueid);
                    }
                }

                // Empty the folder, and forget the recordsets.
                folder.Children.Clear();
            }

            Application.Current.Dispatcher.Invoke(dispatch_action1);

            foreach (var recordset in list)
            {
                RecordsetItem rsi = new RecordsetItem(_project, folder, recordset.ClassName);
                rsi.ClassSummary = CLASS_SUMMARY;
                rsi.SqlScript = recordset.SqlScript;
                rsi.RowloadIncremental = recordset.AsIncremental;

                if (recordset.KeyColumns != null)
                {
                    // Create Parameters
                    for (int i = 0; i < recordset.KeyColumns.Count; i++)
                    {
                        var key_column = recordset.KeyColumns[i];

                        ParameterItem parameter_item = new ParameterItem(_project);
                        parameter_item.Name = _project.ParameterPrefix + key_column.ColumnName;
                        parameter_item.FullTypename = key_column.ColumnType.FullName;
                        parameter_item.DbTypeString = key_column.DbTypeString;
                        parameter_item.DesignValue = "null";
                        rsi.Parameters.Add(parameter_item);
                    }
                }

                // Resultsets
                ResultsetItem resultsetitem = new ResultsetItem(_project, 1, "Resultset1");
                resultsetitem.UpdateableTableName = recordset.UpdateableTableName;
                resultsetitem.GenerateReferencedTablesList(null); // Add the UpdateableTableName to the list for the dropdown combobox
                rsi.Resultsets.Add(resultsetitem);

                void dispatch_action2()
                {
                    folder.Children.Add(rsi);
                }

                Application.Current.Dispatcher.Invoke(dispatch_action2);
            }

        }

        private void Error(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool SuperCompare(List<RecordsetAutoCreator.AutoCreateRecordset> list, FolderItem folder)
        {
            if (list.Count != folder.Children.Count)
                return false;

            foreach (var folder_item in folder.Children)
            {
                if (folder_item.ItemKind != TreeViewModelKind.RecordsetItem)
                    return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var list_item = list[i];
                var folder_item = folder.Children[i];

                if (list_item.ClassName != folder_item.Name)
                    return false;
            }

            // Compare the definition against the existing recordset.
            for (int i = 0; i < list.Count; i++)
            {
                bool result = CompareOne(list[i], (RecordsetItem)folder.Children[i]);

                if (result == false)
                    return false;
            }

            return false;
        }

        private bool CompareOne(RecordsetAutoCreator.AutoCreateRecordset new_rs, RecordsetItem existing_rs)
        {
            if (existing_rs.ClassSummary != CLASS_SUMMARY)
                return false;

            if (existing_rs.ImplementDatabinding != true)
                return false;

            if (existing_rs.SqlScript != new_rs.SqlScript)
                return false;

            // the code is not finished yet. this means that the project becomes 'modified' every time the auto-create runs.

            return false;
        }

    }
}
