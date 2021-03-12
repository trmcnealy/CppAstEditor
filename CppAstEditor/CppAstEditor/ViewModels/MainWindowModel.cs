using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

using CppAst;
using CppAst.CodeGen.Common;
using CppAst.CodeGen.CSharp;

using JitBuddy;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Win32;

using Prism.Commands;
using Prism.Mvvm;

using Zio;
using Zio.FileSystems;

using CSharpCompilation = CppAst.CodeGen.CSharp.CSharpCompilation;
using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;
using TypeInfo = System.Reflection.TypeInfo;

namespace CppAstEditor
{
    internal sealed class IndividualAssemblyLoadContext : AssemblyLoadContext
    {
        internal new static readonly AssemblyLoadContext Default = new IndividualAssemblyLoadContext();

        internal IndividualAssemblyLoadContext()
            : base("Dummy", true)
        {
        }
    }

    public class MainWindowModel : BindableBase
    {
        private static readonly Func<string, string> dummyClass = body =>
                                                                  {
                                                                      string html = "internal static class DummyClass {" + body + "}";

                                                                      return html;
                                                                  };

        private CSharpConverterOptions _converterOptions;

        private TextDocument _cppText = new TextDocument();

        private TextDocument _cSharpText = new TextDocument();

        private TextDocument _csJitText = new TextDocument();

        private TextDocument _asmJitText = new TextDocument();

        public CSharpConverterOptions ConverterOptions
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

        public TextDocument CsJitText
        {
            get { return _csJitText; }
            set { SetProperty(ref _csJitText, value); }
        }

        public TextDocument AsmJitText
        {
            get { return _asmJitText; }
            set { SetProperty(ref _asmJitText, value); }
        }

        public DelegateCommand                    ClosingCommand                          { get; set; }
        public DelegateCommand                    ConvertCodeCppToCsCommand               { get; set; }
        public DelegateCommand                    ConvertCodeCsToAsmCommand               { get; set; }
        public DelegateCommand                    ImportSettingsCommand                   { get; set; }
        public DelegateCommand                    ExportSettingsCommand                   { get; set; }
        public DelegateCommand                    DefinesAddButtonCommand                 { get; set; }
        public DelegateCommand                    DefinesRemoveButtonCommand              { get; set; }
        public DelegateCommand                    AdditionalArgumentsAddButtonCommand     { get; set; }
        public DelegateCommand                    AdditionalArgumentsRemoveButtonCommand  { get; set; }
        public DelegateCommand                    IncludeFoldersAddButtonCommand          { get; set; }
        public DelegateCommand                    IncludeFoldersRemoveButtonCommand       { get; set; }
        public DelegateCommand                    SystemIncludeFoldersAddButtonCommand    { get; set; }
        public DelegateCommand                    SystemIncludeFoldersRemoveButtonCommand { get; set; }
        public DelegateCommand<EditableTextBlock> DefinesTextBoxCommand                   { get; set; }
        public DelegateCommand<EditableTextBlock> AdditionalArgumentsTextBoxCommand       { get; set; }
        public DelegateCommand<EditableTextBlock> IncludeFoldersTextBoxCommand            { get; set; }
        public DelegateCommand<EditableTextBlock> SystemIncludeFoldersTextBoxCommand      { get; set; }

