using System;
using System.Collections.Generic;
using Exolutio.Controller.Commands;

namespace Exolutio.Controller
{
    /// <summary>
    /// Last-in-first-out command stack
    /// </summary>
    public class CommandStack: Stack<StackedCommand>
    {
        public event EventHandler ItemsChanged;

    	public event EventHandler Invalidated;

		public new void Push(StackedCommand item)
        {
            base.Push(item);
            if (ItemsChanged != null)
				ItemsChanged(null, new EventArgs());
        }

		public new StackedCommand Pop()
        {
			StackedCommand tmp = base.Pop();
            if (ItemsChanged != null)
				ItemsChanged(null, new EventArgs());
            return tmp;
        }

        public new void Clear()
        {
            base.Clear();
            if (ItemsChanged != null)
                ItemsChanged(null, new EventArgs());
        }

		public void Invalidate()
		{
			Clear();
			if (Invalidated != null)
				Invalidated(null, new EventArgs());
		}

    	public bool Empty
    	{
			get
			{
				return this.Count == 0;
			}
    	}
    }
}
