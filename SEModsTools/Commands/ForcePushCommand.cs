using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SEModsTools.Commands
{
    internal sealed class ForcePushCommand
    {
        public const int CommandId = 512;
        public static readonly Guid CommandSet = new Guid("9657cb9f-323b-4e54-ba7a-00b32cf4cecb");
        private readonly AsyncPackage package;
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get { return this.package; } }
        public static ForcePushCommand Instance { get; private set; }

        private ForcePushCommand(AsyncPackage package, OleMenuCommandService commandService)
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
            Instance = new ForcePushCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            PushCommand.PushProject(true);
        }
    }
}
