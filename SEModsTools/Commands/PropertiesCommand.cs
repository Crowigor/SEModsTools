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
    internal sealed class PropertiesCommand
    {
        public const int CommandId = 768;
        public static readonly Guid CommandSet = new Guid("9657cb9f-323b-4e54-ba7a-00b32cf4cecb");
        private readonly AsyncPackage package;
        public static PropertiesCommand Instance { get; private set; }
        private PropertiesCommand(AsyncPackage package, OleMenuCommandService commandService)
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
            Instance = new PropertiesCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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

            string csproj = project.FileName;
            PropertiesForm form = new PropertiesForm(csproj);
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            IVsHierarchy hierarchy;
            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            if (hierarchy is IVsBuildPropertyStorage propertyStorage)
            {
                SEModsToolsPackage.PrintMessage($"========== Update Project Properties  ==========");
                foreach (var keypear in form.GetReplacesValues())
                {
                    propertyStorage.SetPropertyValue(keypear.Key, "", (uint)_PersistStorageType.PST_PROJECT_FILE, keypear.Value);
                    SEModsToolsPackage.PrintMessage($"Updated {keypear.Key} to {keypear.Value}");
                }

                project.Save();
                SEModsToolsPackage.PrintMessage($"========== Finish Update Properties ==========\n");

                if (!Services.ProjectsWatcher.Projects.ContainsKey(csproj))
                {
                    MessageBox.Show($"Reload Visual Studio for enabled SEModsTools functions", "Info", MessageBoxButtons.OK);
                }
            }
        }
    }
}
