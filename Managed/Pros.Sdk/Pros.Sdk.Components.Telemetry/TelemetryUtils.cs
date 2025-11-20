using System.Linq;
using Hydra.Api.Telemetry;
using RedLynx.Api.Telemetry;

namespace Pros.Sdk.Components.Telemetry;

public static class TelemetryUtils
{
	public static Hydra.Api.Telemetry.TelemetryEventBaseEntry ConvertEvent(RedLynx.Api.Telemetry.TelemetryEventBaseEntry ev)
	{
		return new Hydra.Api.Telemetry.TelemetryEventBaseEntry
		{
			EventType = ev.EventType,
			EventUid = ev.EventUid,
			Version = ev.Version,
			JsonParams = ev.JsonParams
		};
	}

	public static RedLynx.Api.Telemetry.TelemetryPackHeader ConvertHeader(Hydra.Api.Telemetry.TelemetryPackHeader header)
	{
		return new RedLynx.Api.Telemetry.TelemetryPackHeader
		{
			Context = { header.Context.Select((Hydra.Api.Telemetry.TelemetryContext c) => new RedLynx.Api.Telemetry.TelemetryContext
			{
				PropertyName = c.PropertyName,
				PropertyValue = c.PropertyValue
			}) },
			EndTime = header.EndTime,
			Format = (RedLynx.Api.Telemetry.TelemetryPackFormat)header.Format,
			InitTime = header.InitTime,
			Options = new RedLynx.Api.Telemetry.TelemetryPackOptions
			{
				IsCompressed = (header.Options.Compression != TelemetryPackCompression.None),
				IsLocalTime = header.Options.IsLocalTime
			},
			PackNumber = header.PackNumber,
			StartTime = header.StartTime,
			Version = new RedLynx.Api.Telemetry.TelemetryPackFormatVersion
			{
				Major = header.Version.Major,
				Minor = header.Version.Minor
			}
		};
	}
}
