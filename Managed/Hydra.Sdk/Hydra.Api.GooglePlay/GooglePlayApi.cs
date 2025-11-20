using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.GooglePlay;

public static class GooglePlayApi
{
	public class GooglePlayApiClient : ClientBase<GooglePlayApiClient>
	{
		private readonly ICaller _caller;

		public GooglePlayApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ValidateGooglePurchaseResponse> ValidateGooglePurchaseAsync(ValidateGooglePurchaseRequest request)
		{
			return _caller.Execute<ValidateGooglePurchaseResponse, ValidateGooglePurchaseRequest>(Descriptor, "ValidateGooglePurchase", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("GooglePlayApi", "Hydra.Api.GooglePlay.GooglePlayApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 2
	};
}
