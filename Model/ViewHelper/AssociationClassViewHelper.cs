using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper for <see cref="AssociationClass"/>. Derives from <see cref="ClassViewHelper"/> and
	/// this inherited part contains information about the "class part" of asssociation class, 
	/// the <see cref="AssociationViewHelper"/> property contains information about 
	/// the "association part" of association class. 
	/// </summary>
	public class AssociationClassViewHelper: ClassViewHelper
	{
		public AssociationViewHelper AssociationViewHelper { get; private set;}

		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public AssociationClassViewHelper()
		{
		}

		public AssociationClassViewHelper(Diagram diagram):
			base(diagram)
		{
			X = double.NaN;
			Y = double.NaN;

			AssociationViewHelper = new AssociationViewHelper(diagram);
			AssociationViewHelper.PropertyChanged += member_PropertyChanged;

			Points = new ObservablePointCollection();
		}

		void member_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e.PropertyName);	
		}

		public ObservablePointCollection Points { get; private set; }

		public override ViewHelper Clone(Diagram diagram)
		{
			return new AssociationClassViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			AssociationClassViewHelper copyAssociationClassViewHelper = (AssociationClassViewHelper) copy;
			this.AssociationViewHelper.FillCopy(copyAssociationClassViewHelper.AssociationViewHelper, modelMap);
			copyAssociationClassViewHelper.Points.AppendRangeAsCopy(this.Points);
		}
	}
}