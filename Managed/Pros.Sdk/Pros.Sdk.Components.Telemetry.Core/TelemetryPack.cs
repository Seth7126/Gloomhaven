using RedLynx.Api.Telemetry;

namespace Pros.Sdk.Components.Telemetry.Core;

public class TelemetryPack
{
	public TelemetryPackHeader Header { get; }

	public byte[] Entries { get; }

	public byte[] Data { get; }

	public TelemetryPack(TelemetryPackHeader header, byte[] entries, byte[] data)
	{
		Header = header;
		Entries = entries;
		Data = data;
	}
}
