namespace Exolutio.SupportingClasses
{
    public class LogMessage : LogMessage<object>
    {
        
    }

    public interface ILogMessage
    {
        /// <summary>
        /// Number of the message in the log.
        /// </summary>
        int Number { get; set; }

        /// <summary>
        /// Severity of the message
        /// </summary>
        /// <value><see cref="ELogMessageSeverity"/></value>
        ELogMessageSeverity Severity { get; set; }

        /// <summary>
        /// Message text
        /// </summary>
        string MessageText { get; set; }

        /// <summary>
        /// Line on which the error occurred
        /// </summary>
        int Line { get; set; }

        /// <summary>
        /// Column in which the error occurred
        /// </summary>
        int Column { get; set; }

        /// <summary>
        /// Returns image for the message. 
        /// </summary>
        object Image { get; }

        ILogMessage RelatedMessage { get; set; }
    }
    /// <summary>
    /// Defines handler that returns image for a log message. 
	/// </summary>
    public delegate object ImageGetterHandler(ILogMessage message);
    
    internal static class LogMessageImageHelper
    {
        public static ImageGetterHandler ImageGetter { get; set; }
    }

    /// <summary>
	/// One log entry 
	/// </summary>
	public class LogMessage<TTag> : ILogMessage
    {
		

		/// <summary>
		/// Fill this property with a function, that returns proper 
		/// icon for a log message (used to return icons for error or 
		/// warning).
		/// </summary>
        public static ImageGetterHandler ImageGetter { get { return LogMessageImageHelper.ImageGetter; } set { LogMessageImageHelper.ImageGetter = value; } }

	    /// <summary>
		/// Number of the message in the log.
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// Severity of the message
		/// </summary>
		/// <value><see cref="ELogMessageSeverity"/></value>
		public ELogMessageSeverity Severity { get; set; }

		/// <summary>
		/// Message text
		/// </summary>
		public string MessageText { get; set; }

		/// <summary>
		/// Line on which the error occurred
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		/// Column in which the error occurred
		/// </summary>
		public int Column { get; set; }
        
		/// <summary>
		/// Returns image for the message. 
		/// The image is selected by the <see cref="ImageGetter"/>
		/// function. If no image getter function is provided, 
		/// <c>null</c> is returned. 
		/// </summary>
		public object Image
		{
			get
			{
				if (ImageGetter != null)
				{
					return ImageGetter(this);
				}
				else
					return null;
			}
		}

        public LogMessage<TTag> RelatedMessage { get; set; }

        ILogMessage ILogMessage.RelatedMessage
        {
            get { return RelatedMessage; }
            set { RelatedMessage = (LogMessage<TTag>) value; }
        }

        public TTag Tag { get; set; }
	}

    /// <summary>
    /// Severity of the message
    /// </summary>
    public enum ELogMessageSeverity
    {
        /// <summary>
        /// Warning message
        /// </summary>
        Warning,
        /// <summary>
        /// Error message, contains severe errors
        /// </summary>
        Error
    }
}