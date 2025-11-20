namespace Manatee.Trello.Rest;

public interface IRestClientProvider
{
	IRestRequestProvider RequestProvider { get; }

	IRestClient CreateRestClient(string apiBaseUrl);
}
