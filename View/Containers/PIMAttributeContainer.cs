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
    /// Implementation of <see cref="IAttributesContainer{PIMAttribute,PIMAttributeTextBox}"/>, displays attributes
    /// using <see cref="PIMAttributeTextBox">AttributeTextBoxes</see>.
    /// </summary>
    public class PIMAttributesContainer : TextBoxContainer<PIMAttribute, PIMAttributeTextBox>, IAttributesContainer<PIMAttribute, PIMAttributeTextBox>
    {
        private ICollection<PIMAttribute> collection;

        ICollection<PIMAttribute> IAttributesContainer<PIMAttribute, PIMAttributeTextBox>.AttributesCollection
        {
            get { return Collection; }
        }

        /// <summary>
        /// Visualized collection 
        /// </summary>
        public override ICollection<PIMAttribute> Collection
        {
            get
            {
                return collection;
            }
            set 
            { 
                collection = value;
                ((INotifyCollectionChanged)collection).CollectionChanged += Collection_CollectionChanged;
                Collection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.container.Visibility = collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;            
            }
        }



        /// <summary>
        /// Returns context menu items for operations provided by the control.
        /// </summary>
        internal IEnumerable<ContextMenuItem> PropertiesMenuItems
        {
            get
            {          
                return new ContextMenuItem[0];
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="PIMAttributesContainer" />. 
        /// </summary>
        /// <param name="container">Panel used to display the items</param>
        /// <param name="exolutioCanvas">canvas owning the control</param>
        public PIMAttributesContainer(Panel container, ExolutioCanvas exolutioCanvas, DiagramView diagram)
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

        public override void AddItem(PIMAttributeTextBox item)
        {
            base.AddItem(item);
            item.Container = this;
        }
    }
}
