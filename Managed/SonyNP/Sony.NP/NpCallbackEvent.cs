namespace Sony.NP;

public class NpCallbackEvent
{
	internal ServiceTypes service;

	internal FunctionTypes apiCalled;

	internal uint npRequestId;

	internal ResponseBase response;

	internal Core.UserServiceUserId userId;

	internal RequestBase request;

	public ServiceTypes Service => service;

	public FunctionTypes ApiCalled => apiCalled;

	public uint NpRequestId => npRequestId;

	public ResponseBase Response => response;

	public RequestBase Request => request;

	public Core.UserServiceUserId UserId => userId;
}