        public MainWindowModel()
        {
            //string defaultFolder = System.Environment.GetEnvironmentVariable("TEMP") ?? System.Environment.GetEnvironmentVariable("TMP");
            
            ClosingCommand                          = new DelegateCommand(OnClosing);
            ConvertCodeCppToCsCommand               = new DelegateCommand(OnConvertCodeCppToCs);
            ConvertCodeCsToAsmCommand               = new DelegateCommand(OnConvertCodeCsToAsm);
            ImportSettingsCommand                   = new DelegateCommand(OnImportSettings);
            ExportSettingsCommand                   = new DelegateCommand(OnExportSettings);
            DefinesAddButtonCommand                 = new DelegateCommand(OnDefinesAddButton);
            DefinesRemoveButtonCommand              = new DelegateCommand(OnDefinesRemoveButton);
            AdditionalArgumentsAddButtonCommand     = new DelegateCommand(OnAdditionalArgumentsAddButton);
            AdditionalArgumentsRemoveButtonCommand  = new DelegateCommand(OnAdditionalArgumentsRemoveButton);
            IncludeFoldersAddButtonCommand          = new DelegateCommand(OnIncludeFoldersAddButton);
            IncludeFoldersRemoveButtonCommand       = new DelegateCommand(OnIncludeFoldersRemoveButton);
            SystemIncludeFoldersAddButtonCommand    = new DelegateCommand(OnSystemIncludeFoldersAddButton);
            SystemIncludeFoldersRemoveButtonCommand = new DelegateCommand(OnSystemIncludeFoldersRemoveButton);
            DefinesTextBoxCommand                   = new DelegateCommand<EditableTextBlock>(OnDefinesTextBox);
            AdditionalArgumentsTextBoxCommand       = new DelegateCommand<EditableTextBlock>(OnAdditionalArgumentsTextBox);
            IncludeFoldersTextBoxCommand            = new DelegateCommand<EditableTextBlock>(OnIncludeFoldersTextBox);
            SystemIncludeFoldersTextBoxCommand      = new DelegateCommand<EditableTextBlock>(OnSystemIncludeFoldersTextBox);


            _converterOptions = new CSharpConverterOptions
            {
                //C++
                TargetCpu           = Settings.Default.TargetCpu,
                TargetCpuSub        = Settings.Default.TargetCpuSub,
                TargetVendor        = Settings.Default.TargetVendor,
                TargetSystem        = Settings.Default.TargetSystem,
                TargetAbi           = Settings.Default.TargetAbi,
                ParseAsCpp          = Settings.Default.ParseAsCpp,
                ParseComments       = Settings.Default.ParseComments,
                ParseMacros         = Settings.Default.ParseMacros,
                AutoSquashTypedef   = Settings.Default.AutoSquashTypedef,
                ParseSystemIncludes = Settings.Default.ParseSystemIncludes,
                ParseAttributes     = Settings.Default.ParseAttributes,
                //C#
                TypedefCodeGenKind               = Settings.Default.TypedefCodeGenKind,
                DefaultOutputFilePath            = (UPath)Settings.Default.DefaultOutputFilePath, //(UPath)Path.Combine(defaultFolder, "CppAstEditor.generated.cs"),
                DefaultNamespace                 = Settings.Default.DefaultNamespace,
                DefaultClassLib                  = Settings.Default.DefaultClassLib,
                DefaultDllImportNameAndArguments = Settings.Default.DefaultDllImportNameAndArguments,
                AllowFixedSizeBuffers            = Settings.Default.AllowFixedSizeBuffers,
                DefaultCharSet                   = Settings.Default.DefaultCharSet,
                DispatchOutputPerInclude         = Settings.Default.DispatchOutputPerInclude,
                DefaultMarshalForString          = new CSharpMarshalAttribute(Settings.Default.DefaultMarshalForString),
                DefaultMarshalForBool            = new CSharpMarshalAttribute(Settings.Default.DefaultMarshalForBool),
                GenerateAsInternal               = Settings.Default.GenerateAsInternal,
                GenerateEnumItemAsFields         = Settings.Default.GenerateEnumItemAsFields
            };

            Settings.Default.SettingsLoaded += Default_SettingsLoaded;
            Settings.Default.SettingsSaving += Default_SettingsSaving;

            if(Settings.Default.Defines != null)
            {
                foreach(string? define in Settings.Default.Defines)
                {
                    if(!ConverterOptions.Defines.Contains(define))
                    {
                        ConverterOptions.Defines.Add(define);
                    }
                }
            }
            else
            {
                Settings.Default.Defines = new();
            }

            if(Settings.Default.AdditionalArguments != null)
            {
                foreach(string? argument in Settings.Default.AdditionalArguments)
                {
                    if(!ConverterOptions.AdditionalArguments.Contains(argument))
                    {
                        ConverterOptions.AdditionalArguments.Add(argument);
                    }
                }
            }
            else
            {
                Settings.Default.AdditionalArguments = new();
            }

            if(Settings.Default.IncludeFolders != null)
            {
                foreach(string? includefolder in Settings.Default.IncludeFolders)
                {
                    if(!ConverterOptions.IncludeFolders.Contains(includefolder))
                    {
                        ConverterOptions.IncludeFolders.Add(includefolder);
                    }
                }
            }
            else
            {
                Settings.Default.IncludeFolders = new();
            }

            if(Settings.Default.SystemIncludeFolders != null)
            {
                foreach(string? systeminclude in Settings.Default.SystemIncludeFolders)
                {
                    if(!ConverterOptions.SystemIncludeFolders.Contains(systeminclude))
                    {
                        ConverterOptions.SystemIncludeFolders.Add(systeminclude);
                    }
                }
            }
            else
            {
                Settings.Default.SystemIncludeFolders = new();
            }

            Defines              = new BindableCollection<string>(ConverterOptions.Defines);
            IncludeFolders       = new BindableCollection<string>(ConverterOptions.IncludeFolders);
            AdditionalArguments  = new BindableCollection<string>(ConverterOptions.AdditionalArguments);
            SystemIncludeFolders = new BindableCollection<string>(ConverterOptions.SystemIncludeFolders);

            RaisePropertyChanged(nameof(Defines));
            RaisePropertyChanged(nameof(IncludeFolders));
            RaisePropertyChanged(nameof(AdditionalArguments));
            RaisePropertyChanged(nameof(SystemIncludeFolders));

            //string defaultFolder = System.Environment.GetEnvironmentVariable("TEMP") ?? System.Environment.GetEnvironmentVariable("TMP");


            //if (Settings.Default.Defines != null)
            //{
            foreach(string? define in Settings.Default.Defines)
            {
                if(!ConverterOptions.Defines.Contains(define))
                {
                    ConverterOptions.Defines.Add(define);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.Defines = new();
            //}

            //if (Settings.Default.AdditionalArguments != null)
            //{
            foreach(string? argument in Settings.Default.AdditionalArguments)
            {
                if(!ConverterOptions.AdditionalArguments.Contains(argument))
                {
                    ConverterOptions.AdditionalArguments.Add(argument);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.AdditionalArguments = new();
            //}

            //if (Settings.Default.IncludeFolders != null)
            //{
            foreach(string? includefolder in Settings.Default.IncludeFolders)
            {
                if(!ConverterOptions.IncludeFolders.Contains(includefolder))
                {
                    ConverterOptions.IncludeFolders.Add(includefolder);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.IncludeFolders = new();
            //}

            //if (Settings.Default.SystemIncludeFolders != null)
            //{
            foreach(string? systeminclude in Settings.Default.SystemIncludeFolders)
            {
                if(!ConverterOptions.SystemIncludeFolders.Contains(systeminclude))
                {
                    ConverterOptions.SystemIncludeFolders.Add(systeminclude);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.SystemIncludeFolders = new();
            //}

            Defines              = new BindableCollection<string>(ConverterOptions.Defines);
            IncludeFolders       = new BindableCollection<string>(ConverterOptions.IncludeFolders);
            AdditionalArguments  = new BindableCollection<string>(ConverterOptions.AdditionalArguments);
            SystemIncludeFolders = new BindableCollection<string>(ConverterOptions.SystemIncludeFolders);

            RaisePropertyChanged(nameof(Defines));
            RaisePropertyChanged(nameof(IncludeFolders));
            RaisePropertyChanged(nameof(AdditionalArguments));
            RaisePropertyChanged(nameof(SystemIncludeFolders));
        }

        public void OnClosing()
        {
            UpdateAndSaveSettings();
        }

        public void OnConvertCodeCppToCs()
        {
            if(CppText.TextLength > 0)
            {
                ConverterOptions.Defines.Clear();
                ConverterOptions.Defines.AddRange(Defines);
                ConverterOptions.AdditionalArguments.Clear();
                ConverterOptions.AdditionalArguments.AddRange(AdditionalArguments);
                ConverterOptions.IncludeFolders.Clear();
                ConverterOptions.IncludeFolders.AddRange(IncludeFolders);
                ConverterOptions.SystemIncludeFolders.Clear();
                ConverterOptions.SystemIncludeFolders.AddRange(SystemIncludeFolders);

                string newCode = ComplieCode(CppText.Text, ConverterOptions);
                
                CSharpText.Text = ConvertDllImports(newCode);
            }
        }
        
        private static string ConvertDllImports(string code)
        {
            //[DllImport(oiujl, CallingConvention = CallingConvention.Cdecl)]
            //public static extern IntPtr LoadLibraryExA(IntPtr lpLibFileName, IntPtr hFile, uint dwFlags);

            string[] lines = code.Split(new char[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);

            Parallel.For(0,
                         lines.Length,
                         (index) =>
                         //for (int index = 0; index < lines.Length; ++index)
                         {
                             if(lines[index].StartsWith("        [DllImport"))
                             {
                                 lines[index] = "        [SuppressGCTransition]";
                             }
                             else if(lines[index].StartsWith("        public static extern"))
                             {
                                 ReadOnlySpan<char> line = lines[index].AsSpan();

                                 int firstBracket = line.IndexOf('(');
                                 int lastBracket = line.IndexOf(')');

                                 ReadOnlySpan<char> lineHeader = line.Slice(29, firstBracket - 29);

                                 string[] header = lineHeader.ToString().Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

                                 ReadOnlySpan<char> lineTypes = line.Slice(firstBracket + 1, lastBracket - firstBracket - 1);

                                 string[] parts = lineTypes.ToString().Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

                                 string returnType = string.Join('\n', header[..^1]);

                                 if(returnType == "IntPtr")
                                 {
                                     returnType = "nint";
                                 }
                                 else if(returnType == "UIntPtr")
                                 {
                                     returnType = "nuint";
                                 }

                                 string name = header.Last();

                                 List<string> types = new(parts.Length);

                                 string[] typeParts;
                                 for (int i = 0; i < parts.Length; ++i)
                                 {
                                     typeParts = parts[i].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);

                                     if(typeParts.Length == 2)
                                     {
                                         if(typeParts[0] == "IntPtr")
                                         {
                                             types.Add("nint");
                                         }
                                         else if(typeParts[0] == "UIntPtr")
                                         {
                                             types.Add("nuint");
                                         }
                                         else
                                         {
                                             types.Add(typeParts[0]);
                                         }
                                     }
                                     else if(typeParts.Length == 3)
                                     {
                                         if(typeParts[1] == "IntPtr")
                                         {
                                             types.Add($"{typeParts[0]} nint");
                                         }
                                         else if(typeParts[1] == "UIntPtr")
                                         {
                                             types.Add($"{typeParts[0]} nuint");
                                         }
                                         else
                                         {
                                             types.Add($"{typeParts[0]} {typeParts[1]}");
                                         }
                                     }

                                 }

                                 StringBuilder signature = new();

                                 signature.Append("delegate* unmanaged<");

                                 for (int i = 0; i < types.Count; ++i)
                                 {
                                     if(i > 0)
                                     {
                                         signature.Append(",");
                                     }
                                     signature.Append(types[i]);
                                 }

                                 signature.Append(",");
                                 signature.Append(returnType);
                                 signature.Append(">");
                                 
                                 lines[index] = $"        public static unsafe {signature} {name} = ({signature})NativeLibrary.GetExport(Handle, \"{name}\");";
                             }
                         }
                         );
                        
            return string.Join('\n',lines);
        }

        public void OnConvertCodeCsToAsm()
        {
            if(CsJitText.TextLength > 0)
            {
                //AsmJitTextBox.Text = MainWindowModel.ComplieCs(CsJitTextBox.Text);
            }
        }

        public void Dispose()
        {
            CppText.Text = "";
        }

        public void Initialize()
        {
            //if (Settings.Default.Defines != null)
            //{
            foreach(string? define in Settings.Default.Defines)
            {
                if(!ConverterOptions.Defines.Contains(define))
                {
                    ConverterOptions.Defines.Add(define);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.Defines = new ();
            //}

            //if (Settings.Default.AdditionalArguments != null)
            //{
            foreach(string? argument in Settings.Default.AdditionalArguments)
            {
                if(!ConverterOptions.AdditionalArguments.Contains(argument))
                {
                    ConverterOptions.AdditionalArguments.Add(argument);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.AdditionalArguments = new ();
            //}

            //if (Settings.Default.IncludeFolders != null)
            //{
            foreach(string? includefolder in Settings.Default.IncludeFolders)
            {
                if(!ConverterOptions.IncludeFolders.Contains(includefolder))
                {
                    ConverterOptions.IncludeFolders.Add(includefolder);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.IncludeFolders = new ();
            //}

            //if (Settings.Default.SystemIncludeFolders != null)
            //{
            foreach(string? systeminclude in Settings.Default.SystemIncludeFolders)
            {
                if(!ConverterOptions.SystemIncludeFolders.Contains(systeminclude))
                {
                    ConverterOptions.SystemIncludeFolders.Add(systeminclude);
                }
            }
            //}
            //else
            //{
            //    Settings.Default.SystemIncludeFolders = new ();
            //}

            Defines.Clear();
            Defines.AddRange(ConverterOptions.Defines);
            IncludeFolders.Clear();
            IncludeFolders.AddRange(ConverterOptions.IncludeFolders);
            AdditionalArguments.Clear();
            AdditionalArguments.AddRange(ConverterOptions.AdditionalArguments);
            SystemIncludeFolders.Clear();
            SystemIncludeFolders.AddRange(ConverterOptions.SystemIncludeFolders);
        }

        public void UpdateAndSaveSettings()
        {
            try
            {
                Settings.Default.Defines.Clear();
                Settings.Default.Defines.AddRange(Defines.Distinct().ToArray());

                Settings.Default.AdditionalArguments.Clear();
                Settings.Default.AdditionalArguments.AddRange(AdditionalArguments.Distinct().ToArray());

                Settings.Default.IncludeFolders.Clear();
                Settings.Default.IncludeFolders.AddRange(IncludeFolders.Distinct().ToArray());

                Settings.Default.SystemIncludeFolders.Clear();
                Settings.Default.SystemIncludeFolders.AddRange(SystemIncludeFolders.Distinct().ToArray());


                Settings.Default.TargetCpu           = _converterOptions.TargetCpu;
                Settings.Default.TargetCpuSub        = _converterOptions.TargetCpuSub;
                Settings.Default.TargetVendor        = _converterOptions.TargetVendor;
                Settings.Default.TargetSystem        = _converterOptions.TargetSystem;
                Settings.Default.TargetAbi           = _converterOptions.TargetAbi;
                Settings.Default.ParseAsCpp          = _converterOptions.ParseAsCpp;
                Settings.Default.ParseComments       = _converterOptions.ParseComments;
                Settings.Default.ParseMacros         = _converterOptions.ParseMacros;
                Settings.Default.AutoSquashTypedef   = _converterOptions.AutoSquashTypedef;
                Settings.Default.ParseSystemIncludes = _converterOptions.ParseSystemIncludes;
                Settings.Default.ParseAttributes     = _converterOptions.ParseAttributes;
                
                Settings.Default.TypedefCodeGenKind               = _converterOptions.TypedefCodeGenKind;
                Settings.Default.DefaultOutputFilePath            = _converterOptions.DefaultOutputFilePath.ToString();
                Settings.Default.DefaultNamespace                 = _converterOptions.DefaultNamespace;
                Settings.Default.DefaultClassLib                  = _converterOptions.DefaultClassLib;
                Settings.Default.DefaultDllImportNameAndArguments = _converterOptions.DefaultDllImportNameAndArguments;
                Settings.Default.AllowFixedSizeBuffers            = _converterOptions.AllowFixedSizeBuffers;
                Settings.Default.DefaultCharSet                   = _converterOptions.DefaultCharSet;
                Settings.Default.DispatchOutputPerInclude         = _converterOptions.DispatchOutputPerInclude;
                Settings.Default.DefaultMarshalForString          = _converterOptions.DefaultMarshalForString.UnmanagedType;
                Settings.Default.DefaultMarshalForBool            = _converterOptions.DefaultMarshalForBool.UnmanagedType;
                Settings.Default.GenerateAsInternal               = _converterOptions.GenerateAsInternal;
                Settings.Default.GenerateEnumItemAsFields         = _converterOptions.GenerateEnumItemAsFields;

                Settings.Default.Save();
            }
            catch
            {
                Console.Error.WriteLine("Saving app settings failed.");
            }
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

            string filename = dlg.FileName;

            using StreamReader sr = new StreamReader(filename);

            //DtoSettings? dto = null;

            //try
            //{
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DtoSettings));

            DtoSettings? dto = (DtoSettings?)xmlSerializer.Deserialize(sr);
            //}
            //catch (FileNotFoundException)
            //{
            //}

            if(dto is null)
            {
                return;
            }

            Defines              = new BindableCollection<string>(dto.Defines);
            AdditionalArguments  = new BindableCollection<string>(dto.AdditionalArguments);
            IncludeFolders       = new BindableCollection<string>(dto.IncludeFolders);
            SystemIncludeFolders = new BindableCollection<string>(dto.SystemIncludeFolders);

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

            //Settings.Default.Defines.Clear();
            //Settings.Default.AdditionalArguments.Clear();
            //Settings.Default.IncludeFolders.Clear();
            //Settings.Default.SystemIncludeFolders.Clear();

            //Settings.Default.Defines.AddRange(Defines.ToArray());
            //Settings.Default.AdditionalArguments.AddRange(AdditionalArguments.ToArray());
            //Settings.Default.IncludeFolders.AddRange(IncludeFolders.ToArray());
            //Settings.Default.SystemIncludeFolders.AddRange(SystemIncludeFolders.ToArray());

            //Settings.Default.Save();
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

            string filename = dlg.FileName;

            using StreamWriter sw = new StreamWriter(filename);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DtoSettings));

            try
            {
                xmlSerializer.Serialize(sw, new DtoSettings(Defines.ToList(), AdditionalArguments.ToList(), IncludeFolders.ToList(), SystemIncludeFolders.ToList(), _converterOptions));

                sw.Flush();
            }
            catch(FileNotFoundException)
            {
            }
        }

        private void Default_SettingsLoaded(object?                 sender,
                                            SettingsLoadedEventArgs e)
        {
            Console.WriteLine(e.Provider.ApplicationName + " settings have been loaded.");
        }

        private void Default_SettingsSaving(object?         sender,
                                            CancelEventArgs e)
        {
            Console.WriteLine("Saving app settings.");
        }

        public static string ComplieCode(string                 text,
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

                    result = fs.ReadAllText(options.DefaultOutputFilePath.ToString());
                }
                else
                {
                    result = csCompilation.Diagnostics.ToString();
                }
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public static string ComplieCs(string code)
        {
            //Compilation compilation = sourceLanguage.CreateLibraryCompilation(assemblyName: "InMemoryAssembly", enableOptimisations: false).AddReferences(_references).AddSyntaxTrees(syntaxTree);

            string assemblyName = "DummyAssembly.dll";
            //string moduleName = "DummyAssembly";
            string mainTypeName = "DummyClass";
            //string scriptClassName = "DummyClass";

            IReadOnlyCollection<MetadataReference> _references = new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location), MetadataReference.CreateFromFile(typeof(ValueTuple<>).GetTypeInfo().Assembly.Location)
            };

            //var options = new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(   OutputKind.DynamicallyLinkedLibrary, OptimizationLevel.Release,          true);

            CSharpLanguage sourceLanguage = new CSharpLanguage();

            SyntaxTree syntaxTree = sourceLanguage.ParseText(dummyClass(code), SourceCodeKind.Regular);

            Compilation compilation = sourceLanguage.CreateLibraryCompilation(assemblyName).AddReferences(_references).AddSyntaxTrees(syntaxTree);

            MemoryStream stream     = new MemoryStream();
            EmitResult   emitResult = compilation.Emit(stream);
            string       result     = string.Empty;

            if(!emitResult.Success)
            {
                StringBuilder sb = new StringBuilder();

                foreach(Diagnostic diagnostic in emitResult.Diagnostics)
                {
                    sb.AppendLine(diagnostic.GetMessage());
                }

                result = sb.ToString();
            }
            else
            {
                stream.Position = 0; //.Seek(0, SeekOrigin.Begin);

                Assembly dummyAssembly = new IndividualAssemblyLoadContext().LoadFromStream(stream);

                TypeInfo? types = dummyAssembly.DefinedTypes.FirstOrDefault(type => type.Name.Contains(mainTypeName));

                if(types != null)
                {
                    MethodInfo[] methodInfos = types.GetMethods();

                    StringBuilder sb = new StringBuilder();

                    foreach(MethodInfo methodInfo in methodInfos)
                    {
                        if((methodInfo.Name == "GetType" || methodInfo.Name == "GetHashCode" || methodInfo.Name == "Equals" || methodInfo.Name == "ToString") && methodInfo.IsHideBySig)
                        {
                            continue;
                        }

                        sb.AppendLine($"//{methodInfo.Name}");
                        sb.AppendLine(methodInfo.ToAsm());
                    }

                    result = sb.ToString();
                }

                //IndividualAssemblyLoadContext.Default.Unload();
            }

            return result;
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

        private BindableCollection<string> _defines = new();

        public BindableCollection<string> Defines
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _defines; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _defines, value, nameof(Defines)))
                {
                    //void CollectionChangedEventHandler(object?                          sender,
                    //                                   NotifyCollectionChangedEventArgs e)
                    //{
                    //    switch (e.Action)
                    //    {
                    //        case NotifyCollectionChangedAction.Add:
                    //        {
                    //            if (e.NewItems != null)
                    //            {
                    //                for (int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.Defines.Add(e.NewItems[i] as string);
                    //                }
                    //            }

                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Remove:
                    //        {
                    //            if (e.OldItems != null)
                    //            {
                    //                for (int i = 0; i < e.OldItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.Defines.Remove(e.OldItems[i] as string);
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Replace:
                    //        {
                    //            if (e.NewItems != null)
                    //            {
                    //                for (int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.Defines[e.NewStartingIndex + i] = e.NewItems[i] as string;
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Reset:
                    //        {
                    //            ConverterOptions.Defines.Clear();

                    //            break;
                    //        }
                    //        default: throw new ArgumentOutOfRangeException();
                    //    }

                    //}

                    //Defines.CollectionChanged -= CollectionChangedEventHandler;
                    //Defines.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedDefinesIndex = -1;

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

        private BindableCollection<string> _additionalArguments = new();

        public BindableCollection<string> AdditionalArguments
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _additionalArguments; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _additionalArguments, value, nameof(AdditionalArguments)))
                {
                    //void CollectionChangedEventHandler(object?                          sender,
                    //                                   NotifyCollectionChangedEventArgs e)
                    //{
                    //    switch(e.Action)
                    //    {
                    //        case NotifyCollectionChangedAction.Add:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.AdditionalArguments.Add(e.NewItems[i] as string);
                    //                }
                    //            }

                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Remove:
                    //        {
                    //            if(e.OldItems != null)
                    //            {
                    //                for(int i = 0; i < e.OldItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.AdditionalArguments.Remove(e.OldItems[i] as string);
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Replace:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.AdditionalArguments[e.NewStartingIndex + i] = e.NewItems[i] as string;
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Reset:
                    //        {
                    //            ConverterOptions.AdditionalArguments.Clear();

                    //            break;
                    //        }
                    //        default: throw new ArgumentOutOfRangeException();
                    //    }
                    //}

                    //AdditionalArguments.CollectionChanged -= CollectionChangedEventHandler;
                    //AdditionalArguments.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedAdditionalArgumentsIndex = -1;

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

        private BindableCollection<string> _includeFolders = new();

        public BindableCollection<string> IncludeFolders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _includeFolders; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _includeFolders, value, nameof(IncludeFolders)))
                {
                    //void CollectionChangedEventHandler(object?                          sender,
                    //                                   NotifyCollectionChangedEventArgs e)
                    //{
                    //    switch(e.Action)
                    //    {
                    //        case NotifyCollectionChangedAction.Add:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    if(e.NewItems[i] is string new_string && !string.IsNullOrEmpty(new_string))
                    //                    {
                    //                        ConverterOptions.IncludeFolders.Add(new_string);
                    //                    }
                    //                }
                    //            }

                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Remove:
                    //        {
                    //            if(e.OldItems != null)
                    //            {
                    //                for(int i = 0; i < e.OldItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.IncludeFolders.Remove(e.OldItems[i] as string);
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Replace:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.IncludeFolders[e.NewStartingIndex + i] = e.NewItems[i] as string;
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Reset:
                    //        {
                    //            ConverterOptions.IncludeFolders.Clear();

                    //            break;
                    //        }
                    //        default: throw new ArgumentOutOfRangeException();
                    //    }
                    //}

                    //IncludeFolders.CollectionChanged -= CollectionChangedEventHandler;
                    //IncludeFolders.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedIncludeFoldersIndex = -1;

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

        private BindableCollection<string> _systemIncludeFolders = new();

        public BindableCollection<string> SystemIncludeFolders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get { return _systemIncludeFolders; }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if(SetProperty(ref _systemIncludeFolders, value, nameof(SystemIncludeFolders)))
                {
                    //void CollectionChangedEventHandler(object?                          sender,
                    //                                   NotifyCollectionChangedEventArgs e)
                    //{
                    //    switch(e.Action)
                    //    {
                    //        case NotifyCollectionChangedAction.Add:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.SystemIncludeFolders.Add(e.NewItems[i] as string);
                    //                }
                    //            }

                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Remove:
                    //        {
                    //            if(e.OldItems != null)
                    //            {
                    //                for(int i = 0; i < e.OldItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.SystemIncludeFolders.Remove(e.OldItems[i] as string);
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Replace:
                    //        {
                    //            if(e.NewItems != null)
                    //            {
                    //                for(int i = 0; i < e.NewItems.Count; ++i)
                    //                {
                    //                    ConverterOptions.SystemIncludeFolders[e.NewStartingIndex + i] = e.NewItems[i] as string;
                    //                }
                    //            }
                    //            break;
                    //        }
                    //        case NotifyCollectionChangedAction.Reset:
                    //        {
                    //            ConverterOptions.SystemIncludeFolders.Clear();

                    //            break;
                    //        }
                    //        default: throw new ArgumentOutOfRangeException();
                    //    }
                    //}

                    //SystemIncludeFolders.CollectionChanged -= CollectionChangedEventHandler;
                    //SystemIncludeFolders.CollectionChanged += CollectionChangedEventHandler;
                }
            }
        }

        private int _selectedSystemIncludeFoldersIndex = -1;

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

        #region Commands

        private void OnDefinesAddButton()
        {
            Defines.Add(string.Empty);
        }

        private void OnDefinesRemoveButton()
        {
            if(SelectedDefinesIndex >= 0 && SelectedDefinesIndex < Defines.Count)
            {
                Defines.RemoveAt(SelectedDefinesIndex);
            }
        }

        private void OnImportSettings()
        {
            ImportSettings();
            //Initialize();
        }

        private void OnExportSettings()
        {
            ExportSettings();
        }

        private void OnAdditionalArgumentsAddButton()
        {
            AdditionalArguments.Add(string.Empty);
        }

        private void OnAdditionalArgumentsRemoveButton()
        {
            if(SelectedAdditionalArgumentsIndex >= 0 && SelectedAdditionalArgumentsIndex < AdditionalArguments.Count)
            {
                AdditionalArguments.RemoveAt(SelectedAdditionalArgumentsIndex);
            }
        }

        private void OnIncludeFoldersAddButton()
        {
            IncludeFolders.Add(string.Empty);
        }

        private void OnIncludeFoldersRemoveButton()
        {
            if(SelectedIncludeFoldersIndex >= 0 && SelectedIncludeFoldersIndex < IncludeFolders.Count)
            {
                IncludeFolders.RemoveAt(SelectedIncludeFoldersIndex);
            }
        }
        
        private void OnDefinesTextBox(EditableTextBlock textBox)
        {
            if(SelectedDefinesIndex >= 0 && SelectedDefinesIndex < Defines.Count)
            {
                Defines[SelectedDefinesIndex] = textBox.Text;
            }
        }

        private void OnAdditionalArgumentsTextBox(EditableTextBlock textBox)
        {
            if(SelectedAdditionalArgumentsIndex >= 0 &&
               SelectedAdditionalArgumentsIndex < AdditionalArguments.Count)
            {
                AdditionalArguments[SelectedAdditionalArgumentsIndex] = textBox.Text;
            }
        }

        private void OnIncludeFoldersTextBox(EditableTextBlock textBox)
        {
            if(SelectedIncludeFoldersIndex >= 0 && SelectedIncludeFoldersIndex < IncludeFolders.Count)
            {
                IncludeFolders[SelectedIncludeFoldersIndex] = textBox.Text;
            }
        }

        private void OnSystemIncludeFoldersTextBox(EditableTextBlock textBox)
        {
            if(SelectedSystemIncludeFoldersIndex >= 0 &&
               SelectedSystemIncludeFoldersIndex < SystemIncludeFolders.Count)
            {
                SystemIncludeFolders[SelectedSystemIncludeFoldersIndex] = textBox.Text;
            }
        }

        private void OnSystemIncludeFoldersAddButton()
        {
            SystemIncludeFolders.Add(string.Empty);
        }

        private void OnSystemIncludeFoldersRemoveButton()
        {
            if(SelectedSystemIncludeFoldersIndex >= 0 && SelectedSystemIncludeFoldersIndex < SystemIncludeFolders.Count)
            {
                SystemIncludeFolders.RemoveAt(SelectedSystemIncludeFoldersIndex);
            }
        }

        #endregion
    }
}
