using System.Threading.Tasks;
using Hydra.Api.GameConfiguration;

namespace Hydra.Sdk.Components.GameConfiguration.Core;

public interface IGameConfigurationCache
{
	Task AwaitCache();

	void Lock();

	void Release();

	bool IsCached(string key);

	bool TryAdd(string key, ComponentDataResult cache);

	bool TryGetValue(string key, out ComponentDataResult value);
}
