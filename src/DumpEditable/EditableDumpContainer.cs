using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            => _editorRules.Insert(0, rule);

        public EditableDumpContainer(T obj, bool failSilently = false)
        {
            _obj = obj;
            _failSilently = failSilently;

            SetContent();
        }
        
        private void SetContent()
        {
            object content = _obj;

            try
            {
                content =
                    _obj
                        .GetType()
                        .GetProperties()
                        .Select(p =>
                            new
                            {
                                Property = p.Name,
                                Value = GetEditor(_obj, p)
                            })
                        .ToList();
            }
            catch
            {
                if (!_failSilently)
                    throw;
            }

            Content = content;
        }

        private object GetEditor(object o, PropertyInfo p)
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

                        OnPropertyValueChanged?.Invoke(_obj, p, newVal);

                        OnChanged?.Invoke();;
                    });

            return o;
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
                EditorRule.ForTypeWithStringBasedEditor((string input, out string output) =>
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