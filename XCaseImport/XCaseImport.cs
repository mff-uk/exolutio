using System.IO;
using EvoX.Model;

namespace EvoX.XCaseImport
{
    public static class XCaseImport
    {
        public static Project LoadXCaseProjectFromFile(string fileName)
        {
            XCase.Model.EvoXExport.EvoXExport evoxExport = new XCase.Model.EvoXExport.EvoXExport();
            Project project = evoxExport.ConvertToEvoXProject(fileName);
            return project;
        }
    }
}