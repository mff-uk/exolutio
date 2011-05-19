using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;
using EvoX.ViewToolkit;

namespace EvoX.View
{
    /// <summary>
    /// Implementation of <see cref="IAttributesContainer"/>, displays attributes
    /// using <see cref="PIMAttributeTextBox">AttributeTextBoxes</see>.
    /// </summary>
    public class PSMAttributesContainer : TextBoxContainer<PSMAttribute, PSMAttributeTextBox>, IAttributesContainer<PSMAttribute, PSMAttributeTextBox>
    {
        //private IControlsAttributes attributeController;

        /// <summary>
        /// Reference to <see cref="IControlsAttributes"/>
        /// </summary>
        //public IControlsAttributes AttributeController
        //{
        //    get
        //    {
        //        return attributeController;
        //    }
        //    set
        //    {
        //        attributeController = value;
        //    }
        //}

        private ICollection<PSMAttribute> attributesCollection;

        ICollection<PSMAttribute> IAttributesContainer<PSMAttribute, PSMAttributeTextBox>.AttributesCollection
        {
            get { return AttributesCollection; }
        }

        /// <summary>
        /// Visualized collection 
        /// </summary>
        public override ICollection<PSMAttribute> AttributesCollection
        {
            get
            {
                return attributesCollection;
            }
            set 
            {
                attributesCollection = value;
                ((INotifyCollectionChanged)attributesCollection).CollectionChanged += attributesCollection_CollectionChanged;
                attributesCollection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.container.Visibility = attributesCollection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;            
            }
        }

        /// <summary>
        /// Returns context menu items for operations provided by the control.
        /// </summary>
        internal IEnumerable<ContextMenuItem> PropertiesMenuItems
        {
            get
            {          
#if SILVERLIGHT
                return new ContextMenuItem[0];
#else
                ContextMenuItem addPropertyItem = new ContextMenuItem("Add new attribute...");
                addPropertyItem.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.AddAttributes);
                //addPropertyItem.Click += delegate
                //{
                //    attributeController.AddNewAttribute(null);
                //};

                return new ContextMenuItem[]
				       	{
				       		addPropertyItem
				       	};
#endif
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="PIMAttributesContainer" />. 
        /// </summary>
        /// <param name="container">Panel used to display the items</param>
        /// <param name="evoxCanvas">canvas owning the control</param>
        public PSMAttributesContainer(Panel container, EvoXCanvas evoxCanvas, Diagram diagram)
            : base(container, evoxCanvas, diagram)
        {

        }
    }
}
