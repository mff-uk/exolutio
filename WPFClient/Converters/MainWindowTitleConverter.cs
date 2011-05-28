using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Exolutio.Model;
using System.Windows;

namespace Exolutio.WPFClient.Converters
{
    public class MainWindowTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Project project = (Project) value;
            string title; 

            if (project.ProjectFile == null)
            {
                title = "Untitled.eXo";
            }
            else
            {
                title = project.ProjectFile.FullName;
            }

            if (project.HasUnsavedChanges == true)
            {
                title += "*";
            }

            title += " - eXolutio";

            return title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}