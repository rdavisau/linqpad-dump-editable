<Query Kind="Program">
  <Reference Relative="..\..\..\..\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll">C:\Users\rdavis\Source\Repos\linqpad-dump-editable\src\DumpEditable\bin\Debug\net47\LINQPad.DumpEditable.dll</Reference>
  <Namespace>LINQPad.DumpEditable</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
	// basic loan balance projector
	// this one stresses LINQPad a bit	
	var loanConfig = new LoanConfig
	{
		InitialBalance = 750000,
		Years = 30,
		Rate = 0.065,
	};

	var ed = EditableDumpContainer.For(loanConfig);
	var output = new DumpContainer();
	
	Util.HorizontalRun(true, ed, output).Dump();
	
	ed.AddEditorRule(
		EditorRule.For((o, p) => p.Name == nameof(loanConfig.InitialBalance),
					   Editors.Slider(250000.0, 5000000.0, x => x, x => x),
					   true));

	ed.AddEditorRule(
		EditorRule.For((o, p) => p.Name == nameof(loanConfig.Years),
					   Editors.Slider(1, 50),
					   true));

	ed.AddEditorRule(
		EditorRule.For((o, p) => p.Name == nameof(loanConfig.Rate),
					   Editors.Slider(0.025, 0.15, x => x * 100, x => x / 100.0),
					   true));
	
	ed.OnChanged += () =>
	{
		var bals = GetBalanceSchedule(loanConfig);
		var chart = Util.Chart(bals, x => x.period, x => x.balance, LINQPad.Util.SeriesType.Line).ToBitmap();

		// since the sliders are multi-threaded this can fail sometimes
		try { output.Content = chart; } catch { }
	};
	
	ed.OnChanged();
}

public List<(int period, double balance)> GetBalanceSchedule(LoanConfig loanConfig)
{
	var freqMultiplier = 
		loanConfig.RepaymentFrequency == RepaymentFrequency.Fortnightly 
		? 26 
		: 12;
	
	var balance = loanConfig.InitialBalance;
	var periods = loanConfig.Years * freqMultiplier;
	var rate = loanConfig.Rate / freqMultiplier;
	var payment = (rate / (1 - (Math.Pow((1 + rate), -(periods))))) * balance;

	return Enumerable
		.Range(0, periods)
		.Select(i =>
		{ 
			var interestForMonth = balance * rate;
			var principalForMonth = payment - interestForMonth;

			balance += interestForMonth;
			balance -= payment;

			return (i, Math.Round(balance,0) );
		})
		.ToList();
}

public class LoanConfig
{
	public double InitialBalance { get; set; }
	public int Years { get; set; }
	public double Rate { get; set; }
	public RepaymentFrequency RepaymentFrequency { get; set; }
}

public enum RepaymentFrequency
{
	Fortnightly,
	Monthly,
}