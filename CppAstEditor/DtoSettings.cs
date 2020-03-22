using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

using CppAst;
using CppAst.CodeGen.CSharp;

namespace CppAstEditor
{
    [Serializable]
    public sealed class DtoOptions
    {
        public bool ParseAsCpp { get; set; }

        public bool ParseComments { get; set; }

        public bool ParseMacros { get; set; }

        public bool AutoSquashTypedef { get; set; }

        public bool ParseSystemIncludes { get; set; }

        public bool ParseAttributes { get; set; }

        public CppTargetCpu TargetCpu { get; set; }

        public string TargetCpuSub { get; set; }

        public string TargetVendor { get; set; }

        public string TargetSystem { get; set; }

        public string TargetAbi { get; set; }

        public string DefaultNamespace { get; set; }

        public string DefaultOutputFilePath { get; set; }

        public string DefaultClassLib { get; set; }

        public bool GenerateAsInternal { get; set; }

        public string DefaultDllImportNameAndArguments { get; set; }

        public bool AllowFixedSizeBuffers { get; set; }

        public CharSet DefaultCharSet { get; set; }

        public bool DispatchOutputPerInclude { get; set; }

        public CSharpUnmanagedKind DefaultMarshalForString { get; set; }

        public CSharpUnmanagedKind DefaultMarshalForBool { get; set; }

        public bool GenerateEnumItemAsFields { get; set; }

        public CppTypedefCodeGenKind TypedefCodeGenKind { get; set; }

        public DtoOptions()
        {
        }

        public DtoOptions(bool                  asCpp,
                          bool                  comments,
                          bool                  macros,
                          bool                  autoSquashTypedef,
                          bool                  systemIncludes,
                          bool                  attributes,
                          CppTargetCpu          targetCpu,
                          string                targetCpuSub,
                          string                targetVendor,
                          string                targetSystem,
                          string                targetAbi,
                          string                defaultNamespace,
                          string                defaultOutputFilePath,
                          string                defaultClassLib,
                          bool                  generateAsInternal,
                          string                defaultDllImportNameAndArguments,
                          bool                  allowFixedSizeBuffers,
                          CharSet               defaultCharSet,
                          bool                  dispatchOutputPerInclude,
                          CSharpUnmanagedKind   defaultMarshalForString,
                          CSharpUnmanagedKind   defaultMarshalForBool,
                          bool                  generateEnumItemAsFields,
                          CppTypedefCodeGenKind typedefCodeGenKind)
        {
            ParseAsCpp                       = asCpp;
            ParseComments                    = comments;
            ParseMacros                      = macros;
            AutoSquashTypedef                = autoSquashTypedef;
            ParseSystemIncludes              = systemIncludes;
            ParseAttributes                  = attributes;
            TargetCpu                        = targetCpu;
            TargetCpuSub                     = targetCpuSub;
            TargetVendor                     = targetVendor;
            TargetSystem                     = targetSystem;
            TargetAbi                        = targetAbi;
            DefaultNamespace                 = defaultNamespace;
            DefaultOutputFilePath            = defaultOutputFilePath;
            DefaultClassLib                  = defaultClassLib;
            GenerateAsInternal               = generateAsInternal;
            DefaultDllImportNameAndArguments = defaultDllImportNameAndArguments;
            AllowFixedSizeBuffers            = allowFixedSizeBuffers;
            DefaultCharSet                   = defaultCharSet;
            DispatchOutputPerInclude         = dispatchOutputPerInclude;
            DefaultMarshalForString          = defaultMarshalForString;
            DefaultMarshalForBool            = defaultMarshalForBool;
            GenerateEnumItemAsFields         = generateEnumItemAsFields;
            TypedefCodeGenKind               = typedefCodeGenKind;
        }

        public DtoOptions(CSharpConverterOptions options)
        {
            ParseAsCpp                       = options.ParseAsCpp;
            ParseComments                    = options.ParseComments;
            ParseMacros                      = options.ParseMacros;
            AutoSquashTypedef                = options.AutoSquashTypedef;
            ParseSystemIncludes              = options.ParseSystemIncludes;
            ParseAttributes                  = options.ParseAttributes;
            TargetCpu                        = options.TargetCpu;
            TargetCpuSub                     = options.TargetCpuSub;
            TargetVendor                     = options.TargetVendor;
            TargetSystem                     = options.TargetSystem;
            TargetAbi                        = options.TargetAbi;
            DefaultNamespace                 = options.DefaultNamespace;
            DefaultOutputFilePath            = options.DefaultOutputFilePath.ToString();
            DefaultClassLib                  = options.DefaultClassLib;
            GenerateAsInternal               = options.GenerateAsInternal;
            DefaultDllImportNameAndArguments = options.DefaultDllImportNameAndArguments;
            AllowFixedSizeBuffers            = options.AllowFixedSizeBuffers;
            DefaultCharSet                   = options.DefaultCharSet;
            DispatchOutputPerInclude         = options.DispatchOutputPerInclude;
            DefaultMarshalForString          = options.DefaultMarshalForString.UnmanagedType;
            DefaultMarshalForBool            = options.DefaultMarshalForBool.UnmanagedType;
            GenerateEnumItemAsFields         = options.GenerateEnumItemAsFields;
            TypedefCodeGenKind               = options.TypedefCodeGenKind;
        }
    }

    [Serializable]
    public sealed class DtoSettings
    {
        public StringCollection Defines { get; set; }

        public StringCollection AdditionalArguments { get; set; }

        public StringCollection IncludeFolders { get; set; }

        public StringCollection SystemIncludeFolders { get; set; }

        public DtoOptions Options { get; set; }

        public DtoSettings()
        {
        }

        public DtoSettings(StringCollection       defines,
                           StringCollection       additionalArguments,
                           StringCollection       includeFolders,
                           StringCollection       systemIncludeFolders,
                           CSharpConverterOptions options)
        {
            Defines              = defines;
            AdditionalArguments  = additionalArguments;
            IncludeFolders       = includeFolders;
            SystemIncludeFolders = systemIncludeFolders;
            Options              = new DtoOptions(options);
        }

        internal DtoSettings(Settings               settings,
                             CSharpConverterOptions options)
        {
            Defines              = settings.Defines;
            AdditionalArguments  = settings.AdditionalArguments;
            IncludeFolders       = settings.IncludeFolders;
            SystemIncludeFolders = settings.SystemIncludeFolders;
            Options              = new DtoOptions(options);
        }
    }
}
