using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.Serialization
{
    public class ProjectSerializationManager
    {
        public Log Log { get; set; }

        public ProjectSerializationManager()
        {
            Log = new Log();
        }

        public void SaveProject(Project project, Stream stream)
        {
            SerializationContext context = new SerializationContext { Log = Log, Document = new XDocument() };
            context.Document.Declaration = new XDeclaration("1.0", "utf-8", null);
            project.Serialize(null, context);
            context.Document.Save(stream);
            project.HasUnsavedChanges = false;
        }

        public void SaveProject(Project project, FileInfo projectFile)
        {
            using (FileStream fileStream = projectFile.Open(FileMode.Create, FileAccess.Write))
            {
                SaveProject(project, fileStream);
            }
            project.ProjectFile = projectFile;
        }

        public void SaveProject(Project project, string projectFile)
        {
            FileInfo f = new FileInfo(projectFile);
            SaveProject(project, f);
        }

        public void SaveProject(Project project, StringWriter stringWriter)
        {
            SerializationContext context = new SerializationContext { Log = Log, Document = new XDocument() };
            context.Document.Declaration = new XDeclaration("1.0", "utf-8", null);
            project.Serialize(null, context);
            context.Document.Save(stringWriter);
            project.HasUnsavedChanges = false;
        }
        
        /// <param name="project"></param>
        public string SaveProjectToString(Project project)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            SaveProject(project, sw);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            sb.Replace("utf-16", "utf-8");
            return sb.ToString();
        }

        /// <param name="file">Project file</param>
        /// <exception cref="FileNotFoundException">File does not exist.</exception>
        public Project LoadProject(FileInfo file)
        {
            if (!file.Exists)
            {
                #if SILVERLIGHT
                throw new FileNotFoundException(string.Format("File '{0}' does not exist.", file.FullName));
                #else
                throw new FileNotFoundException("File does not exist.", file.FullName);
                #endif
            }

            SerializationContext context = new SerializationContext {Log = Log};
            context.Document = XDocument.Load(file.FullName);
            
            Project project = new Project();
            project.Deserialize(null, context);
            project.HasUnsavedChanges = false; 
            project.ProjectFile = file;
            return project;
        }

        public Project LoadProject(Stream projectStream)
        {
            SerializationContext context = new SerializationContext { Log = Log };
            context.Document = XDocument.Load(projectStream);

            Project project = new Project();
            project.Deserialize(null, context);
            project.HasUnsavedChanges = false;
            
            return project;
        }

        /// <param name="xml">XML representing the project</param>
        public Project LoadProjectFromString(string xml)
        {
            using (TextReader r = new StringReader(xml))
            {
                SerializationContext context = new SerializationContext { Log = Log,  };
                context.Document = XDocument.Load(r);
                Project project = new Project();
                project.Deserialize(null, context);
                project.HasUnsavedChanges = false;
                project.ProjectFile = null;
                return project;
            }
        }

        /// <param name="filePath">Full path of the destination file</param>
        public Project LoadProject(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            return LoadProject(file);
        }

        public Project CreateEmptyProject()
        {
            Project p = new Project();
            p.InitNewEmptyProject();
            p.HasUnsavedChanges = false;
            return p;
        }

        #if SILVERLIGHT
        public Project LoadProjectFromClientFile(FileInfo projectFile)
        {
            using (FileStream projectStream = projectFile.OpenRead())
            {
                return LoadProject(projectStream);
            }
        }
        #endif
    }
}