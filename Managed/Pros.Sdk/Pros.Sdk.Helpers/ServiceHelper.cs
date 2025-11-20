using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hydra.Api.Errors;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Helpers;
using RedLynx.Api.Account;
using RedLynx.Api.Auth;
using RedLynx.Api.Banner;
using RedLynx.Api.CrossSave;
using RedLynx.Api.EndpointDispatcher;
using RedLynx.Api.Entitlement;
using RedLynx.Api.Telemetry;

namespace Pros.Sdk.Helpers;

public static class ServiceHelper
{
	public static readonly ConcurrentBag<ServiceInfo> KnownServices = new ConcurrentBag<ServiceInfo>();

	private static readonly string[] _filter = new string[4] { "Google.Protobuf", "Grpc.", "System.", "Microsoft." };

	private static bool _isInitialized;

	public static void AddServiceInfo(ServiceInfo serviceInfo)
	{
		if (!KnownServices.Contains(serviceInfo))
		{
			KnownServices.Add(serviceInfo);
		}
	}

	public static void RegisterServices()
	{
		if (!_isInitialized)
		{
			GetServices().ForEach(delegate(ServiceInfo s)
			{
				AddServiceInfo(s);
			});
			_isInitialized = true;
		}
	}

	public static ServiceInfo Get(string descriptorFullName)
	{
		return KnownServices.FirstOrDefault((ServiceInfo s) => s?.Name == descriptorFullName) ?? throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Service '" + descriptorFullName + "' is not defined.");
	}

	private static List<ServiceInfo> GetServices()
	{
		return new List<ServiceInfo>
		{
			ServiceInfo.FromType(typeof(EndpointDispatcherApi)),
			ServiceInfo.FromType(typeof(AuthorizationApi)),
			ServiceInfo.FromType(typeof(AccountApi)),
			ServiceInfo.FromType(typeof(BannerApi)),
			ServiceInfo.FromType(typeof(CrossSaveApi)),
			ServiceInfo.FromType(typeof(EntitlementApi)),
			ServiceInfo.FromType(typeof(TelemetryApi))
		};
	}
}
