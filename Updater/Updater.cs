using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using EvoX.Updater.UpdaterService;
using Version = System.Version;

namespace EvoX.Updater
{
    public class Updater
    {
        private UpdaterService.UpdaterServiceSoapClient service;

        public UpdaterService.UpdaterServiceSoapClient UpdaterService
        {
            get 
            {
                if (service == null)
                {
                    service = new UpdaterService.UpdaterServiceSoapClient();
                }
                return service; 
            }
        }

        public bool MustReinstal(Dictionary<string, Version> clientVersions)
        {
            ArrayOfString keys = new ArrayOfString();
            ArrayOfString versions = new ArrayOfString();
            keys.AddRange(clientVersions.Keys);
            versions.AddRange(clientVersions.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.ToString()));
            return UpdaterService.MustReinstal(keys, versions);
        }

        public bool AreNewVersionsAvailable(Dictionary<string, Version> clientVersions, out Dictionary<string, Version> newAvailableVersions)
        {
            ArrayOfString keys = new ArrayOfString();
            keys.AddRange(clientVersions.Keys);
            FileWithVersion[] _versions = UpdaterService.GetVersions(keys);
            Dictionary<string, Version> versions = new Dictionary<string, Version>();
            foreach (FileWithVersion fileWithVersion in _versions)
            {
                if (!string.IsNullOrEmpty(fileWithVersion.Version))
                    versions.Add(fileWithVersion.File, new Version(fileWithVersion.Version));
                //_versions.Select(_v => string.IsNullOrEmpty(_v) ? null : new Version(_v)).ToList();
            }
            
            
            newAvailableVersions = new Dictionary<string, Version>();

            int i = 0;
            foreach (KeyValuePair<string, Version> kvp in clientVersions)
            {
                Version serverVersion = versions.ContainsKey(kvp.Key) ? kvp.Value : null;
                Version clientVersion = kvp.Value;

                if (serverVersion != null)
                {
                    if (serverVersion > clientVersion)
                        newAvailableVersions[kvp.Key] = serverVersion;
                }
                i++;
            }

            foreach (KeyValuePair<string, Version> kvp in versions)
            {
                if (!clientVersions.ContainsKey(kvp.Key))
                {
                    newAvailableVersions[kvp.Key] = kvp.Value;
                }
            }

            return newAvailableVersions.Count > 0;
        }

        public string GetDownloadUrl(string file)
        {
            return string.Format("{0}GetFile.ashx?filename={1}",
                UpdaterService.Endpoint.Address.ToString().Remove(UpdaterService.Endpoint.Address.ToString().IndexOf("UpdaterService.asmx")), file);
        }
    }
}