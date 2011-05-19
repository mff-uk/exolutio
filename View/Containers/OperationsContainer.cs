using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using XCase.Model;
using XCase.Controller.Interfaces;

namespace XCase.View.Controls.Containers
{
	/// <summary>
	/// Interface for control displaying operations. 
	/// </summary>
	public interface IOperationsContainer : ITextBoxContainer
	{
		/// <summary>
		/// Reference to <see cref="IControlsOperations"/>
		/// </summary>
		IControlsOperations ClassController
		{
			get;
			set;
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		ObservableCollection<Operation> OperationsCollection
		{
			get;
		}

		/// <summary>
		/// Adds visualization of <paramref name="operation"/> to the control
		/// </summary>
		/// <param name="operation">visualized operation</param>
		/// <returns>Control displaying the operation</returns>
		OperationTextBox AddOperation(Operation operation);

		/// <summary>
		/// Removes visualization of <paramref name="operation"/>/
		/// </summary>
		/// <param name="operation">removed operation</param>
		void RemoveOperation(Operation operation);

		/// <summary>
		/// Reflects changs in <see cref="OperationsCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		void operationsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

		/// <summary>
		/// Removes all attriutes
		/// </summary>
		void Clear();


		/// <summary>
		/// Cancels editing if any of the displayed operations is being edited. 
		/// </summary>
		void CancelEdit();
	}

	/// <summary>
	/// Implementation of <see cref="IOperationsContainer"/>, displays operations
	/// using <see cref="OperationTextBox">OperationTextBoxes</see>.
	/// </summary>
	public class OperationsContainer : TextBoxContainer<OperationTextBox>, IOperationsContainer
	{
        private IControlsOperations classController;

		/// <summary>
		/// Reference to <see cref="IControlsOperations"/>
		/// </summary>
		public IControlsOperations ClassController
		{
			get
			{
				return classController;
			}
			set
			{
				classController = value;
				classController.OperationHolder.Operations.CollectionChanged += operationsCollection_CollectionChanged;
				operationsCollection_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.container.Visibility = classController.OperationHolder.Operations.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Visualized collection 
		/// </summary>
		public ObservableCollection<Operation> OperationsCollection
		{
			get
			{
                return classController.OperationHolder.Operations;
			}
		}

		internal IEnumerable<ContextMenuItem> OperationsMenuItems
		{
			get
			{
				ContextMenuItem addOperationItem = new ContextMenuItem("Add new operation...");
                addOperationItem.Icon = ContextMenuIcon.GetContextIcon("AddAttributes");
				addOperationItem.Click += delegate
				                          	{
				                          		classController.AddNewOperation(null);
				                          	};

				return new ContextMenuItem[] { addOperationItem };
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OperationsContainer"/> class.
		/// </summary>
		/// <param name="container">Panel used to display the items</param>
		/// <param name="xCaseCanvas">canvas owning the control</param>
		public OperationsContainer(Panel container, XCaseCanvas xCaseCanvas)
			: base(container, xCaseCanvas)
		{
			
		}

		/// <summary>
		/// Adds visualization of <paramref name="operation"/> to the control
		/// </summary>
		/// <param name="operation">visualized operation</param>
		/// <returns>Control displaying the operation</returns>
		public OperationTextBox AddOperation(Operation operation)
		{
			OperationTextBox t = new OperationTextBox(operation, classController);
			base.AddItem(t);
			return t;
		}

		/// <summary>
		/// Removes visualization of <paramref name="operation"/>/
		/// </summary>
		/// <param name="operation">removed operation</param>
		public void RemoveOperation(Operation operation)
		{
            classController.RemoveOperation(operation);
		}

		/// <summary>
		/// Reflects changs in <see cref="IOperationsContainer.OperationsCollection"/>.
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="e">event arguments</param>
		public void operationsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Clear();

			foreach (Operation operation in OperationsCollection)
			{
				AddOperation(operation);
			}
		}

		
	}
}