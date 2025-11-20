namespace Hydra.Sdk.Communication.Channels.Http;

public static class HydraHttpConstants
{
	public const string HeaderAuth = "Authorization";

	public const string HeaderContentType = "application/grpc";

	public const string HeaderStatus = "grpc-status";

	public const string HeaderMessage = "grpc-message";

	public const string HeaderCorrelationId = "x-error-correlation-id";

	public const string PostMethod = "POST";
}
