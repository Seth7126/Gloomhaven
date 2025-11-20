namespace Manatee.Trello.Json;

public interface IJsonFactory
{
	T Create<T>() where T : class;
}
