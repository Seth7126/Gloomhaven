using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.Banner;

public static class BannerApi
{
	public class BannerApiClient : ClientBase<BannerApiClient>
	{
		private readonly ICaller _caller;

		public BannerApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetBannersResponse> GetBannersAsync(GetBannersRequest request)
		{
			return _caller.Execute<GetBannersResponse, GetBannersRequest>(Descriptor, "GetBanners", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("BannerApi", "RedLynx.Api.Banner.BannerApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
