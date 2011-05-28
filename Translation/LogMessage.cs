using Exolutio.Model;

namespace Exolutio.Translation
{
	/// <summary>
	/// One translation log entry 
	/// </summary>
	public class LogMessage
	{
		/// <summary>
		/// Defines handler that returns image for a log message. 
		/// </summary>
		public delegate object ImageGetterHandler(LogMessage message);

		/// <summary>
		/// Fill this property with a function, that returns proper 
		/// icon for a log message (used to return icons for error or 
		/// warning).
		/// </summary>
		public static ImageGetterHandler ImageGetter;

		/// <summary>
		/// Severity of the message
		/// </summary>
		public enum ESeverity
		{
			/// <summary>
			/// Warning message
			/// </summary>
			Warning,
			/// <summary>
			/// Error message, translation contains severe errors
			/// </summary>
			Error
		}

		/// <summary>
		/// Number of the message in the log.
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// Severity of the message
		/// </summary>
		/// <value><see cref="ESeverity"/></value>
		public ESeverity Severity { get; set; }

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
		/// Element to which the message is related to 
		/// </summary>
		public Component RelatedElement { get; set; }

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
	}
}