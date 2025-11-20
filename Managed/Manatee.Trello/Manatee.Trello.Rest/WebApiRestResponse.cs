using System;
using System.Net;

namespace Manatee.Trello.Rest;

internal class WebApiRestResponse : IRestResponse
{
	public string Content { get; set; }

	public Exception Exception { get; set; }

	public HttpStatusCode StatusCode { get; set; }
}
internal class WebApiRestResponse<T> : WebApiRestResponse, IRestResponse<T>, IRestResponse
{
	public T Data { get; set; }
}
