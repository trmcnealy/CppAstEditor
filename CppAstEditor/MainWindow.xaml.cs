using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
#if DEBUG
using System.Configuration;
using System.Diagnostics;
#endif
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

using CppAst;
using CppAst.CodeGen.Common;
using CppAst.CodeGen.CSharp;

using ICSharpCode.AvalonEdit.Document;

using Microsoft.Win32;

using Zio;
using Zio.FileSystems;

namespace CppAstEditor
{
    public partial class MainWindow : INotifyPropertyChanged, IDisposable
    {
        private CSharpConverterOptions _converterOptions;

        private TextDocument _cppText = new TextDocument();

        private TextDocument _cSharpText = new TextDocument();

        private CSharpConverterOptions ConverterOptions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _converterOptions; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set { SetProperty(ref _converterOptions, value); }
        }

        public TextDocument CppText
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _cppText; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set { SetProperty(ref _cppText, value); }
        }

        public TextDocument CSharpText
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _cSharpText; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set { SetProperty(ref _cSharpText, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

#if DEBUG
            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            Settings.Default.SettingsSaving += Default_SettingsSaving;
#endif

            string defaultFolder = System.Environment.GetEnvironmentVariable("USERPROFILE") ?? System.Environment.GetEnvironmentVariable("HOME");

            ConverterOptions = new CSharpConverterOptions
            {
                //C++
                TargetCpu           = CppTargetCpu.X86_64,
                TargetCpuSub        = string.Empty,
                TargetVendor        = "w64",
                TargetSystem        = "windows",
                TargetAbi           = "gnu",
                ParseAsCpp          = true,
                ParseComments       = false,
                ParseMacros         = true,
                AutoSquashTypedef   = true,
                ParseSystemIncludes = false,
                ParseAttributes     = true,
                //C#
                TypedefCodeGenKind               = CppTypedefCodeGenKind.NoWrap,
                DefaultOutputFilePath            = (UPath)Path.Combine(defaultFolder, "LibNative.generated.cs"),
                DefaultNamespace                 = "LibNative",
                DefaultClassLib                  = "libnative",
                DefaultDllImportNameAndArguments = "",
                AllowFixedSizeBuffers            = true,
                DefaultCharSet                   = CharSet.Ansi,
                DispatchOutputPerInclude         = false,
                DefaultMarshalForString          = new CSharpMarshalAttribute(CSharpUnmanagedKind.LPStr),
                DefaultMarshalForBool            = new CSharpMarshalAttribute(CSharpUnmanagedKind.Bool),
                GenerateAsInternal               = false,
                GenerateEnumItemAsFields         = false
            };

            Initialize();
        }

        public void Dispose()
        {
            //UpdateAndSaveSettings();
        }

        public void Initialize()
        {
            if(Settings.Default.Defines != null)
            {
                foreach(string define in Settings.Default.Defines)
                {
                    if(!ConverterOptions.Defines.Contains(define))
                    {
                        ConverterOptions.Defines.Add(define);
                    }
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
                    if(!ConverterOptions.AdditionalArguments.Contains(argument))
                    {
                        ConverterOptions.AdditionalArguments.Add(argument);
                    }
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
                    if(!ConverterOptions.IncludeFolders.Contains(includefolder))
                    {
                        ConverterOptions.IncludeFolders.Add(includefolder);
                    }
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
                    if(!ConverterOptions.SystemIncludeFolders.Contains(systeminclude))
                    {
                        ConverterOptions.SystemIncludeFolders.Add(systeminclude);
                    }
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

#if DEBUG
        private void Default_SettingsLoaded(object                  sender,
                                            SettingsLoadedEventArgs e)
        {
            Debug.WriteLine(e.Provider.ApplicationName + " settings have been loaded.");
        }

        private void Default_SettingsSaving(object          sender,
                                            CancelEventArgs e)
        {
            Debug.WriteLine("Saving app settings.");
        }
#endif

        public void UpdateAndSaveSettings()
        {
            try
            {
                Settings.Default.Defines.Clear();
                Settings.Default.Defines.AddRange(ConverterOptions.Defines.Distinct().ToArray());

                Settings.Default.AdditionalArguments.Clear();
                Settings.Default.AdditionalArguments.AddRange(ConverterOptions.AdditionalArguments.Distinct().ToArray());

                Settings.Default.IncludeFolders.Clear();
                Settings.Default.IncludeFolders.AddRange(ConverterOptions.IncludeFolders.Distinct().ToArray());

                Settings.Default.SystemIncludeFolders.Clear();
                Settings.Default.SystemIncludeFolders.AddRange(ConverterOptions.SystemIncludeFolders.Distinct().ToArray());

                Settings.Default.Save();
            }
            catch
            {
                Console.Error.WriteLine("Saving app settings failed.");
            }
        }

        private void ImportSettingsCommand_Executed(object          sender,
                                                    RoutedEventArgs e)
        {
            ImportSettings();
            Initialize();
        }

        public void ImportSettings()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName   = "UserSettings",
                DefaultExt = ".config",
                Filter     = "User settings|*.config;*.xml"
            };

            bool? result = dlg.ShowDialog();

            if(result != true)
            {
                return;
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DtoSettings));

            string filename = dlg.FileName;

            using StreamReader sr = new StreamReader(filename);

            DtoSettings dto = (DtoSettings)xmlSerializer.Deserialize(sr);

            Settings.Default.Defines              = dto.Defines;
            Settings.Default.AdditionalArguments  = dto.AdditionalArguments;
            Settings.Default.IncludeFolders       = dto.IncludeFolders;
            Settings.Default.SystemIncludeFolders = dto.SystemIncludeFolders;

            Settings.Default.Save();

            ParseAsCpp                       = dto.Options.ParseAsCpp;
            ParseComments                    = dto.Options.ParseComments;
            ParseMacros                      = dto.Options.ParseMacros;
            AutoSquashTypedef                = dto.Options.AutoSquashTypedef;
            ParseSystemIncludes              = dto.Options.ParseSystemIncludes;
            ParseAttributes                  = dto.Options.ParseAttributes;
            TargetCpu                        = dto.Options.TargetCpu;
            TargetCpuSub                     = dto.Options.TargetCpuSub;
            TargetVendor                     = dto.Options.TargetVendor;
            TargetSystem                     = dto.Options.TargetSystem;
            TargetAbi                        = dto.Options.TargetAbi;
            DefaultNamespace                 = dto.Options.DefaultNamespace;
            DefaultOutputFilePath            = dto.Options.DefaultOutputFilePath;
            DefaultClassLib                  = dto.Options.DefaultClassLib;
            GenerateAsInternal               = dto.Options.GenerateAsInternal;
            DefaultDllImportNameAndArguments = dto.Options.DefaultDllImportNameAndArguments;
            AllowFixedSizeBuffers            = dto.Options.AllowFixedSizeBuffers;
            DefaultCharSet                   = dto.Options.DefaultCharSet;
            DispatchOutputPerInclude         = dto.Options.DispatchOutputPerInclude;
            DefaultMarshalForString          = dto.Options.DefaultMarshalForString;
            DefaultMarshalForBool            = dto.Options.DefaultMarshalForBool;
            GenerateEnumItemAsFields         = dto.Options.GenerateEnumItemAsFields;
            TypedefCodeGenKind               = dto.Options.TypedefCodeGenKind;
        }

        private void ExportSettingsCommand_Executed(object          sender,
                                                    RoutedEventArgs e)
        {
            ExportSettings();
        }

        public void ExportSettings()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName   = "UserSettings",
                DefaultExt = ".config",
                Filter     = "User settings|*.config;*.xml"
            };

            bool? result = dlg.ShowDialog();

            if(result != true)
            {
                return;
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DtoSettings));

            string filename = dlg.FileName;

            using StreamWriter sw = new StreamWriter(filename);

            xmlSerializer.Serialize(sw, new DtoSettings(Settings.Default, _converterOptions));

            sw.Flush();
        }

        private static string ComplieCode(string                 text,
                                          CSharpConverterOptions options)
        {
            string result = string.Empty;

            try
            {
                CSharpCompilation csCompilation = CSharpConverter.Convert(text, options);

                if(!csCompilation.HasErrors)
                {
                    MemoryFileSystem fs = new MemoryFileSystem();

                    CodeWriterOptions cwo = new CodeWriterOptions(fs);

                    CodeWriter codeWriter = new CodeWriter(cwo);

                    csCompilation.DumpTo(codeWriter);

                    if(File.Exists(options.DefaultOutputFilePath.ToString()))
                    {
                        result = fs.ReadAllText(options.DefaultOutputFilePath.ToString());
                    }
                    else
                    {
                        result = Path.GetTempFileName();
                    }
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
                CSharpText.Text = ComplieCode(_cppText.Text, ConverterOptions);
            }
        }

        private void CppTextBox_TextChanged(object    sender,
                                            EventArgs e)
        {
            if(_cppText.TextLength > 0)
            {
                CSharpText.Text = ComplieCode(_cppText.Text, ConverterOptions);
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

        #region ConverterOptions

        public bool ParseAsCpp
        {
            get { return ConverterOptions.ParseAsCpp; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.ParseAsCpp, value))
                {
                    ConverterOptions.ParseAsCpp = value;

                    RaisePropertyChanged(nameof(ParseAsCpp));
                }
            }
        }

        public bool ParseComments
        {
            get { return ConverterOptions.ParseComments; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.ParseComments, value))
                {
                    ConverterOptions.ParseComments = value;

                    RaisePropertyChanged(nameof(ParseComments));
                }
            }
        }

        public bool ParseMacros
        {
            get { return ConverterOptions.ParseMacros; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.ParseMacros, value))
                {
                    ConverterOptions.ParseMacros = value;

                    RaisePropertyChanged(nameof(ParseMacros));
                }
            }
        }

        public bool AutoSquashTypedef
        {
            get { return ConverterOptions.AutoSquashTypedef; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.AutoSquashTypedef, value))
                {
                    ConverterOptions.AutoSquashTypedef = value;

                    RaisePropertyChanged(nameof(AutoSquashTypedef));
                }
            }
        }

        public bool ParseSystemIncludes
        {
            get { return ConverterOptions.ParseSystemIncludes; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.ParseSystemIncludes, value))
                {
                    ConverterOptions.ParseSystemIncludes = value;

                    RaisePropertyChanged(nameof(ParseSystemIncludes));
                }
            }
        }

        public bool ParseAttributes
        {
            get { return ConverterOptions.ParseAttributes; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.ParseAttributes, value))
                {
                    ConverterOptions.ParseAttributes = value;

                    RaisePropertyChanged(nameof(ParseAttributes));
                }
            }
        }

        public CppTargetCpu TargetCpu
        {
            get { return ConverterOptions.TargetCpu; }
            set
            {
                if(!EqualityComparer<CppTargetCpu>.Default.Equals(ConverterOptions.TargetCpu, value))
                {
                    ConverterOptions.TargetCpu = value;

                    RaisePropertyChanged(nameof(TargetCpu));
                }
            }
        }

        public string TargetCpuSub
        {
            get { return ConverterOptions.TargetCpuSub; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.TargetCpuSub, value))
                {
                    ConverterOptions.TargetCpuSub = value;

                    RaisePropertyChanged(nameof(TargetCpuSub));
                }
            }
        }

        public string TargetVendor
        {
            get { return ConverterOptions.TargetVendor; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.TargetVendor, value))
                {
                    ConverterOptions.TargetVendor = value;

                    RaisePropertyChanged(nameof(TargetVendor));
                }
            }
        }

        public string TargetSystem
        {
            get { return ConverterOptions.TargetSystem; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.TargetSystem, value))
                {
                    ConverterOptions.TargetSystem = value;

                    RaisePropertyChanged(nameof(TargetSystem));
                }
            }
        }

        public string TargetAbi
        {
            get { return ConverterOptions.TargetAbi; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.TargetAbi, value))
                {
                    ConverterOptions.TargetAbi = value;

                    RaisePropertyChanged(nameof(TargetAbi));
                }
            }
        }

        public string DefaultNamespace
        {
            get { return ConverterOptions.DefaultNamespace; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.DefaultNamespace, value))
                {
                    ConverterOptions.DefaultNamespace = value;

                    RaisePropertyChanged(nameof(DefaultNamespace));
                }
            }
        }

        public string DefaultOutputFilePath
        {
            get { return ConverterOptions.DefaultOutputFilePath.ToString(); }
            set
            {
                if(!EqualityComparer<UPath>.Default.Equals(ConverterOptions.DefaultOutputFilePath.ToString(), value))
                {
                    ConverterOptions.DefaultOutputFilePath = (UPath)value;

                    RaisePropertyChanged(nameof(DefaultOutputFilePath));
                }
            }
        }

        public string DefaultClassLib
        {
            get { return ConverterOptions.DefaultClassLib; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.DefaultClassLib, value))
                {
                    ConverterOptions.DefaultClassLib = value;

                    RaisePropertyChanged(nameof(DefaultClassLib));
                }
            }
        }

        public bool GenerateAsInternal
        {
            get { return ConverterOptions.GenerateAsInternal; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.GenerateAsInternal, value))
                {
                    ConverterOptions.GenerateAsInternal = value;

                    RaisePropertyChanged(nameof(GenerateAsInternal));
                }
            }
        }

        public string DefaultDllImportNameAndArguments
        {
            get { return ConverterOptions.DefaultDllImportNameAndArguments; }
            set
            {
                if(!EqualityComparer<string>.Default.Equals(ConverterOptions.DefaultDllImportNameAndArguments, value))
                {
                    ConverterOptions.DefaultDllImportNameAndArguments = value;

                    RaisePropertyChanged(nameof(DefaultDllImportNameAndArguments));
                }
            }
        }

        public bool AllowFixedSizeBuffers
        {
            get { return ConverterOptions.AllowFixedSizeBuffers; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.AllowFixedSizeBuffers, value))
                {
                    ConverterOptions.AllowFixedSizeBuffers = value;

                    RaisePropertyChanged(nameof(AllowFixedSizeBuffers));
                }
            }
        }

        public CharSet DefaultCharSet
        {
            get { return ConverterOptions.DefaultCharSet; }
            set
            {
                if(!EqualityComparer<CharSet>.Default.Equals(ConverterOptions.DefaultCharSet, value))
                {
                    ConverterOptions.DefaultCharSet = value;

                    RaisePropertyChanged(nameof(DefaultCharSet));
                }
            }
        }

        public bool DispatchOutputPerInclude
        {
            get { return ConverterOptions.DispatchOutputPerInclude; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.DispatchOutputPerInclude, value))
                {
                    ConverterOptions.DispatchOutputPerInclude = value;

                    RaisePropertyChanged(nameof(DispatchOutputPerInclude));
                }
            }
        }

        public CSharpUnmanagedKind DefaultMarshalForString
        {
            get { return ConverterOptions.DefaultMarshalForString.UnmanagedType; }
            set
            {
                if(!EqualityComparer<CSharpUnmanagedKind>.Default.Equals(ConverterOptions.DefaultMarshalForString.UnmanagedType, value))
                {
                    ConverterOptions.DefaultMarshalForString = new CSharpMarshalAttribute(value);

                    RaisePropertyChanged(nameof(DefaultMarshalForString));
                }
            }
        }

        public CSharpUnmanagedKind DefaultMarshalForBool
        {
            get { return ConverterOptions.DefaultMarshalForBool.UnmanagedType; }
            set
            {
                if(!EqualityComparer<CSharpUnmanagedKind>.Default.Equals(ConverterOptions.DefaultMarshalForBool.UnmanagedType, value))
                {
                    ConverterOptions.DefaultMarshalForBool = new CSharpMarshalAttribute(value);

                    RaisePropertyChanged(nameof(DefaultMarshalForBool));
                }
            }
        }

        public bool GenerateEnumItemAsFields
        {
            get { return ConverterOptions.GenerateEnumItemAsFields; }
            set
            {
                if(!EqualityComparer<bool>.Default.Equals(ConverterOptions.GenerateEnumItemAsFields, value))
                {
                    ConverterOptions.GenerateEnumItemAsFields = value;

                    RaisePropertyChanged(nameof(GenerateEnumItemAsFields));
                }
            }
        }

        public CppTypedefCodeGenKind TypedefCodeGenKind
        {
            get { return ConverterOptions.TypedefCodeGenKind; }
            set
            {
                if(!EqualityComparer<CppTypedefCodeGenKind>.Default.Equals(ConverterOptions.TypedefCodeGenKind, value))
                {
                    ConverterOptions.TypedefCodeGenKind = value;

                    RaisePropertyChanged(nameof(TypedefCodeGenKind));
                }
            }
        }

        #endregion

        #region Defines

        private BindableCollection<string> _defines;

        public BindableCollection<string> Defines
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _defines; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _defines, value, nameof(Defines)))
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
                if(SetProperty(ref _selectedDefinesIndex, value, nameof(SelectedDefinesIndex)))
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
                if(SetProperty(ref _additionalArguments, value, nameof(AdditionalArguments)))
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
                if(SetProperty(ref _selectedAdditionalArgumentsIndex, value, nameof(SelectedAdditionalArgumentsIndex)))
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
                if(SetProperty(ref _includeFolders, value, nameof(IncludeFolders)))
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
                if(SetProperty(ref _selectedIncludeFoldersIndex, value, nameof(SelectedIncludeFoldersIndex)))
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
                if(SetProperty(ref _systemIncludeFolders, value, nameof(SystemIncludeFolders)))
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
                if(SetProperty(ref _selectedSystemIncludeFoldersIndex, value, nameof(SelectedSystemIncludeFoldersIndex)))
                {
                }
            }
        }

        #endregion

        #region Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T                     storage,
                                              T                         value,
                                              [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(storage, value))
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
            if(EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;

            onChanged?.Invoke();

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

            propertyChanged?.Invoke(this, args);
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
                throw new ArgumentException("PropertySupport_NotMemberAccessExpression_Exception", nameof(expression));
            }

            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

            if(propertyInfo == null)
            {
                throw new ArgumentException("PropertySupport_ExpressionNotProperty_Exception", nameof(expression));
            }

            if(propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic)
            {
                throw new ArgumentException("PropertySupport_StaticExpression_Exception", nameof(expression));
            }

            return memberExpression.Member.Name;
        }

        #endregion
    }
}
