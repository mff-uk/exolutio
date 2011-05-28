using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
    /// <summary>
    /// Implementation of <see cref="IAttributesContainer"/>, displays attributes
    /// using <see cref="PIMAttributeTextBox">AttributeTextBoxes</see>.
    /// </summary>
    public class PIMAttributesContainer : TextBoxContainer<PIMAttribute, PIMAttributeTextBox>, IAttributesContainer<PIMAttribute, PIMAttributeTextBox>
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

        private ICollection<PIMAttribute> attributesCollection;

        ICollection<PIMAttribute> IAttributesContainer<PIMAttribute, PIMAttributeTextBox>.AttributesCollection
        {
            get { return AttributesCollection; }
        }

        /// <summary>
        /// Visualized collection 
        /// </summary>
        public override ICollection<PIMAttribute> AttributesCollection
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
                addPropertyItem.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes);
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
        /// <param name="exolutioCanvas">canvas owning the control</param>
        public PIMAttributesContainer(Panel container, ExolutioCanvas exolutioCanvas, Diagram diagram)
            : base(container, exolutioCanvas, diagram)
        {

        }

        public IEnumerator<PIMAttributeTextBox> GetEnumerator()
        {
            foreach (PIMAttributeTextBox attributeTextBox in container.Children)
            {
                yield return attributeTextBox;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
