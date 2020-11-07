using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;

namespace CppAstEditor
{
    public class CSharpLanguage : ILanguageService
    {
        private static readonly LanguageVersion MaxLanguageVersion = Enum.GetValues(typeof(LanguageVersion)).Cast<LanguageVersion>().Max();

        private readonly IReadOnlyCollection<MetadataReference> _references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location), MetadataReference.CreateFromFile(typeof(ValueTuple<>).GetTypeInfo().Assembly.Location)
        };

        public SyntaxTree ParseText(string         sourceCode,
                                    SourceCodeKind kind)
        {
            CSharpParseOptions options = new CSharpParseOptions(kind: kind, languageVersion: MaxLanguageVersion);

            return CSharpSyntaxTree.ParseText(sourceCode, options);
        }

        public Compilation CreateLibraryCompilation(string assemblyName,
                                                    bool   enableOptimisations = true)
        {
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                                                            optimizationLevel: enableOptimisations ? OptimizationLevel.Release : OptimizationLevel.Debug,
                                                                            allowUnsafe: true);

            return CSharpCompilation.Create(assemblyName, options: options, references: _references);
        }
    }
}