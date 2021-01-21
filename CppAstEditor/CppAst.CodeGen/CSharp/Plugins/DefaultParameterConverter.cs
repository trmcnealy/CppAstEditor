// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CppAst.CodeGen.CSharp
{
    [StructLayout(LayoutKind.Explicit)]
    public class DefaultParameterConverter : ICSharpConverterPlugin
    {
        /// <inheritdoc />
        public void Register(CSharpConverter converter, CSharpConverterPipeline pipeline)
        {
            pipeline.ParameterConverters.Add(ConvertParameter);
        }

        public static CSharpElement ConvertParameter(CSharpConverter converter, CppParameter cppParam, int index, CSharpElement context)
        {
            CSharpElement parent = null;
            List<CSharpParameter> parameters = null;

            if(context is CSharpFunctionPointer cSharpFunctionPointer)
            {
                parent = cSharpFunctionPointer;
                parameters = cSharpFunctionPointer.Parameters;
            }
            else if(context is CSharpMethod cSharpMethod)
            {
                parent = cSharpMethod;
                parameters = cSharpMethod.Parameters;
            }
            else if(context is CSharpDelegate cSharpDelegate)
            {
                parent = cSharpDelegate;
                parameters = cSharpDelegate.Parameters;
            }

            if (parameters == null)
            {
                return null;
            }

            string csParamName = string.IsNullOrEmpty(cppParam.Name) ? "arg" + index : converter.GetCSharpName(cppParam, context);
            CSharpParameter csParam = new CSharpParameter(csParamName) { CppElement = cppParam, Parent = parent };
            parameters.Add(csParam);

            CSharpType csParamType = converter.GetCSharpType(cppParam.Type, csParam);
            csParam.Index = index;
            csParam.ParameterType = csParamType;

            return csParam;
        }
    }
}