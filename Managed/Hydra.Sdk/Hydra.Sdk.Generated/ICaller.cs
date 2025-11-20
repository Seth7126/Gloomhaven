using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Sdk.Generated;

public interface ICaller
{
	Task<TResponse> Execute<TResponse, TRequest>(IDescriptor descriptor, string method, TRequest request) where TResponse : IMessage<TResponse> where TRequest : IMessage<TRequest>;
}
