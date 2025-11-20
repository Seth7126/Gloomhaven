using System.IO;
using Mono.Net.Security;

namespace Mono.Security.Interface;

public static class CertificateValidationHelper
{
	private const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

	private static readonly bool noX509Chain;

	private static readonly bool supportsTrustAnchors;

	public static bool SupportsX509Chain => !noX509Chain;

	public static bool SupportsTrustAnchors => supportsTrustAnchors;

	static CertificateValidationHelper()
	{
		if (File.Exists("/System/Library/Frameworks/Security.framework/Security"))
		{
			noX509Chain = true;
			supportsTrustAnchors = true;
		}
		else
		{
			noX509Chain = false;
			supportsTrustAnchors = false;
		}
	}

	public static ICertificateValidator GetValidator(MonoTlsSettings settings)
	{
		return (ICertificateValidator)NoReflectionHelper.GetDefaultValidator(settings);
	}
}
