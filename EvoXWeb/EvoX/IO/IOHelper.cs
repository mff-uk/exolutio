using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EvoX.Model;
using EvoX.Model.Serialization;
using System.Linq;

namespace EvoX.Web.IO
{
    public static class IOHelper
    {
        public static IEnumerable<FileInfo> GetAvailableProjectFiles(DirectoryInfo directory)
        {
            FileInfo[] fileInfosX = directory.GetFiles("*.xml");
            FileInfo[] fileInfosE = directory.GetFiles("*.evox");
            return fileInfosX.Concat(fileInfosE);
        }

        public static Project LoadProjectFromFile(string selectedValue)
        {
            FileInfo f = new FileInfo(selectedValue);
            return LoadProjectFromFile(f);
        }

        public static Project LoadProjectFromString(string serializedProject)
        {
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project project = m.LoadProjectFromString(serializedProject);
            return project;
        }

        public static Project LoadProjectFromFile(FileInfo filePath)
        {
            if (!filePath.Exists)
            {
                throw new FileNotFoundException("File not found", filePath.FullName);
            }
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project project = m.LoadProject(filePath);
            return project;
        }

        public static string GetFileText(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            else return null;
        }

        public static string SerializeProjectToString(Project project)
        {
            ProjectSerializationManager m = new ProjectSerializationManager();
            return m.SaveProjectToString(project);
        }

        public static string LoadXCaseProjectFromStream(Stream fileContent)
        {
            string tempFileName = System.IO.Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(tempFileName);
            StreamReader sr = new StreamReader(fileContent);
            sw.Write(sr.ReadToEnd());
            sr.Close();
            sw.Flush();
            sw.Close();
            Model.Serialization.ProjectSerializationManager m = new ProjectSerializationManager();
            Project loaded = XCaseImport.XCaseImport.LoadXCaseProjectFromFile(tempFileName);
            return m.SaveProjectToString(loaded);
        }
    }
}