using System.Threading.Tasks;
using Hydra.Api.Auth;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.Auth;

public static class AuthorizationApi
{
	public class AuthorizationApiClient : ClientBase<AuthorizationApiClient>
	{
		private readonly ICaller _caller;

		public AuthorizationApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<SignInHydraResponse> SignInHydraAsync(SignInHydraRequest request)
		{
			return _caller.Execute<SignInHydraResponse, SignInHydraRequest>(Descriptor, "SignInHydra", request);
		}

		public Task<SignInEpicOnlineServicesResponse> SignInEpicOnlineServicesAsync(SignInEpicOnlineServicesRequest request)
		{
			return _caller.Execute<SignInEpicOnlineServicesResponse, SignInEpicOnlineServicesRequest>(Descriptor, "SignInEpicOnlineServices", request);
		}

		public Task<SignInStadiaResponse> SignInStadiaAsync(SignInStadiaRequest request)
		{
			return _caller.Execute<SignInStadiaResponse, SignInStadiaRequest>(Descriptor, "SignInStadia", request);
		}

		public Task<SignInXboxResponse> SignInXboxAsync(SignInXboxRequest request)
		{
			return _caller.Execute<SignInXboxResponse, SignInXboxRequest>(Descriptor, "SignInXbox", request);
		}

		public Task<SignInMsStoreResponse> SignInMsStoreAsync(SignInMsStoreRequest request)
		{
			return _caller.Execute<SignInMsStoreResponse, SignInMsStoreRequest>(Descriptor, "SignInMsStore", request);
		}

		public Task<GetCustomDeveloperStringMsStoreResponse> GetCustomDeveloperStringMsStoreAsync(GetCustomDeveloperStringMsStoreRequest request)
		{
			return _caller.Execute<GetCustomDeveloperStringMsStoreResponse, GetCustomDeveloperStringMsStoreRequest>(Descriptor, "GetCustomDeveloperStringMsStore", request);
		}

		public Task<SignInPsnResponse> SignInPsnAsync(SignInPsnRequest request)
		{
			return _caller.Execute<SignInPsnResponse, SignInPsnRequest>(Descriptor, "SignInPsn", request);
		}

		public Task<SignInPsnTokenResponse> SignInPsnTokenAsync(SignInPsnTokenRequest request)
		{
			return _caller.Execute<SignInPsnTokenResponse, SignInPsnTokenRequest>(Descriptor, "SignInPsnToken", request);
		}

		public Task<SignInSteamResponse> SignInSteamAsync(SignInSteamRequest request)
		{
			return _caller.Execute<SignInSteamResponse, SignInSteamRequest>(Descriptor, "SignInSteam", request);
		}

		public Task<SignInNintendoResponse> SignInNintendoAsync(SignInNintendoRequest request)
		{
			return _caller.Execute<SignInNintendoResponse, SignInNintendoRequest>(Descriptor, "SignInNintendo", request);
		}

		public Task<RefreshUserResponse> RefreshUserAsync(RefreshUserRequest request)
		{
			return _caller.Execute<RefreshUserResponse, RefreshUserRequest>(Descriptor, "RefreshUser", request);
		}

		public Task<SignOutUserResponse> SignOutUserAsync(SignOutUserRequest request)
		{
			return _caller.Execute<SignOutUserResponse, SignOutUserRequest>(Descriptor, "SignOutUser", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("AuthorizationApi", "RedLynx.Api.Auth.AuthorizationApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 2
	};
}
