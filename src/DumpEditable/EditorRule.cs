using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad.DumpEditable.Helpers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

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
                            .Select(v => new Hyperlinq(() => { SetValue(o,p,v); c(); }, $"{v}")))
                            .Concat(new [] { "]" }))
                        );

        public static EditorRule ForBool() =>
            EditorRule.For(
                (_, p) => p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?),
                (o, p, c) =>
                    Util.HorizontalRun(true,
                        Enumerable.Concat(
                                new object[] { p.GetValue(o) ?? NullString, "[" },
                                new bool?[] { true, false, null }
                                    .Where(b => p.PropertyType == typeof(bool?) || b != null)
                                    .Select(v => new Hyperlinq(() => { SetValue(o, p, v); c(); }, $"{(object)v ?? NullString }")))
                            .Concat(new[] { "]" }))
            );

        public static EditorRule ForTypeWithStringBasedEditor<T>(ParseFunc<string, T, bool> parseFunc, bool supportNullable = true, bool supportEnumerable = true)
            => new EditorRule
            {
                Match = (o, info) => 
                    info.PropertyType == typeof(T)
                    || (supportNullable && Nullable.GetUnderlyingType(info.PropertyType) == typeof(T))
                    || (supportEnumerable && info.PropertyType.GetArrayLikeElementType() == typeof(T)),
                Editor = (o, info, changed) => GetStringInputBasedEditor(o, info, changed, parseFunc, supportNullable, supportEnumerable)
            };

        protected static object GetStringInputBasedEditor<TOut>(object o, PropertyInfo p, Action changeCallback, EditorRule.ParseFunc<string, TOut, bool> parseFunc,
            bool supportNullable = true, bool supportEnumerable = true)
        {
            var type = p.PropertyType;
            var currVal = p.GetValue(o);
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
                        SetValue(o,p,val);
                    }
                    catch
                    {
                        return; // can't deserialise
                    }
                }
                else if (canConvert)
                {
                    SetValue(o,p,output);
                }
                else if (supportNullable && (newVal == String.Empty))
                {
                    SetValue(o, p, null);
                }
                else
                    return; // can't convert

                changeCallback?.Invoke();

            }, desc);

            return Util.HorizontalRun(true, change);
        }
        
        public static void SetValue(object o, PropertyInfo p, object v)
        {
            if (o.GetType().IsAnonymousType())
                AnonymousObjectMutator.Set(o, p, v);
            else
                p.SetValue(o, v);
        }
        
        public delegate V ParseFunc<T, U, V>(T input, out U output);

        private const string NullString = "(null)";
    }
}