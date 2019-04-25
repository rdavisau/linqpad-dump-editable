<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// Previous samples showed adding 'global editors' which are used by any EditableDumpContainers
	// We can add editors to individual EditableDumpContainers too

	// create an editor that will be lucky enough to be able to edit guids
	"".Dump("will be able to edit guids");
	new { TheGuid = Guid.NewGuid() }.DumpEditable(out var editorThatWillHaveAGuidEditor);

	// create our favourite guid editor
	var guidEditor = EditorRule.ForTypeWithStringBasedEditor<Guid>(Guid.TryParse);

	// add it to a specific EditableDumpContainer
	editorThatWillHaveAGuidEditor.AddEditorRule(guidEditor);

	// create an editor that wont be able to edit guids
	"".Dump("wont be able to edit guids");
	new { TheGuid = Guid.NewGuid() }.DumpEditable(out var editorThatWillNotHaveAGuidEditor);
	
	// observe that our "will have" editor has an editor, but our "wont have" doesnt!
}