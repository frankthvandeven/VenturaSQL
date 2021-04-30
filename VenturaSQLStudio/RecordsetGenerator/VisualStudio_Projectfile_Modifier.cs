using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace VenturaSQLStudio.RecordsetGenerator
{
    /// <summary>
    /// Read and modify Visual Studio project files (.csproj)
    /// Added .Net Core project file support in January 2018.
    /// </summary>
    internal class VisualStudio_Projectfile_Modifier
    {
        private ProjectfileFormatEnum _projectfileformat;
        private bool _wildcard_include;

        private string _projectfilefullpath;

        private string _projectfilefolder;

        private FileStream _filestream = null;

        private XmlDocument _doc = new XmlDocument();
        //private XmlNamespaceManager _manager;

        private string _rootNamespace;
        private XmlNode _compilenode;

        private bool _projectChanged;

        private event Action<string> _logOutputEvent;

        internal VisualStudio_Projectfile_Modifier(string projectfilefullpath, Action<string> logoutputevent)
        {
            _projectfilefullpath = projectfilefullpath;
            _logOutputEvent = logoutputevent;

            _projectChanged = false;
            _projectfileformat = ProjectfileFormatEnum.Classic;
            _wildcard_include = false;

            _compilenode = null;
        }

        /// <summary>
        /// Opens and loads the .csproj file.
        /// </summary>
        internal void Load()
        {
            try
            {
                _projectfilefolder = Path.GetDirectoryName(_projectfilefullpath);

                _rootNamespace = Path.GetFileNameWithoutExtension(_projectfilefullpath);

                // Open the project file for ReadWrite.
                _filestream = File.Open(_projectfilefullpath, FileMode.Open, FileAccess.ReadWrite);

                // Load the data into an XML Document object.
                _doc.Load(_filestream);

                //_manager = new XmlNamespaceManager(_doc.NameTable);
                //_manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                //bool is_classic = _doc.DocumentElement.HasAttribute("ToolsVersion")

                if (_doc.DocumentElement.HasAttribute("Sdk") == true)
                {
                    _projectfileformat = ProjectfileFormatEnum.NetCore;
                    _wildcard_include = true;
                }

                // Find the Default namespace name.
                //XmlNode rootNamespaceNode = _doc.SelectSingleNode("/x:Project/x:PropertyGroup/x:RootNamespace", _manager);
                //_rootNamespace = rootNamespaceNode.InnerText;

                foreach (XmlNode groupnode in _doc.GetElementsByTagName("PropertyGroup"))
                    foreach (XmlNode childnode in groupnode.ChildNodes)
                        if (childnode.Name == "RootNamespace")
                        {
                            _rootNamespace = childnode.InnerText;
                            goto label1; ;
                        }

                    label1:

                if (_projectfileformat == ProjectfileFormatEnum.Classic)
                    SetCompileNode();

                _logOutputEvent($"Loaded output project file '{_projectfilefullpath}'.");
            }
            catch (Exception ex)
            {
                StudioGeneral.SmartClose(_filestream);
                /* \n will insert a newline in the error message */
                throw new FileLoadException($"Error loading visual studio project file. Error message: {ex.Message}. Filename: '{_projectfilefullpath}'.");
            }
        }

        private void SetCompileNode()
        {
            // Find the ItemGroup nodes that contains "Compile" Nodes.
            foreach (XmlNode groupnode in _doc.GetElementsByTagName("ItemGroup"))
                foreach (XmlNode childnode in groupnode.ChildNodes)
                    if (childnode.Name == "Compile")
                    {
                        _compilenode = groupnode;
                        goto label2;
                    }

                label2:;
        }

        /// <summary>
        /// Adds the specified file to the project file.
        /// </summary>
        /// <param name="relative_outputfolder">The relative path to the output folder. The vs-project file's folder is the root folder. For example: "Recordsets\Customers".</param>
        internal void AddCSharpFileToProject(string relative_outputfolder, string filename)
        {
            // >Net Core projects have a wildcard include. No need to add file explicitly.
            if (_wildcard_include)
                return;

            if (_compilenode == null) // create a new ItemGroup on the fly
            {
                // When creating an element, if the NamespaceURI matches the parent's URI, an xlns attribute will not be set.
                _compilenode = _doc.CreateElement("ItemGroup", _doc.DocumentElement.NamespaceURI);
                _doc.DocumentElement.AppendChild(_compilenode);
            }

            string path = Path.Combine(relative_outputfolder, filename);

            foreach (XmlNode node in _compilenode)
                if (node.Name == "Compile")
                    if (node.Attributes["Include"].Value.ToLower() == path.ToLower())
                        return;

            XmlNode newchild = _doc.CreateNode(XmlNodeType.Element, "Compile", _doc.DocumentElement.NamespaceURI);
            XmlAttribute include = _doc.CreateAttribute("Include");

            include.Value = path;

            newchild.Attributes.Append(include);

            _compilenode.AppendChild(newchild);

            _projectChanged = true;

        } // end of method

        internal ProjectfileFormatEnum ProjectfileFormat
        {
            get { return _projectfileformat; }
        }

        internal string RootNamespace
        {
            get { return _rootNamespace; }
        }


        /// <summary>
        /// Converts a relative path to a namespace.
        /// For example: relative path "Recordsets\Customers" becomes "DefaultNamespace.Recordsets.Customers"
        /// </summary>
        internal string RelativePath2Namespace(string relativepath)
        {
            if (relativepath.Length == 0)
                return _rootNamespace;

            StringBuilder sb = new StringBuilder();

            sb.Append(_rootNamespace);

            string[] parts = relativepath.Split('\\');

            foreach (string part in parts)
            {
                sb.Append(".");
                sb.Append(part);
            }

            return sb.ToString();
        }

        internal string ProjectFileFolder
        {
            get { return _projectfilefolder; }
        }

        internal void SaveAndCloseProjectFile()
        {
            if (_filestream == null) /*nothing to to */
                return;

            if (_projectChanged == true)
            {
                _filestream.SetLength(0);
                _doc.Save(_filestream);
            }

            _filestream.Close();

            if (_projectChanged)
                _logOutputEvent($"Updated and closed output project file '{_projectfilefullpath}'.");
            else
                _logOutputEvent($"Closed unchanged output project file '{_projectfilefullpath}'.");

        }

        // <Compile Include="VenturaRecordsets\FastSpring\LicensesPerFastSpringOrderRecordset.cs" />
        internal void Vacuum(List<string> expected_cs_files, List<string> managed_rootfolders)
        {
            if (_compilenode == null)
                return;

            if (_projectfileformat != ProjectfileFormatEnum.Classic)
                return;

            bool ends_wrong = managed_rootfolders.Any(a => a.EndsWith(@"\") == false);

            if (ends_wrong)
                throw new ArgumentException("Every managed rootfolder must end with '\\'");

            List<XmlNode> nodes_to_remove = new List<XmlNode>();

            // Create a list of nodes that need removing
            foreach (XmlNode node in _compilenode)
            {
                if (node.Name == "Compile")
                {
                    var attr_include = node.Attributes["Include"];

                    if (attr_include != null)
                    {
                        string filepath = attr_include.Value; // VenturaRecordsets\FastSpring\LicensesPerFastSpringOrderRecordset.cs
                        bool is_managed_folder = false;

                        int pos = filepath.IndexOf(@"\");

                        if (pos != -1) // if there is a backslash, there is a folder in the path.
                        {
                            string start_part = filepath.Substring(0, pos + 1); // for example "VenturaRecordsets\"

                            // Check if it is in the managed folder list.
                            is_managed_folder = managed_rootfolders.Any(a => a.ToLower() == start_part.ToLower());
                        }

                        if (is_managed_folder == true)
                        {
                            bool is_expected = expected_cs_files.Any(a => a.ToLower() == attr_include.Value.ToLower());

                            if (is_expected == false) // is the Compile node in the list with expected .cs files?
                                nodes_to_remove.Add(node);

                        }
                    }
                }

            }

            // Remove the nodes
            foreach (XmlNode node in nodes_to_remove)
            {
                _compilenode.RemoveChild(node);
                _projectChanged = true;
            }
        }

        internal enum ProjectfileFormatEnum
        {
            Classic,
            NetCore
        }

    }
}
