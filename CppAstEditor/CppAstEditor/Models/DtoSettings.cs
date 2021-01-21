using System;
using System.Collections.Generic;
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
        public List<string> Defines { get; set; }

        public List<string> AdditionalArguments { get; set; }

        public List<string> IncludeFolders { get; set; }

        public List<string> SystemIncludeFolders { get; set; }

        public DtoOptions Options { get; set; }

        public DtoSettings()
        {
        }

        public DtoSettings(List<string>       defines,
                           List<string>       additionalArguments,
                           List<string>       includeFolders,
                           List<string>       systemIncludeFolders,
                           CSharpConverterOptions options)
        {
            Defines = new List<string>(defines.Count);
            AdditionalArguments = new List<string>(additionalArguments.Count);
            IncludeFolders = new List<string>(includeFolders.Count);
            SystemIncludeFolders = new List<string>(systemIncludeFolders.Count);


            foreach (string item in defines)
            {
                Defines.Add(item);
            }

            foreach (string item in additionalArguments)
            {
                AdditionalArguments.Add(item);
            }

            foreach (string item in includeFolders)
            {
                IncludeFolders.Add(item);
            }

            foreach (string item in systemIncludeFolders)
            {
                SystemIncludeFolders.Add(item);
            }


            Options              = new DtoOptions(options);
        }

        internal DtoSettings(Settings               settings,
                             CSharpConverterOptions options)
        {
            Defines = new List<string>(settings.Defines.Count);
            AdditionalArguments = new List<string>(settings.AdditionalArguments.Count);
            IncludeFolders = new List<string>(settings.IncludeFolders.Count);
            SystemIncludeFolders = new List<string>(settings.SystemIncludeFolders.Count);

            foreach (string item in settings.Defines)
            {
                Defines.Add(item);
            }

            foreach (string item in settings.AdditionalArguments)
            {
                AdditionalArguments.Add(item);
            }

            foreach (string item in settings.IncludeFolders)
            {
                IncludeFolders.Add(item);
            }

            foreach (string item in settings.SystemIncludeFolders)
            {
                SystemIncludeFolders.Add(item);
            }


            Options              = new DtoOptions(options);
        }
    }
}
