using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.User;

public static class UserApi
{
	public class UserApiClient : ClientBase<UserApiClient>
	{
		private readonly ICaller _caller;

		public UserApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<UsersPublicDataByProviderUserIdResponse> GetUsersPublicDataByProviderUserIdAsync(UsersPublicDataByProviderUserIdRequest request)
		{
			return _caller.Execute<UsersPublicDataByProviderUserIdResponse, UsersPublicDataByProviderUserIdRequest>(Descriptor, "GetUsersPublicDataByProviderUserId", request);
		}

		public Task<UsersPublicDataByUserIdResponse> GetUsersPublicDataByUserIdAsync(UsersPublicDataByUserIdRequest request)
		{
			return _caller.Execute<UsersPublicDataByUserIdResponse, UsersPublicDataByUserIdRequest>(Descriptor, "GetUsersPublicDataByUserId", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("UserApi", "Hydra.Api.User.UserApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
