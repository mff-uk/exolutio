using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Services;

namespace UpdaterService
{
    /// <summary>
    /// Summary description for UpdaterService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class UpdaterService : WebService
    {
        public UpdaterService()
        {

        }

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public Version GetExeVersion()
        {
            string path = MapBin("EvoX.exe");
            AssemblyName name = AssemblyName.GetAssemblyName(path);
            return name.Version;
        }


        [WebMethod]
        public Version GetVersion(string file)
        {
            string path = MapBin(file);
            AssemblyName name = AssemblyName.GetAssemblyName(path);
            return name.Version;
        }

        private string MapBin(string filename)
        {
            return Server.MapPath("EvoXbin/" + filename);
        }

        [Serializable]
        public struct FileWithVersion
        {
            public string File;
            public string Version;

            public FileWithVersion(string file, string version)
            {
                File = file;
                Version = version;
            }
        }

        [WebMethod]
        public List<FileWithVersion> GetVersions(List<string> files)
        {
            List<FileWithVersion> result = new List<FileWithVersion>();

            foreach (string file in files)
            {
                if (File.Exists(MapBin(file)))
                {
                    result.Add(new FileWithVersion(Path.GetFileName(file), AssemblyName.GetAssemblyName(MapBin(file)).Version.ToString()));
                }
                else
                {
                    result.Add(new FileWithVersion(Path.GetFileName(file), null));
                }
            }

            foreach (string file in Directory.GetFiles(MapBin(string.Empty), "*.dll").Concat(Directory.GetFiles(MapBin(string.Empty), "*.exe")))
            {
                string fileName = Path.GetFileName(file);
                if (!files.Contains(fileName))
                {
                    result.Add(new FileWithVersion(Path.GetFileName(file), AssemblyName.GetAssemblyName(MapBin(fileName)).Version.ToString()));
                }
            }

            return result;
        }

        [WebMethod]
        public bool MustReinstal(List<string> files, List<string> versions)
        {
            return false;
        }

        [WebMethod]
        public string GetMsiUri()
        {
            return this.Context.Request.Url.AbsoluteUri.Remove(this.Context.Request.Url.AbsoluteUri.IndexOf("UpdaterService.asmx")) + "GetFile.ashx?filename=EvoX.msi";
        }
    }
}
