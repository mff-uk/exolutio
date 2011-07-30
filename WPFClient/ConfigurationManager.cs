using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Exolutio.SupportingClasses;

namespace Exolutio.WPFClient
{
    public class ConfigStruct
    {
        private readonly List<FileInfo> recentFiles = new List<FileInfo>();

        private readonly List<DirectoryInfo> recentDirectories = new List<DirectoryInfo>();

        public List<FileInfo> RecentFiles
        {
            get { return recentFiles; }
        }

        public List<DirectoryInfo> RecentDirectories
        {
            get { return recentDirectories; }
        }

        public void AddToRecentFiles(FileInfo file, bool addAsFirst = true, bool addRecentDirectory = true)
        {
            recentFiles.RemoveAll(info => info.FullName == file.FullName);
            if (addAsFirst)
                recentFiles.Insert(0, file);
            else
                recentFiles.Add(file);

            if (addRecentDirectory)
                AddToRecentDirectories(file.Directory, addAsFirst);
        }

        public void AddToRecentDirectories(DirectoryInfo directory, bool addAsFirst = true)
        {
            recentDirectories.RemoveAll(info => info.FullName == directory.FullName);
            if (addAsFirst)
                recentDirectories.Insert(0, directory);
            else
                recentDirectories.Add(directory);
        }
    }

    public class ConfigurationManager
    {
        public const string UserDataFolder = "eXolutio";

        public const string ConfigFileName = "UserConfig.xml";

        public const string LayoutFileName = "Layout.xml";

        private static string ExolutioUserDataDir
        {
            get
            {
                string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appDataDir, UserDataFolder);
            }
        }

        public static string ConfigFilePath
        {
            get
            {
                string exolutioDataDir = ExolutioUserDataDir;
                if (!Directory.Exists(exolutioDataDir))
                {
                    Directory.CreateDirectory(exolutioDataDir);
                }
                return Path.Combine(exolutioDataDir, ConfigFileName);
            }
        }

        public static string LayoutFilePath
        {
            get { return Path.Combine(ExolutioUserDataDir, LayoutFileName); }
        }

        private static ConfigStruct configuration;

        public static ConfigStruct Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = new ConfigStruct();
                }
                return configuration;
            }
        }

        public static bool HasStoredLayout
        {
            get { return File.Exists(LayoutFilePath); }
        }

        private static readonly XNamespace exolutioNS = @"http://eXolutio.eu/Project/Configuration/";

        public static void LoadConfiguration()
        {
            if (File.Exists(ConfigFilePath))
            {
                XDocument doc = XDocument.Load(ConfigFilePath);
                XElement elConfiguration = doc.Element(exolutioNS + "Configuration");

                {
                    XElement elRecentFiles = elConfiguration.Element(exolutioNS + "RecentFiles");
                    if (elRecentFiles != null)
                    {
                        foreach (XElement elFile in elRecentFiles.Elements(exolutioNS + "File"))
                        {
                            Configuration.AddToRecentFiles(new FileInfo(elFile.Value), false, false);
                        }
                    }
                }

                {
                    XElement elRecentDirectories = elConfiguration.Element(exolutioNS + "RecentDirectories");
                    if (elRecentDirectories != null)
                    {
                        foreach (XElement elDirectory in elRecentDirectories.Elements(exolutioNS + "Directory"))
                        {
                            Configuration.AddToRecentDirectories(new DirectoryInfo(elDirectory.Value), false);
                        }
                    }
                }
            }
        }

        public static void SaveConfiguration()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement elConfiguration = new XElement(exolutioNS + "Configuration");
            elConfiguration.Add(new XAttribute(XNamespace.Xmlns + "eXoConf", exolutioNS.NamespaceName));
            doc.Add(elConfiguration);
            XElement elRecentFiles = new XElement(exolutioNS + "RecentFiles");
            XElement elRecentDirectories = new XElement(exolutioNS + "RecentDirectories");
            elConfiguration.Add(elRecentFiles);
            elConfiguration.Add(elRecentDirectories);

            foreach (FileInfo recentFile in Configuration.RecentFiles)
            {
                XElement xElement = new XElement(exolutioNS + "File");
                xElement.Value = recentFile.FullName;
                elRecentFiles.Add(xElement);
            }

            foreach (DirectoryInfo recentDirectory in Configuration.RecentDirectories)
            {
                XElement xElement = new XElement(exolutioNS + "Directory");
                xElement.Value = recentDirectory.FullName;
                elRecentDirectories.Add(xElement);
            }

            doc.Save(ConfigFilePath);
        }
    }
}