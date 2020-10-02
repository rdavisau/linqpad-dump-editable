<Query Kind="Program">
  <Reference Relative="..\..\..\..\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows</Namespace>
</Query>

async Task Main()
{
	// this demo shows using an EditableDumpContainer to trial-an-error cutting up of a spritesheet
	var url = "http://tsgk.captainn.net/dld.php?s=custom&f=xander_marioandluigi_sheet.png";
	var bs = await new HttpClient().GetByteArrayAsync(url);
	var img = new Bitmap(new MemoryStream(bs));
	var (w,h) = (img.Width, img.Height);
	
	var config = new SpriteCuttingConfig
	{
		XOffset = 10,
		YOffset = 0,
		SpriteWidth = 30,
		SpriteHeight = 39,
		Rows = 6,
		Cols = 8,
	};	
		
	var editor = EditableDumpContainer.For(config);
	var output = new DumpContainer().Dump();
	
	editor.OnChanged += () =>
	{
		var width = config.SpriteWidth * config.Cols;
		var height = config.SpriteHeight * config.Rows;

		var spritesArea = img.CropAtRect(new Rectangle(config.XOffset, config.YOffset, width, height));
	    var spriteRects = new List<Rectangle>();
		
		var sprites = 
			Enumerable
				.Range(0, config.Rows)
				.SelectMany(y => Enumerable.Range(0, config.Cols),
					(y, x) => 
						Util.VerticalRun(
							spritesArea.CropAtRect(
								new Rectangle(
									x * config.SpriteWidth,
									y * config.SpriteHeight,
									config.SpriteWidth,
									config.SpriteHeight).DoSingle(spriteRects.Add)
						), $"{x},{y}"))
				.ToList();
				
		var regionsInOriginalSpace = 
			spriteRects
				.Select(r => new Rectangle(r.X + config.XOffset, r.Y + config.YOffset, r.Width, r.Height))
				.ToList();
		
		var text = config.IsCorrect() ? GoodText : BadText;
		var color = config.IsCorrect() ? Color.Green : Color.Red;
		var display = new Bitmap(img).MarkRegions(regionsInOriginalSpace, color);
		editor.Refresh();
		output.Content = Util.VerticalRun(Util.Metatext(text),
										  Util.HorizontalRun(true, editor, display), 
										  "", "", Util.Metatext("Results:"), "",
										  Util.HorizontalRun(true, sprites)); 
	};
	
	editor.OnChanged();
}

public class SpriteCuttingConfig
{
	public short XOffset { get; set; }
	public short YOffset { get; set; }
	public short SpriteWidth { get; set; }
	public short SpriteHeight { get; set; }
	public short Rows { get; set; }
	public short Cols { get; set; }
}

const string BadText =
	"Our sprite sizes seem to be ok, but we're missing some sprites!\r\n" +
	"Maybe we need to fiddle with the X Offset and Rows/Cols a little...\r\n\r\n";

const string GoodText =
	"Great! You did it!\r\n\r\n";

public static class Ext
{
	public static bool IsCorrect(this SpriteCuttingConfig c)
		=> c.XOffset == 30 && c.YOffset == 0
		 && c.SpriteWidth == 30 && c.SpriteHeight == 39
		 && c.Rows == 9 && c.Cols == 12;
	
	public static Bitmap CropAtRect(this Bitmap b, Rectangle r)
	{
		Bitmap nb = new Bitmap(r.Width, r.Height);
		Graphics g = Graphics.FromImage(nb);
		g.DrawImage(b, -r.X, -r.Y);
		return nb;
	}
	
	public static Bitmap MarkRegions(this Bitmap b, List<Rectangle> regions, Color color)
	{
		var p = new Pen(color);
		
		using (var g = Graphics.FromImage(b))
			foreach (var r in regions)
				g.DrawRectangle(p, r);

		return b;
	}
	
	public static T DoSingle<T>(this T item, Action<T> action)
	{
		action(item);
		return item;
	}
	
	public static IEnumerable<T> Do<T>(this IEnumerable<T> items, Action<T> action)
	{
		foreach (var item in items)
		{
			action(item);
			yield return item;
		}
	}
}
