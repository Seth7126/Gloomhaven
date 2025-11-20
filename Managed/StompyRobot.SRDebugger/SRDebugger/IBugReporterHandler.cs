using System;

namespace SRDebugger;

public interface IBugReporterHandler
{
	bool IsUsable { get; }

	void Submit(BugReport report, Action<BugReportSubmitResult> onComplete, IProgress<float> progress);
}
