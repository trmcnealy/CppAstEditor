﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using CppAst;
using CppAst.CodeGen.Common;
using CppAst.CodeGen.CSharp;

using ICSharpCode.AvalonEdit.Document;

using Zio;
using Zio.FileSystems;

namespace CppAstEditor
{
    public partial class MainWindow : INotifyPropertyChanged, IDisposable
    {
        private CSharpConverterOptions _converterOptions;

        private CSharpConverterOptions ConverterOptions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _converterOptions; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                SetProperty(ref _converterOptions,
                            value);
            }
        }

        private TextDocument _cppText = new TextDocument();

        public TextDocument CppText
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _cppText; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                SetProperty(ref _cppText,
                            value);
            }
        }

        private TextDocument _cSharpText = new TextDocument();

        public TextDocument CSharpText
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _cSharpText; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                SetProperty(ref _cSharpText,
                            value);
            }
        }

        #region Defines

        private BindableCollection<string> _defines;

        public BindableCollection<string> Defines
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _defines; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _defines,
                               value,
                               nameof(Defines)))
                {
                    void CollectionChangedEventHandler(object                           sender,
                                                       NotifyCollectionChangedEventArgs e)
                    {
                        ConverterOptions.Defines.Clear();
                        ConverterOptions.Defines.AddRange(value);
                    }

                    Defines.CollectionChanged -= CollectionChangedEventHandler;
                    Defines.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedDefinesIndex;

        public int SelectedDefinesIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _selectedDefinesIndex; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _selectedDefinesIndex,
                               value,
                               nameof(SelectedDefinesIndex)))
                {
                }
            }
        }

        #endregion

        #region AdditionalArguments

        private BindableCollection<string> _additionalArguments;

        public BindableCollection<string> AdditionalArguments
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _additionalArguments; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _additionalArguments,
                               value,
                               nameof(AdditionalArguments)))
                {
                    void CollectionChangedEventHandler(object                           sender,
                                                       NotifyCollectionChangedEventArgs e)
                    {
                        ConverterOptions.AdditionalArguments.Clear();
                        ConverterOptions.AdditionalArguments.AddRange(value);
                    }

                    AdditionalArguments.CollectionChanged -= CollectionChangedEventHandler;
                    AdditionalArguments.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedAdditionalArgumentsIndex;

        public int SelectedAdditionalArgumentsIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _selectedAdditionalArgumentsIndex; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _selectedAdditionalArgumentsIndex,
                               value,
                               nameof(SelectedAdditionalArgumentsIndex)))
                {
                }
            }
        }

        #endregion

        #region IncludeFolders

        private BindableCollection<string> _includeFolders;

        public BindableCollection<string> IncludeFolders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _includeFolders; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _includeFolders,
                               value,
                               nameof(IncludeFolders)))
                {
                    void CollectionChangedEventHandler(object                           sender,
                                                       NotifyCollectionChangedEventArgs e)
                    {
                        ConverterOptions.IncludeFolders.Clear();
                        ConverterOptions.IncludeFolders.AddRange(value);
                    }

                    IncludeFolders.CollectionChanged -= CollectionChangedEventHandler;
                    IncludeFolders.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedIncludeFoldersIndex;

        public int SelectedIncludeFoldersIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _selectedIncludeFoldersIndex; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _selectedIncludeFoldersIndex,
                               value,
                               nameof(SelectedIncludeFoldersIndex)))
                {
                }
            }
        }

        #endregion

        #region SystemIncludeFolders

        private BindableCollection<string> _systemIncludeFolders;

        public BindableCollection<string> SystemIncludeFolders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _systemIncludeFolders; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _systemIncludeFolders,
                               value,
                               nameof(SystemIncludeFolders)))
                {
                    void CollectionChangedEventHandler(object                           sender,
                                                       NotifyCollectionChangedEventArgs e)
                    {
                        ConverterOptions.SystemIncludeFolders.Clear();
                        ConverterOptions.SystemIncludeFolders.AddRange(value);
                    }

                    SystemIncludeFolders.CollectionChanged -= CollectionChangedEventHandler;
                    SystemIncludeFolders.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedSystemIncludeFoldersIndex;

        public int SelectedSystemIncludeFoldersIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _selectedSystemIncludeFoldersIndex; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _selectedSystemIncludeFoldersIndex,
                               value,
                               nameof(SelectedSystemIncludeFoldersIndex)))
                {
                }
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

#if DEBUG
            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            Settings.Default.SettingsSaving += Default_SettingsSaving;
