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

        public static EditorRule ForTypeWithStringBasedEditor<T>(ParseFunc<string, T, bool> parseFunc, bool supportNullable = true, bool supportEnumerable = true)
            => new EditorRule
            {
                Match = (o, info) => 
                    info.PropertyType == typeof(T)
                    || (supportNullable && Nullable.GetUnderlyingType(info.PropertyType) == typeof(T))
                    || (supportEnumerable && GetArrayLikeElementType(info.PropertyType) == typeof(T)),
                Editor = (o, info, changed) => GetStringInputBasedEditor(o, info, changed, parseFunc, supportNullable, supportEnumerable)
            };

        protected static object GetStringInputBasedEditor<TOut>(object o, PropertyInfo p, Action changeCallback, EditorRule.ParseFunc<string, TOut, bool> parseFunc,
            bool supportNullable = true, bool supportEnumerable = true)
        {
            var type = p.PropertyType;
            var currVal = p.GetValue(o);
            var isEnumerable = supportEnumerable && GetArrayLikeElementType(type) != null;

            // handle string which is IEnumerable<char> 
            if (typeof(TOut) == typeof(string) && GetArrayLikeElementType(type) == typeof(char))
                isEnumerable = false;

            var desc = currVal == null 
                    ? "null"
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
                        changeCallback?.Invoke();
                    }
                    catch
                    {
                        return; // can't deserialise
                    }
                }
                if (canConvert)
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

        // https://stackoverflow.com/a/17713382/752273
        public static Type GetArrayLikeElementType(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                .Where(t => t.IsGenericType &&
                            t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return enumType;
        }

        public static void SetValue(object o, PropertyInfo p, object v)
        {
            if (o.GetType().IsAnonymousType())
                AnonymousObjectMutator.Set(o, p, v);
            else
                p.SetValue(o, v);
        }
        
        public delegate V ParseFunc<T, U, V>(T input, out U output);
    }
}