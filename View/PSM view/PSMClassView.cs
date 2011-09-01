using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;
using Component = Exolutio.Model.Component;

namespace Exolutio.View
{
    public class PSMClassView : NodeComponentViewBase<PSMClassViewHelper>, IEnumerable<PSMAttributeTextBox>, IChangesInScreenShotView
    {
        #region inner controls
        private StackPanel stackPanel;
        private EditableTextBox tbClassHeader;
        private PSMAttributesContainer attributesContainer;
        private Border headerBorder;
        private Border attributesBorder;
        private EditableTextBox tbSRHeader;
        private Border border;
        private FoldingButton foldingButton;

        #endregion

        public PSMClassView()
        {

        }

        public PSMClass PSMClass { get; private set; }

        public override Component ModelComponent
        {
            get { return PSMClass; }
            protected set { PSMClass = (PSMClass) value; }
        }

        public override PSMClassViewHelper ViewHelper { get; protected set; }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (tbClassHeader != null)
                {
                    tbClassHeader.Text = value;
                }
            }
        }

        protected override void CreateInnerControls(ExolutioCanvas canvas)
        {
            base.CreateInnerControls(canvas);

            foldingButton = new FoldingButton();
            MainNode.InnerConnectorControl.Children.Add(foldingButton);
            Canvas.SetBottom(foldingButton, -15);
            foldingButton.Click += delegate { this.ViewHelper.IsFolded = !this.ViewHelper.IsFolded; };
            
            #region main node content components
            border = new Border
                         {
                             BorderBrush = ViewToolkitResources.NodeBorderBrush,
                             BorderThickness = ViewToolkitResources.Thickness1,
                             VerticalAlignment = VerticalAlignment.Stretch,
                             Opacity = ViewToolkitResources.LittleOpaque
                         };
            MainNode.InnerContentControl.Content = border;

            stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            border.Child = stackPanel;

            headerBorder = new Border
                               {
                                   Background = ViewToolkitResources.ClassHeader,
                                   BorderThickness = ViewToolkitResources.Thickness0,
                                   Padding = ViewToolkitResources.Thickness2,
                                   BorderBrush = ViewToolkitResources.BlackBrush
                               };

            tbSRHeader = new EditableTextBox
                             {
                                 Visibility = Visibility.Collapsed
                             };

            tbClassHeader = new EditableTextBox
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                FontSize = 15,
            };

            StackPanel headerPanel = new StackPanel();
            headerPanel.Children.Add(tbSRHeader);
            headerPanel.Children.Add(tbClassHeader);
            headerBorder.Child = headerPanel;
            stackPanel.Children.Add(headerBorder);

            attributesBorder = new Border
                                   {
                                       BorderBrush = ViewToolkitResources.BlackBrush,
                                       Visibility = Visibility.Collapsed,
                                   };
            StackPanel attributesSection = new StackPanel();
            attributesBorder.Child = attributesSection;

            attributesContainer = new PSMAttributesContainer(attributesSection, canvas, DiagramView);

            stackPanel.Children.Add(attributesBorder);
            //Border operationsBorder = new Border
            //{
            //    BorderBrush = ViewToolkitResources.BlackBrush,
            //    Visibility = Visibility.Collapsed,
            //    Background = ViewToolkitResources.SeaShellBrush
            //};

            //StackPanel operationsSection = new StackPanel
            //{
            //    Background = ViewToolkitResources.SeaShellBrush
            //};
            //operationsBorder.Child = operationsSection;
            //stackPanel.Children.Add(operationsBorder);

            Border[] stackBorders = new Border[] { headerBorder, attributesBorder };
            ITextBoxContainer[] stackContainers = new ITextBoxContainer[] { attributesContainer };
            attributesContainer.StackBorders = stackBorders;
            attributesContainer.StackContainers = stackContainers;
            //classOperations.StackBorders = stackBorders;
            //classOperations.StackContainers = stackContainers;

            #endregion

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMClass, this.DiagramView.Diagram);
            ContextMenu = exolutioContextMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(headerBorder, ContextMenu);
            DoubleClickSupplement dc = new DoubleClickSupplement();
            tbClassHeader.MouseLeftButtonDown += dc.Click;
            dc.DoubleClickW += tbClassHeader_MouseDoubleClick;
#else
            tbClassHeader.MouseDoubleClick += tbClassHeader_MouseDoubleClick;
            tbClassHeader.MouseEnter += tbClassHeader_MouseEnter;
            tbClassHeader.MouseLeave += tbClassHeader_MouseLeave;
#endif

        }

        protected void tbClassHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            DiagramView.InvokeVersionedElementMouseEnter(this, PSMClass);
        }

        protected void tbClassHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            DiagramView.InvokeVersionedElementMouseLeave(this, PSMClass);
        }

        void tbClassHeader_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenPSMClassDialog();
        }

        public void OpenPSMClassDialog()
        {
            #if SILVERLIGHT
            #else
            PSMClassDialog dialog = new PSMClassDialog();
            dialog.Topmost = true;
            dialog.Initialize(Current.Controller, PSMClass);
            dialog.Show();
            #endif
        }


