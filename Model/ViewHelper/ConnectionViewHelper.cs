using System;
using System.Collections.Generic;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.ViewHelper
{
	/// <summary>
	/// View helper for elements that are represented as (poly)lines on the diagram . 
	/// </summary>
	public abstract class ConnectionViewHelper : ViewHelper
	{
	    protected ConnectionViewHelper(Diagram diagram)
			: base(diagram)
		{
		}

	    /// <summary>
	    /// Returns connection points.
	    /// </summary>
	    public abstract ObservablePointCollection Points { get; }
	}
}