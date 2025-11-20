using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Interfaces;

public interface IHydraSdkLogger
{
	void Log(HydraLogType type, string category, string description, params object[] args);
}
