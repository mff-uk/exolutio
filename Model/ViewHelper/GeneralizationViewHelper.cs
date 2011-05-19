using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper for <see cref="Generalization"/>, contains points of the 
	/// generalization arrow. 
	/// </summary>
	public class GeneralizationViewHelper : ConnectionViewHelper<Generalization>
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public GeneralizationViewHelper()
		{
		}

		public GeneralizationViewHelper(Diagram diagram)
			: base(diagram)
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
			return new GeneralizationViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			GeneralizationViewHelper copyGeneralizationViewHelper = (GeneralizationViewHelper) copy;
			
			copyGeneralizationViewHelper.Points.AppendRangeAsCopy(Points);
		}
	}
}