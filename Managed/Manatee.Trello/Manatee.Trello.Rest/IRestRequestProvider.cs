using System.Collections.Generic;

namespace Manatee.Trello.Rest;

public interface IRestRequestProvider
{
	IRestRequest Create(string endpoint, IDictionary<string, object> parameters = null);
}
