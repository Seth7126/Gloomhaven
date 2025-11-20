using System;
using System.Net;

namespace Manatee.Trello.Rest;

public interface IRestResponse
{
	string Content { get; }

	Exception Exception { get; set; }

	HttpStatusCode StatusCode { get; }
}
public interface IRestResponse<out T> : IRestResponse
{
	T Data { get; }
}
