﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace CppAst.CodeGen.CSharp
{
    /// <summary>
    /// Equivalent of <see cref="UnmanagedType"/>
    /// </summary>
    public enum CSharpUnmanagedKind
    {
        Bool = 0x2,         // 4 byte boolean value (true != 0, false == 0)
        I1 = 0x3,           // 1 byte signed value
        U1 = 0x4,           // 1 byte unsigned value
        I2 = 0x5,           // 2 byte signed value
        U2 = 0x6,           // 2 byte unsigned value
        I4 = 0x7,           // 4 byte signed value
        U4 = 0x8,           // 4 byte unsigned value
        I8 = 0x9,           // 8 byte signed value
        U8 = 0xa,           // 8 byte unsigned value
        R4 = 0xb,           // 4 byte floating point
        R8 = 0xc,           // 8 byte floating point
        Currency = 0xf,     // A currency
        BStr = 0x13,        // OLE Unicode BSTR
        LPStr = 0x14,       // Ptr to SBCS string
        LPWStr = 0x15,      // Ptr to Unicode string
        LPTStr = 0x16,      // Ptr to OS preferred (SBCS/Unicode) string
        ByValTStr = 0x17,   // OS preferred (SBCS/Unicode) inline string (only valid in structs)
        IUnknown = 0x19,    // COM IUnknown pointer.
        IDispatch = 0x1a,   // COM IDispatch pointer
        Struct = 0x1b,      // Structure
        Interface = 0x1c,   // COM interface
        SafeArray = 0x1d,   // OLE SafeArray
        ByValArray = 0x1e,  // Array of fixed size (only valid in structs)
        SysInt = 0x1f,      // Hardware natural sized signed integer
        SysUInt = 0x20,
        VBByRefStr = 0x22,
        AnsiBStr = 0x23,    // OLE BSTR containing SBCS characters
        TBStr = 0x24,       // Ptr to OS preferred (SBCS/Unicode) BSTR
        VariantBool = 0x25, // OLE defined BOOLEAN (2 bytes, true == -1, false == 0)
        FunctionPtr = 0x26, // Function pointer
        AsAny = 0x28,       // Paired with Object type and does runtime marshalling determination
        LPArray = 0x2a,     // C style array
        LPStruct = 0x2b,    // Pointer to a structure
        CustomMarshaler = 0x2c,
        Error = 0x2d,
        IInspectable = 0x2e,
        HString = 0x2f,     // Windows Runtime HSTRING
        LPUTF8Str = 0x30,   // UTF8 string
    }

    public class CSharpMarshalAttribute : CSharpAttribute
    {
        public CSharpMarshalAttribute(CSharpUnmanagedKind unmanagedType)
        {
            UnmanagedType = unmanagedType;
        }

        public CSharpUnmanagedKind UnmanagedType { get; set; }

        public CSharpUnmanagedKind? ArraySubType { get; set; }

        public int? SizeParamIndex { get; set; }

        public int? SizeConst { get; set; }

        /// <summary>
        /// Gets or sets a custom marshal type as a string
        /// </summary>
        public string MarshalType { get; set; }

        /// <summary>
        /// Gets or sets a custom marshal type as a System.Type
        /// </summary>
        public string MarshalTypeRef { get; set; }

        /// <summary>
        /// Gets or sets the custom Marshal cookie.
        /// </summary>
        public string MarshalCookie { get; set; }

        /// <inheritdoc />
        public override string ToText()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("MarshalAs(UnmanagedType.");
            builder.Append(UnmanagedType.ToString());
            if (ArraySubType.HasValue)
            {
                builder.Append(", ArraySubType = UnmanagedType.");
                builder.Append(ArraySubType.Value.ToString());
            }

            if (SizeParamIndex.HasValue)
            {
                builder.Append(", SizeParamIndex = ");
                builder.Append(SizeParamIndex.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (SizeConst.HasValue)
            {
                builder.Append(", SizeConst = ");
                builder.Append(SizeConst.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MarshalType != null)
            {
                builder.Append(", MarshalType = ").Append(MarshalType);
            }

            if (MarshalTypeRef != null)
            {
                builder.Append(", MarshalTypeRef = ").Append(MarshalTypeRef);
            }

            if (MarshalCookie != null)
            {
                builder.Append(", MarshalCookie = ").Append(MarshalCookie);
            }
            builder.Append(")");
            return builder.ToString();
        }
    }
}