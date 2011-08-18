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

        private static readonly KeyGesture controlY = new KeyGesture(Key.Y, ModifierKeys.Control);

        public static KeyGesture ControlY
        {
            get { return controlY; }
        }
        private static readonly KeyGesture controlZ = new KeyGesture(Key.Z, ModifierKeys.Control);

        public static KeyGesture ControlZ
        {
            get { return controlZ; }
        }

        private static readonly KeyGesture backspace = new KeyGesture(Key.Back);

        public static KeyGesture Backspace
        {
            get { return backspace; }
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

        private static readonly KeyGesture f2 = new KeyGesture(Key.F2);
        public static KeyGesture F2
        {
            get { return f2; }
        }

        private static readonly KeyGesture delete = new KeyGesture(Key.Delete);
        public static KeyGesture Delete
        {
            get { return delete; }
        }

        private static readonly KeyGesture shiftdelete = new KeyGesture(Key.Delete, ModifierKeys.Shift);
        public static KeyGesture ShiftDelete
        {
            get { return shiftdelete; }
        }
        private static readonly KeyGesture controldelete = new KeyGesture(Key.Delete, ModifierKeys.Control);
        public static KeyGesture ControlDelete
        {
            get { return controldelete; }
        }

        private static readonly KeyGesture tab = new KeyGesture(Key.Tab);
        public static KeyGesture Tab
        {
            get { return tab; }
        }

        private static readonly KeyGesture shiftTab = new KeyGesture(Key.Tab, ModifierKeys.Shift);
        public static KeyGesture ShiftTab
        {
            get { return shiftTab; }
        }
    }
}