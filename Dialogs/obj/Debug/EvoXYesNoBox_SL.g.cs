﻿#pragma checksum "D:\Programování\EvoX\Dialogs\EvoXYesNoBox_SL.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "606C6CF232A551BF3BA4B29D948AF755"
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
    
    
    public partial class EvoXYesNoBox : SilverFlow.Controls.FloatingWindow {
        
        internal System.Windows.Controls.StackPanel stackPanel1;
        
        internal System.Windows.Controls.Image image1;
        
        internal System.Windows.Controls.TextBlock messageText;
        
        internal System.Windows.Controls.TextBlock messageQuestion;
        
        internal System.Windows.Controls.StackPanel canvas1;
        
        internal System.Windows.Controls.Button bYes;
        
        internal System.Windows.Controls.Button bNo;
        
        internal System.Windows.Controls.Button bOk;
        
        internal System.Windows.Controls.Button bStorno;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/EvoX.Dialogs;component/EvoXYesNoBox_SL.xaml", System.UriKind.Relative));
            this.stackPanel1 = ((System.Windows.Controls.StackPanel)(this.FindName("stackPanel1")));
            this.image1 = ((System.Windows.Controls.Image)(this.FindName("image1")));
            this.messageText = ((System.Windows.Controls.TextBlock)(this.FindName("messageText")));
            this.messageQuestion = ((System.Windows.Controls.TextBlock)(this.FindName("messageQuestion")));
            this.canvas1 = ((System.Windows.Controls.StackPanel)(this.FindName("canvas1")));
            this.bYes = ((System.Windows.Controls.Button)(this.FindName("bYes")));
            this.bNo = ((System.Windows.Controls.Button)(this.FindName("bNo")));
            this.bOk = ((System.Windows.Controls.Button)(this.FindName("bOk")));
            this.bStorno = ((System.Windows.Controls.Button)(this.FindName("bStorno")));
        }
    }
}

