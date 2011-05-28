using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.Serialization;

namespace Exolutio.Model.ViewHelper
{
	/// <summary>
	/// View helper for positionable elements (elements that 
	/// have coordinates and width and height). Used as a base 
	/// class for many other view helpers.
	/// </summary>
	public abstract class PositionableElementViewHelper : ViewHelper
	{
		protected double x;
		protected double y;
		protected double width = double.NaN;
		protected double height = double.NaN;

	    protected PositionableElementViewHelper(Diagram diagram) : base(diagram)
	    {
	    }

	    /// <summary>
		/// X coordinate of the element representation in the diagram
		/// </summary>
		public double X
		{
			get { return x; }
			set
			{
				if (x != value)
				{
					x = value;
					NotifyPropertyChanged("X");
				}
			}
		}

		/// <summary>
		/// X coordinate of the element representation in the diagram
		/// </summary>
		public double Y
		{
			get { return y; }
			set
			{
				if (y != value)
				{
					y = value;
					NotifyPropertyChanged("Y");
				}
			}
		}

	    public Point Position
	    {
	        get { return new Point(X, Y); }
	    }

		public void SetPositionSilent(Point p)
		{
			SetPositionSilent(p.X, p.Y);	
		}

		public void SetPositionSilent(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Width of the element representation in the diagram
		/// </summary>
		public double Width
		{
			get { return width; }
			set
			{
				if (width != value)
				{
					width = value;
					NotifyPropertyChanged("Width");
				}
			}
		}

		/// <summary>
		/// Height of the element representation in the diagram
		/// </summary>
		public double Height
		{
			get { return height; }
			set
			{
				if (height != value)
				{
					height = value;
					NotifyPropertyChanged("Height");
				}
			}
		}

		/// <summary>
		/// Returns encompassing rectangle of the element on the diagram.
		/// </summary>
		/// <returns>Encompassing rectangle of the element on the diagram, based directly on 
		/// <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, <see cref="Height"/> properties. </returns>
		public Rect GetBounds()
		{
			return new Rect(X, Y, Width, Height);
		}

        public override void FillCopy(Versioning.IExolutioCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
 	        base.FillCopy(copyComponent, projectVersion, createdCopies);
            PositionableElementViewHelper copyPositionableElementViewHelper = (PositionableElementViewHelper)copyComponent;
            copyPositionableElementViewHelper.X = this.X;
            copyPositionableElementViewHelper.Y = this.Y;
            copyPositionableElementViewHelper.Height = this.Height;
            copyPositionableElementViewHelper.Width = this.Width;
        }

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeSimpleValueToAttribute("X", X, parentNode, context);
            this.SerializeSimpleValueToAttribute("Y", Y, parentNode, context);
            this.SerializeSimpleValueToAttribute("Width", !double.IsNaN(Width) ? Width.ToString() : "NaN", parentNode, context);
            this.SerializeSimpleValueToAttribute("Height", !double.IsNaN(Height) ? Height.ToString() : "NaN", parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            X = double.Parse(this.DeserializeSimpleValueFromAttribute("X", parentNode, context));
            Y = double.Parse(this.DeserializeSimpleValueFromAttribute("Y", parentNode, context));
            string widthStr = this.DeserializeSimpleValueFromAttribute("Width", parentNode, context);
            Width = widthStr != "NaN" ? double.Parse(widthStr) : double.NaN;
            string heightStr = this.DeserializeSimpleValueFromAttribute("Height", parentNode, context);
            Height = heightStr != "NaN" ? double.Parse(heightStr) : double.NaN;
        }
    }
}