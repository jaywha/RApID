﻿#pragma checksum "..\..\..\frmViewMultipleIssues.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "FB63B8866594387984FCD8D529ECD02C1AE2DE43"
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
    /// frmViewMultipleIssues
    /// </summary>
    public partial class frmViewMultipleIssues : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\frmViewMultipleIssues.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgvIssueList;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\frmViewMultipleIssues.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnEditSelectedItem;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\frmViewMultipleIssues.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRemoveSelectedItem;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\frmViewMultipleIssues.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnExit;
        
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
            System.Uri resourceLocater = new System.Uri("/RApID Project WPF;component/frmviewmultipleissues.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\frmViewMultipleIssues.xaml"
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
            
            #line 4 "..\..\..\frmViewMultipleIssues.xaml"
            ((RApID_Project_WPF.frmViewMultipleIssues)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.dgvIssueList = ((System.Windows.Controls.DataGrid)(target));
            
            #line 13 "..\..\..\frmViewMultipleIssues.xaml"
            this.dgvIssueList.BeginningEdit += new System.EventHandler<System.Windows.Controls.DataGridBeginningEditEventArgs>(this.dgvIssueList_BeginningEdit);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btnEditSelectedItem = ((System.Windows.Controls.Button)(target));
            
            #line 15 "..\..\..\frmViewMultipleIssues.xaml"
            this.btnEditSelectedItem.Click += new System.Windows.RoutedEventHandler(this.btnEditSelectedItem_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnRemoveSelectedItem = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\frmViewMultipleIssues.xaml"
            this.btnRemoveSelectedItem.Click += new System.Windows.RoutedEventHandler(this.btnRemoveSelectedItem_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnExit = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\frmViewMultipleIssues.xaml"
            this.btnExit.Click += new System.Windows.RoutedEventHandler(this.btnExit_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

