using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.ViewHelper;
using EvoX.SupportingClasses;
using EvoX.ViewToolkit;

namespace EvoX.View
{
    public class PSMClassView : NodeComponentViewBase<PSMClassViewHelper>, IEnumerable<PSMAttributeTextBox>
    {
        #region inner controls
        private StackPanel stackPanel;
        private EditableTextBox tbClassHeader;
        private PSMAttributesContainer attributesContainer;
        private Border headerBorder;
        private Border attributesBorder;
        private EditableTextBox tbSRHeader;

        private Border border;

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

        protected override void CreateInnerControls(EvoXCanvas canvas)
        {
            base.CreateInnerControls(canvas);
            #region main node content components
            border = new Border
                         {
                             BorderBrush = ViewToolkitResources.NodeBorderBrush,
                             BorderThickness = new Thickness(0.8),
                             VerticalAlignment = VerticalAlignment.Stretch,
                             Opacity = 0.8
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
                                   BorderThickness = new Thickness(0),
                                   Padding = new Thickness(2),
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

            attributesContainer = new PSMAttributesContainer(attributesSection, canvas, DiagramView.Diagram);

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

            EvoXContextMenu evoXContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMClass, this.DiagramView.Diagram);
            ContextMenu = evoXContextMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(headerBorder, ContextMenu);
            DoubleClickSupplement dc = new DoubleClickSupplement();
            tbClassHeader.MouseLeftButtonDown += dc.Click;
            dc.DoubleClickW += tbClassHeader_MouseDoubleClick;
#else
            tbClassHeader.MouseDoubleClick += tbClassHeader_MouseDoubleClick;
#endif

        }

        void tbClassHeader_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenPSMClassDialog();
        }

        public void OpenPSMClassDialog()
        {
            PSMClassDialog dialog = new PSMClassDialog();
            dialog.Initialize(Current.Controller, PSMClass);
            #if SILVERLIGHT
            Current.MainWindow.FloatingWindowHost.Add(dialog);
            dialog.ShowModal();
            #else
            dialog.ShowDialog();
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
            ((EvoXContextMenu)ContextMenu).ScopeObject = PSMClass;
            ((EvoXContextMenu)ContextMenu).Diagram = DiagramView.Diagram;
            if (PSMClass != null)
            {
                attributesContainer.AttributesCollection = PSMClass.PSMAttributes;
            }
        }

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        public override void UpdateView()
        {
            base.UpdateView();
            if (PSMClass != null)
            {
                this.Name = PSMClass.Name;
                
                Brush header = PSMClass.IsStructuralRepresentative ? ViewToolkitResources.StructuralRepresentativeHeader : ViewToolkitResources.ClassHeader;
                if (PSMClass.Interpretation == null)
                {
                    header = ViewToolkitResources.NoInterpretationBrush;
                }
                Brush body = PSMClass.IsStructuralRepresentative ? ViewToolkitResources.StructuralRepresentativeBody : ViewToolkitResources.TransparentBrush;

                if (headerBorder.Background != header)
                    headerBorder.Background = header;
                if (attributesBorder.Background != body)
                    attributesBorder.Background = body;

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
                    border.BorderThickness = new Thickness(0.8);
                    border.BorderBrush = ViewToolkitResources.RedBrush;
                }
                else
                {
                    border.BorderThickness = new Thickness(0.8);
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
    }
}