using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.SupportingClasses.Annotations;

namespace Exolutio.SupportingClasses
{
	/// <summary>
	/// Log class 
	/// </summary>
	public class Log : List<LogMessage>
	{
		private int errors = 0;

		private int warnings = 0;

	    public void AddLogMessage(LogMessage logMessage)
	    {
	        this.Add(logMessage);
	        if (logMessage.Severity == LogMessage.ESeverity.Error)
	        {
	            errors++;
	        }
            else
	        {
	            warnings++;
	        }
	    }

		/// <summary>
		/// Adds error message.
		/// </summary>
		/// <param name="text">The message text.</param>
		public LogMessage AddError(string text)
		{
		    LogMessage logMessage = new LogMessage { MessageText = text, Severity = LogMessage.ESeverity.Error, Number = errors};
		    Add(logMessage);
			errors++;
		    return logMessage;
		}

		/// <summary>
		/// Adds warning message.
		/// </summary>
		/// <param name="text">The message text.</param>
		public LogMessage AddWarning(string text)
		{
		    LogMessage logMessage = new LogMessage { MessageText = text, Severity = LogMessage.ESeverity.Warning, Number = warnings};
		    Add(logMessage);
			warnings++;
		    return logMessage;
		}

        /// <summary>
        /// Adds warning message.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format string arguments.</param>
        [StringFormatMethod("format")]
        public LogMessage AddWarningFormat(string format, params object[] args)
        {
            return AddWarning(string.Format(format, args));
        }

        /// <summary>
        /// Adds error message.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format string arguments.</param>
        [StringFormatMethod("format")]
        public LogMessage AddErrorFormat(string format, params object[] args)
        {
            return AddError(string.Format(format, args));
        }

		/// <summary>
		/// Removes all elements from the log.
		/// </summary>
		public new void Clear()
		{
			base.Clear();
			errors = 0;
			warnings = 0;
		}

	    public IEnumerable<LogMessage> Errors
	    {
            get { return this.Where(m => m.Severity == LogMessage.ESeverity.Error); }
	    }

        public IEnumerable<LogMessage> Warnings
        {
            get { return this.Where(m => m.Severity == LogMessage.ESeverity.Warning); }
        }
	}
}