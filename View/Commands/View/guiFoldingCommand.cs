using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands.View
{
    public abstract class guiGeneralFoldingCommand : guiScopeCommand
    {
        public EAlignment Alignment { get; set; }

        public abstract EFoldingAction FoldingAction { get; }

        public override bool CanExecute(object parameter = null)
        {
            if (Current.ActiveDiagramView == null)
                return false;

            IEnumerable<INodeComponentViewBase> selectedNodes = Current.ActiveDiagramView.SelectedViews.OfType<INodeComponentViewBase>();

            if (!(selectedNodes.Count() > 0 && selectedNodes.All(n => n.ViewHelper is IFoldableComponentViewHelper)))
                return false;

            return true;
        }

        public override void Execute(object parameter = null)
        {
            foreach (ComponentViewBase componentView in Current.ActiveDiagramView.SelectedViews)
            {
                ((IFoldableComponentViewHelper) (((((IComponentViewBaseVH) componentView).ViewHelper)))).IsFolded =
                    FoldingAction == EFoldingAction.Fold;
            }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }
    }

    [Scope(ScopeAttribute.EScope.PSMSchemaClass | ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMContentModel)]
    public class guiFoldCommand : guiGeneralFoldingCommand
    {
        public override string Text
        {
            get { return "Fold"; }
            set
            {
                base.Text = value;
            }
        }

        public override EFoldingAction FoldingAction
        {
            get { return EFoldingAction.Fold; }
        }
    }

    [Scope(ScopeAttribute.EScope.PSMSchemaClass | ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMContentModel)]
    public class guiUnfoldCommand : guiGeneralFoldingCommand
    {
        public override string Text
        {
            get { return "Unfold"; }
            set
            {
                base.Text = value;
            }
        }

        public override EFoldingAction FoldingAction
        {
            get { return EFoldingAction.Unfold; }
        }
    }
}