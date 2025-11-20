namespace Manatee.Trello.Rest;

public interface IRestRequest
{
	RestMethod Method { get; set; }

	string Resource { get; }

	void AddParameter(string name, object value);

	void AddBody(object body);
}
