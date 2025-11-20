namespace PlayEveryWare.EpicOnlineServices;

public class NotifyEventHandle : GenericSafeHandle<ulong>
{
	public delegate void RemoveDelegate(ulong aHandle);

	private RemoveDelegate removeDelegate;

	public NotifyEventHandle(ulong aLong, RemoveDelegate aRemoveDelegate)
		: base(aLong)
	{
		removeDelegate = aRemoveDelegate;
	}

	protected override void ReleaseHandle()
	{
		if (IsValid())
		{
			removeDelegate(handleObject);
			handleObject = 0uL;
		}
	}

	public override bool IsValid()
	{
		return handleObject != 0;
	}
}
