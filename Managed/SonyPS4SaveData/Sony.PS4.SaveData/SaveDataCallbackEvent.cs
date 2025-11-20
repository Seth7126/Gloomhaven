namespace Sony.PS4.SaveData;

public class SaveDataCallbackEvent
{
	internal FunctionTypes apiCalled;

	internal int requestId;

	internal int userId;

	internal RequestBase request;

	internal ResponseBase response;

	public FunctionTypes ApiCalled => apiCalled;

	public int RequestId => requestId;

	public ResponseBase Response => response;

	public RequestBase Request => request;

	public int UserId => userId;
}
