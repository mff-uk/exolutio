using System;
using System.Web.UI.WebControls;
using EvoX.Model;
using EvoX.Model.PSM;

namespace EvoX.Web.OperationParameters
{
    public class PSMSchemaLookup : DropDownList,
                                   IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<PSMSchema>
    {
        public ProjectVersion ProjectVersion { get; set; }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        public void InitControl()
        {
            Items.Clear();
            foreach (PSMSchema psmSchema in ProjectVersion.PSMSchemas)
            {
                ListItem listItem = new ListItem();
                listItem.Text = psmSchema.ToString();
                listItem.Value = psmSchema.ID.ToString();
                Items.Add(listItem);
                SelectedIndex = 0;    
            }
        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SelectedValue = suggestedValue.ToString();
        }

        public PSMSchema Value
        {
            get
            {
                if (ProjectVersion != null)
                {
                    return ProjectVersion.Project.TranslateComponent<PSMSchema>((this as IOperationParameterControl<Guid>).Value);
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