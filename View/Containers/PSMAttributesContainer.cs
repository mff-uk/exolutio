using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace EvoX.View
{
	/// <summary>
	/// Interface for control displaying psm attributes. 
	/// </summary>
	public interface IPSMAttributesContainer : ITextBoxContainer
	{
		/// <summary>
		/// Reference to <see cref="IControlsPSMAttributes"/>
		/// </summary>
		IControlsPSMAttributes AttributeController
		{
			get;
			set;
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		ObservableCollection<PSMAttribute> AttributesCollection
		{
			get;
		}

		/// <summary>
		/// Adds visualization of <paramref name="attribute"/> to the control
		/// </summary>
		/// <param name="attribute">visualized attribute</param>
		/// <returns>Control displaying the attribute</returns>
		PSMAttributeTextBox AddAttribute(PSMAttribute attribute);

		/// <summary>
		/// Removes visualization of <paramref name="attribute"/>/
		/// </summary>
		/// <param name="attribute">removed attribute</param>
		void RemoveAttribute(PSMAttribute attribute);

		/// <summary>
		/// Reflects changs in <see cref="AttributesCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		void attributesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

		/// <summary>
		/// Removes all attriutes
		/// </summary>
		void Clear();

		/// <summary>
		/// Cancels editing if any of the displayed attributes is being edited. 
		/// </summary>
		void CancelEdit();

		/// <summary>
		/// Sets whether "any attribute" is shown at the and of the attribute container. 
		/// </summary>
		bool DisplayAnyAttribute { get; set; }
	}


	/// <summary>
	/// Implementation of <see cref="IPSMAttributesContainer"/>, displays attributes
	/// using <see cref="PSMAttributeTextBox">PSMAttributeTextBoxes</see>.
	/// </summary>
	public class PSMAttributesContainer : TextBoxContainer<PSMAttributeTextBox>, IPSMAttributesContainer
	{
		private IControlsPSMAttributes attributeController;

		/// <summary>
		/// Reference to <see cref="IControlsPSMAttributes"/>
		/// </summary>
		public IControlsPSMAttributes AttributeController
		{
			get
			{
				return attributeController;
			}
			set
			{
				attributeController = value;
				attributeController.AttributeHolder.PSMAttributes.CollectionChanged += attributesCollection_CollectionChanged;
				attributesCollection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				this.container.Visibility = attributeController.AttributeHolder.PSMAttributes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		public ObservableCollection<PSMAttribute> AttributesCollection
		{
			get
			{
				return attributeController.AttributeHolder.PSMAttributes;
			}
		}

		/// <summary>
		/// Returns context menu items for operations provided by the control.
		/// </summary>
		internal IEnumerable<ContextMenuItem> PropertiesMenuItems
		{
			get
			{
				ContextMenuItem addPropertyItem = new ContextMenuItem("Add new attribute...");
				addPropertyItem.Icon = ContextMenuIcon.GetContextIcon("AddAttributes");
				addPropertyItem.Click += delegate
											{
												attributeController.AddNewAttribute();
											};

				return new ContextMenuItem[]
				       	{
				       		addPropertyItem
				       	};
			}
		}

		/// <summary>
		/// Creates new instance of <see cref="PSMAttributesContainer" />. 
		/// </summary>
		/// <param name="container">Panel used to display the items</param>
		/// <param name="xCaseCanvas">canvas owning the control</param>
		public PSMAttributesContainer(Panel container, XCaseCanvas xCaseCanvas)
			: base(container, xCaseCanvas)
		{

		}

		/// <summary>
		/// Adds visualization of <paramref name="attribute"/> to the control
		/// </summary>
		/// <param name="attribute">visualized attribute</param>
		/// <returns>Control displaying the attribute</returns>
		public PSMAttributeTextBox AddAttribute(PSMAttribute attribute)
		{
			PSMAttributeTextBox t = new PSMAttributeTextBox(attribute, attributeController);
			base.AddItem(t);
			return t;
		}

		/// <summary>
		/// Adds visualization of a virtual attribute that will be 
		/// translated into "anyAttribute" in XML Schema. 
		/// </summary>
		private void AddAnyAttributeDefinition()
		{
			PSMAttributeTextBox t = new PSMAttributeTextBox(AttributeController);
			base.AddItem(t);
		}

		/// <summary>
		/// Removes visualization of <paramref name="attribute"/>/
		/// </summary>
		/// <param name="attribute">remoed attribute</param>
		public void RemoveAttribute(PSMAttribute attribute)
		{
			attributeController.RemoveAttribute(attribute);
		}

		/// <summary>
		/// Reflects changs in <see cref="IAttributesContainer.AttributesCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		public void attributesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Clear();

			foreach (PSMAttribute property in AttributesCollection)
			{
				AddAttribute(property);
			}

			if (AttributeController is PSM_ClassController)
			{
				if (((PSM_ClassController)AttributeController).Class.AllowAnyAttribute)
				{
					AddAnyAttributeDefinition();
				}
			}
		}

		private bool displayAnyAttribute = false;
		/// <summary>
		/// Sets whether "any attribute" is shown at the and of the attribute container.
		/// </summary>
		/// <value></value>
		public bool DisplayAnyAttribute
		{
			get { return displayAnyAttribute; }
			set
			{
				displayAnyAttribute = value;
				attributesCollection_CollectionChanged(null, null);
			}
		}
	}
}