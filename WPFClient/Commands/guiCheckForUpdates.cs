using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.View;
using Exolutio.View.Commands;
using Exolutio.WPFClient;

namespace Exolutio.WPFClient
{
    public class guiCheckForUpdates : guiCommandBase
    {
        public override void Execute(object parameter)
        {
            Updater.Updater updater = new Updater.Updater();
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll").Concat(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.exe"));

            Dictionary<string, Version> clientVersions = new Dictionary<string, Version>();
            foreach (string file in files)
            {
                Version clientVersion = AssemblyName.GetAssemblyName(file).Version;
                clientVersions[Path.GetFileName(file)] = clientVersion;
            }

            Dictionary<string, Version> newAvailableVersions;
            if (updater.AreNewVersionsAvailable(clientVersions, out newAvailableVersions) && ExolutioYesNoBox.ShowYesNoCancel("New version available", "New version is available. \r\nDo you wish to update?") == MessageBoxResult.Yes )
            {
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("Exolutio.Updater.exe");
                info.UseShellExecute = true;
                (Current.MainWindow).Close();
                System.Diagnostics.Process.Start(info);
            }
            else
            {
                ExolutioMessageBox.Show("Exolutio Update", "Updates checked", "This is the latest version.");
            }
        }

        public override string Text
        {
            get { return "Check for updates"; }
        }

        public override string ScreenTipText
        {
            get { return "Check for updates"; }
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        //public override ImageSource Icon
        //{
        //    get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.XXX); }
        //}

    }
}