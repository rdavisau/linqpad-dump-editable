using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LINQPad.DumpEditable.Helpers;
using LINQPad.DumpEditable.Models;

namespace LINQPad.DumpEditable
{
    public partial class EditableDumpContainer<T> : DumpContainer
    {
        private readonly T _obj;
        private readonly bool _failSilently;
        private readonly Dictionary<PropertyInfo, Action<T, PropertyInfo, object>> _changeHandlers 
            = new Dictionary<PropertyInfo, Action<T, PropertyInfo, object>>();

        private readonly List<EditorRule> _editorRules = new List<EditorRule>();

        public Action OnChanged { get; set; }
        public Action<T, PropertyInfo, object> OnPropertyValueChanged { get; set; }

        public static void AddGlobalEditorRule(EditorRule rule) 
            => GlobalEditorRules.Insert(0, rule);

        public void AddChangeHandler<U>(Expression<Func<T, U>> selector,
            Action<T, PropertyInfo, U> onChangedAction)
        {
            var pi = (selector.Body as MemberExpression)?.Member as PropertyInfo;
            if (pi is null)
                throw new Exception($"Invalid expression passed to {nameof(AddChangeHandler)}");

            _changeHandlers[pi] = (obj, p, val) => onChangedAction(obj, p, (U) val);
        }

        public void AddEditorRule(EditorRule rule)
        {
            _editorRules.Insert(0, rule);
            SetContent();
        }

        public EditableDumpContainer(T obj, bool failSilently = false)
        {
            _obj = obj;
            _failSilently = failSilently;

            SetContent();
        }
        
        private void SetContent()
        {
            object content = _obj;

            var isEnumerable = content.GetType().GetArrayLikeElementType() != null;

            try
            {
                content = 
                    !isEnumerable 
                        ? GetObjectEditorRepresentation(_obj)
                        : (_obj as IEnumerable).OfType<object>().Select(GetObjectEditorRepresentation).ToList();
            }
            catch
            {
                if (!_failSilently)
                    throw;
            }

            Content = content;
        }

        private object GetObjectEditorRepresentation(object input)
        {
            var properties = input
                .GetType()
                .GetProperties()
                .Select(p =>
                    new PropertyEditor
                    {
                        Property = p.Name,
                        Value = GetPropertyEditor(input, p)
                    })
                .ToList();

            return GetDynamicEditorTypeForObject(input, properties);
        }

        private readonly Dictionary<Type, Type> _dynamicTypeMappings = new Dictionary<Type, Type>();
        private object GetDynamicEditorTypeForObject(object input, List<PropertyEditor> propertyEditors)
        {
            var inType = input.GetType();

            if (!_dynamicTypeMappings.TryGetValue(inType, out var outType))
            {
                outType = DynamicTypeBuilder.CreateTypeForEditor(input, propertyEditors);
                _dynamicTypeMappings[inType] = outType;
            }

            var @out = Activator.CreateInstance(outType);
            var props = outType.GetProperties().ToDictionary(p => p.Name);
            foreach (var pe in propertyEditors)
            {
                var p = props[pe.Property];
                p.SetValue(@out, pe.Value);
            }

            return @out;
        }

        private object GetPropertyEditor(object o, PropertyInfo p)
        {
            var allRules = Enumerable.Concat(_editorRules, GlobalEditorRules);

            foreach (var editor in allRules)
                if (editor.Match(o, p))
                    return editor.Editor(o, p, () =>
                    {
                        SetContent();

                        var newVal = p.GetValue(o);

                        if (_changeHandlers.TryGetValue(p, out var handler))
                            handler.Invoke((T)o,p,newVal);

                        OnPropertyValueChanged?.Invoke((T)o, p, newVal);

                        OnChanged?.Invoke();
                    });

            return p.GetValue(o);
        }
    }

    public partial class EditableDumpContainer<T>
    {
        private static readonly List<EditorRule> GlobalEditorRules = GetDefaultEditors();
        private static List<EditorRule> GetDefaultEditors() =>
            new List<EditorRule>
            {
                EditorRule.ForEnums(),
                EditorRule.ForTypeWithStringBasedEditor<int>(int.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<uint>(uint.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<short>(short.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<ushort>(ushort.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<double>(double.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<decimal>(decimal.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<float>(float.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<long>(long.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<ulong>(ulong.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<DateTime>(DateTime.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<DateTimeOffset>(DateTimeOffset.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<string>((string input, out string output) =>
                {
                    output = input;
                    return true;
                }),
            };
    }

    public static class EditableDumpContainer
    {
        public static EditableDumpContainer<T> For<T>(T obj, bool failSilently = false)
            => new EditableDumpContainer<T>(obj, failSilently);
    }
}