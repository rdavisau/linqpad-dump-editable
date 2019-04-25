<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// DumpEditable lets you modify anonymous types (even though anonymous types are read-only)
	// See how we can update this anonyPet that looks a lot like Pet but is really an anonymous type
	var anonyPet = new 
	{
		Name = "King",
		Kind = AnimalKind.Doggo,
		DateOfBirth = DateTime.Today.AddYears(-10),
		IsFirstPet = true,
		FavouriteActivities = new [] { "eating", "sleeping", "saving the world" },
	}.DumpEditable(out var editor);

	editor.AddChangeHandler(x => x.Kind, (p, v) => $"AnonyPet just turned into a {v}!".Dump());
	
	// Modifying anonymous types is Fine for the kind of things you're likely to use them for with DumpEditable
	// but keep in mind that Bad Things will probably happen if you modify instances of anonymous types that 
	// end up in Dictionaries, HashSets, etc.
}

public enum AnimalKind
{
	Doggo,
	Cat,
	Birb
}