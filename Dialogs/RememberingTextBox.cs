using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace EvoX.Dialogs
{
	/// <summary>
	/// Textbox that remembers the value with which it was initialized.	
	/// </summary>
	public class RememberingTextBox: TextBox
	{
		private string OldValue { get; set; } 

		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;

				if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(OldValue))
				{
					OldValue = value;
				}
			}
		}

		public void ForgetOldValue()
		{
			OldValue = Text;
		}

		public bool ValueChanged
		{
			get
			{
				if (String.IsNullOrEmpty(OldValue) && String.IsNullOrEmpty(Text))
					return false;
				if (String.IsNullOrEmpty(OldValue) || String.IsNullOrEmpty(Text))
					return true;
				return OldValue != Text;
			}
		}
	}
}