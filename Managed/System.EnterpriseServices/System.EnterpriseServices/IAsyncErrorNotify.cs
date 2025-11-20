using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Implements error trapping on the asynchronous batch work that is submitted by the <see cref="T:System.EnterpriseServices.Activity" /> object.</summary>
[ComImport]
[Guid("FE6777FB-A674-4177-8F32-6D707E113484")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IAsyncErrorNotify
{
	/// <summary>Handles errors for asynchronous batch work.</summary>
	/// <param name="hresult">The HRESULT of the error that occurred while the batch work was running asynchronously. </param>
	void OnError(int hresult);
}
