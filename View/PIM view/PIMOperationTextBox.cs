using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;

namespace Exolutio.View
{
    /// <summary>
    /// Textbox displaying operation
    /// </summary>
    public class PIMOperationTextBox : EditableTextBox, IComponentTextBox, ISelectableSubItem
    {
        public ModelOperation ModelOperation { get; private set; }

        public PIMOperationsContainer Container { get; set; }

        public override void SetDisplayedObject(object property, object diagram)
        {
            this.ModelOperation = (ModelOperation)property;

            //this.classController = classController;

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMOperation, (Diagram)diagram);
            exolutioContextMenu.ScopeObject = property;
            exolutioContextMenu.Diagram = diagram;
            ContextMenu = exolutioContextMenu;


#if SILVERLIGHT
            ContextMenuService.SetContextMenu(this, ContextMenu);
#else
            MouseDoubleClick += PIMOperationTextBox_MouseDoubleClick;
            MouseDown += PIMOperationTextBox_MouseDown;
            PreviewMouseDown += PIMOperationTextBox_PreviewMouseDown;
#endif

            this.ModelOperation.PropertyChanged += OnPropertyChangedEvent;
            Background = ViewToolkitResources.TransparentBrush;
            RefreshTextContent();
            BindType();
        }

        private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            RefreshTextContent();
            if (e.PropertyName == "Type")
            {
                BindType();
            }
        }

        private void BindType()
        {
            if (type != null)
            {
                type.PropertyChanged -= Type_PropertyChanged;
            }

            if (ModelOperation.ResultType != null)
            {
                type = ModelOperation.ResultType;
                type.PropertyChanged += Type_PropertyChanged;
            }
        }

        public override void UnBindModelView()
        {
            Selected = false;
            if (type != null)
            {
                type.PropertyChanged -= Type_PropertyChanged;
            }
            ModelOperation.PropertyChanged -= OnPropertyChangedEvent;
            base.UnBindModelView();
        }


        void Type_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshTextContent();
        }

        private StringBuilder textBuilder;
        
        private void RefreshTextContent()
        {
            if (ModelOperation.Parameters.Count != 0 || ModelOperation.ResultType != null)
            {
                if (textBuilder == null)
                {
                    textBuilder = new StringBuilder(Name);
                }
                else
                {
                    textBuilder.Clear();
                }
                textBuilder.Append(ModelOperation.Name);
                textBuilder.Append("(");
                if (ModelOperation.Parameters.Count > 0)
                {                    
                    foreach (ModelOperationParameter parameter in ModelOperation.Parameters)
                    {
                        textBuilder.Append(parameter.Name);
                        if (parameter.Type != null)
                        {
                            textBuilder.Append(":");
                            textBuilder.Append(parameter.Type.Name);
                        }
                    }
                }
                textBuilder.Append(")");
                if (ModelOperation.ResultType != null)
                {
                    textBuilder.Append(":");
                    textBuilder.Append(ModelOperation.ResultType.Name);
                }
                this.Text = textBuilder.ToString();
            }
            else
            {
                this.Text = string.Format("{0}()", ModelOperation.Name);
            }
        }

        private Exolutio.Model.AttributeType type;

        void PIMOperationTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Current.InvokeComponentTouched(ModelOperation);
        }

        void PIMOperationTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Container.ExolutioCanvas.SelectableItem_PreviewMouseDown(this, e);
        }

        private void PIMOperationTextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
#if SILVERLIGHT
#else
            if (ModelOperation != null)
            {
                PIMClassDialog d = new PIMClassDialog();
                d.Initialize(Current.Controller, ModelOperation.PIMClass);
                d.Topmost = true;
                d.Show();
                e.Handled = true;
            }
#endif
        }



        #region Versioned element highlighting support

        //protected override void OnMouseEnter(MouseEventArgs e)
        //{
        //    base.OnMouseEnter(e);
        //    XCaseCanvas.InvokeVersionedElementMouseEnter(this, property);
        //}

        //protected override void OnMouseLeave(MouseEventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //    XCaseCanvas.InvokeVersionedElementMouseLeave(this, property);
        //}

        #endregion

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
                if (value)
                {
                    Background = ViewToolkitResources.ClassSelectedAttribute;
                    Container.DiagramView.SelectedTextBoxes.AddIfNotContained(this);
                }
                else
                {
                    Background = ViewToolkitResources.ClassBody;
                    Container.DiagramView.SelectedTextBoxes.Remove(this);
                }

                Container.DiagramView.InvokeSelectionChanged();
            }
        }

        public Component Component
        {
            get { return ModelOperation; }
        }
    }
}
