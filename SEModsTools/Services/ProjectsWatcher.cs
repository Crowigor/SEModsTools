using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SEModsTools.Services
{
    class ProjectsWatcher
    {
        public static Dictionary<string, ModProject> Projects = new Dictionary<string, ModProject>();
        private static Dictionary<string, string> Files = new Dictionary<string, string>();
        private static Dictionary<string, string> Folders = new Dictionary<string, string>();
        private static Dictionary<string, FileSystemWatcher> Watchers = new Dictionary<string, FileSystemWatcher>();
        private Dictionary<string, DateTime> LastChangedTimes = new Dictionary<string, DateTime>();

        public ProjectsWatcher()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            if (dte == null)
            {
                return;
            }

            if (dte.Solution.Projects.Count == 0)
            {
                return;
            }

            var solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.ProjectAdded += OnAddProject;
            solutionEvents.ProjectRemoved += OnRemoveProject;

            foreach (Project project in dte.Solution.Projects)
            {
                OnAddProject(project);
            }
        }
       
        private void OnAddProject(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProgressProject(project.FileName);
        }

        private void OnRemoveProject(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project == null)
            {
                return;
            }

            if (Projects.ContainsKey(project.FileName))
            {
                DeleteProject(project.FileName);
            }
        }

        private void ProgressProject(string fileName, bool force = false)
        {
            lock (this)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                if (Projects.ContainsKey(fileName) && !force)
                {
                    return;
                }

                if (!Projects.ContainsKey(fileName))
                {
                    SEModsToolsPackage.PrintMessage("========== Initializing Project  ==========");

                }
                else
                {
                    SEModsToolsPackage.PrintMessage($"========== ReInitializing Project  ==========");
                }

                SEModsToolsPackage.PrintMessage($"Parce: {fileName}");
                DeleteProject(fileName);

                DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
                if (dte == null)
                {
                    return;
                }
                Project project = null;
                foreach (Project item in dte.Solution.Projects)
                {
                    if (item.FileName == fileName)
                    {
                        project = item;
                        break;
                    }
                }
                if (project == null)
                {
                    return;
                }

                string projectKey = project.FileName;

                // Create project
                ModProject modProject = new ModProject(project.FileName);
                if (!modProject.IsModProject || string.IsNullOrEmpty(modProject.ModName) || string.IsNullOrEmpty(modProject.ModsFolder))
                {
                    if (Projects.ContainsKey(projectKey))
                    {
                        Projects.Remove(projectKey);
                    }

                    SEModsToolsPackage.PrintMessage($"This is not SE Mod project");
                    return;
                }
                SEModsToolsPackage.PrintMessage($"File: {modProject.File}");
                SEModsToolsPackage.PrintMessage($"RootPath: {modProject.RootPath}");
                SEModsToolsPackage.PrintMessage($"ModName: {modProject.ModName}");
                SEModsToolsPackage.PrintMessage($"Namespace: {modProject.Namespace}");
                SEModsToolsPackage.PrintMessage($"GameBinPath: {modProject.GameBinPath}");
                SEModsToolsPackage.PrintMessage($"ModsFolder: {modProject.ModsFolder}");
                SEModsToolsPackage.PrintMessage($"UploadPath: {modProject.UploadPath}");
                SEModsToolsPackage.PrintMessage($"AllowedExtensions: {String.Join(",", modProject.AllowedExtensions)}");
                SEModsToolsPackage.PrintMessage($"AutomaticUpload: {(modProject.AutomaticUpload ? "yes" : "no")}\n");

                // Add projects resurses
                List<string> folders = new List<string>() { modProject.RootPath };
                List<string> files = new List<string>() { project.FileName };
                if (modProject.AutomaticUpload)
                {
                    ModProject.ParseProjectItems(project.ProjectItems, modProject.RootPath, ref folders, ref files);
                }

                // Add folders and watchers
                SEModsToolsPackage.PrintMessage($"Add files and folders to Watcher");
                foreach (string folder in folders)
                {
                    Folders[folder] = projectKey;

                    string watcherKey = projectKey + "||" + folder;
                    if (Watchers.ContainsKey(watcherKey))
                    {
                        Watchers[watcherKey].EnableRaisingEvents = true;
                    }
                    else
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher();
                        watcher.Path = folder;
                        watcher.Filter = "*.*";
                        watcher.Changed += OnFileChanged;
                        watcher.Renamed += OnFileRenamed;
                        watcher.EnableRaisingEvents = true;
                        Watchers[watcherKey] = watcher;
                    }

                    SEModsToolsPackage.PrintMessage($"Add Folder {folder}");
                }

                // Add files
                foreach (string file in files)
                {
                    Files[file] = project.FileName;

                    SEModsToolsPackage.PrintMessage($"Add File {file}");
                }

                // Add project
                Projects[project.FileName] = modProject;
                SEModsToolsPackage.PrintMessage($"========== Finish Initializing ==========\n");
            }
        }

        private void DeleteProject(string projectKey)
        {
            if (Projects.ContainsKey(projectKey))
            {
                Projects.Remove(projectKey);
            }

            List<string> foldersToRemove = new List<string>();
            foreach (KeyValuePair<string, string> pair in Folders)
            {
                if (pair.Value == projectKey)
                {
                    foldersToRemove.Add(pair.Key);
                }
            }
            foreach (string key in foldersToRemove)
            {
                Folders.Remove(key);
            }

            List<string> watchersToRemove = new List<string>();
            foreach (KeyValuePair<string, FileSystemWatcher> pair in Watchers)
            {
                string projectWatcherKeyPart = projectKey + "||";
                if (pair.Key.Contains(projectWatcherKeyPart))
                {
                    watchersToRemove.Add(pair.Key);
                }
            }
            foreach (string key in watchersToRemove)
            {
                Watchers[key].EnableRaisingEvents = false;
            }

            List<string> filesToRemove = new List<string>();
            foreach (KeyValuePair<string, string> pair in Files)
            {
                if (pair.Value == projectKey)
                {
                    filesToRemove.Add(pair.Key);
                }
            }
            foreach (string key in filesToRemove)
            {
                Files.Remove(key);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                OnFileEventListner("changed", e);
            });
        }

        private void OnFileRenamed(object sender, FileSystemEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                OnFileEventListner("rename", e);
            });
        }

        private void OnFileEventListner(string action, FileSystemEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DateTime lastChangedTime = DateTime.Now;
                string lastChangeKey = e.FullPath;

                if (LastChangedTimes.ContainsKey(lastChangeKey) && (lastChangedTime - LastChangedTimes[lastChangeKey]).TotalMilliseconds < 1000)
                {
                    return;
                }
                LastChangedTimes[lastChangeKey] = lastChangedTime;

                if (Projects.ContainsKey(e.FullPath))
                {
                    ProgressProject(e.FullPath, true);

                    return;
                }

                if (!Files.ContainsKey(e.FullPath))
                {
                    return;
                }

                string projectKey = Files[e.FullPath];
                if (projectKey == null)
                {
                    return;
                }

                ModProject project = Projects[projectKey];
                if (project == null)
                {
                    return;
                }

                if (!project.AutomaticUpload)
                {
                    return;
                }

                string fileExtension = Path.GetExtension(e.FullPath);
                if (Array.IndexOf(project.AllowedExtensions, fileExtension) == -1)
                {
                    return;
                }

                try
                {
                    string copyTo = e.FullPath.Replace(project.RootPath, project.UploadPath);
                    copyTo = Environment.ExpandEnvironmentVariables(copyTo);
                    string targetFolderPath = Path.GetDirectoryName(copyTo);
                    if (!Directory.Exists(targetFolderPath))
                    {
                        Directory.CreateDirectory(targetFolderPath);
                    }

                    File.Copy(e.FullPath, copyTo, true);
                    SEModsToolsPackage.PrintMessage($"File {e.FullPath} tranfer to {copyTo} at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    SEModsToolsPackage.PrintMessage($"Error tranfer file: {ex.Message}");
                }
            });
        }
    }
}
