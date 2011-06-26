using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
    public class PSMContentModelView : NodeComponentViewBase<PSMContentModelViewHelper>
    {
        #region inner controls
        private StackPanel stackPanel;
        private EditableTextBox tbContentModelHeader;
        private Border border;

        #endregion

        public PSMContentModelView()
        {

        }

        public PSMContentModel PSMContentModel { get; private set; }

        public override Component ModelComponent
        {
            get { return PSMContentModel; }
            protected set { PSMContentModel = (PSMContentModel) value; }
        }

        public override PSMContentModelViewHelper ViewHelper { get; protected set; }

        protected override void CreateInnerControls(ExolutioCanvas canvas)
        {
            base.CreateInnerControls(canvas);
            #region main node content components
            border = new Border
                         {
                             BorderBrush = ViewToolkitResources.NodeBorderBrush,
                             BorderThickness = ViewToolkitResources.Thickness1,
                             Background = ViewToolkitResources.NoInterpretationBrush,
                             Opacity = ViewToolkitResources.LittleOpaque,
                             Padding = ViewToolkitResources.Thickness0,
                             CornerRadius = new CornerRadius(15)
                         };
            MainNode.InnerContentControl.Content = border;
            MainNode.MinWidth = 50;

            stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            border.Child = stackPanel;

            tbContentModelHeader = new EditableTextBox
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Background = ViewToolkitResources.TransparentBrush,
                FontSize = 15,
            };

            stackPanel.Children.Add(tbContentModelHeader);
            
            #endregion

            ExolutioContextMenu exolutioContextMenu = MenuHelper.GetContextMenu(ScopeAttribute.EScope.PSMContentModel, this.DiagramView.Diagram);
            ContextMenu = exolutioContextMenu;

#if SILVERLIGHT
            ContextMenuService.SetContextMenu(tbContentModelHeader, ContextMenu);
#endif
        }

        public override ContextMenu ContextMenu
        {
            get { return tbContentModelHeader.ContextMenu; }
            set { tbContentModelHeader.ContextMenu = value; }
        }

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        public override void UpdateView()
        {
            base.UpdateView();
            if (PSMContentModel != null)
            {
                switch (PSMContentModel.Type)
                {
                    case PSMContentModelType.Sequence:
                        tbContentModelHeader.Text = " , ";
                        break;
                    case PSMContentModelType.Choice:
                        tbContentModelHeader.Text = " | ";
                        break;
                    case PSMContentModelType.Set:
                        tbContentModelHeader.Text = " { } ";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            MainNode.UpdateCanvasPosition(true);
        }

        protected override void BindModelView()
        {
            base.BindModelView();
            ((ExolutioContextMenu) ContextMenu).ScopeObject = this.ModelComponent;
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