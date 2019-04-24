using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.DumpEditable.Helpers
{
    // Adapted from https://stackoverflow.com/a/30242237/752273
    // (the "more elaborate" version - here: https://ideone.com/ALG9DE)
    internal static class AnonymousObjectMutator
    {
        private const BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags PropFlags = BindingFlags.Public | BindingFlags.Instance;
        private static readonly string[] BackingFieldFormats = { "<{0}>i__Field", "<{0}>" };
        private static ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>> _map =
            new ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>>();

        public static T Set<T, TProperty>(
            this T instance,
            Expression<Func<T, TProperty>> propExpression,
            TProperty newValue) where T : class
        {
            GetSetterFor(propExpression)(instance, newValue);
            return instance;
        }

        public static void Set(
            this object instance,
            PropertyInfo p,
            object newValue)
        {
            GetSetterFor(instance.GetType(), p)(instance, newValue);
        }

        private static Action<object, object> GetSetterFor(Type t, PropertyInfo property)
        {
            Action<object, object> setter = null;
            GetPropMap(t).TryGetValue(property.Name, out setter);
            if (setter == null)
                throw new InvalidOperationException("No setter found");
            return setter;
        }

        private static Action<object, object> GetSetterFor<T, TProperty>(Expression<Func<T, TProperty>> propExpression)
        {
            var memberExpression = propExpression.Body as MemberExpression;
            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
                throw new InvalidOperationException("Only property expressions are supported");
            Action<object, object> setter = null;
            GetPropMap<T>().TryGetValue(memberExpression.Member.Name, out setter);
            if (setter == null)
                throw new InvalidOperationException("No setter found");
            return setter;
        }

        private static IDictionary<string, Action<object, object>> GetPropMap<T>()
            => GetPropMap(typeof(T));

        private static IDictionary<string, Action<object, object>> GetPropMap(Type t) 
            => _map.GetOrAdd(t, x => BuildPropMap(t));

        private static IDictionary<string, Action<object, object>> BuildPropMap(Type t)
        {
            var typeMap = new Dictionary<string, Action<object, object>>();
            var fields = t.GetFields(FieldFlags);
            foreach (var pi in t.GetProperties(PropFlags))
            {
                var backingFieldNames = BackingFieldFormats.Select(x => string.Format(x, pi.Name)).ToList();
                var fi = fields.FirstOrDefault(f => backingFieldNames.Contains(f.Name) && f.FieldType == pi.PropertyType);
                if (fi == null)
                    throw new NotSupportedException(string.Format("No backing field found for property {0}.", pi.Name));
                typeMap.Add(pi.Name, (inst, val) => fi.SetValue(inst, val));
            }
            return typeMap;
        }
    }
}
