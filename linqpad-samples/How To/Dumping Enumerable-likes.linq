<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// DumpEditable might do a reasonable job at dumping lists of items as well
	// use DumpEditableEnumerable instead of DumpEditable
	
	var animalKinds = Enum.GetValues(typeof(AnimalKind)).OfType<AnimalKind>().ToList();
	
	var anonyPets = 
		Enumerable
			.Range(0, 5)
			.Select(i => 
		new 
		{
			Name = $"King {i}",
			Kind = animalKinds[i % animalKinds.Count],
			DateOfBirth = DateTime.Today.AddYears(-(i + 1)),
			IsFirstPet = i == 0,
			FavouriteActivities = new [] { "eating", "sleeping", "saving the world", "more eating", "more sleeping" }.Take(i),
		})
		.ToList() // if you don't materialise an ienumerable you'll have a bad time 
		.DumpEditableEnumerable(out var editor);
		
	
}

public enum AnimalKind
{
	Doggo,
	Cat,
	Birb
}