using System;
using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

internal struct APIResult
{
	public APIResultTypes apiResult;

	private IntPtr _message;

	private IntPtr _filename;

	public int lineNumber;

	public int sceErrorCode;

	public string message => Marshal.PtrToStringAnsi(_message);

	public string filename => Marshal.PtrToStringAnsi(_filename);

	public bool RaiseException => apiResult != APIResultTypes.Success;
}
