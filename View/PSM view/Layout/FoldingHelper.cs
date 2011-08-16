using System;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;

namespace Exolutio.View
{
    public static class FoldingHelper
    {
        public static void FoldChildrenRecursive(PSMComponent parent, DiagramView activeDiagramView, EFoldingAction foldingAction)
        {
            IEnumerable<PSMComponent> childComponents = ModelIterator.GetPSMChildren(parent, false, true);
            foreach (PSMComponent childComponent in childComponents)
            {
                if (!activeDiagramView.RepresentantsCollection.IsElementPresent(childComponent))
                {
                    continue;
                }
                ComponentViewBase childView = activeDiagramView.RepresentantsCollection[childComponent];
                bool proceed = true;
                if (childView.DownCastSatisfies<IComponentViewBaseVH>(c => c.ViewHelper.DownCastSatisfies<IFoldableComponentViewHelper>(
                    fvh => (fvh.IsFolded && foldingAction == EFoldingAction.Fold) || fvh.IsFolded && foldingAction == EFoldingAction.Unfold)))
                {
                    proceed = false;
                }
                FoldRecursive(childComponent, activeDiagramView, proceed, foldingAction);
            }
        }



        public static void FoldRecursive(PSMComponent psmComponent, DiagramView activeDiagramView, bool proceed, EFoldingAction foldingAction)
        {
            if (!activeDiagramView.RepresentantsCollection.IsElementPresent(psmComponent))
            {
                return;
            }

            ComponentViewBase componentView = activeDiagramView.RepresentantsCollection[psmComponent];

            if (foldingAction == EFoldingAction.Fold)
            {
                componentView.HideAllControls();
            }
            else
            {
                componentView.UnHideAllControls();
            }
            if (proceed)
            {
                FoldChildrenRecursive(psmComponent, activeDiagramView, foldingAction);
            }
        }
    }
}