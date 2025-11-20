namespace System.Net.Security;

internal sealed class SafeDeleteContext_SECURITY : SafeDeleteContext
{
	internal SafeDeleteContext_SECURITY()
	{
	}

	protected override bool ReleaseHandle()
	{
		if (_EffectiveCredential != null)
		{
			_EffectiveCredential.DangerousRelease();
		}
		return global::Interop.SspiCli.DeleteSecurityContext(ref _handle) == 0;
	}
}
