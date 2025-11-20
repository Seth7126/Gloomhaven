using System;

namespace SRDebugger.Internal;

internal class InternalBugReporterHandler : IBugReporterHandler
{
	public bool IsUsable
	{
		get
		{
			if (Settings.Instance.EnableBugReporter)
			{
				return !string.IsNullOrWhiteSpace(Settings.Instance.ApiKey);
			}
			return false;
		}
	}

	public void Submit(BugReport report, Action<BugReportSubmitResult> onComplete, IProgress<float> progress)
	{
		BugReportApi.Submit(report, Settings.Instance.ApiKey, onComplete, progress);
	}
}
