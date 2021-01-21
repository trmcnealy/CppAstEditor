﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst.CodeGen.CSharp
{
    public class DefaultEnumConverter : ICSharpConverterPlugin
    {
        /// <inheritdoc />
        public void Register(CSharpConverter converter, CSharpConverterPipeline pipeline)
        {
            pipeline.EnumConverters.Add(ConvertEnum);
        }

        public static CSharpElement ConvertEnum(CSharpConverter converter, CppEnum cppEnum, CSharpElement context)
        {
            string enumName = converter.GetCSharpName(cppEnum, context);

            CSharpEnum csEnum = new CSharpEnum(enumName)
            {
                CppElement = cppEnum
            };

            if (cppEnum.IsAnonymous)
            {
                csEnum.Name += "AnonymousEnum";
            }

            ICSharpContainer container = converter.GetCSharpContainer(cppEnum, context);
            container.Members.Add(csEnum);

            converter.ApplyDefaultVisibility(csEnum, container);

            csEnum.Comment = converter.GetCSharpComment(cppEnum, csEnum);
            csEnum.BaseTypes.Add(converter.GetCSharpType(cppEnum.IntegerType, csEnum));

            return csEnum;
        }
    }
}