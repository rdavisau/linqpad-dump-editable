using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad.DumpEditable.Models
{
    public class DumpEditableOptions
    {
        public static DumpEditableOptions Defaults => new DumpEditableOptions();
        public bool AutomaticallyKeepQueryRunning { get; set; } = true;
    }
}
