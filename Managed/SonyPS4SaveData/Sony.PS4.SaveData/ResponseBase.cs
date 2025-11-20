using System;

namespace Sony.PS4.SaveData;

public abstract class ResponseBase
{
	internal int returnCode;

	internal bool locked;

	internal Exception exception = null;

	public int ReturnCodeValue
	{
		get
		{
			ThrowExceptionIfLocked();
			return returnCode;
		}
	}

	public ReturnCodes ReturnCode
	{
		get
		{
			ThrowExceptionIfLocked();
			return (ReturnCodes)returnCode;
		}
	}

	public Exception Exception
	{
		get
		{
			ThrowExceptionIfLocked();
			return exception;
		}
	}

	public bool Locked => locked;

	public bool IsErrorCode
	{
		get
		{
			ThrowExceptionIfLocked();
			if (returnCode < 0)
			{
				return true;
			}
			return false;
		}
	}

	internal bool IsErrorCodeWithoutLockCheck
	{
		get
		{
			if (returnCode < 0)
			{
				return true;
			}
			return false;
		}
	}

	internal ResponseBase()
	{
	}

	internal void Populate(APIResult result)
	{
		returnCode = result.sceErrorCode;
	}

	public string ConvertReturnCodeToString(FunctionTypes apiCalled)
	{
		ThrowExceptionIfLocked();
		string text = "(0x" + returnCode.ToString("X8") + ")";
		ReturnCodes returnCodes = (ReturnCodes)returnCode;
		if (Enum.IsDefined(typeof(ReturnCodes), returnCodes))
		{
			return text + " (" + returnCodes.ToString() + ") ";
		}
		return text + " (UNKNOWN) ";
	}

	internal void ThrowExceptionIfLocked()
	{
		if (locked)
		{
			throw new SaveDataException("This response object can't be read while it is waiting to be processed.");
		}
	}
}
