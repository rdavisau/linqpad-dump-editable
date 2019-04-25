<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
</Query>

void Main()
{
	// you can respond to changes being made to your object by taking a reference to the EditableDumpContainer 
	// it is available as an out parameter of the DumpEditable method, or you can create a EditableDumpContainer by hand
	var pet = new Pet
	{
		Name = "King",
		Kind = AnimalKind.Doggo,
		DateOfBirth = DateTime.Today.AddYears(-10),
		IsFirstPet = true,
		FavouriteActivities = { "eating", "sleeping", "saving the world" },
	}.DumpEditable(out var editor); // <-- here we get a reference to the editor

	// if you don't care what changed, just that something changed, you can implement OnChanged
	editor.OnChanged += () => "A change occurred!".Dump();
	
	// if you want to know about property changes and values, you can implement OnPropertyValueChanged, which gives you the object, propertyinfo and new value
	editor.OnPropertyValueChanged += (o, p, v) => Util.HorizontalRun(true, $"The value for '{p.Name}' on '{o}' changed to: ", v).Dump();

	// if you want strongly-typed notifications for specific property changes, you can use AddChangeHandler
	editor.AddChangeHandler(x => x.FavouriteActivities, (o, activities) => $"{nameof(Pet.FavouriteActivities)} changed and the first favourite activity is now '{activities.First()}'".Dump());
	
	// as you'll see, change notifications are fired in order of specificity, most specific to least
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