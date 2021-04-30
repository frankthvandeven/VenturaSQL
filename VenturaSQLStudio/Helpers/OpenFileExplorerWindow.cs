using System.IO;

namespace VenturaSQLStudio.Helpers
{
    public static class OpenFileExplorerWindow
    {

        public static void Exec(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            // Detect whether its a directory or file.
            if (attr.HasFlag(FileAttributes.Directory))
            {
                // Path is a Directory.
                System.Diagnostics.Process.Start(path);
            }
            else
            {
                // Path is a File.
                string argument = @"/select, " + path;
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }

        }
    }
}
