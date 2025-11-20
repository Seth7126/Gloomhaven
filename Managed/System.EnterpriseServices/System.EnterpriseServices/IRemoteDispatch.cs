using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Implemented by the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class to determine if the <see cref="T:System.EnterpriseServices.AutoCompleteAttribute" /> class attribute is set to true or false for a remote method invocation.</summary>
[Guid("6619a740-8154-43be-a186-0319578e02db")]
public interface IRemoteDispatch
{
	/// <summary>Ensures that, in the COM+ context, the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class object's done bit is set to true after a remote method invocation.</summary>
	/// <returns>A string converted from a response object that implements the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface.</returns>
	/// <param name="s">A string to be converted into a request object that implements the <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> interface.</param>
	[AutoComplete]
	string RemoteDispatchAutoDone(string s);

	/// <summary>Does not ensure that, in the COM+ context, the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class object's done bit is set to true after a remote method invocation.</summary>
	/// <returns>A string converted from a response object implementing the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface.</returns>
	/// <param name="s">A string to be converted into a request object implementing the <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> interface.</param>
	[AutoComplete(false)]
	string RemoteDispatchNotAutoDone(string s);
}
