<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// You can add your own "editors" to DumpEditable by specifying a rule function and providing an implementation
	// EditorRule has some helpers or you can write one from scratch
	
	// note how we can't edit a guid because there's no editor implementation
	"".Dump("Can't edit, no guid editor implementation");
	new { TheGuid = Guid.NewGuid() }.DumpEditable();
	
	// let's add one
	// the string-based editor that most default items are based on has a an easy to use helper
	// you specify the type <T> and a 'parseFunc' that returns true/false depending on whether the string could be parsed
	// and has an out T parameter that you set to the result. This mirrors the 'TryParse' pattern found in across .NET BCL
	// so it's easy to add a Guid editor: 
	var guidEditor = EditorRule.ForTypeWithStringBasedEditor<Guid>(Guid.TryParse);
	
	// you can add a rule to the global rules
	EditableDumpContainer.AddGlobalEditorRule(guidEditor);

	// now we can edit a guid!
	"".Dump("Can edit, we added an implementation!");
	new { TheGuid = Guid.NewGuid() }.DumpEditable();
	
	// installing DumpEditable in your MyExtensions and adding rules there can make them available by default to all queries!
}