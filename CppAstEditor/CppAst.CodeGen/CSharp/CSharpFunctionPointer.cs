
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using CppAst.CodeGen.Common;

namespace CppAst.CodeGen.CSharp
{
    public class CSharpFunctionPointer : CSharpMethod, ICSharpAttributesProvider, ICSharpElementWithVisibility
    {
        public CSharpFunctionPointer(CSharpMethod csharpMethod)
        {
            Method     = csharpMethod;

            Attributes = Method.Attributes ?? new List<CSharpAttribute>();
            Parameters = Method.Parameters ?? new List<CSharpParameter>();
            Visibility = CSharpVisibility.Public;

            Comment=Method.Comment;
            Modifiers=Method.Modifiers;
            ReturnType=Method.ReturnType;
            IsConstructor=Method.IsConstructor;
            Name=Method.Name;
            Body=Method.Body;
        }
        
        public CSharpMethod Method { get; set; }
        
        //public CSharpComment Comment { get; set; }
        
        public string? CallingConvention { get; set; }

        //public CSharpVisibility Visibility { get; set; }

        //public List<CSharpAttribute> Attributes { get; }

        //public CSharpType ReturnType { get; set; }

        //public List<CSharpParameter> Parameters { get; }

        /// <inheritdoc />
        public override void DumpTo(CodeWriter writer)
        {
            if(writer.Mode == CodeWriterMode.Full)
            {
                Comment?.DumpTo(writer);
            }

            this.DumpAttributesTo(writer);
            ReturnType?.DumpContextualAttributesTo(writer, false, CSharpAttributeScope.Return);

            Visibility.DumpTo(writer);
            writer.Write("static unsafe delegate* unmanaged");

            if(CallingConvention is not null)
            {
                writer.Write("["+CallingConvention+"] ");
            }

            //
            writer.Write("<");

            CSharpParameter param = Parameters[0];
            
            param.ParameterType.DumpTo(writer);

            //param.DumpTo(writer);

            for(int i = 1; i < Parameters.Count; i++)
            {
                param = Parameters[i];
                writer.Write(", ");
                param.ParameterType.DumpTo(writer);
            }

            if(Parameters.Count > 0)
            {
                writer.Write(", ");
            }

            ReturnType?.DumpReferenceTo(writer);

            writer.Write(">");
            //

            writer.Write(" ");
            writer.Write(Name);
            
            writer.Write(" = (delegate* unmanaged");

            if(CallingConvention is not null)
            {
                writer.Write("["+CallingConvention+"] ");
            }


            writer.Write("<");

            param = Parameters[0];
            
            param.ParameterType.DumpTo(writer);

            //param.DumpTo(writer);

            for(int i = 1; i < Parameters.Count; i++)
            {
                param = Parameters[i];
                writer.Write(", ");
                param.ParameterType.DumpTo(writer);
            }

            if(Parameters.Count > 0)
            {
                writer.Write(", ");
            }

            ReturnType?.DumpReferenceTo(writer);

            writer.Write(">)PlatformApi.NativeLibrary.GetExport(Handle, \"");

            writer.Write(Name);
            
            writer.Write("\");");

            writer.WriteLine();
        }


        //public static void DumpTo(List<CSharpParameter> parameters,
        //                          CodeWriter            writer)
        //{
        //    writer.Write("(");

        //    for(int i = 0; i < parameters.Count; i++)
        //    {
        //        CSharpParameter param = parameters[i];

        //        if(i > 0)
        //        {
        //            writer.Write(", ");
        //        }

        //        param.DumpTo(writer);
        //    }

        //    writer.Write(")");
        //}
    }
}