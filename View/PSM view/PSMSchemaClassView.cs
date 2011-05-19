using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;

namespace EvoX.View
{
    public class PSMSchemaClassView : NodeComponentViewBase<PSMSchemaClassViewHelper>
    {
        #region inner controls
        private StackPanel stackPanel;
        private EditableTextBox tbClassHeader;
        private PSMAttributesContainer attributesContainer;
        private Border border;

        #endregion

        public PSMSchemaClassView()
        {

        }

        public PSMSchemaClass PSMSchemaClass { get; private set; }

        public override Component ModelComponent
        {
            get { return PSMSchemaClass; }
            protected set { PSMSchemaClass = (PSMSchemaClass) value; }
        }

        public override PSMSchemaClassViewHelper ViewHelper { get; protected set; }

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

            Border headerBorder = new Border
            {
                Background = ViewToolkitResources.NoInterpretationBrush,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(2),
                BorderBrush = ViewToolkitResources.BlackBrush
            };

            tbClassHeader = new EditableTextBox
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Background = ViewToolkitResources.NoInterpretationBrush,
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
            StackPanel attributesSection = new StackPanel
            {
                Background = ViewToolkitResources.ClassBody
            };
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

            EvoXContextMenu evoXContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMSchemaClass, this.DiagramView.Diagram);
            evoXContextMenu.ScopeObject = PSMSchemaClass;
            evoXContextMenu.Diagram = DiagramView.Diagram;
            ContextMenu = evoXContextMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(headerBorder, ContextMenu);
#else
            headerBorder.ContextMenu = ContextMenu;
#endif
        }

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        public override void UpdateView()
        {
            base.UpdateView();
            if (PSMSchemaClass != null)
            {
                tbClassHeader.Text = PSMSchemaClass.Name;    
            }
            
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
                this.border.BorderBrush = value ? ViewToolkitResources.SelectedBorderBrush : ViewToolkitResources.BlackBrush;
            }
        }
    }
}