using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.Account;

public static class AccountApi
{
	public class AccountApiClient : ClientBase<AccountApiClient>
	{
		private readonly ICaller _caller;

		public AccountApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetRegistrationQRCodeResponse> GetRegistrationQRCodeAsync(GetRegistrationQRCodeRequest request)
		{
			return _caller.Execute<GetRegistrationQRCodeResponse, GetRegistrationQRCodeRequest>(Descriptor, "GetRegistrationQRCode", request);
		}

		public Task<GetRegistrationStatusResponse> GetRegistrationStatusAsync(GetRegistrationStatusRequest request)
		{
			return _caller.Execute<GetRegistrationStatusResponse, GetRegistrationStatusRequest>(Descriptor, "GetRegistrationStatus", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("AccountApi", "RedLynx.Api.Account.AccountApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 1
	};
}
