﻿#pragma checksum "..\..\..\EditableTextBlock.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0588D8BD0F2C497F6CCAF86C43A7DC85344F67EC"
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
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace CppAstEditor {
    
    
    /// <summary>
    /// EditableTextBlock
    /// </summary>
    public partial class EditableTextBlock : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\..\EditableTextBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal CppAstEditor.EditableTextBlock ThisEditableTextBlock;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\EditableTextBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock DisplayBox;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\EditableTextBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EditBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CppAstEditor;component/editabletextblock.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\EditableTextBlock.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ThisEditableTextBlock = ((CppAstEditor.EditableTextBlock)(target));
            return;
            case 2:
            this.DisplayBox = ((System.Windows.Controls.TextBlock)(target));
            
            #line 13 "..\..\..\EditableTextBlock.xaml"
            this.DisplayBox.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DisplayBox_MouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.EditBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 29 "..\..\..\EditableTextBlock.xaml"
            this.EditBox.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.EditBox_PreviewKeyDown);
            
            #line default
            #line hidden
            
            #line 30 "..\..\..\EditableTextBlock.xaml"
            this.EditBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.EditBox_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

