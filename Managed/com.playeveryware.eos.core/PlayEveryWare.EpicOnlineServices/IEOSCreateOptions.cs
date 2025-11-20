using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSCreateOptions
{
	IntPtr Reserved { get; set; }

	Utf8String ProductId { get; set; }

	Utf8String SandboxId { get; set; }

	ClientCredentials ClientCredentials { get; set; }

	bool IsServer { get; set; }

	Utf8String EncryptionKey { get; set; }

	Utf8String OverrideCountryCode { get; set; }

	Utf8String OverrideLocaleCode { get; set; }

	Utf8String DeploymentId { get; set; }

	PlatformFlags Flags { get; set; }

	Utf8String CacheDirectory { get; set; }

	uint TickBudgetInMilliseconds { get; set; }
}
