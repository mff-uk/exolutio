using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EvoX.View
{
    #if SILVERLIGHT
    #else
    [ValueConversion(typeof(string), typeof(Visibility))]
    #endif
    public class IconConverter : IValueConverter
    {
        private static readonly Dictionary<ImageSource, ImageBrush>
            _sharedIconBrushDictionary = new Dictionary<ImageSource, ImageBrush>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageSource)
            {
                ImageBrush brush;
                ImageSource imageSource = (ImageSource)value;

                if (_sharedIconBrushDictionary.ContainsKey(imageSource))
                {
                    brush = _sharedIconBrushDictionary[imageSource];
                }
                else
                {
                    brush = new ImageBrush() { ImageSource = imageSource };
                    _sharedIconBrushDictionary[imageSource] = brush;
                }

                Rectangle rect = new Rectangle { Width = 16, Height = 16 };
                rect.Fill = brush;
                return rect;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #if SILVERLIGHT
            return null;
            #else
            return Binding.DoNothing;
            #endif
        }
    }
}