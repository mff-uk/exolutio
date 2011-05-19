using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using EvoX.SupportingClasses;

namespace EvoX.WPFClient
{
    public class ConfigStruct
    {
        private readonly List<FileInfo> recentFiles = new List<FileInfo>();

        private readonly List<DirectoryInfo> recentDirectories = new List<DirectoryInfo>();

        public IEnumerable<FileInfo> RecentFiles
        {
            get { return recentFiles; }
        }

        public IEnumerable<DirectoryInfo> RecentDirectories
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
        public const string UserDataFolder = "EvoX";

        public const string ConfigFileName = "UserConfig.xml";

        public const string LayoutFileName = "Layout.xml";

        private static string EvoXUserDataDir
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
                string evoXDataDir = EvoXUserDataDir;
                if (!Directory.Exists(evoXDataDir))
                {
                    Directory.CreateDirectory(evoXDataDir);
                }
                return Path.Combine(evoXDataDir, ConfigFileName);
            }
        }

        public static string LayoutFilePath
        {
            get { return Path.Combine(EvoXUserDataDir, LayoutFileName); }
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

        private static readonly XNamespace evoXNS = @"http://evox.ms.mff.cuni.cz/";

        public static void LoadConfiguration()
        {
            if (File.Exists(ConfigFilePath))
            {
                XDocument doc = XDocument.Load(ConfigFilePath);
                XElement elConfiguration = doc.Element(evoXNS + "Configuration");

                {
                    XElement elRecentFiles = elConfiguration.Element(evoXNS + "RecentFiles");
                    if (elRecentFiles != null)
                    {
                        foreach (XElement elFile in elRecentFiles.Elements(evoXNS + "File"))
                        {
                            Configuration.AddToRecentFiles(new FileInfo(elFile.Value), false, false);
                        }
                    }
                }

                {
                    XElement elRecentDirectories = elConfiguration.Element(evoXNS + "RecentDirectories");
                    if (elRecentDirectories != null)
                    {
                        foreach (XElement elDirectory in elRecentDirectories.Elements(evoXNS + "Directory"))
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
            XElement elConfiguration = new XElement(evoXNS + "Configuration");
            elConfiguration.Add(new XAttribute(XNamespace.Xmlns + "evox", evoXNS.NamespaceName));
            doc.Add(elConfiguration);
            XElement elRecentFiles = new XElement(evoXNS + "RecentFiles");
            XElement elRecentDirectories = new XElement(evoXNS + "RecentDirectories");
            elConfiguration.Add(elRecentFiles);
            elConfiguration.Add(elRecentDirectories);

            foreach (FileInfo recentFile in Configuration.RecentFiles)
            {
                XElement xElement = new XElement(evoXNS + "File");
                xElement.Value = recentFile.FullName;
                elRecentFiles.Add(xElement);
            }

            foreach (DirectoryInfo recentDirectory in Configuration.RecentDirectories)
            {
                XElement xElement = new XElement(evoXNS + "Directory");
                xElement.Value = recentDirectory.FullName;
                elRecentDirectories.Add(xElement);
            }

            doc.Save(ConfigFilePath);
        }
    }
}