using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.SupportingClasses.Annotations;

namespace Exolutio.SupportingClasses
{
	public interface ILog : IList<ILogMessage>
	{
		IEnumerable<ILogMessage> Errors { get; }
		
		IEnumerable<ILogMessage> Warnings { get; }
		
		int CountOfErrors { get; }
		
		int CountOfWarnings { get; }
		
		void AddLogMessage(ILogMessage logMessage);
	}


	public class Log : Log<object>
	{
		
	}

	/// <summary>
	/// Log class 
	/// </summary>
	public class Log<TMessageTag> : List<LogMessage<TMessageTag>>, ILog
	{
		public int CountOfErrors { get { return Errors.Count(); } }

		public int CountOfWarnings { get { return Warnings.Count(); } }
		
		public void AddLogMessage(ILogMessage logMessage)
		{
			if (logMessage is LogMessage<TMessageTag>)
			{
				base.Add((LogMessage<TMessageTag>) logMessage);
			}
			else
			{
				throw new ArgumentException("Log accepts only messages of type 'LogMessage<TMessageTag>'");
			}
		}

		public void AddLogMessage(LogMessage<TMessageTag> logMessage)
		{
            logMessage.Number = logMessage.Severity == ELogMessageSeverity.Error ? CountOfErrors + 1 : CountOfWarnings + 1;
			base.Add(logMessage);
		}

		/// <summary>
		/// Adds error message.
		/// </summary>
		/// <param name="text">The message text.</param>
		/// <param name="tag">Tag of the created message</param>
		public LogMessage<TMessageTag> AddError(string text, TMessageTag tag = default(TMessageTag))
		{
			LogMessage<TMessageTag> logMessage = new LogMessage<TMessageTag> { MessageText = text, Severity = ELogMessageSeverity.Error, Number = CountOfErrors + 1, Tag = tag };
			Add(logMessage);
			return logMessage;
		}

		/// <summary>
		/// Adds warning message.
		/// </summary>
		/// <param name="text">The message text.</param>
		/// <param name="tag">Tag of the created message</param>
		public LogMessage<TMessageTag> AddWarning(string text, TMessageTag tag = default(TMessageTag))
		{
			LogMessage<TMessageTag> logMessage = new LogMessage<TMessageTag> { MessageText = text, Severity = ELogMessageSeverity.Warning, Number = CountOfWarnings + 1, Tag = tag };
			Add(logMessage);
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
		/// Adds warning message.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="args">The format string arguments.</param>
		/// <param name="tag">Tag of the created message</param>
		[StringFormatMethod("format")]
		public LogMessage<TMessageTag> AddWarningTaggedFormat(string format, TMessageTag tag, params object[] args)
		{
			return AddWarning(string.Format(format, args), tag);
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
		/// Adds error message.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="args">The format string arguments.</param>
		/// <param name="tag">Tag of the created message</param>
		[StringFormatMethod("format")]
		public LogMessage<TMessageTag> AddErrorTaggedFormat(string format, TMessageTag tag, params object[] args)
		{
			return AddError(string.Format(format, args), tag);
		}

        public void Add(ILogMessage item)
		{
			if (item is LogMessage<TMessageTag>)
			{
			    item.Number = item.Severity == ELogMessageSeverity.Error ? CountOfErrors + 1 : CountOfWarnings + 1;
				base.Add((LogMessage<TMessageTag>)item);
			}
			else
			{
				throw new ArgumentException("Log accepts only messages of type 'LogMessage<TMessageTag>'");
			}
		}

		/// <summary>
		/// Removes all elements from the log.
		/// </summary>
		public new void Clear()
		{
			base.Clear();
		}

		public bool Contains(ILogMessage item)
		{
			if (item is LogMessage<TMessageTag>)
				return base.Contains((LogMessage<TMessageTag>) item);
			else return false;
		}

		public void CopyTo(ILogMessage[] array, int arrayIndex)
		{
			foreach (LogMessage<TMessageTag> logMessage in this)
			{
				array[arrayIndex] = logMessage;
				arrayIndex++;
			}
		}

		public bool Remove(ILogMessage item)
		{
			if (item is LogMessage<TMessageTag>)
			{
				return base.Remove((LogMessage<TMessageTag>) item);
			}
			else
			{
				return false;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerable<LogMessage<TMessageTag>> Errors
		{
			get { return this.Where<LogMessage<TMessageTag>>(m => m.Severity == ELogMessageSeverity.Error); }
		}

		public IEnumerable<LogMessage<TMessageTag>> Warnings
		{
			get { return this.Where<LogMessage<TMessageTag>>(m => m.Severity == ELogMessageSeverity.Warning); }
		}

		IEnumerable<ILogMessage> ILog.Warnings
		{
			get { return Warnings;}
		}

		IEnumerable<ILogMessage> ILog.Errors
		{
			get { return Errors; }
		}

		#region Implementation of IList<ILogMessage>

		public int IndexOf(ILogMessage item)
		{
			if (item is LogMessage<TMessageTag>)
			{
				return base.IndexOf((LogMessage<TMessageTag>) item);
			}
			else
			{
				return -1; 
			}
		}

		public void Insert(int index, ILogMessage item)
		{
			if (item is LogMessage<TMessageTag>)
			{
                item.Number = item.Severity == ELogMessageSeverity.Error ? CountOfErrors + 1 : CountOfWarnings + 1;
				base.Insert(index, (LogMessage<TMessageTag>)item);
			}
			else
			{
				throw new ArgumentException("Log accepts only messages of type 'LogMessage<TMessageTag>'");
			}
		}

		ILogMessage IList<ILogMessage>.this[int index]
		{
			get { return base[index]; }
			set
			{
				if (value is LogMessage<TMessageTag>)
				{
					base[index] = (LogMessage<TMessageTag>) value;
				}
				else
				{
					throw new ArgumentException("Log accepts only messages of type 'LogMessage<TMessageTag>'");
				}
			}
		}

		#endregion

		#region Implementation of IEnumerable<out ILogMessage>

		IEnumerator<ILogMessage> IEnumerable<ILogMessage>.GetEnumerator()
		{
			return base.GetEnumerator();
		}

		#endregion
	}
}