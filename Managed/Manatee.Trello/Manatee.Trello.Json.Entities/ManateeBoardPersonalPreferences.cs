using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeBoardPersonalPreferences : IJsonBoardPersonalPreferences, IJsonSerializable
{
	public bool? ShowSidebar { get; set; }

	public bool? ShowSidebarMembers { get; set; }

	public bool? ShowSidebarBoardActions { get; set; }

	public bool? ShowSidebarActivity { get; set; }

	public bool? ShowListGuide { get; set; }

	public IJsonPosition EmailPosition { get; set; }

	public IJsonList EmailList { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			ShowSidebar = obj.TryGetBoolean("showSidebar");
			ShowSidebarMembers = obj.TryGetBoolean("showSidebarMembers");
			ShowSidebarBoardActions = obj.TryGetBoolean("showSidebarBoardActions");
			ShowSidebarActivity = obj.TryGetBoolean("showSidebarActivity");
			ShowListGuide = obj.TryGetBoolean("showListGuide");
			EmailPosition = obj.Deserialize<IJsonPosition>(serializer, "emailPosition");
			EmailList = obj.Deserialize<IJsonList>(serializer, "idEmailList");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		ShowSidebar.Serialize(jsonObject, serializer, "showSidebar");
		ShowSidebarMembers.Serialize(jsonObject, serializer, "showSidebarMembers");
		ShowSidebarBoardActions.Serialize(jsonObject, serializer, "showSidebarBoardActions");
		ShowSidebarActivity.Serialize(jsonObject, serializer, "showSidebarActivity");
		ShowListGuide.Serialize(jsonObject, serializer, "showListGuide");
		EmailPosition.Serialize(jsonObject, serializer, "emailPosition");
		EmailList.Serialize(jsonObject, serializer, "idEmailList");
		return jsonObject;
	}
}
