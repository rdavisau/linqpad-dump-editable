using System;
using System.Collections;
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
        public bool DisableAutomaticRefresh { get; set; }
    }

    public partial class EditorRule
    {
        public static EditorRule For(Func<object, PropertyInfo, bool> rule,
            Func<object, PropertyInfo, Func<object>, Action<object>, object> getEditor, bool disableAutomaticRefresh = false)
            => new EditorRule
            {
                Match = rule,
                Editor = getEditor,
                DisableAutomaticRefresh = disableAutomaticRefresh
            };

        public static EditorRule ForType<T>(Func<object, PropertyInfo, Func<object>, Action<object>, object> getEditor, bool disableAutomaticRefresh = false)
            => new EditorRule
            {
                Match = (o, info) => info.PropertyType == typeof(T),
                Editor = getEditor,
                DisableAutomaticRefresh = disableAutomaticRefresh
            };

        public static EditorRule ForExpansion(Func<object, PropertyInfo, bool> rule)
            => EditorRule.For(
                rule,
                (o, p, get, set) =>
                {
                    var v = get();
                    var isEnumerable = v.GetType().GetArrayLikeElementType() != null;

                    var editor = isEnumerable
                        ? EditableDumpContainer.ForEnumerable(((IEnumerable)v).OfType<object>())
                        : EditableDumpContainer.For(v);

                    editor.OnChanged += () => set(v);
                    return editor;
                });

        public static EditorRule ForExpansionAttribute()
            => EditorRule.ForExpansion((_, p) => p.GetCustomAttributes<DumpEditableExpandAttribute>().Any());

        public static EditorRule ForNestedAnonymousType()
            => EditorRule.ForExpansion((_, p) => 
                    p.PropertyType.IsAnonymousType()
                || (p.PropertyType.GetArrayLikeElementType()?.IsAnonymousType() ?? false));

        public static EditorRule ForEnums() =>
            EditorRule.For(
                (_, p) => p.PropertyType.IsEnum || p.PropertyType.IsNullableEnum(),
                (o, p, get, set) =>
                {
                    var isNullable = p.PropertyType.IsNullableEnum();
                    var type = isNullable 
                        ? Nullable.GetUnderlyingType(p.PropertyType) 
                        : p.PropertyType;

                    var options = type.GetEnumValues().OfType<object>().ToList();

                    return EditableDumpContainer.DefaultOptions.OptionsEditor(
                        options,
                        isNullable 
                            ? NullableOptionInclusionKind.IncludeAtEnd
                            : NullableOptionInclusionKind.DontInclude,
                        null)(o, p, get, set);
                });

        public static EditorRule ForBool() =>
            EditorRule.For(
                (_, p) => p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?),
                (o, p, get, set) => EditableDumpContainer.DefaultOptions.OptionsEditor(
                        new [] { true, false }.OfType<object>(),
                        p.PropertyType == typeof(bool?) 
                            ? NullableOptionInclusionKind.IncludeAtEnd
                            : NullableOptionInclusionKind.DontInclude,
                        null)(o, p, get, set));

        public static EditorRule ForTypeWithStringBasedEditor<T>(ParseFunc<string, T, bool> parseFunc, bool supportNullable = true, bool supportEnumerable = true)
            => new EditorRule
            {
                Match = (o, info) => 
                    info.PropertyType == typeof(T)
                    || (supportNullable && Nullable.GetUnderlyingType(info.PropertyType) == typeof(T))
                    || (supportEnumerable && info.PropertyType.GetArrayLikeElementType() == typeof(T)),
                Editor = (o, info, get, set) => EditableDumpContainer.DefaultOptions.StringBasedEditor(WrapParseFunc(parseFunc), supportNullable, supportEnumerable)(o, info, get, set),          
                DisableAutomaticRefresh = true,
            };

        private static ParseFunc<string, object, bool> WrapParseFunc<T>(ParseFunc<string, T, bool> parseFunc) 
            => (string input, out object output) =>
            {
                var ret = parseFunc(input, out var tOut);
                output = tOut;

                return ret;
            };
        
        public delegate V ParseFunc<T, U, V>(T input, out U output);
        
        public const string NullString = "(null)";
        public const string EmptyString = "(empty string)";
    }
}