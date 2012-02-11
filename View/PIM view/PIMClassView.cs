using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
    public class PIMClassView : NodeComponentViewBase<PIMClassViewHelper>
    {
        #region inner controls
        private StackPanel stackPanel;
        private EditableTextBox tbClassHeader;
        private PIMAttributesContainer attributesContainer;
        private PIMOperationsContainer operationsContainer;
        private Border border;

        #endregion

        public PIMClassView()
        {

        }

        public PIMClass PIMClass { get; private set; }

        public override Component ModelComponent
        {
            get { return PIMClass; }
            protected set { PIMClass = (PIMClass) value; }
        }

        public override PIMClassViewHelper ViewHelper { get; protected set; }

        protected override void CreateInnerControls(ExolutioCanvas canvas)
        {
            base.CreateInnerControls(canvas);
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

            Border headerBorder = new Border
            {
                Background = ViewToolkitResources.ClassHeader,
                BorderThickness = ViewToolkitResources.Thickness0,
                Padding = ViewToolkitResources.Thickness2,
                BorderBrush = ViewToolkitResources.BlackBrush
            };

            tbClassHeader = new EditableTextBox
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Background = ViewToolkitResources.ClassHeader,
                FontSize = 15,
            };

            headerBorder.Child = tbClassHeader;
            stackPanel.Children.Add(headerBorder);

            Border attributesBorder = new Border
            {
                BorderBrush = ViewToolkitResources.BlackBrush,
                Visibility = Visibility.Collapsed,
                Background = ViewToolkitResources.ClassBody
            };
            Border operationsBorder = new Border
            {
                BorderBrush = ViewToolkitResources.BlackBrush,
                Visibility = Visibility.Collapsed,
                Background = ViewToolkitResources.ClassBody
            };
            StackPanel attributesSection = new StackPanel
            {
                Background = ViewToolkitResources.ClassBody
            };
            StackPanel operationsSection = new StackPanel
            {
                Background = ViewToolkitResources.ClassBody
            };
            attributesBorder.Child = attributesSection;
            operationsBorder.Child = operationsSection;
            attributesContainer = new PIMAttributesContainer(attributesSection, canvas, DiagramView);
            operationsContainer = new PIMOperationsContainer(operationsSection, canvas, DiagramView);

            stackPanel.Children.Add(attributesBorder);
            stackPanel.Children.Add(operationsBorder);

            Border[] stackBorders = new Border[] { headerBorder, attributesBorder, operationsBorder };
            ITextBoxContainer[] stackContainers = new ITextBoxContainer[] { attributesContainer, operationsContainer };
            attributesContainer.StackBorders = stackBorders;
            attributesContainer.StackContainers = stackContainers;
            operationsContainer.StackBorders = stackBorders;
            operationsContainer.StackContainers = stackContainers;

            #endregion

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PIMClass, this.DiagramView.Diagram);
            ContextMenu = exolutioContextMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(tbClassHeader, ContextMenu);          
            DoubleClickSupplement dc = new DoubleClickSupplement();
            tbClassHeader.MouseLeftButtonDown += dc.Click;
            dc.DoubleClickW += tbClassHeader_MouseDoubleClick;
#else
            tbClassHeader.MouseDoubleClick += tbClassHeader_MouseDoubleClick;
#endif

        }

        void tbClassHeader_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenPIMClassDialog();
            e.Handled = true;
        }

        public void OpenPIMClassDialog()
        {
#if SILVERLIGHT
            //PIMClassDialog dialog = new PIMClassDialog();
            //dialog.Initialize(Current.Controller, PIMClass);
            //Current.MainWindow.FloatingWindowHost.Add(dialog);
            //dialog.ShowModal();
#else
            PIMClassDialog dialog = new PIMClassDialog();
            dialog.Topmost = true;
            dialog.Initialize(Current.Controller, PIMClass);
            dialog.Show();
            dialog.Focus();
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
            ((ExolutioContextMenu) ContextMenu).ScopeObject = PIMClass;
            ((ExolutioContextMenu) ContextMenu).Diagram = DiagramView.Diagram;
            if (PIMClass != null)
            {
                attributesContainer.Collection = PIMClass.PIMAttributes;
                operationsContainer.Collection = PIMClass.PIMOperations;
            }
        }

        protected override void UnBindModelView()
        {
            attributesContainer.Clear();
            operationsContainer.Clear();
            base.UnBindModelView();
        }

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        /// <param name="propertyName"></param>
        public override void UpdateView(string propertyName = null)
        {
            base.UpdateView(propertyName);
            if (PIMClass != null)
            {
                tbClassHeader.Text = PIMClass.Name;
            }

            this.X = ViewHelper.X;
            this.Y = ViewHelper.Y;
            MainNode.Y = this.Y;
            MainNode.X = this.X;
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
                    foreach (PIMAttributeTextBox attributeTextBox in attributesContainer)
                    {
                        attributeTextBox.Selected = false;
                    }
                    foreach (PIMOperationTextBox operationTextBox in operationsContainer)
                    {
                        operationTextBox.Selected = false;
                    }
                }
            }
        }

        public override void RemoveFromDiagram()
        {
            base.RemoveFromDiagram();
        }

        public IEnumerable<PIMAttributeTextBox> AttributeTextBoxes
        {
            get { return attributesContainer; }
        }

        public IEnumerable<PIMOperationTextBox> OperationTextBoxes
        {
            get { return operationsContainer; }
        }
    }
}