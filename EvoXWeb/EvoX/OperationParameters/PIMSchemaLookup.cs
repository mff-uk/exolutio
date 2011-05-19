using System;
using System.Web.UI.WebControls;
using EvoX.Model;
using EvoX.Model.PIM;

namespace EvoX.Web.OperationParameters
{
    public class PIMSchemaLookup : DropDownList,
                                   IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<PIMSchema>
    {
        public ProjectVersion ProjectVersion { get; set; }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        public void InitControl()
        {
            Items.Clear();

            ListItem listItem = new ListItem();
            listItem.Text = ProjectVersion.PIMSchema.ToString();
            listItem.Value = ProjectVersion.PIMSchema.ID.ToString();
            Items.Add(listItem);
            SelectedIndex = 0;
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SelectedValue = suggestedValue.ToString();
        }

        public PIMSchema Value
        {
            get
            {
                if (ProjectVersion != null)
                {
                    return ProjectVersion.Project.TranslateComponent<PIMSchema>((this as IOperationParameterControl<Guid>).Value);
                }
                else
                {
                    return null;
                }
            }
        }

        Guid IOperationParameterControl<Guid>.Value
        {
            get { return Guid.Parse(this.SelectedValue); }
        }
    }
}