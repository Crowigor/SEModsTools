using System;
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
                PropertiesForm form = new PropertiesForm();

                if (form.ShowDialog() == DialogResult.Cancel)
                {
                    throw new Exception("");
                }

                foreach (var keypear in form.GetReplacesValues())
                {
                    replacementsDictionary.Add("$" + keypear.Key + "$", keypear.Value);
                }
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
    }
}
