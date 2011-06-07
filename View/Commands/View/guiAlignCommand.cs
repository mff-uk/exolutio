using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands.View
{
    /// <summary>
    /// Variants of alignment
    /// </summary>
    public enum EAlignment
    {
        Top,
        Bottom,
        Right,
        Left,
        CenterV,
        CenterH,
        DistributeV,
        DistributeH
    }

    public class guiAlignCommand : guiSelectionDependentCommand
    {
        public EAlignment Alignment { get; set; }

        public override bool CanExecute(object parameter = null)
        {
            if (Current.ActiveDiagramView == null)
                return false;
            IEnumerable<Node> selectedNodes = Current.ActiveDiagramView.SelectedViews.OfType<INodeComponentViewBase>().Select(c => c.MainNode);
            if (selectedNodes.Count() == 0)
                return false;
            IEnumerable<double> values = null;
            switch (Alignment)
            {
                case EAlignment.Top:
                    values = from Node node in selectedNodes
                             select node.Y;
                    break;
                case EAlignment.Bottom:
                    values = from Node node in selectedNodes
                             select node.Bottom;
                    break;
                case EAlignment.Left:
                    values = from Node node in selectedNodes
                             select node.X;
                    break;
                case EAlignment.Right:
                    values = from Node node in selectedNodes
                             select node.Bottom;
                    break;
                case EAlignment.CenterH:
                    values = from Node node in selectedNodes
                             select Math.Round(node.X + (node.Right - node.X) / 2);
                    break;
                case EAlignment.CenterV:
                    values = from Node node in selectedNodes
                             select Math.Round(node.Y + (node.Bottom - node.Y) / 2);
                    break;
                case EAlignment.DistributeH:
                case EAlignment.DistributeV:
                    return selectedNodes.Count() > 2;
            }

            return values != null && values.Distinct().Count() > 1;
        }

        private static void MoveNode(double ? x, double ? y, Node node)
        {
            if (x != null)
            {
                node.X = x.Value;
            }
            if (y != null)
            {
                node.Y = y.Value;
            }
        }

        public override void Execute(object parameter = null)
        {
            double minTop;
            double maxBottom;
            double distance;
            double offset;

            if (Current.ActiveDiagramView == null)
                return;
            IEnumerable<Node> selectedNodes = Current.ActiveDiagramView.SelectedViews.OfType<INodeComponentViewBase>().Select(c => c.MainNode);
            if (selectedNodes.Count() == 0)
                return;

            switch (Alignment)
            {
                case EAlignment.Top:
                    minTop = selectedNodes.Min(item => item.Y);
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(null, minTop, node);
                    }
                    break;
                case EAlignment.Bottom:
                    maxBottom = selectedNodes.Max(item => item.Bottom);
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(null, maxBottom - (node.Bottom - node.Y), node);
                    }
                    break;
                case EAlignment.Left:
                    double minLeft = selectedNodes.Min(item => item.X);
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(minLeft, null, node);
                    }
                    break;
                case EAlignment.Right:
                    double maxRight = selectedNodes.Max(item => item.Right);
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(maxRight - (node.Right - node.X), null, node);
                    }
                    break;
                case EAlignment.CenterV:
                    double centerH = Math.Round(selectedNodes.Average(item => item.Y + (item.Bottom - item.Y) / 2));
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(null, (centerH - (node.Bottom - node.Y) / 2), node);
                    }
                    break;
                case EAlignment.CenterH:
                    double centerV = Math.Round(selectedNodes.Average(item => item.X + (item.Right - item.X) / 2));
                    foreach (Node node in selectedNodes)
                    {
                        MoveNode(centerV - (node.Right - node.X) / 2, null, node);
                    }
                    break;
                case EAlignment.DistributeV:
                    minTop = selectedNodes.Min(item => item.Y);
                    maxBottom = selectedNodes.Max(item => item.Bottom);
                    double sumHeight = selectedNodes.Sum(item => item.Bottom - item.Y);

                    distance = Math.Max(0, (maxBottom - minTop - sumHeight) / (selectedNodes.Count() - 1));
                    offset = minTop;

                    foreach (Node node in selectedNodes.OrderBy(item => item.Y))
                    {
                        double delta = offset - node.Y;
                        MoveNode(null, node.Y + delta, node);

                        offset = offset + node.Bottom - node.Y + distance;
                    }

                    break;
                case EAlignment.DistributeH:
                    minLeft = selectedNodes.Min(item => item.X);
                    maxRight = selectedNodes.Max(item => item.Right);
                    double sumWidth = selectedNodes.Sum(item => item.Right - item.X);

                    distance = Math.Max(0, (maxRight - minLeft - sumWidth) / (selectedNodes.Count() - 1));
                    offset = minLeft;

                    foreach (Node node in selectedNodes.OrderBy(item => item.X))
                    {
                        double delta = offset - node.X;
                        MoveNode(node.X + delta, null, node);

                        offset = offset + node.Right - node.X + distance;
                    }

                    break;
            }
            foreach (guiAlignCommand command in AllAlignCommands)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        public override string Text
        {
            get
            {
                switch (Alignment)
                {
                    case EAlignment.Top:
                        return "Align top";
                    case EAlignment.Bottom:
                        return "Align bottm";
                    case EAlignment.Right:
                        return "Align right";
                    case EAlignment.Left:
                        return "Align left";
                    case EAlignment.CenterV:
                        return "Align center vertically";
                    case EAlignment.CenterH:
                        return "Align center horizontally";
                    case EAlignment.DistributeV:
                        return "Distribute vertically";
                    case EAlignment.DistributeH:
                        return "Distribute horizontally";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                switch (Alignment)
                {
                    case EAlignment.Top:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsTop);
                    case EAlignment.Bottom:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsBottom);
                    case EAlignment.Right:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsRight);
                    case EAlignment.Left:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsLeft);
                    case EAlignment.CenterV:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsCenteredVertical);
                    case EAlignment.CenterH:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AlignObjectsCenteredHorizontal);
                    case EAlignment.DistributeV:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.DistributeObjectsVertical);
                    case EAlignment.DistributeH:
                        return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.DistributeObjectsHorizontal);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                base.Icon = value;
            }
        }

        public IEnumerable<guiAlignCommand> AllAlignCommands { get; set; }
    }
}