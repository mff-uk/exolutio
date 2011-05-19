using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace SilverlightClient.Web
{
    /// <summary>
    /// Summary description for ProjectFilesService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ProjectFilesService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public List<string> GetProjectFiles()
        {
            string[] strings = System.IO.Directory.GetFiles(Server.MapPath("~/ProjectFiles"));
            return strings.Select(s => new FileInfo(s).Name).ToList();
        }

        [WebMethod]
        public byte[] GetProjectFile(string filename)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(Server.MapPath("~/ProjectFiles/" + filename));
            return bytes; 
        }
    }
}
