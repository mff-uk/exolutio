using System;
using System.Windows.Input;

namespace Exolutio.SupportingClasses
{
    /// <summary>
    /// Handles double click events for a single control
    /// </summary>
    public class DoubleClickSupplement
    {
        /// <summary>
        /// Holds the last time that the control was clicked
        /// </summary>
        private DateTime m_LastClickTime = DateTime.MinValue;

        /// <summary>
        /// Fired when a double click is registered
        /// </summary>
        public event EventHandler DoubleClick;

        public event MouseButtonEventHandler DoubleClickW;

        /// <summary>
        /// Registers a single click on the control.  Call this every time a click happens.
        /// </summary>
        public void Click()
        {
            // If the time between clicks is less than 500 milliseconds then this is a double click
            if (DateTime.Now - m_LastClickTime < TimeSpan.FromMilliseconds(500))
            {
                if (DoubleClick != null)
                {
                    DoubleClick(this, new EventArgs());
                }

                if (DoubleClickW != null)
                {
                    DoubleClickW(this, null);
                }

                // Reset the time so that 3 clicks is not registered as 2 double clicks
                m_LastClickTime = DateTime.MinValue;
            }
            else
            {
                // Record the last click time
                m_LastClickTime = DateTime.Now;
            }
        }

        public void Click(object sender, MouseButtonEventArgs e)
        {
            Click();
        }
    }
}