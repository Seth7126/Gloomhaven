using Manatee.Trello.Rest;

namespace Manatee.Trello.Json;

public interface IDeserializer
{
	T Deserialize<T>(IRestResponse<T> response);

	T Deserialize<T>(string content);
}
