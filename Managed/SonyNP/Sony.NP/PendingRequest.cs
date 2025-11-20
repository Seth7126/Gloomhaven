namespace Sony.NP;

public class PendingRequest
{
	internal uint npRequestId;

	internal RequestBase request;

	internal bool abortPending;

	public uint NpRequestId => npRequestId;

	public RequestBase Request => request;

	public bool AbortPending => abortPending;
}
