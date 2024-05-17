using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SEModsTools.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SEModsTools.Commands
{
    internal sealed class PushCommand
    {
        public const int CommandId = 256;
        public static readonly Guid CommandSet = new Guid("9657cb9f-323b-4e54-ba7a-00b32cf4cecb");
        private readonly AsyncPackage package;
        public static PushCommand Instance { get; private set; }
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get { return this.package; } }

        private PushCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PushCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            PushProject();
        }

        public static void PushProject(bool force = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SEModsToolsPackage.PrintMessage("\n========== Start push project  ==========");
            DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE80.DTE2;
            Project project = ((IEnumerable)dte.ToolWindows.SolutionExplorer.SelectedItems).OfType<UIHierarchyItem>()
                .Select(item =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return item.Object;
                })
                 .OfType<Project>()
                 .FirstOrDefault();
            if (project == null)
            {
                SEModsToolsPackage.PrintMessage($"Error: Project not selected");
                return;
            }

            SEModsToolsPackage.PrintMessage($"Parse {project.FullName}");
            ModProject modProject = ModProject.Parse(project.FullName);
            if (modProject == null)
            {
                SEModsToolsPackage.PrintMessage($"Error: Mod project not found in {project.FullName}");
                return;
            }

            List<string> folders = new List<string>();
            List<string> files = new List<string>();
            ModProject.ParseProjectItems(project.ProjectItems, modProject.RootPath, ref folders, ref files);
            if (files.Count == 0)
            {
                SEModsToolsPackage.PrintMessage($"Error: Mod project empty");
                return;
            }

            if (force)
            {
                DialogResult result = MessageBox.Show("Mod files will be deleted. Do you want to continue?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    string folder = Environment.ExpandEnvironmentVariables(modProject.UploadPath);
                    if (Directory.Exists(folder))
                    {
                        SEModsToolsPackage.PrintMessage($"Folder {folder} was deleted");
                        Directory.Delete(folder, true);
                    }
                }
                else
                {
                    SEModsToolsPackage.PrintMessage("========== Push Canceled  ==========");
                    return;
                }
            }

            foreach (string file in files)
            {             
                string fileExtension = Path.GetExtension(file).ToLower();
                if (Array.IndexOf(ModProject.AllowedExtensions, fileExtension) == -1)
                {
                    continue;
                }
                try
                {
                    string copyTo = file.Replace(modProject.RootPath, modProject.UploadPath);
                    copyTo = copyTo.Replace(Path.GetExtension(file), fileExtension);
                    copyTo = Environment.ExpandEnvironmentVariables(copyTo);
                    string targetFolderPath = Path.GetDirectoryName(copyTo);
                    if (!Directory.Exists(targetFolderPath))
                    {
                        Directory.CreateDirectory(targetFolderPath);
                    }

                    File.Copy(file, copyTo, true);
                    SEModsToolsPackage.PrintMessage($"File {file} push to {copyTo} at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    SEModsToolsPackage.PrintMessage($"Error push file: {ex.Message}");
                }
            }

            SEModsToolsPackage.PrintMessage("========== Push finished  ==========");
        }
    }
}
