using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LINQPad.DumpEditable.Helpers
{
    public static class TypeExtensions
    {
        public static bool IsAnonymousType(this Type type) =>
            type.FullName.Contains("AnonymousType") // name contains Anonymous Type
            && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;


        // https://stackoverflow.com/a/17713382/752273
        public static Type GetArrayLikeElementType(this Type type)
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

        public static bool IsNullableEnum(this Type t)
            => (Nullable.GetUnderlyingType(t)?.IsEnum ?? false);
    }
}