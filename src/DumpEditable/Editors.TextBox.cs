using System;
using System.Collections.Generic;
using System.Reflection;

namespace LINQPad.DumpEditable
{
    public static partial class Editors
    {
        private const string DefaultWidth = "15em";
        private const string ByteWidth = "3em";
        private const string Int16Width = "5em";
        private const string Int32Width = "7em";
        private const string NumericWidth = "9em";
        private const string DateTimeWidth = "13.5em";
        private const string DateTimeOffsetWidth = "17em";

        public static readonly Dictionary<Type, string> TextBoxTypeWidths = new Dictionary<Type, string>
        {
            [typeof(byte)] = ByteWidth,
            [typeof(Int16)] = Int16Width, [typeof(UInt16)] = Int16Width,
            [typeof(Int32)] = Int32Width, [typeof(UInt32)] = Int32Width,
            [typeof(Int64)] = NumericWidth, [typeof(UInt64)] = NumericWidth,
            [typeof(double)] = NumericWidth,
            [typeof(float)] = NumericWidth,
            [typeof(decimal)] = NumericWidth,
            [typeof(DateTime)] = DateTimeWidth, [typeof(DateTimeOffset)] = DateTimeOffsetWidth
        };

        public static Func<object, PropertyInfo, string> WidthForTextBox { get; set; } = (o, p) =>
        {
            var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

            return TextBoxTypeWidths.TryGetValue(type, out var width) 
                ? width 
                : DefaultWidth;
        };
    }
}