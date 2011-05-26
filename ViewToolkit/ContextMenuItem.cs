using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EvoX.ViewToolkit
{
    /// <summary>
    /// Context menu item
    /// </summary>
    public class ContextMenuItem : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItem"/> class.
        /// </summary>
        public ContextMenuItem()
            : base()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItem"/> class.
        /// </summary>
        /// <param name="text">Item caption</param>
        public ContextMenuItem(string text)
            : base()
        {
            Header = text;
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        public object ScopeObject { get; set; }

        //#if SILVERLIGHT

        /*
         * JM: I don't know why, but assigning an ImageSource directly to MenuItem.Icon 
         * does not work. 
         */

        private static readonly Dictionary<ImageSource, ImageBrush> 
            _sharedIconBrushDictionary = new Dictionary<ImageSource, ImageBrush>();
        
        public new object Icon 
        { 
            get
            {
                return base.Icon;
            }
            set
            {
                if (value == null)
                {
                    base.Icon = null;
                    return;
                }

                ImageBrush brush;
                ImageSource imageSource = (ImageSource) value;
                
                if (_sharedIconBrushDictionary.ContainsKey(imageSource))
                {
                    brush = _sharedIconBrushDictionary[imageSource];
                }
                else
                {
                    brush = new ImageBrush() {ImageSource = imageSource};
                    _sharedIconBrushDictionary[imageSource] = brush;
                }

                Rectangle rect = new Rectangle {Width = 16, Height = 16};
                rect.Fill = brush;
                base.Icon = rect;
            }
        }

        //#else 
        //#endif

    }

    public class EvoXContextMenu : ContextMenu
    {
        public object ScopeObject { get; set; }

        public object Diagram { get; set; }

        public EvoXContextMenu()
        {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
    }
}