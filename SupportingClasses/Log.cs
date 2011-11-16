using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.SupportingClasses.Annotations;

namespace Exolutio.SupportingClasses
{
    public class Log : Log<object>
    {
        
    }

	/// <summary>
	/// Log class 
	/// </summary>
    public class Log<TMessageTag> : List<LogMessage<TMessageTag>>
	{
		private int errors = 0;

		private int warnings = 0;

        public void AddLogMessage(LogMessage<TMessageTag> logMessage)
	    {
	        this.Add(logMessage);
            if (logMessage.Severity == ELogMessageSeverity.Error)
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
        public LogMessage<TMessageTag> AddError(string text)
		{
            LogMessage<TMessageTag> logMessage = new LogMessage<TMessageTag> { MessageText = text, Severity = ELogMessageSeverity.Error, Number = errors };
		    Add(logMessage);
			errors++;
		    return logMessage;
		}

		/// <summary>
		/// Adds warning message.
		/// </summary>
		/// <param name="text">The message text.</param>
        public LogMessage<TMessageTag> AddWarning(string text)
		{
            LogMessage<TMessageTag> logMessage = new LogMessage<TMessageTag> { MessageText = text, Severity = ELogMessageSeverity.Warning, Number = warnings };
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
        public LogMessage<TMessageTag> AddWarningFormat(string format, params object[] args)
        {
            return AddWarning(string.Format(format, args));
        }

        /// <summary>
        /// Adds error message.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format string arguments.</param>
        [StringFormatMethod("format")]
        public LogMessage<TMessageTag> AddErrorFormat(string format, params object[] args)
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

        public IEnumerable<LogMessage<TMessageTag>> Errors
	    {
            get { return this.Where(m => m.Severity == ELogMessageSeverity.Error); }
	    }

        public IEnumerable<LogMessage<TMessageTag>> Warnings
        {
            get { return this.Where(m => m.Severity == ELogMessageSeverity.Warning); }
        }

	    public IEnumerable<ILogMessage>  AllMessages
	    {
            get { return this; }
	    }
    }
}