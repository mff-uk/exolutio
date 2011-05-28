<%@ WebHandler Language="C#" Class="GetFile" %>

using System;
using System.IO;
using System.Web;

public class GetFile : IHttpHandler {

    public void ProcessRequest(HttpContext context)
    {
        if (context.Request.QueryString["filename"] != null)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
            context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(2));
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(2));
            context.Response.Cache.SetNoTransforms();

            string filename = context.Request.QueryString["FileName"];

            string ph = Path.Combine(context.Server.MapPath("ExolutioBin"), filename);

            if (!File.Exists(ph))
            {
                throw new FileNotFoundException("File not found.", System.IO.Path.GetFileName(filename));
            }

            FileInfo fi = new FileInfo(ph);

            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");
            context.Response.AddHeader("Content-Length", fi.Length.ToString());

            // Tohle urÄŤuje, jak pochopĂ­ data browser 
            context.Response.ContentType = "application/octet-stream";

            context.Response.WriteFile(ph);
        }
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}