using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;

namespace Exolutio.View
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
    public class PSMAttributeTextBox : EditableTextBox, IComponentTextBox, ISelectableSubItem
	{
	    public PSMAttribute PSMAttribute { get; private set; }

	    public PSMAttributesContainer Container { get; set; }

	    //private IControlsAttributes classController;

        public PSMAttributeTextBox()
		{
			
		}

        public override void SetDisplayedObject(object property, object diagram)
        {
            this.PSMAttribute = (PSMAttribute) property;

            //this.classController = classController;

            #region property context menu

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMAttribute, (Diagram)diagram);
            exolutioContextMenu.ScopeObject = property;
            exolutioContextMenu.Diagram = diagram;
            ContextMenu = exolutioContextMenu;

//#if SILVERLIGHT
//            change.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil);
//            remove.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete2);
//#else
//            change.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil);
//            remove.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete2);
//#endif

            #endregion

            this.PSMAttribute.PropertyChanged += OnPropertyChangedEvent;
#if SILVERLIGHT
            ContextMenuService.SetContextMenu(this, ContextMenu);
#else
            MouseDoubleClick += PSMAttributeTextBox_MouseDoubleClick;
            MouseDown += PSMAttributeTextBox_MouseDown;
            PreviewMouseDown += PSMAttributeTextBox_PreviewMouseDown;
#endif
            Background = ViewToolkitResources.TransparentBrush;
            RefreshTextContent();
            BindType();
        }

	    private Exolutio.Model.AttributeType type;

		private void BindType()
		{
			if (type != null)
			{
				type.PropertyChanged -= Type_PropertyChanged;
			}

			if (PSMAttribute.AttributeType != null)
			{
                type = PSMAttribute.AttributeType;
				type.PropertyChanged += Type_PropertyChanged;
			}
		}

        public override void UnBindModelView()
        {
            if (type != null)
            {
                type.PropertyChanged -= Type_PropertyChanged;
            }
            PSMAttribute.PropertyChanged -= OnPropertyChangedEvent;
        }

		void Type_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshTextContent();
		}

		public void RefreshTextContent()
		{
            if (PSMAttribute.AttributeType != null)
                this.Text = string.Format("{0} : {1}", PSMAttribute.Name, PSMAttribute.AttributeType.Name);
			else
				this.Text = PSMAttribute.Name;

            //if (property.Default != null)
            //    this.Text += string.Format(" [{0}]", property.Default);

            if (!String.IsNullOrEmpty(PSMAttribute.GetCardinalityString()) && PSMAttribute.GetCardinalityString() != "1") 
			{
                this.Text += String.Format(" {{{0}}}", PSMAttribute.GetCardinalityString());
			}

            if (!this.PSMAttribute.Element)
                this.Text = "@" + this.Text;
            else
                this.Text = this.Text;

            if (this.PSMAttribute.Interpretation == null)
            {
                if (this.PSMAttribute.PSMClass != null && !this.PSMAttribute.PSMClass.IsStructuralRepresentative)
                {
                    if (!Selected)
                    {
                        this.Background = ViewToolkitResources.NoInterpretationBrush;
                    }
                    else
                    {
                        this.Background = ViewToolkitResources.NoInterpretationBrushSelected;
                    }
                }
                else
                {
                    if (!Selected)
                    {
                        this.Background = ViewToolkitResources.StructuralRepresentativeBodyNoInterpretation;
                    }
                    else
                    {
                        this.Background = ViewToolkitResources.NoInterpretationBrushSelected;
                    }
                }
            }
            else
            {
                if (this.PSMAttribute.PSMClass != null && this.PSMAttribute.PSMClass.IsStructuralRepresentative)
                {
                    if (!Selected)
                    {
                        this.Background = ViewToolkitResources.StructuralRepresentativeBody;
                    }
                    else
                    {
                        this.Background = ViewToolkitResources.StructuralRepresentativeBodySelected;
                    }
                }
                else
                {
                    if (!Selected)
                    {
                        this.Background = ViewToolkitResources.InterpretedAttributeBrush;
                    }
                    else
                    {
                        this.Background = ViewToolkitResources.ClassSelectedAttribute;
                    }
                }
            }
		}

	    private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
	    {
	        RefreshTextContent();
	        if (e.PropertyName == "Type")
	        {
	            BindType();
	        }
	    }

	    void PSMAttributeTextBox_MouseDown(object sender, MouseButtonEventArgs e)
	    {
	        Current.InvokeComponentTouched(PSMAttribute);
	    }

        void PSMAttributeTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Container.ExolutioCanvas.SelectableItem_PreviewMouseDown(this, e);
            e.Handled = true;
        }

	    private void PSMAttributeTextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            if (PSMAttribute != null)
            {
                PSMClassDialog d = new PSMClassDialog();
                d.Topmost = true;
                d.Initialize(Current.Controller, PSMAttribute.PSMClass, PSMAttribute);
                d.Show();
            }
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
                    //Background = ViewToolkitResources.ClassSelectedAttribute;
                    Container.DiagramView.SelectedTextBoxes.AddIfNotContained(this);
                }
                else
                {
                    //Background = ViewToolkitResources.ClassBody;
                    Container.DiagramView.SelectedTextBoxes.Remove(this);
                }

                RefreshTextContent();

                Container.DiagramView.InvokeSelectionChanged();
            }
        }

	    public Component Component
	    {
	        get { return PSMAttribute; }
	    }
	}
}
