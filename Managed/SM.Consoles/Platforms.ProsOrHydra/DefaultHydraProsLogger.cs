#define ENABLE_LOGS
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;
using SM.Utils;

namespace Platforms.ProsOrHydra;

public class DefaultHydraProsLogger : IHydraSdkLogger
{
	public void Log(HydraLogType type, string category, string description, params object[] args)
	{
		LogUtils.Log($"[{type}] {category} :: {string.Format(description, args)}");
	}
}
