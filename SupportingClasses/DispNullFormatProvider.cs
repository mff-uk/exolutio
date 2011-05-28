using System;

namespace Exolutio.SupportingClasses
{
    public class DispNullFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }


        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if ((format == "ShowNull" || format == "SN") && (arg == null || (arg is string && ((string)arg) == String.Empty)))
            {
                return "(null)";
            }
            else
            {
                return arg.ToString();
            }
        }

        private static DispNullFormatProvider _instance = new DispNullFormatProvider();

        public static DispNullFormatProvider Instance
        {
            get { return _instance; }
        }
    }
}