<Query Kind="Program">
  <Reference Relative="..\..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// Anonymous type properties can be expanded automatically
	var anonyPet = new 
	{
		Name = "King",
		Kind = AnimalKind.Doggo,
		DateOfBirth = DateTime.Today.AddYears(-10),
		IsFirstPet = true,
		FavouriteActivities = new [] { "eating", "sleeping", "saving the world" },
		InnerObject = new {
			InnerPropertyA = "A",
			InnerProperyB = 12
		}
	}.DumpEditable(out var editor);
}

public enum AnimalKind
{
	Doggo,
	Cat,
	Birb
}