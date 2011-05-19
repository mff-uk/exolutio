using System;
using System.Web.UI.WebControls;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;

namespace EvoX.Web.OperationParameters
{
    public class GuidLookup : DropDownList,
        IOperationParameterControl, IOperationParameterControl<Guid>, IOperationParameterControl<EvoXObject>,
        IOperationParameterControlWithConsistencyCheck
    {
        public delegate bool VerifyDelegate(params object[] verifiedObjects);

        public bool AllowNullInput { get; set; }

        public Type LookedUpType { get; set; }

        public ProjectVersion ProjectVersion { get; set; }

        public void InitControl()
        {
            Items.Clear();
            //if (Schema != null)
            //{
            //    foreach (Component schemaComponent in Schema.SchemaComponents)
            //    {
            //        if (schemaComponent.GetType() == LookedUpType)
            //        {
            //            ListItem listItem = new ListItem();
            //            listItem.Text = schemaComponent.ToString();
            //            listItem.Value = schemaComponent.ID.ToString();
            //            Items.Add(listItem);
            //        }
            //    }
            //}

        }

        public void SetSuggestedValue(object suggestedValue)
        {
            this.SelectedValue = suggestedValue.ToString();
        }

        object IOperationParameterControl.Value
        {
            get { return Value; }
        }

        public EvoXObject Value
        {
            get
            {
                Guid guid = (this as IOperationParameterControl<Guid>).Value;
                if (guid != Guid.Empty)
                    return ProjectVersion.Project.TranslateComponent<EvoXObject>(guid);
                else
                    return null;
            }
        }

        Guid IOperationParameterControl<Guid>.Value
        {
            get
            {
                if (this.SelectedIndex != -1)
                    return Guid.Parse(this.SelectedValue);
                else
                    return Guid.Empty;
            }
        }

        public void LoadAllPossibleValues(Type componentType)
        {
            if (componentType == typeof (PSMSchema))
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
            else if (componentType == typeof (PIMSchema))
            {
                ListItem listItem = new ListItem();
                listItem.Text = ProjectVersion.PIMSchema.ToString();
                listItem.Value = ProjectVersion.PIMSchema.ID.ToString();
                Items.Add(listItem);
                SelectedIndex = 0;
            }
            else if (componentType == typeof(Schema))
            {
                Items.Clear();
                ListItem listItem = new ListItem();
                listItem.Text = ProjectVersion.PIMSchema.ToString();
                listItem.Value = ProjectVersion.PIMSchema.ID.ToString();
                Items.Add(listItem);
                SelectedIndex = 0;
                foreach (PSMSchema psmSchema in ProjectVersion.PSMSchemas)
                {
                    ListItem psmListItem = new ListItem();
                    psmListItem.Text = psmSchema.ToString();
                    psmListItem.Value = psmSchema.ID.ToString();
                    Items.Add(psmListItem);
                    SelectedIndex = 0;
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("Member GuidLookup.LoadAllPossibleValues not implemented for type {0}.", componentType.Name));
            }

            if (AllowNullInput)
            {
                Items.Insert(0, new ListItem("(null)", Guid.Empty.ToString()));
                SelectedIndex = 0;
            }
        }

        public void MakeValuesConsistentWith(object superiorObject)
        {
            Items.Clear();

            if (superiorObject != null)
            {
                Schema schema;
                Guid superiorObjectGuid = Guid.Empty;
                if (typeof (PIMComponent).IsAssignableFrom(LookedUpType) && (superiorObject is PSMComponent))
                {
                    schema = ((PSMComponent) superiorObject).ProjectVersion.PIMSchema;
                    superiorObjectGuid = ((PSMComponent) superiorObject).ID;
                }
                else if (superiorObject is Schema)
                {
                    schema = (Schema)superiorObject;
                    superiorObjectGuid = ((Schema)superiorObject).ID;
                }
                else if (superiorObject is Component)
                {
                    schema = ((Component) superiorObject).Schema;
                    superiorObjectGuid = ((Component)superiorObject).ID;
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (LookedUpType == typeof(AttributeType))
                {
                    foreach (AttributeType schemaComponent in schema.ProjectVersion.AttributeTypes)
                    {
                        if (ConsistencyChecker == null || superiorObjectGuid == Guid.Empty ||
                            ConsistencyChecker.VerifyConsistency(superiorObject, schemaComponent))
                        {
                            ListItem listItem = new ListItem();
                            listItem.Text = schemaComponent.ToString();
                            listItem.Value = schemaComponent.ID.ToString();
                            Items.Add(listItem);
                        }
                    }
                }
                else
                {
                    foreach (Component schemaComponent in schema.SchemaComponents)
                    {
                        //if (schemaComponent.GetType() == LookedUpType)
                        if (LookedUpType.IsAssignableFrom(schemaComponent.GetType()))
                        {
                            if (ConsistencyChecker == null ||
                                ConsistencyChecker.VerifyConsistency(superiorObject, schemaComponent))
                            {
                                ListItem listItem = new ListItem();
                                listItem.Text = schemaComponent.ToString();
                                listItem.Value = schemaComponent.ID.ToString();
                                Items.Add(listItem);
                            }
                        }
                    }
                }
            }

            bool select1 = Items.Count > 0 && AllowNullInput;

            if (AllowNullInput)
            {
                Items.Insert(0, new ListItem("(null)", Guid.Empty.ToString()));
                SelectedIndex = 0;
            }

            if (Items.Count > 0)
            {
                if (Items.Count > 1 && select1)
                {
                    SelectedIndex = select1 ? 1 : 0;
                }
                else
                {
                    SelectedIndex = 0;
                }
                OnSelectedIndexChanged(null);
            }

            OnSelectedIndexChanged(null);
        }

        public IOperationParameterControl SuperiorPropertyEditor { get; set; }

        public ParameterConsistency ConsistencyChecker { get; set; }
        public void OnSuperiorPropertyChanged(object sender, EventArgs e)
        {
            this.MakeValuesConsistentWith(SuperiorPropertyEditor.Value);
        }
    }
}