#if SILVERLIGHT
#else

        public override ContextMenu ContextMenu
        {
            get { return tbClassHeader.ContextMenu; }
            set { tbClassHeader.ContextMenu = value; }
        }
#endif

        protected override void BindModelView()
        {
            base.BindModelView();
            ((ExolutioContextMenu)ContextMenu).ScopeObject = PSMClass;
            ((ExolutioContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
            if (PSMClass != null)
            {
                attributesContainer.Collection = PSMClass.PSMAttributes;
                if (PSMClass.RepresentedClass != null)
                {
                    BindToRepresentedClass(PSMClass.RepresentedClass);
                }
            }
        }

        protected override void  UnBindModelView()
        {
            if (representedClass != null)
            {
                UnBindFromRepresentedClass();
            }
 	        base.UnBindModelView();
        }

        #region represented class binding
        /* This component view must update also when the PSMClass's represented class is updated. 
         * This piece ensures that (the situation is not covered by standard binding to model) */

        private PSMClass representedClass;

        private void BindToRepresentedClass(PSMClass representedClass)
        {
            representedClass.PropertyChanged += RepresentedClass_PropertyChanged;
            this.representedClass = representedClass;
            UpdateView();
        }

        private void UnBindFromRepresentedClass()
        {
            representedClass.PropertyChanged -= RepresentedClass_PropertyChanged;
            this.representedClass = null;
        }

        void RepresentedClass_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
 	        UpdateView();
        }

        #endregion

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        /// <param name="propertyName"></param>
        public override void UpdateView(string propertyName = null)
        {
            base.UpdateView(propertyName);
            if (PSMClass != null)
            {
                this.Name = PSMClass.Name;
                
                Brush header;
                Brush body;

                if (!PSMClass.IsStructuralRepresentative && PSMClass.Interpretation != null)
                {
                    header = ViewToolkitResources.ClassHeader;
                    body = ViewToolkitResources.TransparentBrush;
                }
                else if (PSMClass.IsStructuralRepresentative && PSMClass.Interpretation != null)
                {
                    header = ViewToolkitResources.StructuralRepresentativeHeader;
                    body = ViewToolkitResources.StructuralRepresentativeBody;
                }
                else if (PSMClass.IsStructuralRepresentative && PSMClass.Interpretation == null)
                {
                    header = ViewToolkitResources.StructuralRepresentativeHeaderNoInterpretation;
                    body = ViewToolkitResources.StructuralRepresentativeBody;
                }
                else //if (!PSMClass.IsStructuralRepresentative && PSMClass.Interpretation == null)
                {
                    header = ViewToolkitResources.NoInterpretationBrush;
                    body = ViewToolkitResources.TransparentBrush;
                }
                
                
                #region represented class binding
                if (PSMClass.RepresentedClass != representedClass)
                {
                    if (representedClass != null)
                    {
                        UnBindFromRepresentedClass();
                    }
                    if (PSMClass.RepresentedClass != null)
                    {
                        BindToRepresentedClass(PSMClass.RepresentedClass);
                    }
                }
                #endregion 

                if (headerBorder.Background != header)
                {
                    headerBorder.Background = header;
                }
                if (attributesBorder.Background != body)
                {
                    attributesBorder.Background = body;
                    foreach (PSMAttributeTextBox psmAttributeTextBox in attributesContainer)
                    {
                        psmAttributeTextBox.RefreshTextContent();
                    }
                }

                #region represented class text
                if (PSMClass.IsStructuralRepresentative)
                {
                    tbSRHeader.Visibility = Visibility.Visible;
                    tbSRHeader.TextAlignment = TextAlignment.Right;
                    tbSRHeader.Text = PSMClass.RepresentedClass.Name;
                }
                else
                {
                    tbSRHeader.Text = String.Empty;
                    tbSRHeader.Visibility = Visibility.Collapsed;
                }
                #endregion

                if (foldingButton.Folded != ViewHelper.IsFolded)
                {
                    foldingButton.Folded = ViewHelper.IsFolded;
                    FoldingHelper.FoldChildrenRecursive(PSMClass, DiagramView, ViewHelper.IsFolded ? EFoldingAction.Fold : EFoldingAction.Unfold);
                }
            }

            tbClassHeader.Text = Name;
            MainNode.UpdateCanvasPosition(true);
        }

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
                if (value)
                {
                    border.BorderThickness = ViewToolkitResources.Thickness1;
                    border.BorderBrush = ViewToolkitResources.RedBrush;
                }
                else
                {
                    border.BorderThickness = ViewToolkitResources.Thickness1;
                    border.BorderBrush = ViewToolkitResources.NodeBorderBrush;
                    foreach (PSMAttributeTextBox attributeTextBox in attributesContainer)
                    {
                        attributeTextBox.Selected = false;
                    }
                }
            }
        }

        public override void RemoveFromDiagram()
        {
            attributesContainer.Clear();
            base.RemoveFromDiagram();
        }

        public IEnumerator<PSMAttributeTextBox> GetEnumerator()
        {
            return attributesContainer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void EnterScreenShotView()
        {
            foldingButton.Visibility = Visibility.Hidden;
        }

        public void ExitScreenShotView()
        {
            foldingButton.Visibility = Visibility.Visible;
        }
    }
}