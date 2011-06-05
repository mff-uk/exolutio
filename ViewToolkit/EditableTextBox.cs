using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit.Geometries;

namespace Exolutio.ViewToolkit
{
    /// <summary>
    /// TextBox for displaying and editing Class properties and methods
    /// </summary>
    public class EditableTextBox : TextBox, ISelectable
    {
        Brush originalTextBrush = ViewToolkitResources.BlackBrush;

        public Brush OriginalTextBrush { get { return originalTextBrush; } set { originalTextBrush = value; Foreground = value; } }
        Brush mouseOverBrush = ViewToolkitResources.RedBrush;
        public Brush MouseOverBrush { get { return mouseOverBrush; } set { mouseOverBrush = value; } }
        Brush originalBackgroundBrush = ViewToolkitResources.TransparentBrush;
        public Brush OriginalBackgroundBrush { get { return originalBackgroundBrush; } set { originalBackgroundBrush = value; Background = value; } }
        Brush editableBackgroundBrush = ViewToolkitResources.WhiteBrush;
        public Brush EditableBackgroundBrush { get { return editableBackgroundBrush; } set { editableBackgroundBrush = value; } }

#if SILVERLIGHT

        public ContextMenu ContextMenu { get; set; }

        public bool Focusable { get; set; }

#endif
        public bool CanBeEmpty = false;

        private string valueBeforeEdit = null;

        public ContextMenuItem mi_Rename;

        bool myeditable;
        
        /// <summary>
        /// Switches between editable and not editable mode
        /// </summary>
        public bool myEditable
        {
            get { return myeditable; }
            set
            {
                if (myeditable != value)
                {
                    myeditable = value;
                    if (value)
                    {
                        valueBeforeEdit = Text;
                        Background = EditableBackgroundBrush;
                        IsReadOnly = false;
                        Focusable = true;
                        Cursor = Cursors.IBeam;
                        Foreground = OriginalTextBrush;
                        //IsHitTestVisible = true;
                        Focus();
                        Select(0, Text.Length);
                    }
                    else
                    {
                        IsReadOnly = true;
                        //IsHitTestVisible = false;
                        Background = OriginalBackgroundBrush;
                        Focusable = false;
                        Cursor = Cursors.Arrow;

                        if (valueBeforeEdit != null && valueBeforeEdit != Text && TextEdited != null)
                        {
                            StringEventArgs args = new StringEventArgs { Data = Text };
                            Text = valueBeforeEdit;
                            if (CanBeEmpty || args.Data.Length > 0) TextEdited(this, args);
                        }
                        valueBeforeEdit = null;
                    }
                }
            }
        }

        public event StringEventHandler TextEdited;

        bool mousein;
        /// <summary>
        /// This is for highlighting textboxes when mouse moves over them
        /// </summary>
        bool MouseIn
        {
            get { return mousein; }
            set
            {
                mousein = value;
                if (mousein)
                {
                    Foreground = MouseOverBrush;
                }
                else
                {
                    Foreground = OriginalTextBrush;
                }
            }
        }


        public EditableTextBox()
            : base()
        {

            //Background = ViewToolkitResources.TransparentBrush;
            TextAlignment = System.Windows.TextAlignment.Left;
            BorderThickness = new Thickness(0.0);
            Padding = new Thickness(5, 0, 5, 0);
            IsReadOnly = true;
            Focusable = false;

            IsTabStop = false;
            IsHitTestVisible = true;
            Cursor = Cursors.Arrow;
            Margin = ViewToolkitResources.Thicknness0;
            KeyDown += ClassTextBox_KeyDown;
            KeyUp += EditableTextBox_KeyUp;
            MouseEnter += ClassTextBox_MouseEnter;
            MouseLeave += ClassTextBox_MouseLeave;
            LostMouseCapture += EditableTextBox_LostMouseCapture;
            LostFocus += ClassTextBox_LostFocus;
            FontFamily = new FontFamily("Trebuchet MS");
#if SILVERLIGHT
            
#else
            
            LostKeyboardFocus += new KeyboardFocusChangedEventHandler(ClassTextBox_LostKeyboardFocus);
            
            FocusVisualStyle = null;
#endif
            Background = ViewToolkitResources.TransparentBrush;
            //ResetContextMenu();
        }
        
        public virtual void SetDisplayedObject(object o, object diagram)
        {
            
        }


        void EditableTextBox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            MouseIn = false; 
        }

        //public void ResetContextMenu()
        //{
        //    ContextMenu = new ContextMenu();
        //    mi_Rename = new ContextMenuItem("Rename");
        //    mi_Rename.Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil);
        //    mi_Rename.Click += mi_Click;
        //    ContextMenu.Items.Add(mi_Rename);

        //    #if SILVERLIGHT
        //    ContextMenuService.SetContextMenu(this, ContextMenu);
        //    ContextMenu.Opened += ClassTextBox_ContextMenuOpening;
        //    ContextMenu.Closed += ClassTextBox_ContextMenuClosing;
        //    #else 
        //    ContextMenuOpening += new ContextMenuEventHandler(ClassTextBox_ContextMenuOpening);
        //    ContextMenuClosing += new ContextMenuEventHandler(ClassTextBox_ContextMenuClosing);
        //    #endif
        //}

        

        void EditableTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && myEditable) e.Handled = true;
        }

#if SILVERLIGHT
        private void ClassTextBox_ContextMenuClosing(object sender, RoutedEventArgs e)
        {
            MouseIn = false;
        }

        private void ClassTextBox_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            MouseIn = false;
        }
#else
        void ClassTextBox_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            MouseIn = false;
        }

        void ClassTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mousein = false;
        }

        void ClassTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            myEditable = false;
        }
#endif

        void ClassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            myEditable = false;
        }

        void ClassTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseIn = false;
#if SILVERLIGHT
#else
            e.Handled = false;
#endif
        }

        void ClassTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!myEditable) MouseIn = true;
#if SILVERLIGHT
#else
            e.Handled = false;
#endif
        }


        void ClassTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                myEditable = false;
                this.Focus();
#if SILVERLIGHT
#else
                UIElement parent = this.Parent as UIElement;
                if (parent != null)
                    parent.Focus();
#endif
            }
            if (e.Key == Key.Delete) e.Handled = true;
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            myEditable = true;
        }

        #if SILVERLIGHT
        public void InvalidateVisual()
        {
            
        }
        #endif

        #region Implementation of ISelectable

        public Rect GetBounds()
        {
            return new Rect(0, 0, this.ActualWidth, this.ActualHeight);
        }

        public bool CanBeDraggedInGroup { get { return false; } }

        private bool selected;
        public virtual bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                InvokeSelectedChanged();
            }
        }

        public bool Highlighted { get; set; }

        public event Action SelectedChanged;

        public void InvokeSelectedChanged()
        {
            Action handler = SelectedChanged;
            if (handler != null) handler();
        }

        #endregion

        public virtual void UnBindModelView()
        {
            
        }
    }
}
