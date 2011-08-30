using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Exolutio.Model.PIM;

namespace Exolutio.View
{
    /// <summary>
    /// Interface for control displaying Operations. 
    /// </summary>
    public interface IOperationsContainer<TMember, TTextBox> : ITextBoxContainer, IEnumerable<TTextBox>
    {
        /// <summary>
        /// Visualized collection 
        /// </summary>
        ICollection<TMember> OperationsCollection
        {
            get;
        }

        /// <summary>
        /// Adds visualization of <paramref name="member"/> to the control
        /// </summary>
        /// <param name="member">visualized Operation</param>
        /// <returns>Control displaying the Operation</returns>
        TTextBox AddMember(TMember member);

        /// <summary>
        /// Removes visualization of <paramref name="member"/>/
        /// </summary>
        /// <param name="member">removed Operation</param>
        void RemoveMember(TMember member);

        /// <summary>
        /// Reflects changs in <see cref="OperationsCollection"/>.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arguments</param>
        void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        /// <summary>
        /// Removes all attriutes
        /// </summary>
        void Clear();

        /// <summary>
        /// Cancels editing if any of the displayed Operations is being edited. 
        /// </summary>
        void CancelEdit();
    }
}