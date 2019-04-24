using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LINQPad.DumpEditable.Helpers
{
    public static class AnonymousTypeExtensions
    {
        public static bool IsAnonymousType(this Type type) =>
            type.FullName.Contains("AnonymousType") // name contains Anonymous Type
            && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
    }
}