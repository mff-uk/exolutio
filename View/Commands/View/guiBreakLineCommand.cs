using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands.View
{
    public class guiBreakLineCommand : guiCommandBase
    {
        public Connector Connector { get; set; }

        public Point ? Point { get; set; }

        public override bool CanExecute(object parameter = null)
        {
            return true; 
        }

        public override void Execute(object parameter = null)
        {
            if (Point == null)
            {
                Connector.BreakAtPoint(Connector.MousePointWhenContextMenuOpened);
            }
            else
            {
                Connector.BreakAtPoint(Point.Value);
            }
        }

        public override string Text
        {
            get { return "Break line here"; }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }
    }
}