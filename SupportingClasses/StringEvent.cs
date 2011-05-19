using System;

namespace EvoX.SupportingClasses
{
    /// <summary>
    /// Event with a string argument
    /// </summary>
    public delegate void StringEventHandler(object sender, StringEventArgs args);

    /// <summary>
    /// Argument for <see cref="StringEventHandler"/>
    /// </summary>
    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// Actual data of the event
        /// </summary>
        /// <value><see cref="String"/></value>
        public String Data { get; set; }
    }
}