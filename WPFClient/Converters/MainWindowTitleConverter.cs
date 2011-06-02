using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Exolutio.Model;
using System.Windows;

namespace Exolutio.WPFClient.Converters
{
    public class MainWindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result;
            string title = (string) values[0];
            bool hasUnsavedChanges = (bool) values[1];

            if (string.IsNullOrEmpty(title))
            {
                result = "Untitled.eXo";
            }
            else
            {
                result = title;
            }

            if (hasUnsavedChanges == true)
            {
                result += "*";
            }

            result += " - eXolutio";

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}