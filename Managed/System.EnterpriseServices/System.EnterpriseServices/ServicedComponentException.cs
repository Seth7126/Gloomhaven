using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>The exception that is thrown when an error is detected in a serviced component.</summary>
[Serializable]
[ComVisible(false)]
public sealed class ServicedComponentException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ServicedComponentException" /> class.</summary>
	public ServicedComponentException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ServicedComponentException" /> class with a specified error message.</summary>
	/// <param name="message">The message displayed to the client when the exception is thrown. </param>
	public ServicedComponentException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ServicedComponentException" /> class.</summary>
	/// <param name="message">The message displayed to the client when the exception is thrown. </param>
	/// <param name="innerException">The <see cref="P:System.Exception.InnerException" />, if any, that threw the current exception. </param>
	public ServicedComponentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
