using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad.DumpEditable.Helpers;
using LINQPad.DumpEditable.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace LINQPad.DumpEditable
{
    public partial class EditorRule
    {
        public Func<object, PropertyInfo, bool> Match { get; set; }
        public Func<object, PropertyInfo, Func<object>, Action<object>, object> Editor { get; set; }
    }

    public partial class EditorRule
    {
        public static EditorRule For(Func<object, PropertyInfo, bool> rule,
            Func<object, PropertyInfo, Func<object>, Action<object>, object> getEditor)
            => new EditorRule
            {
                Match = rule,
                Editor = getEditor,
            };

        public static EditorRule ForType<T>(Func<object, PropertyInfo, Func<object>, Action<object>, object> getEditor)
            => new EditorRule
            {
                Match = (o, info) => info.PropertyType == typeof(T),
                Editor = getEditor,
            };

        public static EditorRule ForExpansion(Func<object, PropertyInfo, bool> rule)
            => EditorRule.For(
                rule,
                (o, p, get, set) =>
                {
                    var v = get();
                    var editor = EditableDumpContainer.For(v);
                    editor.OnChanged += () => set(v);
                    return editor;
                });

        public static EditorRule ForExpansionAttribute()
            => EditorRule.ForExpansion((_, p) => p.GetCustomAttributes<DumpEditableExpandAttribute>().Any());

        public static EditorRule ForNestedAnonymousType()
            => EditorRule.ForExpansion((_, p) => p.PropertyType.IsAnonymousType());

        public static EditorRule ForEnums() =>
            EditorRule.For(
                (_, p) => p.PropertyType.IsEnum,
                (o, p, get, set) =>
                    Util.HorizontalRun(true,
                        Enumerable.Concat(
                            new object[] { get(), "[" },
                        p.PropertyType
                            .GetEnumValues()
                            .OfType<object>()
                            .Select(v => new Hyperlinq(() => set(v), $"{v}")))
                            .Concat(new [] { "]" }))
                        );

        public static EditorRule ForBool() =>
            EditorRule.For(
                (_, p) => p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?),
                (o, p, get, set) =>
                    Util.HorizontalRun(true,
                        Enumerable.Concat(
                                new object[] { get() ?? NullString, "[" },
                                new bool?[] { true, false, null }
                                    .Where(b => p.PropertyType == typeof(bool?) || b != null)
                                    .Select(v => new Hyperlinq(() => set(v), $"{(object)v ?? NullString }")))
                            .Concat(new[] { "]" }))
            );

        public static EditorRule ForTypeWithStringBasedEditor<T>(ParseFunc<string, T, bool> parseFunc, bool supportNullable = true, bool supportEnumerable = true)
            => new EditorRule
            {
                Match = (o, info) => 
                    info.PropertyType == typeof(T)
                    || (supportNullable && Nullable.GetUnderlyingType(info.PropertyType) == typeof(T))
                    || (supportEnumerable && info.PropertyType.GetArrayLikeElementType() == typeof(T)),
                Editor = (o, info, get, set) => GetStringInputBasedEditor(o, info, get, set, parseFunc, supportNullable, supportEnumerable)
            };

        protected static object GetStringInputBasedEditor<TOut>(object o, PropertyInfo p, Func<object> getCurrValue, Action<object> setNewValue, EditorRule.ParseFunc<string, TOut, bool> parseFunc,
            bool supportNullable = true, bool supportEnumerable = true)
        {
            var type = p.PropertyType;
            var currVal = getCurrValue();
            var isEnumerable = supportEnumerable && type.GetArrayLikeElementType() != null;

            // handle string which is IEnumerable<char> 
            if (typeof(TOut) == typeof(string) && type.GetArrayLikeElementType() == typeof(char))
                isEnumerable = false;

            var desc = currVal == null 
                    ? NullString
                    : (isEnumerable ? JsonConvert.SerializeObject(currVal) : $"{currVal}");

            var change = new Hyperlinq(() =>
            {
                var newVal = Interaction.InputBox("Set value for " + p.Name, p.Name, desc);

                var canConvert = parseFunc(newVal, out var output);
                if (isEnumerable)
                {
                    try
                    {
                        var val = JsonConvert.DeserializeObject(newVal, type);
                        setNewValue(val);
                    }
                    catch
                    {
                        return; // can't deserialise
                    }
                }
                else if (canConvert)
                {
                    setNewValue(output);
                }
                else if (supportNullable && (newVal == String.Empty))
                {
                    setNewValue(null);
                }
                else
                    return; // can't convert
            }, desc);

            return Util.HorizontalRun(true, change);
        }
        
        public delegate V ParseFunc<T, U, V>(T input, out U output);

        private const string NullString = "(null)";
    }
}