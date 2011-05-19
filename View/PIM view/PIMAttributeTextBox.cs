using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.ResourceLibrary;
using EvoX.ViewToolkit;

namespace EvoX.View
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
	public class PIMAttributeTextBox : EditableTextBox
	{
		private PIMAttribute property;

		//private IControlsAttributes classController;

        public override void SetDisplayedObject(object property, object diagram)
        {
            this.property = (PIMAttribute) property;

            //this.classController = classController;

            EvoXContextMenu evoXContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAttribute, (Diagram) diagram);
            evoXContextMenu.ScopeObject = property;
            evoXContextMenu.Diagram = diagram;
            ContextMenu = evoXContextMenu;
           

            #if SILVERLIGHT
            ContextMenuService.SetContextMenu(this, ContextMenu);
            #else
            MouseDoubleClick += OnMouseDoubleClick;
            #endif
            this.property.PropertyChanged += OnPropertyChangedEvent;
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
		}

		private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
