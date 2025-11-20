using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Rating;

public static class RatingApi
{
	public class RatingApiClient : ClientBase<RatingApiClient>
	{
		private readonly ICaller _caller;

		public RatingApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetRatingsServerResponse> GetRatingsServerAsync(GetRatingsServerRequest request)
		{
			return _caller.Execute<GetRatingsServerResponse, GetRatingsServerRequest>(Descriptor, "GetRatingsServer", request);
		}

		public Task<GetRatingsUserResponse> GetRatingsUserAsync(GetRatingsUserRequest request)
		{
			return _caller.Execute<GetRatingsUserResponse, GetRatingsUserRequest>(Descriptor, "GetRatingsUser", request);
		}

		public Task<UpdateRatingsServerResponse> UpdateRatingsServerAsync(UpdateRatingsServerRequest request)
		{
			return _caller.Execute<UpdateRatingsServerResponse, UpdateRatingsServerRequest>(Descriptor, "UpdateRatingsServer", request);
		}

		public Task<UpdateCustomRatingsServerResponse> UpdateCustomRatingsServerAsync(UpdateCustomRatingsServerRequest request)
		{
			return _caller.Execute<UpdateCustomRatingsServerResponse, UpdateCustomRatingsServerRequest>(Descriptor, "UpdateCustomRatingsServer", request);
		}

		public Task<UpdateRatingsUserResponse> UpdateRatingsUserAsync(UpdateRatingsUserRequest request)
		{
			return _caller.Execute<UpdateRatingsUserResponse, UpdateRatingsUserRequest>(Descriptor, "UpdateRatingsUser", request);
		}

		public Task<UpdateCustomRatingsUserResponse> UpdateCustomRatingsUserAsync(UpdateCustomRatingsUserRequest request)
		{
			return _caller.Execute<UpdateCustomRatingsUserResponse, UpdateCustomRatingsUserRequest>(Descriptor, "UpdateCustomRatingsUser", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("RatingApi", "Hydra.Api.Rating.RatingApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 1
	};
}
