using Hydra.Api.GameConfiguration;

namespace Hydra.Sdk.Components.GameConfiguration.Core;

public class ConfigurationCacheEntry
{
	public readonly ComponentDataResult Result;

	public readonly int Size;

	public int Usages;

	public ConfigurationCacheEntry(ComponentDataResult result)
	{
		Result = result;
		Size = result.CalculateSize();
	}
}
