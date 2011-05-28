using System;
using System.Windows.Input;
using Exolutio.View.Commands;

namespace Exolutio.View
{
    public static class KeyGestures
    {
        static readonly KeyGesture controlN = new KeyGesture(Key.N, ModifierKeys.Control);

        public static KeyGesture ControlN
        {
            get
            {
                return controlN;
            }
        }

        private static readonly KeyGesture controlO = new KeyGesture(Key.O, ModifierKeys.Control);

        public static KeyGesture ControlO
        {
            get { return controlO; }
        }

        private static readonly KeyGesture controlS = new KeyGesture(Key.S, ModifierKeys.Control);

        public static KeyGesture ControlS
        {
            get { return controlS; }
        }

        private static readonly KeyGesture controlShiftS = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

        public static KeyGesture ControlShiftS
        {
            get { return controlShiftS; }
        }

        private static readonly KeyGesture controlX = new KeyGesture(Key.X, ModifierKeys.Control);

        public static KeyGesture ControlX
        {
            get { return controlX; }
        }

        private static readonly KeyGesture controlShiftX = new KeyGesture(Key.X, ModifierKeys.Control | ModifierKeys.Shift);

        public static KeyGesture ControlShiftX
        {
            get { return controlShiftX; }
        }

        private static readonly KeyGesture controlF4 = new KeyGesture(Key.F4, ModifierKeys.Control);
        public static KeyGesture ControlF4
        {
            get { return controlF4; }
        }
    }
}