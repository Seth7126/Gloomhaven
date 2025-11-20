using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.User.Zen;

public static class ZenEconomyApi
{
	public class ZenEconomyApiClient : ClientBase<ZenEconomyApiClient>
	{
		private readonly ICaller _caller;

		public ZenEconomyApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ApplyDiscountOffersResponse> ApplyDiscountOffersAsync(ApplyDiscountOffersRequest request)
		{
			return _caller.Execute<ApplyDiscountOffersResponse, ApplyDiscountOffersRequest>(Descriptor, "ApplyDiscountOffers", request);
		}

		public Task<SendPlaytimeEventsResponse> SendPlaytimeEventsAsync(SendPlaytimeEventsRequest request)
		{
			return _caller.Execute<SendPlaytimeEventsResponse, SendPlaytimeEventsRequest>(Descriptor, "SendPlaytimeEvents", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("ZenEconomyApi", "Hydra.Api.User.Zen.ZenEconomyApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 2
	};
}
