using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Exolutio.SupportingClasses;
using Exolutio.View;

namespace Exolutio.WPFClient
{
    /// <summary>
    /// Interaction logic for FileTab.xaml
    /// </summary>
    public partial class FileTab 
    {
        public FileTab()
        {
            InitializeComponent();
        }

        public void DisplayFile(EDisplayedFileType displayedFileType, Stream fileContents)
        {
            fileView.DisplayFile(displayedFileType, fileContents);
        }

        public void DisplayFile(EDisplayedFileType displayedFileType, string fileContents)
        {
            fileView.DisplayFile(displayedFileType, fileContents);
        }
    }
}
