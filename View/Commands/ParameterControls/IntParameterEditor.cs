using System.Windows.Controls;

namespace EvoX.View.Commands.ParameterControls
{
    public class IntParameterEditor: TextBox, IOperationParameterControl<uint>, IOperationParameterControl
    {
        public uint Value
        {
            get { return uint.Parse(Text); }
        }

        public void InitControl()
        {
            
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.Text = suggestedValue.ToString();
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }
    }
}