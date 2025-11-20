using System;
using System.Reflection;
using Hydra.Api;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Generated;

namespace Hydra.Sdk.Helpers;

public class ServiceInfo
{
	public static ServiceVersion DefaultServiceVersion = new ServiceVersion
	{
		Major = 5,
		Minor = 0,
		Hash = string.Empty
	};

	public string Name { get; private set; }

	public ServiceVersion Version { get; private set; }

	public ServiceAccessRole Role { get; private set; }

	public Build BuildInfo { get; private set; }

	public static ServiceInfo FromType(Type type)
	{
		GenDescriptor genDescriptor = type.ExtractDescriptor<GenDescriptor>();
		if (genDescriptor == null)
		{
			return null;
		}
		ServiceVersion version = (ServiceVersion)(type.GetProperty("Version", BindingFlags.Static | BindingFlags.Public)?.GetValue(null, null));
		return new ServiceInfo
		{
			Name = genDescriptor.FullName,
			Version = version,
			BuildInfo = new Build
			{
				Hash = (type.ExtractHash() ?? string.Empty)
			}
		};
	}

	public ServiceIdentity AsServiceIdentity()
	{
		return new ServiceIdentity
		{
			Name = Name,
			Version = Version
		};
	}
}
