using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;

namespace Exolutio.View
{
    /// <summary>
    /// A class for automatic layout of PSM diagrams.
    /// These diagrams have strictly tree structure where also the order of children is important,
    /// that's why user layouting is not supported and fixed automatic layout performed.
    /// </summary>
    public class VerticalTree
    {
        /// <summary>
        /// Space on canvas between two neighbouring subtrees.
        /// </summary>
        private const int horizontalSpace = 20;

        /// <summary>
        /// Space on canvas between a parent and its children.
        /// </summary>
        private const int verticalSpace = 40;

        /// <summary>
        /// Completely performs layouting of a PSM diagram.
        /// </summary>
        /// <param name="psmDiagramView">The diagram (resp. canvas) to be layouted.</param>
        public virtual void LayoutDiagram(PSMDiagramView psmDiagramView)
        {
            double left = horizontalSpace;
            foreach (PSMAssociationMember root in psmDiagramView.PSMDiagram.PSMSchema.Roots)
            {
                if (root.ParentAssociation != null)
                {
                    continue;
                }
                left += DrawTree(psmDiagramView, root, verticalSpace/2, left) + horizontalSpace;
            }

            //foreach (PSMDiagramReference reference in diagram.DiagramReferences)
            //{
            //    left += TreeLayout.DrawTree(psmDiagramView, reference, TreeLayout.verticalSpace/2, left) +
            //            TreeLayout.horizontalSpace;
            //}
        }

        /// <summary>
        /// Draws all children of given root element and counts width of its subtree.
        /// </summary>
        /// <param name="psmDiagramView">Diagram to be layouted</param>
        /// <param name="root">Root element of layouted subtree</param>
        /// <param name="top">Location of the upper border of the root's children</param>
        /// <param name="left">Location of the left border of the entire subtree</param>
        /// <returns>Width of the subtree (root not included)</returns>
        protected virtual double DrawSubtree(PSMDiagramView psmDiagramView, PSMComponent root, double top, double left)
        {
            double right = left;
            
            if (root is PSMAssociationMember)
            {
                ComponentViewBase componentView = psmDiagramView.RepresentantsCollection[root];
                if (componentView is IComponentViewBaseVH && ((IComponentViewBaseVH)componentView).ViewHelper is IFoldableComponentViewHelper)
                {
                    if (((IFoldableComponentViewHelper)((IComponentViewBaseVH)componentView).ViewHelper).IsFolded)
                    {
                        return right - left;
                    }
                }

                PSMAssociationMember rootAM = (PSMAssociationMember) root;
                if (rootAM.ChildPSMAssociations.Count > 0)
                {
                    foreach (PSMAssociation childAssociation in rootAM.ChildPSMAssociations)
                    {
                        right += DrawTree(psmDiagramView, (childAssociation).Child, top, right) + horizontalSpace;
                    }
                    if (rootAM is PSMClass) foreach (PSMGeneralization generalization in (rootAM as PSMClass).GeneralizationsAsGeneral)
                    {
                        right += DrawTree(psmDiagramView, (generalization).Specific, top, right) + horizontalSpace;
                    }
                }
                if (right != left) right -= horizontalSpace;
            }
            
            return right - left;
        }

        /// <summary>
        /// Draws given root element and all its children and counts width of its subtree.
        /// </summary>
        /// <param name="psmDiagramView">Diagram to be layouted</param>
        /// <param name="root">Root element of layouted subtree</param>
        /// <param name="top">Location of the upper border of the root</param>
        /// <param name="left">Location of the left border of the entire subtree</param>
        /// <returns>Width of the subtree (root included)</returns>
        protected virtual double DrawTree(PSMDiagramView psmDiagramView, PSMComponent root, double top, double left)
        {
            if (!psmDiagramView.RepresentantsCollection.IsElementPresent(root)) return -horizontalSpace;
            INodeComponentViewBase element = (psmDiagramView.RepresentantsCollection[root] as INodeComponentViewBase);
            if (element.MainNode == null)
            {
                return -1;
            }
            double height = element.ActualHeight;
            double width = element.ActualWidth;
            double right = left + DrawSubtree(psmDiagramView, root, top + height + verticalSpace, left);
            if (right == left)
            {
                right = left + width;
            }
            else
            {
                if (right < left + width)
                {
                    double subtreeWidth = right - left;
                    DrawSubtree(psmDiagramView, root, top + height + verticalSpace, left + (width - subtreeWidth) / 2);
                    right = left + width;
                }
            }
            double x = Math.Round((right + left) / 2 - width / 2);
            double y = Math.Round(top);
            
            // ReSharper disable RedundantCheckBeforeAssignment
            //if (element.X != x)
            {
                element.X = x;
            }
            //if (element.Y != y)
            {
                element.Y = y;
            }
            // ReSharper restore RedundantCheckBeforeAssignment
            
            return right - left;
        }
    }
}
