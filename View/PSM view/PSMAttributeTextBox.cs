using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
	public class PSMAttributeTextBox : EditableTextBox
	{
	    public PSMAttribute PSMAttribute { get; private set; }

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
            MouseDoubleClick += PSMAttributeTextBox_MMouseDoubleClick;
            MouseDown += PSMAttributeTextBox_MouseDown;
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
                this.Background = ViewToolkitResources.NoInterpretationBrush;
            }
            else
            {
                if (this.PSMAttribute.PSMClass.IsStructuralRepresentative)
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
	        Current.InvokeComponentTouched(PSMAttribute);
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
