using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Exolutio.ViewToolkit
{
    public class Label: Node
    {
        private EditableTextBox textBox;

        public Label()
        {
            CreateInnerControls();
        }

        public string Text
        {
            get { return textBox.Text; }
            set
            {
                this.Visibility = String.IsNullOrEmpty(value) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                textBox.Text = value; textBox.InvalidateVisual(); this.InvalidateVisual();
            }
        }

        public override bool CanBeDraggedInGroup
        {
            get { return false; }
        }

        public void CreateInnerControls()
        {
            textBox = new EditableTextBox
                          {
                              FontWeight = FontWeights.Bold,
                              TextAlignment = TextAlignment.Center,
                              Background = ViewToolkitResources.WhiteBrush,
                
                          };

            base.InnerContentControl.Content = textBox;

            LayoutUpdated += new EventHandler(Label_LayoutUpdated);
            PositionChanged += Label_PositionChanged;
        }

        

        void Label_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (DragThumb != null)
            {
                //Text = DragThumb.ToString();
            }
        }

        void Label_LayoutUpdated(object sender, EventArgs e)
        {
            if (DragThumb != null)
            {
                //Text = DragThumb.ToString();
            }
        }

        #if SILVERLIGHT
        public void InvalidateVisual()
        {

        }
        #endif

        #if SILVERLIGHT
        public ContextMenu ContextMenu
        {
            get
            {
                return ContextMenuService.GetContextMenu(this);
            }
            set
            {
                ContextMenuService.SetContextMenu(this, value);
            }
        }
        #endif

        #region mouse events redirected to drag thumb
#if SILVERLIGHT
        protected override void  OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
 	        base.OnMouseLeftButtonDown(e);

#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
#endif
            //ExolutioCanvas.SelectedItems.Clear();
            //ExolutioCanvas.SelectedItems.Add(this);

            //{
            //    DragThumb.OnMouseDown(e);
            //}
        }
        #endregion 
    }
}