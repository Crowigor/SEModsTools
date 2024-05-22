using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SEModsTools.Services
{
    public class ModProject
    {
        public string File = String.Empty;
        public string RootPath = String.Empty;
        public bool IsModProject = false;
        public string ModName = String.Empty;
        public string Namespace = String.Empty;
        public string GameBinPath = String.Empty;
        public string ModsFolder = String.Empty;
        public string UploadPath = String.Empty;
        public string[] AllowedExtensions = [];
        public bool AutomaticUpload = false;
        public static readonly Dictionary<string, string> DefaultProperties = new Dictionary<string, string>{
            {"SEModsToolsModName", "Hello World"},
            {"RootNamespace", "MyFraction"},
            {"SEModsToolsGameBinPath", String.Empty},
            {"SEModsToolsModsFolder", "%AppData%\\SpaceEngineers\\Mods"},
            {"SEModsToolsAllowedExtensions", ".cs,.sbc,.png,.mwm,.dds,.xml,.txt,.resx,.sbl"},
            {"SEModsToolsAutomaticUpload", "false"},
        };

        public ModProject(string csproj)
        {
            File = csproj;
            RootPath = Path.GetDirectoryName(csproj);

            ThreadHelper.ThrowIfNotOnUIThread();
            Dictionary<string, string> properties = ParseProjectProperties(csproj);
            if (properties == null || properties.Count == 0 || !properties.ContainsKey("SEModsToolsModName"))
            {
                return;
            }

            IsModProject = true;
            ModName = properties["SEModsToolsModName"];
            Namespace = (properties.ContainsKey("RootNamespace")) ? properties["RootNamespace"] : DefaultProperties["RootNamespace"];
            GameBinPath = (properties.ContainsKey("SEModsToolsGameBinPath")) ? properties["SEModsToolsGameBinPath"] : DefaultProperties["SEModsToolsGameBinPath"];
            ModsFolder = (properties.ContainsKey("SEModsToolsModsFolder")) ? properties["SEModsToolsModsFolder"] : DefaultProperties["SEModsToolsModsFolder"];
            ModsFolder = ModsFolder.Replace("%25", "%");
            UploadPath = ModsFolder + "\\" + ModName;

            string allowedExtensionsString = (properties.ContainsKey("SEModsToolsAllowedExtensions")) ? properties["SEModsToolsAllowedExtensions"] : DefaultProperties["SEModsToolsAllowedExtensions"];
            AllowedExtensions = allowedExtensionsString.Split(',')
                .Select(ext => ext.Trim().ToLower())
                .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
                .ToArray();

            string automaticUploadString = (properties.ContainsKey("SEModsToolsAutomaticUpload")) ? properties["SEModsToolsAutomaticUpload"] : DefaultProperties["SEModsToolsAutomaticUpload"];
            AutomaticUpload = (automaticUploadString == "true");
        }

        public static Dictionary<string, string> ParseProjectProperties(string csproj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                string[] properties = ModProject.DefaultProperties.Keys.ToArray();
                Dictionary<string, string> result = new Dictionary<string, string>();

                XDocument projectFile = XDocument.Load(csproj);
                XNamespace ns = projectFile.Root.GetDefaultNamespace();
                foreach (XElement propertyGroup in projectFile.Root.Elements(ns + "PropertyGroup"))
                {
                    foreach (XElement property in propertyGroup.Elements())
                    {
                        string propertyName = property.Name.LocalName;
                        string propertyValue = property.Value;

                        if (properties.Contains(propertyName) && !string.IsNullOrWhiteSpace(propertyValue))
                        {
                            result[propertyName] = propertyValue;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SEModsToolsPackage.PrintMessage($"Error reading csproj: {ex.Message}");

                return null;
            }
        }

        public static void ParseProjectItems(ProjectItems items, string currentFolderPath, ref List<string> folders, ref List<string> files)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (ProjectItem item in items)
            {
                if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    string folderPath = Path.Combine(currentFolderPath, item.Name);
                    folders.Add(folderPath);
                    ParseProjectItems(item.ProjectItems, folderPath, ref folders, ref files);
                }
                else if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                {
                    string filePath = Path.Combine(currentFolderPath, item.Name);
                    files.Add(filePath);
                }
            }
        }
    }
}
