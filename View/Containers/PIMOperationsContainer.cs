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
    /// Implementation of <see cref="IOperationsContainer{PIMOperation, PIMOperationTextBox}"/>, displays operations
    /// using <see cref="PIMOperationTextBox">OperationTextBoxes</see>.
    /// </summary>
    public class PIMOperationsContainer : TextBoxContainer<PIMOperation, PIMOperationTextBox>, IOperationsContainer<PIMOperation, PIMOperationTextBox>
    {
        private ICollection<PIMOperation> operationsCollection;

        ICollection<PIMOperation> IOperationsContainer<PIMOperation, PIMOperationTextBox>.OperationsCollection
        {
            get { return Collection; }
        }

        /// <summary>
        /// Visualized collection 
        /// </summary>
        public override ICollection<PIMOperation> Collection
        {
            get
            {
                return operationsCollection;
            }
            set 
            { 
                operationsCollection = value;
                ((INotifyCollectionChanged)operationsCollection).CollectionChanged += Collection_CollectionChanged;
                Collection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.container.Visibility = operationsCollection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;            
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
        /// Creates new instance of <see cref="PIMOperationsContainer" />. 
        /// </summary>
        /// <param name="container">Panel used to display the items</param>
        /// <param name="exolutioCanvas">canvas owning the control</param>
        public PIMOperationsContainer(Panel container, ExolutioCanvas exolutioCanvas, DiagramView diagram)
            : base(container, exolutioCanvas, diagram)
        {

        }

        public IEnumerator<PIMOperationTextBox> GetEnumerator()
        {
            foreach (PIMOperationTextBox operationTextBox in container.Children)
            {
                yield return operationTextBox;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void AddItem(PIMOperationTextBox item)
        {
            base.AddItem(item);
            item.Container = this;
        }
    }
}
