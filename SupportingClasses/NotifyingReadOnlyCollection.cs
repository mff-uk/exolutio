using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace EvoX.SupportingClasses
{
	public class NotifyingReadOnlyCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged
	{
		public NotifyingReadOnlyCollection(IList<T> list) : base(list)
		{
		}

		#region Implementation of INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void InvokeCollectionChanged()
		{
			InvokeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void InvokeCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler changed = CollectionChanged;
			if (changed != null) changed(this, e);
		}

		#endregion
	}
}