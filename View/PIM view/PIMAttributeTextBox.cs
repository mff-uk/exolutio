using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
	/// <summary>
	/// Textbox displaying attribute
	/// </summary>
	public class PIMAttributeTextBox : EditableTextBox
	{
	    public PIMAttribute PIMAttribute { get; private set; }

	    public PIMAttributesContainer Container { get; set; }

	    //private IControlsAttributes classController;

        public override void SetDisplayedObject(object property, object diagram)
        {
            this.PIMAttribute = (PIMAttribute) property;

            //this.classController = classController;

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMAttribute, (Diagram) diagram);
            exolutioContextMenu.ScopeObject = property;
            exolutioContextMenu.Diagram = diagram;
            ContextMenu = exolutioContextMenu;
           

            #if SILVERLIGHT
            ContextMenuService.SetContextMenu(this, ContextMenu);
            #else
            MouseDoubleClick += PIMAttributeTextBox_MouseDoubleClick;
            MouseDown += PIMAttributeTextBox_MouseDown;
            #endif

            this.PIMAttribute.PropertyChanged += OnPropertyChangedEvent;
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

			if (PIMAttribute.AttributeType != null)
			{
                type = PIMAttribute.AttributeType;
				type.PropertyChanged += Type_PropertyChanged;
			}
		}

        public override void UnBindModelView()
        {
            if (type != null)
            {
                type.PropertyChanged -= Type_PropertyChanged;
            }
            PIMAttribute.PropertyChanged -= OnPropertyChangedEvent;
        }


		void Type_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshTextContent();
		}

		private void RefreshTextContent()
		{
            if (PIMAttribute.AttributeType != null)
                this.Text = string.Format("{0} : {1}", PIMAttribute.Name, PIMAttribute.AttributeType.Name);
			else
				this.Text = PIMAttribute.Name;

            //if (property.Default != null)
            //    this.Text += string.Format(" [{0}]", property.Default);

            if (!String.IsNullOrEmpty(PIMAttribute.GetCardinalityString()) && PIMAttribute.GetCardinalityString() != "1") 
			{
                this.Text += String.Format(" {{{0}}}", PIMAttribute.GetCardinalityString());
			}
		}

	    private Exolutio.Model.AttributeType type;

	    void PIMAttributeTextBox_MouseDown(object sender, MouseButtonEventArgs e)
	    {
	        Current.InvokeComponentTouched(PIMAttribute);
	    }

	    private void PIMAttributeTextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
	        if (PIMAttribute != null)
	        {
	            PIMClassDialog d = new PIMClassDialog();
                d.Initialize(Current.Controller, PIMAttribute.PIMClass, PIMAttribute);
                d.ShowDialog();
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
	}
}
