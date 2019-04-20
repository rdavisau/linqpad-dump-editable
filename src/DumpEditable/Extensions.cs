using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.DumpEditable
{
    public static class Extensions
    {
        public static T DumpEditable<T>(this T obj, bool failSilently = false)
            => DumpEditable(obj, out _, failSilently);

        public static T DumpEditable<T>(this T obj, out EditableDumpContainer<T> container, bool failSilently = false)
        {
            container = new EditableDumpContainer<T>(obj, failSilently);
            container.Dump();

            return obj;
        }
    }
}