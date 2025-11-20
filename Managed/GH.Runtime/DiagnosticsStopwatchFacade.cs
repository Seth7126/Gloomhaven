using System.Diagnostics;

public class DiagnosticsStopwatchFacade
{
	private readonly IDiagnosticsLogger _diagnosticsLogger;

	private readonly Stopwatch _stopwatch;

	public DiagnosticsStopwatchFacade(IDiagnosticsLogger diagnosticsLogger)
	{
		_diagnosticsLogger = diagnosticsLogger;
		_stopwatch = new Stopwatch();
	}

	public void Start()
	{
		_stopwatch.Start();
	}

	public double StopAndLog(string logObject)
	{
		if (!_stopwatch.IsRunning)
		{
			_diagnosticsLogger.LogError("Stopwatch wasn't running!");
			return 0.0;
		}
		_stopwatch.Stop();
		double totalSeconds = _stopwatch.Elapsed.TotalSeconds;
		_diagnosticsLogger.LogWarning($"{logObject} elapsed {totalSeconds} seconds.");
		_stopwatch.Reset();
		return totalSeconds;
	}
}
