﻿#pragma checksum "D:\Programování\EvoX\Dialogs\InputBox_SL.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0BF6C7A056C3CEF7E1D75C6B663D10DA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SilverFlow.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace EvoX.Dialogs {
    
    
    public partial class InputBox : SilverFlow.Controls.FloatingWindow {
        
        internal System.Windows.Controls.TextBox textBox1;
        
        internal System.Windows.Controls.Button buttonOK;
        
        internal System.Windows.Controls.Button buttonCancel;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/EvoX.Dialogs;component/InputBox_SL.xaml", System.UriKind.Relative));
            this.textBox1 = ((System.Windows.Controls.TextBox)(this.FindName("textBox1")));
            this.buttonOK = ((System.Windows.Controls.Button)(this.FindName("buttonOK")));
            this.buttonCancel = ((System.Windows.Controls.Button)(this.FindName("buttonCancel")));
        }
    }
}

