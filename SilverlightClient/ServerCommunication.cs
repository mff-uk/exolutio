using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using Exolutio.Model;
using Exolutio.Model.Serialization;
using Exolutio.View;
using SilverlightClient.ExolutioService;

namespace SilverlightClient.W
{
    public static class ServerCommunication
    {
        public static readonly ProjectSerializationManager ProjectSerializationManager = new ProjectSerializationManager();

        public static void LoadWebFile(string file)
        {
            //WebClient client = new WebClient();
            ProjectFilesServiceSoapClient client = new ProjectFilesServiceSoapClient();
            client.GetProjectFileCompleted += client_GetProjectFileCompletedCompleted;
            client.GetProjectFileAsync(file);
            
            //client.OpenReadAsync(new Uri(filePathString, UriKind.Relative));
        }
        
        public static event Action<Project> ProjectLoaded;

        private static void SerializationCallback(Project project)
        {
            Current.Project = project;
        }

        public static void client_GetProjectFileCompletedCompleted(object sender, GetProjectFileCompletedEventArgs args)
        {
            try
            {
                MemoryStream ms = new MemoryStream(args.Result);
                Project p = ProjectSerializationManager.LoadProject(ms);
                Current.Project = p;
                if (ProjectLoaded != null)
                {
                    ProjectLoaded(p);
                }
            }
            catch
            {

            }
        }

        public static void GetServerProjects()
        {

            try
            {
                ProjectFilesServiceSoapClient client = new ProjectFilesServiceSoapClient();
                client.GetProjectFilesCompleted += ServerProjectListLoaded;
                client.GetProjectFilesAsync();
            }
            catch 
            {
                
            }
            return;
        }

        public static event EventHandler<GetProjectFilesCompletedEventArgs> ServerProjectListLoaded;        
    }
}