using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hydra.Api.AbstractData;
using Hydra.Api.Auth;
using Hydra.Api.Challenges;
using Hydra.Api.Diagnostics;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Errors;
using Hydra.Api.Facts;
using Hydra.Api.GameConfiguration;
using Hydra.Api.GooglePlay;
using Hydra.Api.Leaderboards;
using Hydra.Api.Messaging;
using Hydra.Api.Presence;
using Hydra.Api.Push;
using Hydra.Api.Rating;
using Hydra.Api.ServerManagerConfiguration;
using Hydra.Api.SessionControl;
using Hydra.Api.Telemetry;
using Hydra.Api.User;
using Hydra.Api.UserReports;
using Hydra.Sdk.Errors;

namespace Hydra.Sdk.Helpers;

public static class ServiceHelper
{
	public static string SdkVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public static readonly ConcurrentBag<ServiceInfo> KnownServices = new ConcurrentBag<ServiceInfo>();

	private static readonly string[] _filter = new string[4] { "Google.Protobuf", "Grpc.", "System.", "Microsoft." };

	private static bool _isInitialized;

	public static void AddByType(Type type)
	{
		if (IsServiceType(type))
		{
			ServiceInfo serviceInfo = ServiceInfo.FromType(type);
			if (serviceInfo == null)
			{
				throw new HydraSdkException(ErrorCode.SdkInternalError, "Failed to find service descriptor in '" + type.FullName + "' type.");
			}
			AddServiceInfo(serviceInfo);
		}
	}

	public static void AddServiceInfo(ServiceInfo serviceInfo)
	{
		if (!KnownServices.Contains(serviceInfo))
		{
			KnownServices.Add(serviceInfo);
		}
	}

	public static ServiceInfo Get(string descriptorFullName)
	{
		ServiceInfo serviceInfo = KnownServices.FirstOrDefault((ServiceInfo s) => s?.Name == descriptorFullName);
		if (serviceInfo == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Service '" + descriptorFullName + "' is not defined.");
		}
		return serviceInfo;
	}

	private static bool IsServiceType(Type type)
	{
		return type.GetTypeInfo().DeclaredNestedTypes?.Any((TypeInfo a) => a.BaseType != null && a.BaseType.Name.EndsWith("ClientBase`1")) ?? false;
	}

	public static List<ServiceInfo> GetServicesFromAssembly(Assembly assembly)
	{
		List<ServiceInfo> list = new List<ServiceInfo>();
		Type[] array = (from t in assembly.GetTypes()
			where IsServiceType(t)
			select t).ToArray();
		Type[] array2 = array;
		foreach (Type type in array2)
		{
			try
			{
				ServiceInfo serviceInfo = ServiceInfo.FromType(type);
				if (serviceInfo != null)
				{
					list.Add(serviceInfo);
				}
			}
			catch (Exception)
			{
				Console.WriteLine("Failed to get service info from type: " + type.FullName);
			}
		}
		return list;
	}

	internal static void RegisterServices()
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

	private static List<ServiceInfo> GetServices()
	{
		List<ServiceInfo> list = new List<ServiceInfo>();
		list.Add(ServiceInfo.FromType(typeof(EndpointDispatcherApi)));
		list.Add(ServiceInfo.FromType(typeof(AuthorizationApi)));
		list.Add(ServiceInfo.FromType(typeof(FactsApi)));
		list.Add(ServiceInfo.FromType(typeof(TelemetryApi)));
		list.Add(ServiceInfo.FromType(typeof(PresenceApi)));
		list.Add(ServiceInfo.FromType(typeof(PushServiceStub)));
		list.Add(ServiceInfo.FromType(typeof(AbstractDataApi)));
		list.Add(ServiceInfo.FromType(typeof(DevDSMConfigurationApi)));
		list.Add(ServiceInfo.FromType(typeof(SessionControlClientApi)));
		list.Add(ServiceInfo.FromType(typeof(SessionControlServerApi)));
		list.Add(ServiceInfo.FromType(typeof(SessionControlServerBrowsingApi)));
		list.Add(ServiceInfo.FromType(typeof(GooglePlayApi)));
		list.Add(ServiceInfo.FromType(typeof(DiagnosticsApi)));
		list.Add(ServiceInfo.FromType(typeof(RatingApi)));
		list.Add(ServiceInfo.FromType(typeof(LeaderboardsApi)));
		list.Add(ServiceInfo.FromType(typeof(GameConfigurationApi)));
		list.Add(ServiceInfo.FromType(typeof(GameConfigurationManagementApi)));
		list.Add(ServiceInfo.FromType(typeof(EconomyApi)));
		list.Add(ServiceInfo.FromType(typeof(UserReportsApi)));
		list.Add(ServiceInfo.FromType(typeof(ChallengesApi)));
		list.Add(ServiceInfo.FromType(typeof(MessagingApi)));
		return list;
	}
}
