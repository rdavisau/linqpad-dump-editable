using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;
using LINQPad.DumpEditable.Helpers;
using LINQPad.DumpEditable.Models;

namespace LINQPad.DumpEditable
{
    public partial class EditableDumpContainer<T> : DumpContainer
    {
        private readonly object _obj;
        private readonly bool _failSilently;
        private readonly Dictionary<PropertyInfo, Action<T, object>> _changeHandlers 
            = new Dictionary<PropertyInfo, Action<T, object>>();

        private readonly List<EditorRule> _editorRules = new List<EditorRule>();

        public Action OnChanged { get; set; }
        public Action<T, PropertyInfo, object> OnPropertyValueChanged { get; set; }
        public IDisposable KeepRunningToken { get; private set; }
        
        public void AddChangeHandler<U>(Expression<Func<T, U>> selector,
            Action<T, U> onChangedAction)
        {
            var pi = (selector.Body as MemberExpression)?.Member as PropertyInfo;
            if (pi is null)
                throw new Exception($"Invalid expression passed to {nameof(AddChangeHandler)}");

            _changeHandlers[pi] = (obj, val) => onChangedAction(obj, (U) val);
        }

        public void AddEditorRule(EditorRule rule)
        {
            _editorRules.Insert(0, rule);
            SetContent();
        }

        public EditableDumpContainer(T obj, bool failSilently = false)
        {
            if (obj.GetType().GetArrayLikeElementType() != null)
                throw new Exception("You must Dump enumerable-like objects with the DumpEnumerable overload.");

            if (EditableDumpContainer.DefaultOptions.AutomaticallyKeepQueryRunning)
            {
                KeepRunningToken = Util.KeepRunning();
                EditableDumpContainer.KeepRunningTokens.Add(KeepRunningToken);
            }

            _obj = obj;
            _failSilently = failSilently;

            SetContent();
        }


        public EditableDumpContainer(IEnumerable<T> obj, bool failSilently = false)
        {
            if (EditableDumpContainer.DefaultOptions.AutomaticallyKeepQueryRunning)
            {
                KeepRunningToken = Util.KeepRunning();
                EditableDumpContainer.KeepRunningTokens.Add(KeepRunningToken);
            }

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
            var allRules = Enumerable.Concat(_editorRules, EditableDumpContainer.GlobalEditorRules);

            foreach (var editor in allRules)
                if (editor.Match(o, p))
                    return editor.Editor(o, p, () => p.GetValue(o), (v) =>
                    {
                        SetValue(o, p, v);
                        SetContent();

                        var newVal = p.GetValue(o);

                        if (_changeHandlers.TryGetValue(p, out var handler))
                            handler.Invoke((T)o,newVal);

                        OnPropertyValueChanged?.Invoke((T)o, p, newVal);

                        OnChanged?.Invoke();
                    });

            return p.GetValue(o);
        }

        public static void SetValue(object o, PropertyInfo p, object v)
        {
            if (o.GetType().IsAnonymousType())
                AnonymousObjectMutator.Set(o, p, v);
            else
                p.SetValue(o, v);
        }

        public new void Refresh()
        {
            SetContent();
            base.Refresh();
        }
    }

    public static class EditableDumpContainer
    {
        public static readonly CompositeDisposable KeepRunningTokens = new CompositeDisposable();
        public static DumpEditableOptions DefaultOptions = DumpEditableOptions.Defaults;
        public static EditableDumpContainer<T> For<T>(T obj, bool failSilently = false)
            => new EditableDumpContainer<T>(obj, failSilently);

        public static EditableDumpContainer<T> ForEnumerable<T>(IEnumerable<T> obj, bool failSilently = false)
            => new EditableDumpContainer<T>(obj, failSilently);

        public static void AddGlobalEditorRule(EditorRule rule)
            => GlobalEditorRules.Insert(0, rule);

        internal static readonly List<EditorRule> GlobalEditorRules = GetDefaultEditors();
        private static List<EditorRule> GetDefaultEditors() =>
            new List<EditorRule>
            {
                EditorRule.ForEnums(),
                EditorRule.ForBool(),
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
                EditorRule.ForTypeWithStringBasedEditor<byte>(byte.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<sbyte>(sbyte.TryParse),
                EditorRule.ForTypeWithStringBasedEditor<char>(char.TryParse),
                EditorRule.ForExpansionAttribute(),
                EditorRule.ForNestedAnonymousType()
            };

    }
}