<Query Kind="Program">
  <Reference Relative="..\..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>LINQPad.DumpEditable.Models</Namespace>
</Query>

async Task Main()
{
	// since I haven't worked out a great way to do it yet, you can manually add `DumpEditableExpand` 
	// to any complex property on a POCO that you'd like DumpEditable to recursively allow editing of
	var pet = new Pet
	{
		Name = "King",
		Kind = AnimalKind.Doggo,
		DateOfBirth = DateTime.Today.AddYears(-10),
		IsFirstPet = true,
		FavouriteActivities = { "eating", "sleeping", "saving the world" },
	}.DumpEditable();
}

public class Pet
{
	public string Name { get; set; }
	public AnimalKind Kind { get; set; }
	public DateTimeOffset DateOfBirth { get; set; }
	public bool? IsFirstPet { get; set; } = true;
	public List<string> FavouriteActivities { get; set; } = new List<string>();

	[DumpEditableExpand] // <- this makes it expand
	public PetTag WillBeEditableBecauseHasAttribute { get; set; } = new PetTag { };
	
	public PetTag WontBeEditableBecauseDoesNotHaveAtribute { get; set; } = new PetTag { };
}

public class PetTag
{
	public string Identifier { get; set; }
	public string Material { get; set; }
}

public enum AnimalKind
{
	Doggo,
	Cat,
	Birb
}