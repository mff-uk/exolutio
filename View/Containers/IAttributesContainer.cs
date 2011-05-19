using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using EvoX.Model.PIM;

namespace EvoX.View
{
    /// <summary>
    /// Interface for control displaying attributes. 
    /// </summary>
    public interface IAttributesContainer<TMember, TTextBox> : ITextBoxContainer
    {
        ///// <summary>
        ///// Reference to <see cref="IControlsAttributes"/>
        ///// </summary>
        //IControlsAttributes AttributeController
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Visualized collection 
        /// </summary>
        ICollection<TMember> AttributesCollection
        {
            get;
        }

        /// <summary>
        /// Adds visualization of <paramref name="attribute"/> to the control
        /// </summary>
        /// <param name="attribute">visualized attribute</param>
        /// <returns>Control displaying the attribute</returns>
        TTextBox AddAttribute(TMember attribute);

        /// <summary>
        /// Removes visualization of <paramref name="attribute"/>/
        /// </summary>
        /// <param name="attribute">removed attribute</param>
        void RemoveAttribute(TMember attribute);

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
    }
}