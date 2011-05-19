using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper for component connectors. Stores points of the connector line. 
	/// </summary>
	public class ComponentConnectorViewHelper : ConnectionViewHelper<PSMSubordinateComponent>
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public ComponentConnectorViewHelper()
		{
		}

		public ComponentConnectorViewHelper(Diagram diagram) : base(diagram)
		{
		}

		private readonly ObservablePointCollection points = new ObservablePointCollection();

		public override ObservablePointCollection Points
		{
			get
			{
				return points;
			}
		}

		public override ViewHelper Clone(Diagram diagram)
		{
			return new ComponentConnectorViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			ComponentConnectorViewHelper copyComponentConnectorViewHelper = (ComponentConnectorViewHelper) copy;
			copyComponentConnectorViewHelper.Points.AppendRangeAsCopy(Points);
		}
	}
}