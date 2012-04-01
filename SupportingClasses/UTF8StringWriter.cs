using System;
using System.IO;
using System.Text;

namespace Exolutio.SupportingClasses
{
    public class UTF8StringWriter: StringWriter 
    {
        public UTF8StringWriter(IFormatProvider formatProvider) : base(formatProvider)
        {
        }

        public UTF8StringWriter(StringBuilder sb) : base(sb)
        {
        }

        public UTF8StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider)
        {
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        public UTF8StringWriter()
        {
        }
    }
}