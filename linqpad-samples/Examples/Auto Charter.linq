<Query Kind="Program">
  <Reference Relative="..\..\..\..\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms.DataVisualization.Charting</Namespace>
  <Namespace>LINQPad.DumpEditable</Namespace>
</Query>

async Task Main()
{
	var charter = GetCharter();
	
	var data = new[]
	{
		new { Series = 'A', Value = 0.0 },
		new { Series = 'B', Value = 1.0 },
		new { Series = 'C', Value = 2.0 },
	}.ToList();
	
	var editor = EditableDumpContainer.ForEnumerable(data);
	var chart = new DumpContainer();
	
	Util.VerticalRun(
		Util.Metatext("Update the values of the series to change the values being plotted"),	
		Util.Metatext(Util.IsDarkThemeEnabled ? "" : "(This probably looks better in dark theme)"),
		Util.HorizontalRun(true, chart,
			Util.VerticalRun(
			new Hyperlinq(() =>
			{
				data.Add(new { Series = (char)(data.Last().Series + 1), Value = data.Last().Value + 1 });
				editor.Refresh();
			}, "add series"),
			editor))
	).Dump();
	
	Bitmap last = null;
	while(true)
	{
		await Task.Delay(TimeSpan.FromSeconds(.25));

		chart.Content = charter(data.Select(x => ($"{x.Series}", x.Value)));
		last?.Dispose();
		last = (Bitmap)(chart.Content);
	}
}

public Func<IEnumerable<(string series, double val)>, Bitmap> GetCharter()
{
	var chart = new Chart();
	var ca = chart.ChartAreas.Add("ca");
	chart.Width = (1920 / 2);
	chart.Height = (1080 / 2);

	if (Util.IsDarkThemeEnabled)
		StyleDark(chart, ca);
	else 
		StyleLight(chart, ca);
	
	return vs =>
	{
		foreach (var v in vs)
		{
			var series = chart.Series.FirstOrDefault(x => x.Name == v.series) ?? chart.Series.Add(v.series);
			series.ChartType = SeriesChartType.FastLine;
			series.BorderWidth = Util.IsDarkThemeEnabled ? 1 : 3;
			
			series.Points.AddXY(DateTime.Now.ToOADate(), v.val);
		}
		
		var ms = new MemoryStream();
		chart.SaveImage(ms, ChartImageFormat.Png);
		ms.Seek(0, SeekOrigin.Begin);

		return new Bitmap(ms);
	};
}

public void StyleDark(Chart chart, ChartArea ca)
{
	chart.BackColor = Color.FromArgb(255, 30, 30, 30);

	ca.BackColor = Color.FromArgb(255, 32, 32, 32);
	ca.AxisY.Minimum = 0.0;
	ca.AxisY.LineColor = Color.LightGray;
	ca.AxisX.LineColor = Color.LightGray;
	ca.AxisY.LabelStyle.ForeColor = Color.LightGray;
	ca.AxisX.LabelStyle.ForeColor = Color.LightGray;
	ca.AxisY.LabelStyle.Format = "";
	ca.AxisX.LabelStyle.Enabled = false;
	ca.AxisY.MajorGrid.Enabled = false;
	ca.AxisY.MinorGrid.Enabled = false;
	ca.AxisX.MajorGrid.Enabled = false;
	ca.AxisX.MinorGrid.Enabled = false;
}

public void StyleLight(Chart chart, ChartArea ca)
{
	ca.BackColor = Color.FromArgb(255, 220, 220, 220);
	ca.AxisY.Minimum = 0.0;
	ca.AxisY.LabelStyle.Format = "";
	ca.AxisX.LabelStyle.Enabled = false;
	ca.AxisY.MajorGrid.Enabled = false;
	ca.AxisY.MinorGrid.Enabled = false;
	ca.AxisX.MajorGrid.Enabled = false;
	ca.AxisX.MinorGrid.Enabled = false;
}