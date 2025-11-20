using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello.Rest;

public interface IRestClient
{
	Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct);

	Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct) where T : class;
}
