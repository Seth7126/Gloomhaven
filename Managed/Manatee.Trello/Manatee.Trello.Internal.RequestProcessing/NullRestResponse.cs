using System;
using System.Net;
using Manatee.Trello.Rest;

namespace Manatee.Trello.Internal.RequestProcessing;

internal class NullRestResponse : IRestResponse
{
	public string Content { get; set; }

	public Exception Exception { get; set; }

	public HttpStatusCode StatusCode => (HttpStatusCode)0;
}
internal class NullRestResponse<T> : NullRestResponse, IRestResponse<T>, IRestResponse
{
	public T Data { get; set; }
}
