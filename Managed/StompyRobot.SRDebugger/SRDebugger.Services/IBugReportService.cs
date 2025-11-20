using System;

namespace SRDebugger.Services;

public interface IBugReportService
{
	bool IsUsable { get; }

	void SetHandler(IBugReporterHandler handler);

	void SendBugReport(BugReport report, BugReportCompleteCallback completeHandler, IProgress<float> progressCallback = null);
}
