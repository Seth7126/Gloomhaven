using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public class RequestBase
{
	internal FunctionTypes functionType;

	internal int userId;

	[MarshalAs(UnmanagedType.I1)]
	internal bool async = true;

	[MarshalAs(UnmanagedType.I1)]
	internal bool locked;

	[MarshalAs(UnmanagedType.I1)]
	private bool ignoreCallback;

	internal uint padding = 1234u;

	public FunctionTypes FunctionType => functionType;

	public int UserId
	{
		get
		{
			return userId;
		}
		set
		{
			ThrowExceptionIfLocked();
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
			ThrowExceptionIfLocked();
			async = value;
		}
	}

	public bool Locked => locked;

	public bool IgnoreCallback
	{
		get
		{
			return ignoreCallback;
		}
		set
		{
			ThrowExceptionIfLocked();
			ignoreCallback = value;
		}
	}

	internal virtual bool IsDeferred => false;

	internal RequestBase(FunctionTypes functionType)
	{
		userId = -1;
		this.functionType = functionType;
	}

	internal virtual void Execute(PendingRequest pendingRequest)
	{
	}

	internal virtual bool ExecutePolling(PendingRequest completedRequest)
	{
		return false;
	}

	internal void ThrowExceptionIfLocked()
	{
		if (locked)
		{
			throw new SaveDataException("This request object can't be changed while it is waiting to be processed.");
		}
	}
}
