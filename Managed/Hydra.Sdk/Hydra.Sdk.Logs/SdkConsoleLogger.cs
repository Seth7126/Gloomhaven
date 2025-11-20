using System;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Logs;

public class SdkConsoleLogger : IHydraSdkLogger
{
	public void Log(HydraLogType type, string category, string description, params object[] args)
	{
		Console.WriteLine($"[{type}] {category} {string.Format(description, args)}");
	}
}
