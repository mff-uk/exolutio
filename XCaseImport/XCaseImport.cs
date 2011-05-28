using System.IO;
using Exolutio.Model;

namespace Exolutio.XCaseImport
{
    public static class XCaseImport
    {
        public static Project LoadXCaseProjectFromFile(string fileName)
        {
            XCase.Model.ExolutioExport.ExolutioExport exolutioExport = new XCase.Model.ExolutioExport.ExolutioExport();
            Project project = exolutioExport.ConvertToExolutioProject(fileName);
            return project;
        }
    }
}