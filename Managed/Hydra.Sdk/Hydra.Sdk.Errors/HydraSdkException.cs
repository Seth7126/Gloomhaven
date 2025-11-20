using System;
using Hydra.Api.Errors;

namespace Hydra.Sdk.Errors;

public class HydraSdkException : Exception
{
	public ErrorCode ErrorCode { get; private set; }

	public string CorrelationId { get; }

	public HydraSdkException(ErrorCode errorCode, string message, Exception innerException = null)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
	}

	public HydraSdkException(ErrorCode errorCode, string correlationId, string message, Exception innerException = null)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
		CorrelationId = correlationId;
	}
}
