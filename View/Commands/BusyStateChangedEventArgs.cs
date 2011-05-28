using System;

namespace Exolutio.View
{
    public class BusyStateChangedEventArgs:EventArgs
    {
        public bool IsBusy { get; private set; }

        public BusyStateChangedEventArgs(bool isBusy)
        {
            IsBusy = isBusy;
        }
    }
}