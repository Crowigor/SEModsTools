using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using SEModsTools.Services;
using System.Collections.Generic;
using EnvDTE;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace SEModsTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SEModsToolsPackage.PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class SEModsToolsPackage : AsyncPackage
    {
        public const string PackageGuidString = "a96ba491-589b-4761-87ae-a2749ec3e1dd";
        public static Guid PackageGuid = new Guid(PackageGuidString);
        public static IVsOutputWindowPane OutputWindow;
        public static Dictionary<string, Project> Projects = new Dictionary<string, Project>();

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            new ProjectsWatcher();
            await Commands.PushCommand.InitializeAsync(this);
            await SEModsTools.Commands.ForcePushCommand.InitializeAsync(this);
            await SEModsTools.Commands.PropertiesCommand.InitializeAsync(this);
        }

        public static IVsOutputWindowPane GetOutputWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OutputWindow == null)
            {
                var window = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

                if (window.GetPane(ref PackageGuid, out OutputWindow) != VSConstants.S_OK)
                {
                    window.CreatePane(ref PackageGuid, "SEModsTools", 1, 0);
                    window.GetPane(ref PackageGuid, out OutputWindow);
                }

                OutputWindow.Activate();
                OutputWindow.OutputStringThreadSafe("Hello Space Engineers and welcome back to SE Mods Tools!\n\n");
            }

            return OutputWindow;
        }

        public static void PrintMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var window = GetOutputWindow();
            window.OutputStringThreadSafe(message);
            window.OutputStringThreadSafe("\n");
        }
    }
}