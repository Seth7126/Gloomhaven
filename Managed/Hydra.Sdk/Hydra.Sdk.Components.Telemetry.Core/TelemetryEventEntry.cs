using System.Collections.Generic;
using System.Linq;
using Hydra.Api.Telemetry;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Telemetry.Core;

public class TelemetryEventEntry : IHydraEntry
{
	private int _size;

	public int PackTimeOffset { get; }

	public TelemetryEventBaseEntry Data { get; }

	public List<TelemetryContext> Contexts { get; }

	public TelemetryEventEntry(int packTimeOffset, TelemetryEventBaseEntry eventRawData, List<TelemetryContext> contexts)
	{
		PackTimeOffset = packTimeOffset;
		Data = eventRawData;
		Contexts = contexts;
		_size = eventRawData.CalculateSize() + (contexts?.Select((TelemetryContext c) => c.CalculateSize()).Sum() ?? 0);
	}

	public int GetSize()
	{
		return _size;
	}
}
