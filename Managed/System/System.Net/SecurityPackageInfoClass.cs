using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Net;

internal class SecurityPackageInfoClass
{
	internal int Capabilities;

	internal short Version;

	internal short RPCID;

	internal int MaxToken;

	internal string Name;

	internal string Comment;

	internal unsafe SecurityPackageInfoClass(SafeHandle safeHandle, int index)
	{
		if (safeHandle.IsInvalid)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Invalid handle: {safeHandle}", ".ctor");
			}
			return;
		}
		IntPtr intPtr = safeHandle.DangerousGetHandle() + sizeof(SecurityPackageInfo) * index;
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, $"unmanagedAddress: {intPtr}", ".ctor");
		}
		SecurityPackageInfo* ptr = (SecurityPackageInfo*)(void*)intPtr;
		Capabilities = ptr->Capabilities;
		Version = ptr->Version;
		RPCID = ptr->RPCID;
		MaxToken = ptr->MaxToken;
		IntPtr name = ptr->Name;
		if (name != IntPtr.Zero)
		{
			Name = Marshal.PtrToStringUni(name);
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Name: {Name}", ".ctor");
			}
		}
		name = ptr->Comment;
		if (name != IntPtr.Zero)
		{
			Comment = Marshal.PtrToStringUni(name);
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Comment: {Comment}", ".ctor");
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, ToString(), ".ctor");
		}
	}

	public override string ToString()
	{
		return "Capabilities:" + string.Format(CultureInfo.InvariantCulture, "0x{0:x}", Capabilities) + " Version:" + Version.ToString(NumberFormatInfo.InvariantInfo) + " RPCID:" + RPCID.ToString(NumberFormatInfo.InvariantInfo) + " MaxToken:" + MaxToken.ToString(NumberFormatInfo.InvariantInfo) + " Name:" + ((Name == null) ? "(null)" : Name) + " Comment:" + ((Comment == null) ? "(null)" : Comment);
	}
}
