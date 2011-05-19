using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;


namespace EvoX.Updater
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UpdaterWindow
    {
        public UpdaterWindow()
        {
            InitializeComponent();

            Updater = new Updater();

            bCheck_Click(null, null);
        }

        Dictionary<string, Version> newAvailableVersions;
        private Dictionary<string, Version> clientVersions;

        private string labelMessage;
        private bool bDownloadEnabled;
        private bool mustReinstal;

        private void CheckForNewVersions()
        {
            clientVersions = GetClientVersions();

            if (Updater.AreNewVersionsAvailable(clientVersions, out newAvailableVersions))
            {
                labelMessage = "New version is available. ";
                if (Updater.MustReinstal(clientVersions))
                {
                    mustReinstal = true;

                }
                else
                {
                    bDownloadEnabled = true;
                }
            }
            else
            {
                labelMessage = "You are using the latest version. ";
                bDownloadEnabled = false;
            }

            Action sr = ShowResult;
            this.Dispatcher.Invoke(sr);
        }

        private void Reinstall()
        {
            try
            {
                downloadSuccessfull =
                    delegate(Dictionary<string, string> files)
                    {
                        if (File.Exists(files.First().Key))
                            File.Delete(files.First().Key);
                        File.Move(files.First().Value, files.First().Key);
                        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("EvoX.msi");
                        info.UseShellExecute = true;
                        this.Dispatcher.Invoke(new Action(this.Close));
                        System.Diagnostics.Process.Start(info);
                    };
                StartDownloading(new[] { "EvoX.msi" });
            }
            catch
            {
                downloadSuccessfull = null;
                throw;
            }
        }

        private Dictionary<string, Version> GetClientVersions()
        {
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll").Concat(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.exe"));

            Dictionary<string, Version> clientVersions = new Dictionary<string, Version>();
            foreach (string file in files)
            {
                Version clientVersion = AssemblyName.GetAssemblyName(file).Version;
                clientVersions[Path.GetFileName(file)] = clientVersion;
            }
            return clientVersions;
        }

        public Updater Updater { get; set; }

        private void bCheck_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            label1.Content = "Checking for new version...";
            try
            {
                labelMessage = null;
                bDownloadEnabled = false;
                mustReinstal = false;

                ThreadStart parameterizedThreadStart = CheckForNewVersions;
                Thread thread = new Thread(parameterizedThreadStart);
                thread.Start();
            }
            catch (Exception ex)
            {
                label1.Content = "Update failed. " + ex.Message;
                bDownload.IsEnabled = false;
                Cursor = Cursors.Arrow;
            }
        }

        private void ShowResult()
        {
            if (bDownloadEnabled)
            {
                bDownload.IsEnabled = true;
            }
            if (!string.IsNullOrEmpty(labelMessage))
            {
                label1.Content = labelMessage;
            }
            if (mustReinstal)
            {
                if (MessageBox.Show("Automatic update can not continue, new version must be installed. \r\nProceed with instalation now?", "Reainstallation needed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Reinstall();
                }
            }
            bDownloadActual.IsEnabled = true;
            Cursor = Cursors.Arrow;
        }

        private void bDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                downloadSuccessfull =
                    delegate(Dictionary<string, string> files)
                    {
                        while(!clientFree)
                        {
                            Thread.Sleep(10);
                        }
                        foreach (KeyValuePair<string, string> downloadedFile in files)
                        {
                            if (downloadedFile.Key == "EvoX.Updater.exe")
                                continue;
                            if (File.Exists(downloadedFile.Key))
                                File.Delete(downloadedFile.Key);
                            File.Move(downloadedFile.Value, downloadedFile.Key);
                        }
                        label1.Dispatcher.Invoke(new Action(() => label1.Content = "Update successful."));
                    };
                StartDownloading(newAvailableVersions.Keys);
            }
            catch
            {
                downloadSuccessfull = null;
                throw;
            }
        }

        private void StartDownloading(IEnumerable<string> files)
        {
            bDownload.IsEnabled = false;
            bCheck.IsEnabled = false;
            bCancel.Visibility = Visibility.Visible;
            if (files != null && files.Count() > 0)
            {
                // Let the user know we are connecting to the server
                lblProgress.Content = "Download Starting";
                // Create a new thread that calls the Download() method
                thrDownload = new Thread(_files => Download((IEnumerable<string>)_files));
                // Start the thread, and thus vcall Download()
                thrDownload.Start(files);
            }
        }

        #region fields
        // The thread inside which the download happens
        private Thread thrDownload;
        // The stream of data retrieved from the web server
        private Stream strResponse;
        // The stream of data that we write to the harddrive
        private FileStream strLocal;
        // The request to the web server for file information
        private HttpWebRequest webRequest;
        // The response from the web server containing information about the file
        private HttpWebResponse webResponse;
        // The progress of the download in percentage
        private static int PercentProgress;
        #endregion

        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            // Make progress on the progress bar
            prgDownload.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => { prgDownload.Value = PercentProgress; }));

            // Display the current progress on the form
            lblProgress.Dispatcher.Invoke(new Action(() => lblProgress.Content = string.Format("Downloaded {0} kB out of {1} kB ({2}%).", BytesRead / 1000, TotalBytes / 1000, PercentProgress)));
        }

        private Dictionary<string, string> downloadedFiles;
        private bool clientFree = true;

        private void Download(IEnumerable<string> files)
        {
            downloadedFiles = new Dictionary<string, string>();
            using (WebClient wcDownload = new WebClient())
            {
                try
                {
                    int i = 0;
                    wcDownload.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wcDownload_DownloadProgressChanged);
                    wcDownload.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wcDownload_DownloadFileCompleted);
                    foreach (string file in files)
                    {
                        i++;
                        string downloadUrl = Updater.GetDownloadUrl(file);
                        lFile.Dispatcher.Invoke(new Action(() => lFile.Content = String.Format("Downloading file {0} (file {1} of {2})", file, i, newAvailableVersions.Count)));
                        prgDownload.Dispatcher.Invoke(new Action(() => { prgDownload.Visibility = Visibility.Visible; prgDownload.Value = 0; }));
                        string tmpFile = file + ".tmp";
                        downloadedFiles[file] = tmpFile;
                        //wcDownload.DownloadFile(downloadUrl, tmpFile
                        while (!clientFree)
                        {
                            Thread.Sleep(10);
                        }
                        clientFree = false;
                        wcDownload.DownloadFileAsync(new Uri(downloadUrl), tmpFile);


                        /*
                        // Create a request to the file we are downloading
                        webRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
                        // Set default authentication for retrieving the file
                        webRequest.Credentials = CredentialCache.DefaultCredentials;
                        // Retrieve the response from the server
                        webResponse = (HttpWebResponse)webRequest.GetResponse();
                        // Ask the server for the file size and store it
                        Int64 fileSize = webResponse.ContentLength;

                        
                        // Open the URL for download 
                        strResponse = wcDownload.OpenRead(downloadUrl);
                        // Create a new file stream where we will be saving the data (local drive)
                        strLocal = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None);

                        // It will store the current number of bytes we retrieved from the server
                        int bytesSize = 0;
                        // A buffer for storing and writing the data retrieved from the server
                        byte[] downBuffer = new byte[2048];

                        // Loop through the buffer until the buffer is empty
                        while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            // Write the data from the buffer to the local hard drive
                            strLocal.Write(downBuffer, 0, bytesSize);
                            // Invoke the method that updates the form's label and progress bar
                            UpdateProgress(strLocal.Length, fileSize);
                        }
                        strLocal.Flush(true);
                        strLocal.Close();
                         */
                    }
                }
                finally
                {
                    // When the above code has ended, close the streams
                    //strResponse.Close();
                    //strLocal.Close();

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        prgDownload.Visibility = Visibility.Collapsed;
                        lFile.Content = null;
                        bCancel.Visibility = Visibility.Hidden;
                        lblProgress.Content = null;
                        bCheck.IsEnabled = true;
                    }));
                }

                if (downloadSuccessfull != null)
                    downloadSuccessfull(downloadedFiles);
            }
        }

        void wcDownload_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            clientFree = true;
        }

        void wcDownload_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateProgress(e.BytesReceived, e.TotalBytesToReceive);
        }

        private delegate void DownloadSuccessfullHandler(Dictionary<string, string> downloadedFiles);

        private DownloadSuccessfullHandler downloadSuccessfull;

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the web response and the streams
            webResponse.Close();
            strResponse.Close();
            strLocal.Close();
            // Abort the thread that's downloading
            thrDownload.Abort();
            // Set the progress bar back to 0 and the label
            prgDownload.Value = 0;
            lblProgress.Content = "Download cancelled.";

            foreach (string tmpFile in downloadedFiles.Values)
            {
                try
                {
                    if (File.Exists(tmpFile))
                        File.Delete(tmpFile);
                }
                catch (Exception)
                {

                }
            }

            prgDownload.Visibility = Visibility.Collapsed;
            lFile.Content = null;
            lblProgress.Content = null;
            bCheck.IsEnabled = true;
            bDownload.IsEnabled = true;
            bCancel.Visibility = Visibility.Hidden;
        }

        private void bDownloadActual_Click(object sender, RoutedEventArgs e)
        {

            clientVersions = new Dictionary<string, Version>();
            Updater.AreNewVersionsAvailable(clientVersions, out newAvailableVersions);
            downloadSuccessfull =
                delegate(Dictionary<string, string> files)
                {
                    while (!clientFree)
                    {
                        Thread.Sleep(10);
                    }
                    foreach (KeyValuePair<string, string> downloadedFile in files)
                    {
                        try
                        {
                            if (downloadedFile.Key == "EvoX.Updater.exe")
                                continue;
                            if (File.Exists(downloadedFile.Key))
                                File.Delete(downloadedFile.Key);
                            File.Move(downloadedFile.Value, downloadedFile.Key);
                        }
                        catch (Exception ex)
                        {
                            if (strLocal != null)
                            {
                                strLocal.Close();
                            }
                            downloadSuccessfull = null;
                            label1.Dispatcher.Invoke(new Action(() => label1.Content = "Update failed, try running again."));
                            MessageBox.Show("Failed to update file: " + downloadedFile.Key + ", " + ex.Message);
                            break;
                        }
                        label1.Dispatcher.Invoke(new Action(() => label1.Content = "Update successful."));
                    }

                };
            StartDownloading(newAvailableVersions.Keys);
        }
    }
}
