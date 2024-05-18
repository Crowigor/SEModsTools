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
        public string File { get; set; }
        public string ModName { get; set; }
        public string RootPath { get; set; }
        public string UploadPath { get; set; }
        public bool AutomaticUpload { get; set; }
        public static string[] AllowedExtensions = { ".cs", ".sbc", ".png", ".mwm", ".dds", ".xml", ".txt", ".resx", ".sbl" };

        public ModProject(string file, string modName, string rootFolder, string modsFolder, bool automaticUpload = false)
        {
            File = file;
            ModName = modName;
            RootPath = rootFolder;
            UploadPath = modsFolder + "\\" + modName;
            AutomaticUpload = automaticUpload;
        }

        public static ModProject Parse(string fileName)
        {
            string modNameValue = null;
            string modsFolderValue = null;
            bool automaticUploadValue = false;
            string projectRootFolder = Path.GetDirectoryName(fileName);

            ThreadHelper.ThrowIfNotOnUIThread();
            ParseProjectParams(fileName, ref modNameValue, ref modsFolderValue, ref automaticUploadValue);

            if (modNameValue == null || modsFolderValue == null)
            {
                return null;
            }

            return new ModProject(fileName, modNameValue, projectRootFolder, modsFolderValue, automaticUploadValue);
        }

        public static void CheckOldProjectParams(string fileName)
        {
            try
            {
                XDocument projectFile = XDocument.Load(fileName);
                XNamespace ns = projectFile.Root.GetDefaultNamespace();
                XElement propertyGroup = projectFile.Root.Descendants(ns + "PropertyGroup")
                    .FirstOrDefault(e => e.Element(ns + "SEModsToolsModName") != null);
                if (propertyGroup != null)
                {
                    SEModsToolsPackage.PrintMessage($"Find old version params");
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SEModsToolsPackage.PrintMessage($"Error reading csproj: {ex.Message}");

                return;
            }
        }

        public static void ParseProjectParams(string fileName, ref string modNameValue, ref string modsFolderValue, ref bool automaticUploadValue)
        {
            try
            {
                XDocument projectFile = XDocument.Load(fileName);
                XNamespace ns = projectFile.Root.GetDefaultNamespace();
                XElement propertyGroup = projectFile.Root.Descendants(ns + "PropertyGroup")
                    .FirstOrDefault(e => e.Element(ns + "SEModsToolsModName") != null);
                if (propertyGroup != null)
                {
                    XElement modNameElement = propertyGroup.Element(ns + "SEModsToolsModName");
                    XElement modsFolderElement = propertyGroup.Element(ns + "SEModsToolsModsFolder");
                    XElement automaticUploadElement = propertyGroup.Element(ns + "SEModsToolsAutomaticUpload");
                    if (modNameElement != null)
                    {
                        modNameValue = modNameElement.Value;
                    }
                    if (modsFolderElement != null)
                    {
                        modsFolderValue = modsFolderElement.Value;
                    }
                    if (automaticUploadElement != null && automaticUploadElement.Value == "true")
                    {
                        automaticUploadValue = true;
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SEModsToolsPackage.PrintMessage($"Error reading csproj: {ex.Message}");

                return;
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
