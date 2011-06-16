using System;
using Exolutio.Model.PSM;

namespace Exolutio.Revalidation
{
    public class DocumentValidator
    {
        public bool ValidateDocument(PSMSchema schema, string text)
        {
            return true;
        }

        public bool SilentMode { get; set; }
    }
}