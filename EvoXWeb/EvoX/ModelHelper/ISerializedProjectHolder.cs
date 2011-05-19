using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Web.ModelHelper
{
    public interface ISerializedProjectHolder
    {
        string SerializedProject { get; set; }

        void DisplayProject(string serializedText, bool updateSession = false);
    }
}
