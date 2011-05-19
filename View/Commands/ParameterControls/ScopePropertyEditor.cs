using System;
using System.Windows.Controls;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;

namespace EvoX.View.Commands.ParameterControls
{
    public class ScopePropertyEditor : Control,
        IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<EvoXObject>
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

        public EvoXObject Value
        {
            get
            {
                if (valueGuid != Guid.Empty)
                    return ProjectVersion.Project.TranslateComponent<EvoXObject>(valueGuid);
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