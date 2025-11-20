using System.Runtime.InteropServices;
using System.Text;

namespace Sony.NP;

public class ServerErrorManaged
{
	private const int JSON_DATA_MAX_LEN = 512;

	internal byte[] jsonData = new byte[512];

	internal long webApiNextAvailableTime = 0L;

	internal int httpStatusCode = 0;

	public string JsonData => Encoding.UTF8.GetString(jsonData, 0, jsonData.Length);

	public long WebApiNextAvailableTime => webApiNextAvailableTime;

	public int HttpStatusCode => httpStatusCode;

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxReadServerError(uint responseId, int apiCalled, out long webApiNextAvailableTime, out int httpStatusCode, [In][Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 512)] byte[] jsonData, out APIResult result);

	internal void ReadResult(uint unqiueId, FunctionTypes apiCalled)
	{
		int apiCalled2 = Compatibility.ConvertFromEnum(apiCalled);
		PrxReadServerError(unqiueId, apiCalled2, out webApiNextAvailableTime, out httpStatusCode, jsonData, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
	}
}
