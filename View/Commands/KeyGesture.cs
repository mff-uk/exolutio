using System;
using System.Globalization;
using System.Windows.Input;

namespace EvoX.View.Commands
{
    public class KeyGesture
    {
        public Key Key { get; set; }
        public ModifierKeys Control { get; set; }

        public KeyGesture()
        {
            
        }

        public KeyGesture(Key key, ModifierKeys control) : this()
        {
            Key = key;
            Control = control;
        }

        public object GetDisplayStringForCulture(CultureInfo currentCulture)
        {
            return null;

        }
    }
}