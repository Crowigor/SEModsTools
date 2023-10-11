using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace SEModsTools.Services
{
    [ComVisible(true)]
    [Guid(SEModsToolsPackage.PackageGuidString)]
    [ProgId("SEModsTools.Services.ModTemplatesWizard")]
    public class TemplatesWizard : IWizard
    {
        private WizardInputForm InputForm;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(Project project)
        {

        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {

        }

        public void RunFinished()
        {

        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                InputForm = new WizardInputForm();

                if (InputForm.ShowDialog() == DialogResult.Cancel)
                {
                    throw new Exception("");
                }

                string gameBinPathValue = InputForm.GetGameBinPathValue();
                if (string.IsNullOrWhiteSpace(gameBinPathValue))
                {
                    throw new Exception("Please select a valid Game Bin Path.");
                }


                string modNameValue = InputForm.GetModNameValue();
                if (string.IsNullOrWhiteSpace(modNameValue))
                {
                    throw new Exception("Please enter a valid Mod name.");
                }

                string namespaceValue = InputForm.GetNamespaceValue();
                if (string.IsNullOrWhiteSpace(namespaceValue))
                {
                    throw new Exception("Please enter a valid Namespace.");
                }

                string modsFolderValue = "%AppData%\\SpaceEngineers\\Mods";

                string automaticUploadValue = InputForm.GetAutomaticUploaValue();
                if (string.IsNullOrWhiteSpace(namespaceValue))
                {
                    automaticUploadValue = "false";
                }

                replacementsDictionary.Add("$SEModsToolsGameBinPath$", gameBinPathValue);
                replacementsDictionary.Add("$SEModsToolsModName$", modNameValue);
                replacementsDictionary.Add("$SEModsToolsNamespace$", namespaceValue);
                replacementsDictionary.Add("$SEModsToolsModsFolder$", modsFolderValue);
                replacementsDictionary.Add("$SEModsToolsAutomaticUpload$", automaticUploadValue);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                throw new WizardBackoutException();
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public partial class WizardInputForm : Form
        {
            private Label LabelGameBinPath;
            private TextBox InputGameBinPath;
            private Button BrowseButtonGameBinPath;
            private Label LabelModName;
            private TextBox InputModName;
            private Label LabelNamespace;
            private TextBox InputNamespace;
            private Label LabelAutomaticUpload;
            private CheckBox InputAutomaticUpload;
            private Button ButtonOk;
            private Button ButtonCancel;

            public WizardInputForm()
            {
                InitializeForm();
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            private void InitializeForm()
            {
                this.Text = "Space Engineers Mod Properties";
                this.ClientSize = new System.Drawing.Size(600, 300);

                LabelGameBinPath = new Label
                {
                    Text = "Game Bin Path",
                    Location = new System.Drawing.Point(12, 15),
                    AutoSize = true
                };

                InputGameBinPath = new TextBox
                {
                    Location = new System.Drawing.Point(150, 12),
                    Size = new System.Drawing.Size(200, 20)
                };

                BrowseButtonGameBinPath = new Button
                {
                    Text = "Browse",
                    Location = new System.Drawing.Point(360, 10),
                };
                BrowseButtonGameBinPath.Click += ClickBrowseButtonGameBinPath;


                LabelModName = new Label
                {
                    Text = "Mod Name",
                    Location = new System.Drawing.Point(12, 45),
                    AutoSize = true
                };

                InputModName = new TextBox
                {
                    Text = "HelloWorld",
                    Location = new System.Drawing.Point(150, 42),
                    Size = new System.Drawing.Size(200, 20)
                };

                LabelNamespace = new Label
                {
                    Text = "Namespace",
                    Location = new System.Drawing.Point(12, 75),
                    AutoSize = true
                };

                InputNamespace = new TextBox
                {
                    Text = "MyFraction",
                    Location = new System.Drawing.Point(150, 72),
                    Size = new System.Drawing.Size(200, 20)
                };

                LabelAutomaticUpload = new Label
                {
                    Text = "Automatic Upload",
                    Location = new System.Drawing.Point(12, 110),
                    AutoSize = true
                };
                InputAutomaticUpload = new CheckBox
                {
                    Location = new System.Drawing.Point(150, 110),
                    AutoSize = true,
                };

                ButtonOk = new Button
                {
                    Text = "Create Project",
                    Location = new System.Drawing.Point(150, 140),
                    Size = new System.Drawing.Size(75, 23),
                };
                ButtonOk.Click += ClickButtonOk;

                ButtonCancel = new Button
                {
                    Text = "Cancel",
                    Location = new System.Drawing.Point(230, 140),
                    Size = new System.Drawing.Size(75, 23),
                    DialogResult = DialogResult.Cancel
                };

                this.Controls.Add(LabelGameBinPath);
                this.Controls.Add(InputGameBinPath);
                this.Controls.Add(BrowseButtonGameBinPath);
                this.Controls.Add(LabelModName);
                this.Controls.Add(InputModName);
                this.Controls.Add(LabelNamespace);
                this.Controls.Add(InputNamespace);
                this.Controls.Add(LabelAutomaticUpload);
                this.Controls.Add(InputAutomaticUpload);
                this.Controls.Add(ButtonOk);
                this.Controls.Add(ButtonCancel);
            }

            public string GetGameBinPathValue()
            {
                return InputGameBinPath.Text;
            }

            public string GetModNameValue()
            {
                return InputModName.Text;
            }

            public string GetNamespaceValue()
            {
                return InputNamespace.Text;
            }

            public string GetAutomaticUploaValue()
            {
                return (InputAutomaticUpload.Checked) ? "true" : "false";
            }

            private void ClickButtonOk(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(InputGameBinPath.Text))
                {
                    MessageBox.Show("Please select a valid Game Bin Path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private void ClickBrowseButtonGameBinPath(object sender, EventArgs e)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Folders|*.none";
                    openFileDialog.CheckFileExists = false;
                    openFileDialog.FileName = "Select Folder";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFolder = Path.GetDirectoryName(openFileDialog.FileName);
                        InputGameBinPath.Text = selectedFolder;
                    }
                }
            }
        }
    }
}
