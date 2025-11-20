namespace Sony.PS4.SaveData;

public class PendingRequest
{
	internal int requestId;

	internal RequestBase request;

	internal ResponseBase response;

	internal bool abortPending = false;

	public int NpRequestId => requestId;

	public RequestBase Request => request;

	public bool AbortPending => abortPending;
}
