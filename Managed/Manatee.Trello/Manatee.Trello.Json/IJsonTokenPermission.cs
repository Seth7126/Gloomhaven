namespace Manatee.Trello.Json;

public interface IJsonTokenPermission
{
	[JsonDeserialize]
	string IdModel { get; set; }

	[JsonDeserialize]
	TokenModelType? ModelType { get; set; }

	[JsonDeserialize]
	bool? Read { get; set; }

	[JsonDeserialize]
	bool? Write { get; set; }
}
