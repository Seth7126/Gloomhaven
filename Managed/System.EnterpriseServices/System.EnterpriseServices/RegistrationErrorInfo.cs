using Unity;

namespace System.EnterpriseServices;

/// <summary>Retrieves extended error information about methods related to multiple COM+ objects. This also includes methods that install, import, and export COM+ applications and components. This class cannot be inherited.</summary>
[Serializable]
public sealed class RegistrationErrorInfo
{
	private int errorCode;

	private string errorString;

	private string majorRef;

	private string minorRef;

	private string name;

	/// <summary>Gets the error code for the object or file.</summary>
	/// <returns>The error code for the object or file.</returns>
	public int ErrorCode => errorCode;

	/// <summary>Gets the description of the <see cref="P:System.EnterpriseServices.RegistrationErrorInfo.ErrorCode" />.</summary>
	/// <returns>The description of the <see cref="P:System.EnterpriseServices.RegistrationErrorInfo.ErrorCode" />.</returns>
	public string ErrorString => errorString;

	/// <summary>Gets the key value for the object that caused the error, if applicable.</summary>
	/// <returns>The key value for the object that caused the error, if applicable.</returns>
	public string MajorRef => majorRef;

	/// <summary>Gets a precise specification of the item that caused the error, such as a property name.</summary>
	/// <returns>A precise specification of the item, such as a property name, that caused the error. If multiple errors occurred, or this does not apply, <see cref="P:System.EnterpriseServices.RegistrationErrorInfo.MinorRef" /> returns the string "&lt;Invalid&gt;".</returns>
	public string MinorRef => minorRef;

	/// <summary>Gets the name of the object or file that caused the error.</summary>
	/// <returns>The name of the object or file that caused the error.</returns>
	public string Name => name;

	[System.MonoTODO]
	internal RegistrationErrorInfo(string name, string majorRef, string minorRef, int errorCode)
	{
		this.name = name;
		this.majorRef = majorRef;
		this.minorRef = minorRef;
		this.errorCode = errorCode;
	}

	internal RegistrationErrorInfo()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
