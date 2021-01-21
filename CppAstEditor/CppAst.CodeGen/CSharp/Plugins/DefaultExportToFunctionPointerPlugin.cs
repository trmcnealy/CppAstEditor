// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Runtime.InteropServices;

namespace CppAst.CodeGen.CSharp
{
    [StructLayout(LayoutKind.Explicit)]
    public class DefaultExportToFunctionPointerPlugin : ICSharpConverterPlugin
    {
        /// <inheritdoc />
        public void Register(CSharpConverter         converter,
                             CSharpConverterPipeline pipeline)
        {
            pipeline.ExportToFunctionPointerConverters.Add(ConvertFunction);
        }

        public static CSharpElement ConvertFunction(CSharpConverter converter,
                                                    CppFunction     cppFunction,
                                                    CSharpElement   context)
        {
            // We process only public export functions
            if(!cppFunction.IsPublicExport() ||
               (cppFunction.Flags & (CppFunctionFlags.Inline | CppFunctionFlags.Method | CppFunctionFlags.Constructor | CppFunctionFlags.Destructor)) != 0 &&
               (cppFunction.Flags & CppFunctionFlags.Virtual)                                                                                         == 0)
            {
                return null;
            }

            // Register the struct as soon as possible
            CSharpMethod csMethod = new CSharpMethod
            {
                CppElement = cppFunction
            };
            
            CSharpFunctionPointer csFunction = new CSharpFunctionPointer(csMethod);

            ICSharpContainer container = converter.GetCSharpContainer(cppFunction, context);
            container.Members.Add(csFunction);

            converter.ApplyDefaultVisibility(csFunction, container);
            
            converter.AddUsing(container, "System.Security");
            converter.AddUsing(container, "System.Runtime.InteropServices");
            converter.AddUsing(container, "System.Runtime.CompilerServices");

            if((cppFunction.Flags & CppFunctionFlags.Virtual) == 0)
            {
                csFunction.Modifiers  |= CSharpModifiers.Unsafe | CSharpModifiers.Static;
                csFunction.Visibility =  CSharpVisibility.Public;
            }
            else
            {
                csFunction.Visibility = CSharpVisibility.None;
            }

            csFunction.Name       = converter.GetCSharpName(cppFunction, csFunction);
            csFunction.Comment    = converter.GetCSharpComment(cppFunction, csFunction);
            csFunction.ReturnType = converter.GetCSharpType(cppFunction.ReturnType, csFunction);
            
            CppCallingConvention callingConvention   = (csFunction.CppElement as CppFunction)?.CallingConvention ?? CppCallingConvention.Default;

            //csFunction.Attributes.Add(new CSharpFreeAttribute($"UnmanagedFunctionPointer(CallingConvention.{callingConvention.GetCSharpFunctionPointerCallingConvention()})"));
            //csFunction.Attributes.Add(new CSharpFreeAttribute("SuppressUnmanagedCodeSecurity"));                    

            return csFunction;            
        }
    }
}