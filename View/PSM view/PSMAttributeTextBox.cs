using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;
using EvoX.ViewToolkit;

namespace EvoX.View
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
	public class PSMAttributeTextBox : EditableTextBox
	{
		private PSMAttribute property;

		//private IControlsAttributes classController;

        public PSMAttributeTextBox()
		{
			
		}

        public override void SetDisplayedObject(object property, object diagram)
        {
            this.property = (PSMAttribute) property;

            //this.classController = classController;

            #region property context menu

            EvoXContextMenu evoXContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMAttribute, (Diagram)diagram);
            evoXContextMenu.ScopeObject = property;
            evoXContextMenu.Diagram = diagram;
            ContextMenu = evoXContextMenu;

//#if SILVERLIGHT
//            change.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.pencil);
//            remove.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.delete2);
//#else
//            change.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.pencil);
//            remove.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.delete2);
//#endif

            #endregion

            this.property.PropertyChanged += OnPropertyChangedEvent;
#if SILVERLIGHT
            ContextMenuService.SetContextMenu(this, ContextMenu);
#else
            MouseDoubleClick += PSMAttributeTextBox_MMouseDoubleClick;
            MouseDown += PSMAttributeTextBox_MouseDown;
#endif
            Background = ViewToolkitResources.TransparentBrush;
            RefreshTextContent();
            BindType();
        }

	    private EvoX.Model.AttributeType type;

		private void BindType()
		{
			if (type != null)
			{
				type.PropertyChanged -= Type_PropertyChanged;
			}

			if (property.AttributeType != null)
			{
                type = property.AttributeType;
				type.PropertyChanged += Type_PropertyChanged;
			}
		}

		void Type_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshTextContent();
		}

		private void RefreshTextContent()
		{
            if (property.AttributeType != null)
                this.Text = string.Format("{0} : {1}", property.Name, property.AttributeType.Name);
			else
				this.Text = property.Name;

            //if (property.Default != null)
            //    this.Text += string.Format(" [{0}]", property.Default);

            if (!String.IsNullOrEmpty(property.GetCardinalityString()) && property.GetCardinalityString() != "1") 
			{
                this.Text += String.Format(" {{{0}}}", property.GetCardinalityString());
			}

            if (!this.property.Element)
                this.Text = "@" + this.Text;
            else
                this.Text = this.Text;

            if (this.property.Interpretation == null)
            {
                this.Background = ViewToolkitResources.NoInterpretationBrush;
            }
            else
            {
                if (this.property.PSMClass.IsStructuralRepresentative)
                {
                    this.Background = ViewToolkitResources.StructuralRepresentativeBody;
                }
                else
                {
                    this.Background = ViewToolkitResources.InterpretedAttributeBrush;
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
	        Current.InvokeComponentTouched(property);
	    }

	    private void PSMAttributeTextBox_MMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            //if (classController != null)
            //    classController.ShowAttributeDialog(property);
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
	}
}
