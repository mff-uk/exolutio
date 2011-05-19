using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XCase.Model
{
	/// <summary>
	/// ViewHelper class stores view-specific data for an instance of an element in the diagram
	/// (such as its position on the diagram or width and height of the element). These data 
	/// are not part of the UML model itself, but need to be saved and loaded to reconstruct a 
	/// previously saved diagram. 
	/// </summary>
	public class CommentViewHelper : PositionableElementViewHelper
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public CommentViewHelper()
		{

		}

        public CommentViewHelper(Diagram diagram)
			: base(diagram)
		{
			LinePoints = new ObservablePointCollection();
        	//X = double.NaN;
        	//Y = double.NaN;
		}

		/// <summary>
		/// Points of the line connecting the commentary with its <see cref="Comment.AnnotatedElement"/>.
		/// </summary>
		public ObservablePointCollection LinePoints { get; set; }

		public override ViewHelper Clone(Diagram diagram)
		{
			return new CommentViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			CommentViewHelper copyCommentViewHelper = (CommentViewHelper) copy;
			copyCommentViewHelper.LinePoints.AppendRangeAsCopy(LinePoints);
		}
	}
}
