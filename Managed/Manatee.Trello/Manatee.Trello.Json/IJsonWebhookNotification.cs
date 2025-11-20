namespace Manatee.Trello.Json;

public interface IJsonWebhookNotification
{
	[JsonDeserialize]
	IJsonAction Action { get; set; }
}
