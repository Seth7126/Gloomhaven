using System.Runtime.InteropServices;

namespace Sony.NP;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public class RequestBase
{
	internal int serviceType;

	internal int functionType;

	internal uint serviceLabel;

	internal Core.UserServiceUserId userId;

	[MarshalAs(UnmanagedType.I1)]
	internal bool async = true;

	internal uint padding = 1234u;

	public ServiceTypes ServiceType => Compatibility.ConvertServiceToEnum(serviceType);

	public FunctionTypes FunctionType => Compatibility.ConvertFunctionToEnum(functionType);

	public uint ServiceLabel
	{
		get
		{
			return serviceLabel;
		}
		set
		{
			serviceLabel = value;
		}
	}

	public Core.UserServiceUserId UserId
	{
		get
		{
			return userId;
		}
		set
		{
			userId = value;
		}
	}

	public bool Async
	{
		get
		{
			return async;
		}
		set
		{
			async = value;
		}
	}

	internal RequestBase(ServiceTypes serviceType, FunctionTypes functionType)
	{
		userId.id = -1;
		this.serviceType = Compatibility.ConvertFromEnum(serviceType);
		this.functionType = Compatibility.ConvertFromEnum(functionType);
	}

	internal static void FinaliseRequest(RequestBase request, ResponseBase response, int npRequestId)
	{
		if (!request.async)
		{
			response.PopulateFromNative((uint)npRequestId, request.FunctionType, request);
			return;
		}
		PendingAsyncRequestList.AddRequest((uint)npRequestId, request);
		PendingAsyncResponseList.AddResponse((uint)npRequestId, response);
		response.UpdateAsyncState((uint)npRequestId, request.FunctionType);
	}
}
