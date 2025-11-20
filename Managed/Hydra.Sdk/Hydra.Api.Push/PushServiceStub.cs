using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Push;

public static class PushServiceStub
{
	public class PushServiceStubClient : ClientBase<PushServiceStubClient>
	{
		private readonly ICaller _caller;

		public PushServiceStubClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<Empty> FakeAsync(Empty request)
		{
			return _caller.Execute<Empty, Empty>(Descriptor, "Fake", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("PushServiceStub", "Hydra.Api.Push.PushServiceStub");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
