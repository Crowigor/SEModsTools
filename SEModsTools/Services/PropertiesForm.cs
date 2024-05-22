using EnvDTE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEModsTools.Services
{
    public partial class PropertiesForm : Form
    {
        private int FieldsMargin = 15;
        private int LabelsWith = 110;

        private Label ModNameLabel;
        private TextBox ModNameInput;

        private Label NamespaceLabel;
        private TextBox NamespaceInput;

        private Label GameBinPathLabel;
        private TextBox GameBinPathInput;
        private Button GameBinPathBrowseButton;

        private Label ModsFolderLabel;
        private TextBox ModsFolderInput;

        private Label AllowedExtensionsLabel;
        private TextBox AllowedExtensionsInput;

        private Label AutomaticUploadLabel;
        private CheckBox AutomaticUploadInput;

        private Button ApplyButton;
        private Button CloseButton;

        public PropertiesForm(string csproj = null)
        {
            InitializeForm();
            LoadFormData(csproj);
        }

        private void InitializeForm()
        {
            // Window
            Name = "Properties";
            Text = "Space Engineers Mod Properties";
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 0);
            StartPosition = FormStartPosition.CenterParent;

            // ModName
            ModNameLabel = GetLabel("Mod Name");
            ModNameInput = GetTextBox(ModNameLabel);
            Controls.Add(ModNameLabel);
            Controls.Add(ModNameInput);

            // Namespace
            NamespaceLabel = GetLabel("Namespace");
            NamespaceInput = GetTextBox(NamespaceLabel);
            Controls.Add(NamespaceLabel);
            Controls.Add(NamespaceInput);

            // GameBinPath
            GameBinPathLabel = GetLabel("Game Bin Path");
            GameBinPathBrowseButton = new Button
            {
                Text = "Browse",
                Location = new Point(LabelsWith, GameBinPathLabel.Top),
                AutoSize = true,
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            GameBinPathInput = new TextBox
            {
                Location = new Point(LabelsWith, GameBinPathLabel.Top),
                Size = new Size(ClientSize.Width - LabelsWith - GameBinPathBrowseButton.Width - 10, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            GameBinPathBrowseButton.Location = new Point(GameBinPathInput.Right + 5, GameBinPathLabel.Top);
            GameBinPathBrowseButton.Click += OnGameBinPathBrowseButtonClick;
            Controls.Add(GameBinPathLabel);
            Controls.Add(GameBinPathInput);
            Controls.Add(GameBinPathBrowseButton);

            // ModsFolder
            ModsFolderLabel = GetLabel("Mods Folder");
            ModsFolderInput = GetTextBox(ModsFolderLabel);
            Controls.Add(ModsFolderLabel);
            Controls.Add(ModsFolderInput);

            // AllowedExtensions
            AllowedExtensionsLabel = GetLabel("Allowed Extensions");
            AllowedExtensionsInput = GetTextBox(AllowedExtensionsLabel);
            Controls.Add(AllowedExtensionsLabel);
            Controls.Add(AllowedExtensionsInput);

            // AutomaticUpload
            AutomaticUploadLabel = GetLabel("Automatic Upload");
            AutomaticUploadInput = GetCheckBox(AutomaticUploadLabel);
            Controls.Add(AutomaticUploadLabel);
            Controls.Add(AutomaticUploadInput);

            // Buttons
            ApplyButton = new Button
            {
                Text = "Apply",
                Location = new Point(LabelsWith, GetFormHeight() + FieldsMargin),
                AutoSize = true,
                UseVisualStyleBackColor = true,
            };
            ApplyButton.Click += OnApplyButtonClick;
            CloseButton = new Button
            {
                Text = "Cancel",
                Location = new Point(ApplyButton.Right, ApplyButton.Top),
                AutoSize = true,
                UseVisualStyleBackColor = true,
                DialogResult = DialogResult.Cancel,
            };
            Controls.Add(ApplyButton);
            Controls.Add(CloseButton);

            // Layout           
            ClientSize = new Size(ClientSize.Width, GetFormHeight() + FieldsMargin);
        }

        private void LoadFormData(string csproj = null)
        {
            if (csproj != null)
            {
                ModProject modProject = new ModProject(csproj);
                if (modProject.IsModProject)
                {
                    ModNameInput.Text = modProject.ModName;
                    NamespaceInput.Text = modProject.Namespace;
                    GameBinPathInput.Text = modProject.GameBinPath;
                    ModsFolderInput.Text = modProject.ModsFolder;
                    AllowedExtensionsInput.Text = String.Join(",", modProject.AllowedExtensions);
                    AutomaticUploadInput.Checked = modProject.AutomaticUpload;
                }
            }

            if (String.IsNullOrEmpty(ModNameInput.Text))
            {
                ModNameInput.Text = ModProject.DefaultProperties["SEModsToolsModName"];
            }

            if (String.IsNullOrEmpty(NamespaceInput.Text))
            {
                NamespaceInput.Text = ModProject.DefaultProperties["RootNamespace"];
            }

            if (String.IsNullOrEmpty(ModsFolderInput.Text))
            {
                ModsFolderInput.Text = ModProject.DefaultProperties["SEModsToolsModsFolder"];
            }

            if (String.IsNullOrEmpty(AllowedExtensionsInput.Text))
            {
                AllowedExtensionsInput.Text = ModProject.DefaultProperties["SEModsToolsAllowedExtensions"];
            }
        }

        public Dictionary<string, string> GetReplacesValues()
        {
            return new Dictionary<string, string>
            {
                {"SEModsToolsModName", ModNameInput.Text},
                {"RootNamespace", NamespaceInput.Text},
                {"SEModsToolsGameBinPath", GameBinPathInput.Text},
                {"SEModsToolsModsFolder", ModsFolderInput.Text},
                {"SEModsToolsAllowedExtensions", AllowedExtensionsInput.Text},
                {"SEModsToolsAutomaticUpload", (AutomaticUploadInput.Checked)? "true": "false"},
            };
        }
        private void OnGameBinPathBrowseButtonClick(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Folders|*.none";
                openFileDialog.CheckFileExists = false;
                openFileDialog.FileName = "Select Folder";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = Path.GetDirectoryName(openFileDialog.FileName);
                    GameBinPathInput.Text = selectedFolder;
                }
            }
        }

        private void OnApplyButtonClick(object sender, EventArgs e)
        {
            Dictionary<string, string> values = new Dictionary<string, string> {
                {ModNameLabel.Text, ModNameInput.Text},
                {NamespaceLabel.Text, NamespaceInput.Text},
                {GameBinPathLabel.Text, GameBinPathInput.Text},
                {ModsFolderLabel.Text, ModsFolderInput.Text},
                {AllowedExtensionsLabel.Text, AllowedExtensionsInput.Text},
            };

            foreach (var field in values)
            {
                if (String.IsNullOrEmpty(field.Value))
                {
                    MessageBox.Show($"Please enter a valid {field.Key} value", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected Label GetLabel(string text)
        {
            return new Label
            {
                Text = text,
                Location = new Point(10, GetFormHeight() + FieldsMargin),
                AutoSize = true
            };
        }

        protected TextBox GetTextBox(Label label)
        {
            return new TextBox
            {
                Location = new Point(LabelsWith, label.Top),
                Size = new Size(ClientSize.Width - LabelsWith - 10, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
        }

        protected CheckBox GetCheckBox(Label label)
        {
            return new CheckBox
            {
                Location = new Point(LabelsWith, label.Top),
                AutoSize = true,
            };
        }

        protected int GetFormHeight()
        {
            int height = 0;
            if (Controls.Count > 0)
            {
                foreach (Control control in Controls)
                {
                    int controlBottom = control.Bottom;
                    if (controlBottom > height)
                    {
                        height = controlBottom;
                    }
                }
            }

            return height;
        }
    }
}