#endif

            ConverterOptions = new CSharpConverterOptions()
            {
                TypedefCodeGenKind = CppTypedefCodeGenKind.Wrap,
                TargetCpu          = CppTargetCpu.X86_64,
                TargetCpuSub       = string.Empty,
                TargetVendor       = "w64",
                TargetSystem       = "windows",
                TargetAbi          = "gnu"
            };

            if(Settings.Default.Defines != null)
            {
                foreach(string define in Settings.Default.Defines)
                {
                    ConverterOptions.Defines.Add(define);
                }
            }
            else
            {
                Settings.Default.Defines = new StringCollection();
            }

            if(Settings.Default.AdditionalArguments != null)
            {
                foreach(string argument in Settings.Default.AdditionalArguments)
                {
                    ConverterOptions.AdditionalArguments.Add(argument);
                }
            }
            else
            {
                Settings.Default.AdditionalArguments = new StringCollection();
            }

            if(Settings.Default.IncludeFolders != null)
            {
                foreach(string includefolder in Settings.Default.IncludeFolders)
                {
                    ConverterOptions.IncludeFolders.Add(includefolder);
                }
            }
            else
            {
                Settings.Default.IncludeFolders = new StringCollection();
            }

            if(Settings.Default.SystemIncludeFolders != null)
            {
                foreach(string systeminclude in Settings.Default.SystemIncludeFolders)
                {
                    ConverterOptions.SystemIncludeFolders.Add(systeminclude);
                }
            }
            else
            {
                Settings.Default.SystemIncludeFolders = new StringCollection();
            }

            Defines              = new BindableCollection<string>(ConverterOptions.Defines);
            IncludeFolders       = new BindableCollection<string>(ConverterOptions.IncludeFolders);
            AdditionalArguments  = new BindableCollection<string>(ConverterOptions.AdditionalArguments);
            SystemIncludeFolders = new BindableCollection<string>(ConverterOptions.SystemIncludeFolders);
        }

        private void Default_SettingsLoaded(object                                       sender,
                                            System.Configuration.SettingsLoadedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(e.Provider.ApplicationName + " settings have been loaded.");
#endif
        }

        private void Default_SettingsSaving(object          sender,
                                            CancelEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("Saving app settings.");
#endif
        }

        public void Dispose()
        {
            UpdateAndSaveSettings();
        }

        public void UpdateAndSaveSettings()
        {
            Settings.Default.Defines.Clear();
            Settings.Default.Defines.AddRange(ConverterOptions.Defines.ToArray());

            Settings.Default.AdditionalArguments.Clear();
            Settings.Default.AdditionalArguments.AddRange(ConverterOptions.AdditionalArguments.ToArray());

            Settings.Default.IncludeFolders.Clear();
            Settings.Default.IncludeFolders.AddRange(ConverterOptions.IncludeFolders.ToArray());

            Settings.Default.SystemIncludeFolders.Clear();
            Settings.Default.SystemIncludeFolders.AddRange(ConverterOptions.SystemIncludeFolders.ToArray());

            Settings.Default.Save();
        }

        private static string ComplieCode(string                 text,
                                          CSharpConverterOptions options)
        {
            string result = string.Empty;

            try
            {
                CSharpCompilation csCompilation = CSharpConverter.Convert(text,
                                                                          options);

                if(!csCompilation.HasErrors)
                {
                    MemoryFileSystem fs         = new MemoryFileSystem();
                    CodeWriter       codeWriter = new CodeWriter(new CodeWriterOptions(fs));
                    csCompilation.DumpTo(codeWriter);

                    result = fs.ReadAllText(options.DefaultOutputFilePath);
                }
                else
                {
                    result = csCompilation.Diagnostics.ToString();
                }
            }
            catch(Exception)
            {
                // ignored
            }

            return result;
        }

        private void CppTextBox_DocumentChanged(object    sender,
                                                EventArgs e)
        {
            if(_cppText.TextLength > 0)
            {
                CSharpText.Text = ComplieCode(_cppText.Text,
                                              ConverterOptions);
            }
        }

        private void CppTextBox_TextChanged(object    sender,
                                            EventArgs e)
        {
            if(_cppText.TextLength > 0)
            {
                CSharpText.Text = ComplieCode(_cppText.Text,
                                              ConverterOptions);
            }
        }

        private void CSharpText_DocumentChanged(object    sender,
                                                EventArgs e)
        {
        }

        private void DefinesTextBox_TextChanged(object               sender,
                                                TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && SelectedDefinesIndex >= 0 && SelectedDefinesIndex < Defines.Count)
            {
                _defines[SelectedDefinesIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void DefinesAddButton_OnClick(object          sender,
                                              RoutedEventArgs e)
        {
            Defines.Add(string.Empty);

            e.Handled = true;
        }

        private void DefinesRemoveButton_OnClick(object          sender,
                                                 RoutedEventArgs e)
        {
            if(SelectedDefinesIndex >= 0 && SelectedDefinesIndex < Defines.Count)
            {
                Defines.RemoveAt(SelectedDefinesIndex);
            }

            e.Handled = true;
        }

        private void AdditionalArgumentsTextBox_TextChanged(object               sender,
                                                            TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && SelectedAdditionalArgumentsIndex >= 0 && SelectedAdditionalArgumentsIndex < AdditionalArguments.Count)
            {
                _additionalArguments[SelectedAdditionalArgumentsIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void AdditionalArgumentsAddButton_OnClick(object          sender,
                                                          RoutedEventArgs e)
        {
            AdditionalArguments.Add(string.Empty);

            e.Handled = true;
        }

        private void AdditionalArgumentsRemoveButton_OnClick(object          sender,
                                                             RoutedEventArgs e)
        {
            if(SelectedAdditionalArgumentsIndex >= 0 && SelectedAdditionalArgumentsIndex < AdditionalArguments.Count)
            {
                AdditionalArguments.RemoveAt(SelectedAdditionalArgumentsIndex);
            }

            e.Handled = true;
        }

        private void IncludeFoldersTextBox_TextChanged(object               sender,
                                                       TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && SelectedAdditionalArgumentsIndex >= 0 && SelectedAdditionalArgumentsIndex < AdditionalArguments.Count)
            {
                _includeFolders[SelectedIncludeFoldersIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void IncludeFoldersAddButton_OnClick(object          sender,
                                                     RoutedEventArgs e)
        {
            IncludeFolders.Add(string.Empty);

            e.Handled = true;
        }

        private void IncludeFoldersRemoveButton_OnClick(object          sender,
                                                        RoutedEventArgs e)
        {
            if(SelectedIncludeFoldersIndex >= 0 && SelectedIncludeFoldersIndex < IncludeFolders.Count)
            {
                IncludeFolders.RemoveAt(SelectedIncludeFoldersIndex);
            }

            e.Handled = true;
        }

        private void SystemIncludeFoldersTextBox_TextChanged(object               sender,
                                                             TextChangedEventArgs e)
        {
            if(sender is EditableTextBlock textBox && SelectedIncludeFoldersIndex >= 0 && SelectedIncludeFoldersIndex < IncludeFolders.Count)
            {
                _systemIncludeFolders[SelectedSystemIncludeFoldersIndex] = textBox.Text;
            }

            e.Handled = true;
        }

        private void SystemIncludeFoldersAddButton_OnClick(object          sender,
                                                           RoutedEventArgs e)
        {
            SystemIncludeFolders.Add(string.Empty);

            e.Handled = true;
        }

        private void SystemIncludeFoldersRemoveButton_OnClick(object          sender,
                                                              RoutedEventArgs e)
        {
            if(SelectedSystemIncludeFoldersIndex >= 0 && SelectedSystemIncludeFoldersIndex < SystemIncludeFolders.Count)
            {
                SystemIncludeFolders.RemoveAt(SelectedSystemIncludeFoldersIndex);
            }

            e.Handled = true;
        }

        #region Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T                     storage,
                                              T                         value,
                                              [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(storage,
                                                  value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected virtual bool SetProperty<T>(ref T                     storage,
                                              T                         value,
                                              Action                    onChanged,
                                              [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(storage,
                                                  value))
            {
                return false;
            }

            storage = value;

            if(onChanged != null)
            {
                onChanged();
            }

            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;

            if(propertyChanged == null)
            {
                return;
            }

            propertyChanged(this,
                            args);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            string propertyName = ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }

        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if(propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            return ExtractPropertyNameFromLambda(propertyExpression);
        }

        internal static string ExtractPropertyNameFromLambda(LambdaExpression expression)
        {
            if(expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            MemberExpression memberExpression = expression.Body as MemberExpression;

            if(memberExpression == null)
            {
                throw new ArgumentException("PropertySupport_NotMemberAccessExpression_Exception",
                                            nameof(expression));
            }

            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

            if(propertyInfo == null)
            {
                throw new ArgumentException("PropertySupport_ExpressionNotProperty_Exception",
                                            nameof(expression));
            }

            if(propertyInfo.GetMethod.IsStatic)
            {
                throw new ArgumentException("PropertySupport_StaticExpression_Exception",
                                            nameof(expression));
            }

            return memberExpression.Member.Name;
        }

        #endregion
    }
}