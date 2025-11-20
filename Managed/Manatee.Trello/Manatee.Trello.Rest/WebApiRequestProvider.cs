using System.Collections.Generic;

namespace Manatee.Trello.Rest;

internal class WebApiRequestProvider : IRestRequestProvider
{
	public IRestRequest Create(string endpoint, IDictionary<string, object> parameters = null)
	{
		WebApiRestRequest webApiRestRequest = new WebApiRestRequest
		{
			Resource = endpoint
		};
		if (parameters != null)
		{
			foreach (KeyValuePair<string, object> parameter in parameters)
			{
				if (parameter.Key == "file")
				{
					RestFile restFile = (RestFile)parameter.Value;
					webApiRestRequest.AddFile(parameter.Key, restFile.ContentBytes, restFile.FileName);
				}
				else
				{
					webApiRestRequest.AddParameter(parameter.Key, parameter.Value);
				}
			}
		}
		return webApiRestRequest;
	}
}
