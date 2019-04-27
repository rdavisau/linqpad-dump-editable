using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.DumpEditable
{
    public static class Extensions
    {
        public static T DumpEditable<T>(this T obj)
            => DumpEditable(obj, out _);

        public static T DumpEditable<T>(this T obj, out EditableDumpContainer<T> container)
        {
            container = new EditableDumpContainer<T>(obj);
            container.Dump();

            return obj;
        }

        public static IEnumerable<T> DumpEditableEnumerable<T>(this IEnumerable<T> obj)
            => DumpEditableEnumerable<T>(obj, out _);

        public static IEnumerable<T> DumpEditableEnumerable<T>(this IEnumerable<T> obj, out EditableDumpContainer<T> container)
        {
            container = new EditableDumpContainer<T>(obj);
            container.Dump();

            return obj;
        }
    }
}