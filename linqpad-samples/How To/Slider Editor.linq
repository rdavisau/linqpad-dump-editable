<Query Kind="Statements">
  <Reference Relative="..\..\..\..\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

var c = new { Pt = 25, R = 150, G = 150, B = 150 }
	.DumpEditable(out var editor);

editor.AddEditorRule(
	EditorRule.ForType<int>(
		Editors.Slider(0, 255),
		true));

var dc = new DumpContainer().Dump();
editor.OnChanged += () =>
	dc.Content = Util.WithStyle("LABEL", 
		$"font-size:{c.Pt}px;" + 
		$"color:#{Color.FromArgb(255, c.R, c.G, c.B).Name.Substring(2)};");
		
editor.OnChanged(); 