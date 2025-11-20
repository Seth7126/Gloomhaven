using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

public class EOSWindowsOptions : IEOSCreateOptions
{
	public WindowsOptions options;

	IntPtr IEOSCreateOptions.Reserved
	{
		get
		{
			return options.Reserved;
		}
		set
		{
			options.Reserved = value;
		}
	}

	Utf8String IEOSCreateOptions.ProductId
	{
		get
		{
			return options.ProductId;
		}
		set
		{
			options.ProductId = value;
		}
	}

	Utf8String IEOSCreateOptions.SandboxId
	{
		get
		{
			return options.SandboxId;
		}
		set
		{
			options.SandboxId = value;
		}
	}

	ClientCredentials IEOSCreateOptions.ClientCredentials
	{
		get
		{
			return options.ClientCredentials;
		}
		set
		{
			options.ClientCredentials = value;
		}
	}

	bool IEOSCreateOptions.IsServer
	{
		get
		{
			return options.IsServer;
		}
		set
		{
			options.IsServer = value;
		}
	}

	Utf8String IEOSCreateOptions.EncryptionKey
	{
		get
		{
			return options.EncryptionKey;
		}
		set
		{
			options.EncryptionKey = value;
		}
	}

	Utf8String IEOSCreateOptions.OverrideCountryCode
	{
		get
		{
			return options.OverrideCountryCode;
		}
		set
		{
			options.OverrideCountryCode = value;
		}
	}

	Utf8String IEOSCreateOptions.OverrideLocaleCode
	{
		get
		{
			return options.OverrideLocaleCode;
		}
		set
		{
			options.OverrideLocaleCode = value;
		}
	}

	Utf8String IEOSCreateOptions.DeploymentId
	{
		get
		{
			return options.DeploymentId;
		}
		set
		{
			options.DeploymentId = value;
		}
	}

	PlatformFlags IEOSCreateOptions.Flags
	{
		get
		{
			return options.Flags;
		}
		set
		{
			options.Flags = value;
		}
	}

	Utf8String IEOSCreateOptions.CacheDirectory
	{
		get
		{
			return options.CacheDirectory;
		}
		set
		{
			options.CacheDirectory = value;
		}
	}

	uint IEOSCreateOptions.TickBudgetInMilliseconds
	{
		get
		{
			return options.TickBudgetInMilliseconds;
		}
		set
		{
			options.TickBudgetInMilliseconds = value;
		}
	}
}
