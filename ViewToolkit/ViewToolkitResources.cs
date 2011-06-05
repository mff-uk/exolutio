using System;
using System.Windows;
using System.Windows.Media;

namespace Exolutio.ViewToolkit
{
    public static class ViewToolkitResources
    {
        private static readonly Brush nodeBorderBrush;
        private static readonly Brush canvasBackgroundBrush;
        private static readonly SolidColorBrush blackBrush;
        private static readonly SolidColorBrush whiteBrush;
        private static readonly SolidColorBrush redBrush;
        private static readonly SolidColorBrush transparentBrush;
        private static readonly SolidColorBrush seaShellBrush;
        private static readonly SolidColorBrush classSelectedAttribute;
        private static readonly SolidColorBrush classHeader;
        private static readonly SolidColorBrush goldBrush;
        private static readonly SolidColorBrush noInterpretationBrush;
        private static readonly SolidColorBrush noInterpretationBrushSelected;
        private static readonly SolidColorBrush structuralRepresentativeHeader;
        private static readonly SolidColorBrush structuralRepresentativeHeaderNoInterpretation;
        private static readonly SolidColorBrush structuralRepresentativeBody;
        private static readonly SolidColorBrush structuralRepresentativeBodySelected;
        private static readonly SolidColorBrush structuralRepresentativeBodyNoInterpretation;
        private static readonly Brush interpretedAttributeBrush;
        private static readonly SolidColorBrush selectedBorderBrush;
        private static readonly SolidColorBrush ribbonBackstageDimText;
        private static readonly SolidColorBrush navyBrush;
        private static readonly SolidColorBrush greyBrush;
        private static readonly Thickness thicnkees0;
        private static readonly Thickness thicnkees1;
        private static readonly Thickness thicnkees2;
        private static readonly Thickness thicnkees5;

#if SILVERLIGHT
        
#else
        private static Pen solidBlackPen;

        private static readonly Pen transparentPen;
        private static Pen junctionSelectedPen;
        private static Pen interpretedAssociationPen;

        public static Pen SolidBlackPen
        {
            get
            {
                return solidBlackPen;
            }
        }

        public static Pen InterpretedAssociationPen
        {
            get { return interpretedAssociationPen; }
        }

        public static Pen TransparentPen
        {
            get { return transparentPen; }
        }

        public static Pen JunctionSelectedPen
        {
            get {
                return junctionSelectedPen;
            }
        }
#endif

        public static Brush NodeBorderBrush
        {
            get { return nodeBorderBrush; }
        }

        public static SolidColorBrush GoldBrush
        {
            get { return goldBrush; }
        }

        public static SolidColorBrush BlackBrush
        {
            get { return blackBrush; }
        }

        public static SolidColorBrush GreyBrush
        {
            get { return greyBrush; }
        }

        public static SolidColorBrush WhiteBrush
        {
            get { return whiteBrush; }
        }

        public static SolidColorBrush RedBrush
        {
            get { return redBrush; }
        }

        public static SolidColorBrush SelectedBorderBrush
        {
            get { return selectedBorderBrush; }
        }

        public static SolidColorBrush TransparentBrush
        {
            get { return transparentBrush; }
        }

        public static Brush CanvasBackgroundBrush
        {
            get { return canvasBackgroundBrush; }
        }

        public static Brush ClassBody
        {
            get { return seaShellBrush; }
        }

        public static Brush ClassSelectedAttribute
        {
            get { return classSelectedAttribute; }
        }

        public static Brush ClassHeader { get { return classHeader; } }

        public static Brush NoInterpretationBrush { get { return noInterpretationBrush; } }
        public static Brush NoInterpretationBrushSelected { get { return noInterpretationBrushSelected; } }

        public static Brush StructuralRepresentativeBody { get { return structuralRepresentativeBody; } }
        
        public static Brush StructuralRepresentativeBodySelected { get { return structuralRepresentativeBodySelected; } }

        public static Brush StructuralRepresentativeBodyNoInterpretation { get { return structuralRepresentativeBodyNoInterpretation; } }

        public static Brush StructuralRepresentativeHeader { get { return structuralRepresentativeHeader; } }

        public static Brush StructuralRepresentativeHeaderNoInterpretation { get { return structuralRepresentativeHeaderNoInterpretation; } }

        public static Brush InterpretedAttributeBrush { get { return interpretedAttributeBrush; } }

        public static Brush RibbonBackstageDimText { get { return ribbonBackstageDimText; } }

        public static Brush NavyBrush { get { return navyBrush; } }

        public static Thickness Thicknness0 { get { return thicnkees0; } }
        
        public static Thickness Thicknness1 { get { return thicnkees1; } }

        public static Thickness Thicknness2 { get { return thicnkees2; } }

        public static Thickness Thicknness5 { get { return thicnkees5; } }

        static ViewToolkitResources()
        {
            nodeBorderBrush = new SolidColorBrush(Colors.Black);
            blackBrush = new SolidColorBrush(Colors.Black);
            greyBrush = new SolidColorBrush(Colors.LightGray);
            whiteBrush = new SolidColorBrush(Colors.White);
            redBrush = new SolidColorBrush(Colors.Red);
            transparentBrush = new SolidColorBrush(Colors.Transparent);
            classHeader = new SolidColorBrush(Color.FromArgb(255, 250, 235, 215));
            seaShellBrush = new SolidColorBrush(Color.FromArgb(255, 255, 245, 238));
            classSelectedAttribute = new SolidColorBrush(Color.FromArgb(255, 255, 204, 153));
            canvasBackgroundBrush = whiteBrush;
            noInterpretationBrush = new SolidColorBrush(Colors.LightGray);
            noInterpretationBrushSelected = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
            goldBrush = new SolidColorBrush(Colors.Yellow);
            navyBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x80));
            structuralRepresentativeHeader = new SolidColorBrush(Color.FromArgb(255, 153, 216, 232));
            structuralRepresentativeHeaderNoInterpretation = new SolidColorBrush(Color.FromArgb(255, 185, 216, 232));
            structuralRepresentativeBody = new SolidColorBrush(Color.FromArgb(255, 200, 233, 241));
            structuralRepresentativeBodySelected = new SolidColorBrush(Color.FromArgb(255, 210, 243, 251));
            structuralRepresentativeBodyNoInterpretation = new SolidColorBrush(Color.FromArgb(255, 220, 233, 241));
            interpretedAttributeBrush = seaShellBrush; 
            ribbonBackstageDimText = new SolidColorBrush(Color.FromArgb(0xFF, 0xa1, 0x6f, 0x89));
            selectedBorderBrush = redBrush;
            thicnkees0 = new Thickness(0);
            thicnkees1 = new Thickness(1);
            thicnkees2 = new Thickness(2);
            thicnkees5 = new Thickness(5);
#if SILVERLIGHT
#else
            interpretedAssociationPen = new Pen();
            junctionSelectedPen = new Pen(SelectedBorderBrush, 3);
            solidBlackPen = new Pen(blackBrush, 1);
            transparentPen = new Pen(Brushes.Transparent, 10);
            interpretedAssociationPen = new Pen(greyBrush, 1);
#endif
        }
    }
}