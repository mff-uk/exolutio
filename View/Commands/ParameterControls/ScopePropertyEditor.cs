using System;
using System.Windows.Controls;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.View.Commands.ParameterControls
{
    public class ScopePropertyEditor : Control,
        IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<ExolutioObject>
    {
        public delegate bool VerifyDelegate(params object[] verifiedObjects);

        public bool AllowNullInput { get; set; }

        public Type LookedUpType { get; set; }

        public ProjectVersion ProjectVersion { get; set; }

        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            // do nothing
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        private Guid valueGuid; 

        public ExolutioObject Value
        {
            get
            {
                if (valueGuid != Guid.Empty)
                    return ProjectVersion.Project.TranslateComponent<ExolutioObject>(valueGuid);
                else
                    return null;
            }
            set 
            { 
                valueGuid = value.ID;
                if (SelectionChanged != null)
                    SelectionChanged(null, null);
            }
        }

        Guid IOperationParameterControl<Guid>.Value
        {
            get { return valueGuid; }
        }

        public event EventHandler<EventArgs> SelectionChanged;
    }
}