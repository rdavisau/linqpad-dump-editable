<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// the simplest demonstration is the dumping of a plain old C# object
	// like LINQPad's Dump, DumpEditable returns the original object so can typically be chained in the same manner
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

	public Guid Tag { get; set; } = Guid.NewGuid();
}

public enum AnimalKind
{
	Doggo,
	Cat,
	Birb
}