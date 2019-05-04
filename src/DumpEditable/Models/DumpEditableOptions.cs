using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.DumpEditable.Models
{
    public class DumpEditableOptions
    {
        public static DumpEditableOptions Defaults => new DumpEditableOptions
        {
            AutomaticallyKeepQueryRunning = true,
            FailSilently = false,
            OptionsEditor = Editors.ChoicesWithRadioButtons,
            StringBasedEditor = Editors.TextBoxBasedStringEditor(false),
        };

        public bool AutomaticallyKeepQueryRunning { get; set; }
        public bool FailSilently { get; set; }

        public Func<IEnumerable<object>, bool, Func<object,string>, Func<object, PropertyInfo, Func<object>, Action<object>, object>> OptionsEditor;
        public Func<EditorRule.ParseFunc<string, object, bool>, bool, bool, Func<object, PropertyInfo, Func<object>, Action<object>, object>> StringBasedEditor;
    }
}
