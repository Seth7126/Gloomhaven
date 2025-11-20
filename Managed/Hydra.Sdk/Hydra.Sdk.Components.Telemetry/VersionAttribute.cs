using System;

namespace Hydra.Sdk.Components.Telemetry;

public class VersionAttribute : Attribute
{
	public int Version { get; }

	public VersionAttribute(int version)
	{
		Version = version;
	}
}
