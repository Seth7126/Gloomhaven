using System;
using Hydra.Api.Errors;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Extensions;

public static class LogExtensions
{
	public static HydraSdkException LogException(this IHydraSdkLogger logger, ErrorCode code, string category, string message, Exception innerException = null)
	{
		logger.Log(HydraLogType.Error, category, message);
		return new HydraSdkException(code, message, innerException);
	}
}
