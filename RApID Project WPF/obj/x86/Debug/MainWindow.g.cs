﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "E1D9855420F758EA758484985B8D5CCD06A0949C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace RApID_Project_WPF {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 75 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblTitle;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblTitleDef;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRework;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRepair;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnQCDQE;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnReportViewer;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSettings;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnTicketLookup;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/RApID Project WPF;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\MainWindow.xaml"
            ((RApID_Project_WPF.MainWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lblTitle = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.lblTitleDef = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.btnRework = ((System.Windows.Controls.Button)(target));
            
            #line 84 "..\..\..\MainWindow.xaml"
            this.btnRework.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnRepair = ((System.Windows.Controls.Button)(target));
            
            #line 85 "..\..\..\MainWindow.xaml"
            this.btnRepair.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnQCDQE = ((System.Windows.Controls.Button)(target));
            
            #line 86 "..\..\..\MainWindow.xaml"
            this.btnQCDQE.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnReportViewer = ((System.Windows.Controls.Button)(target));
            
            #line 87 "..\..\..\MainWindow.xaml"
            this.btnReportViewer.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            case 8:
            this.btnSettings = ((System.Windows.Controls.Button)(target));
            
            #line 88 "..\..\..\MainWindow.xaml"
            this.btnSettings.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            case 9:
            this.btnTicketLookup = ((System.Windows.Controls.Button)(target));
            
            #line 93 "..\..\..\MainWindow.xaml"
            this.btnTicketLookup.Click += new System.Windows.RoutedEventHandler(this.btnClicks);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

