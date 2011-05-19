using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using EvoX.Model;
using System.Windows;

namespace EvoX.WPFClient.Converters
{
    public class MainWindowTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string title;

            if (values[0] == DependencyProperty.UnsetValue)
            {
                title = "EvoX";
            }
            else
            {
                FileInfo projectfile = values[0] as FileInfo;
                bool? hasUnsavedChanges = values[1] as bool?;
                if (projectfile == null)
                {
                    title = "Untitled.EvoX";
                }
                else
                {
                    title = projectfile.FullName;
                }

                if (hasUnsavedChanges == true)
                {
                    title += "*";
                }

                title += " - EvoX";
            }

            return title;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}