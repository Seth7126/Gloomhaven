using System.Runtime.InteropServices;

namespace System.Net;

internal class NegotiationInfoClass
{
	internal string AuthenticationPackage;

	internal const string NTLM = "NTLM";

	internal const string Kerberos = "Kerberos";

	internal const string Negotiate = "Negotiate";

	internal const string Basic = "Basic";

	internal unsafe NegotiationInfoClass(SafeHandle safeHandle, int negotiationState)
	{
		if (safeHandle.IsInvalid)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Invalid handle:{safeHandle}", ".ctor");
			}
			return;
		}
		IntPtr intPtr = safeHandle.DangerousGetHandle();
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, $"packageInfo:{intPtr} negotiationState:{negotiationState:x}", ".ctor");
		}
		if (negotiationState == 0 || negotiationState == 1)
		{
			string text = null;
			IntPtr name = ((SecurityPackageInfo*)(void*)intPtr)->Name;
			if (name != IntPtr.Zero)
			{
				text = Marshal.PtrToStringUni(name);
			}
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"packageInfo:{intPtr} negotiationState:{negotiationState:x} name:{text}", ".ctor");
			}
			if (string.Compare(text, "Kerberos", StringComparison.OrdinalIgnoreCase) == 0)
			{
				AuthenticationPackage = "Kerberos";
			}
			else if (string.Compare(text, "NTLM", StringComparison.OrdinalIgnoreCase) == 0)
			{
				AuthenticationPackage = "NTLM";
			}
			else
			{
				AuthenticationPackage = text;
			}
		}
	}
}
