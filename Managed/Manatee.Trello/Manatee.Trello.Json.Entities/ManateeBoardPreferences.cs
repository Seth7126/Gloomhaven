using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeBoardPreferences : IJsonBoardPreferences, IJsonSerializable
{
	public BoardPermissionLevel? PermissionLevel { get; set; }

	public BoardVotingPermission? Voting { get; set; }

	public BoardCommentPermission? Comments { get; set; }

	public BoardInvitationPermission? Invitations { get; set; }

	public bool? SelfJoin { get; set; }

	public bool? CardCovers { get; set; }

	public bool? CalendarFeed { get; set; }

	public CardAgingStyle? CardAging { get; set; }

	public IJsonBoardBackground Background { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject jsonObject = json.Object;
			PermissionLevel = jsonObject.Deserialize<BoardPermissionLevel?>(serializer, "permissionLevel");
			Voting = jsonObject.Deserialize<BoardVotingPermission?>(serializer, "voting");
			Comments = jsonObject.Deserialize<BoardCommentPermission?>(serializer, "comments");
			Invitations = jsonObject.Deserialize<BoardInvitationPermission?>(serializer, "invitations");
			SelfJoin = jsonObject.TryGetBoolean("selfJoin");
			CardCovers = jsonObject.TryGetBoolean("cardCovers");
			CalendarFeed = jsonObject.TryGetBoolean("calendarFeed");
			CardAging = jsonObject.Deserialize<CardAgingStyle?>(serializer, "cardAging");
			IJsonBoardBackground jsonBoardBackground = serializer.Deserialize<IJsonBoardBackground>(jsonObject);
			if (jsonBoardBackground.Id != null)
			{
				Background = jsonBoardBackground;
			}
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return null;
	}
}
