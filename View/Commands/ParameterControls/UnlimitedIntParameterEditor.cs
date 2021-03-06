using System.Windows.Controls;
using Exolutio.Model;

namespace Exolutio.View.Commands.ParameterControls
{
    public class UnlimitedintParameterEditor : TextBox, IOperationParameterControl<UnlimitedInt>, IOperationParameterControl
    {
        public UnlimitedInt Value
        {
            get { return UnlimitedInt.Parse(Text); }
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