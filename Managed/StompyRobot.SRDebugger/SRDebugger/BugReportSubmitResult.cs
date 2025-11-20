namespace SRDebugger;

public sealed class BugReportSubmitResult
{
	public static BugReportSubmitResult Success => new BugReportSubmitResult(successful: true, null);

	public bool IsSuccessful { get; }

	public string ErrorMessage { get; }

	public static BugReportSubmitResult Error(string errorMessage)
	{
		return new BugReportSubmitResult(successful: false, errorMessage);
	}

	private BugReportSubmitResult(bool successful, string error)
	{
		IsSuccessful = successful;
		ErrorMessage = error;
	}
}
