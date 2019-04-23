using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic;

namespace LINQPad.DumpEditable
{
    public partial class EditorRule
    {
        public Func<object, PropertyInfo, bool> Match { get; set; }
        public Func<object, PropertyInfo, Action, object> Editor { get; set; }
    }

    public partial class EditorRule
    {
        public static EditorRule For(Func<object, PropertyInfo, bool> rule,
            Func<object, PropertyInfo, Action, object> getEditor)
            => new EditorRule
            {
                Match = rule,
                Editor = getEditor,
            };

        public static EditorRule ForType<T>(Func<object, PropertyInfo, Action, object> getEditor)
            => new EditorRule
            {
                Match = (o, info) => info.PropertyType == typeof(T),
                Editor = getEditor,
            };
        public static EditorRule ForEnums() =>
            EditorRule.For(
                (_, p) => p.PropertyType.IsEnum,
                (o, p, c) =>
                    Util.HorizontalRun(true,
                        Enumerable.Concat(
                            new object[] { p.GetValue(o), "[" },
                        p.PropertyType
                            .GetEnumValues()
                            .OfType<object>()
                            .Select(v => new Hyperlinq(() => { p.SetValue(o, v); c(); }, $"{v}")))
                            .Concat(new [] { "]" }))
                        );

        public static EditorRule ForTypeWithStringBasedEditor<T>(ParseFunc<string, T, bool> parseFunc, bool supportNullable = true)
            => new EditorRule
            {
                Match = (o, info) => 
                    info.PropertyType == typeof(T)
                    || (supportNullable && Nullable.GetUnderlyingType(info.PropertyType) == typeof(T)),
                Editor = (o, info, changed) => GetStringInputBasedEditor(o, info, changed, parseFunc, supportNullable)
            };

        protected static object GetStringInputBasedEditor<TOut>(object o, PropertyInfo p, Action changeCallback, EditorRule.ParseFunc<string, TOut, bool> parseFunc,
            bool supportNullable = true)
        {
            var currVal = p.GetValue(o);
            var desc = currVal != null ? $"{currVal}" : "null";

            var change = new Hyperlinq(() =>
            {
                var newVal = Interaction.InputBox("Set value for " + p.Name, p.Name, $"{currVal}");

                var canConvert = parseFunc(newVal, out var output);
                if (canConvert)
                {
                    p.SetValue(o, output);
                }
                else if (supportNullable && (newVal == String.Empty))
                {
                    p.SetValue(o, null);
                }
                else
                    return; // can't convert

                changeCallback?.Invoke();

            }, desc);

            return Util.HorizontalRun(true, change);
        }

        public delegate V ParseFunc<T, U, V>(T input, out U output);
    }
}