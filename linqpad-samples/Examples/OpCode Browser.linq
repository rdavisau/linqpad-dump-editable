<Query Kind="Program">
  <Reference Relative="..\..\..\..\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Reflection.Emit</Namespace>
</Query>

void Main()
{
	EditableDumpContainer.DefaultOptions.StringBasedEditor = 
		Editors.TextBoxBasedStringEditor(liveUpdates: true);

	var filter = new OpCodeFilter().DumpEditable(out var editor);
	var opcodes = new DumpContainer().Dump();
	var selectedOpcode = new DumpContainer() { DumpDepth = 2 }.Dump();
	
	var filteredOpcodes = 
		AllOpCodes
			.Where(filter.Matches)
			.Select(o => new Hyperlinq(() => selectedOpcode.Content = o, o.Name));

	editor.OnChanged += () => opcodes.Content = Util.HorizontalRun(true, filteredOpcodes);
	editor.OnChanged();
}

public IEnumerable<OpCode> AllOpCodes
	=> typeof(OpCodes)
			.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Default)
			.Select(f => (OpCode) f.GetValue(null));
			
public class OpCodeFilter
{
	public string Name { get; set; }
	public OpCodeType? OpCodeType { get; set; }
	public FlowControl? FlowControl { get; set; }
	public OperandType? OperandType{ get; set; }
	
	public bool Matches(OpCode o)
		=> (String.IsNullOrWhiteSpace(Name) || o.Name.ToLower().Contains(Name.ToLower()))
		&& (OpCodeType is null || o.OpCodeType == OpCodeType)
		&& (FlowControl is null || o.FlowControl == FlowControl)
		&& (OperandType is null || o.OperandType == OperandType);
}