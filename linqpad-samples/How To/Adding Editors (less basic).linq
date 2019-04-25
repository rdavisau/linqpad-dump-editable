<Query Kind="Program">
  <Reference Relative="..\..\..\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\source\repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	// You can add an editor implementation from scratch to have more influence over matching and input.
	var rule = EditorRule.For(
		rule: (obj, prop) => // return true from here if this editor should be used 
		{
			// here we _only_ want to be used for the FavouriteFood property on pet
			return obj is Pet && prop.Name == nameof(Pet.FavouriteFood);
		},
		getEditor: (obj, prop, getVal, setVal) => // return the object that should be displayed for the property and that can take input
		{
			// here we limit the user to three specific values designated by the images
			var foods = new []
			{
				("pasta", "https://image.flaticon.com/icons/png/128/123/123315.png"),
				("pizza", "http://icons.iconarchive.com/icons/sonya/swarm/128/Pizza-icon.png"),
				("ice cream", "https://img.icons8.com/dusk/2x/ice-cream-cone.png")
			};

			return 
				Util.VerticalRun(getVal(), // <- we can get the current value using getVal()
				Util.HorizontalRun(true,
					foods.Select(f => Util.VerticalRun(
							Util.Image(f.Item2), 
							new Hyperlinq(() => setVal(f.Item1), // <- we use setVal to update the property.
																 // we should use setVal rather than reflection on obj/prop directly
																 // because setVal handles things like modifying anonymous types
																 // and refreshing the editor for us. 
										"  - select -  ")))));
		});
	
	EditableDumpContainer.AddGlobalEditorRule(rule);
	
	new Pet
	{
		Name = "Rover",
		FavouriteFood = "pizza"
	}
	.DumpEditable();
}


public class Pet
{
	public string Name { get; set; }
	public string FavouriteFood { get; set; }
}
