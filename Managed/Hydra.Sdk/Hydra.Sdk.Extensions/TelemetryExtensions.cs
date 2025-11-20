using System.Linq;
using Hydra.Sdk.Components.Telemetry;

namespace Hydra.Sdk.Extensions;

public static class TelemetryExtensions
{
	public static int GetVersion(this TelemetryEvent e)
	{
		if (e == null)
		{
			return 0;
		}
		object[] customAttributes = e.GetType().GetCustomAttributes(typeof(VersionAttribute), inherit: false);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return 0;
		}
		if (!(customAttributes.FirstOrDefault() is VersionAttribute versionAttribute))
		{
			return 0;
		}
		return versionAttribute?.Version ?? 0;
	}
}
