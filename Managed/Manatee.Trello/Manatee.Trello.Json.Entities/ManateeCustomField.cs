using System;
using System.Globalization;
using Manatee.Json;
using Manatee.Json.Serialization;
using Manatee.Trello.Internal;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCustomField : IJsonCustomField, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonCustomFieldDefinition Definition { get; set; }

	public string Text { get; set; }

	public double? Number { get; set; }

	public DateTime? Date { get; set; }

	public bool? Checked { get; set; }

	public IJsonCustomDropDownOption Selected { get; set; }

	public CustomFieldType Type { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Definition = obj.Deserialize<IJsonCustomFieldDefinition>(serializer, "idCustomField");
			Selected = obj.Deserialize<IJsonCustomDropDownOption>(serializer, "idValue");
			Type = obj.Deserialize<CustomFieldType?>(serializer, "type").GetValueOrDefault();
			ValidForMerge = true;
			if (Selected != null)
			{
				Type = CustomFieldType.DropDown;
				break;
			}
			JsonObject jsonObject = obj.TryGetObject("value");
			if (jsonObject == null)
			{
				break;
			}
			Text = jsonObject.TryGetString("text");
			if (Text != null)
			{
				Type = CustomFieldType.Text;
				break;
			}
			string text = jsonObject.TryGetString("number");
			Number = ((text != null && double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)) ? new double?(result) : ((double?)null));
			if (Number.HasValue)
			{
				Type = CustomFieldType.Number;
				break;
			}
			string text2 = jsonObject.TryGetString("checked");
			Checked = ((text2 != null && bool.TryParse(text2, out var result2)) ? new bool?(result2) : ((bool?)null));
			if (Checked.HasValue)
			{
				Type = CustomFieldType.CheckBox;
				break;
			}
			string text3 = jsonObject.TryGetString("date");
			Date = ((text3 != null && DateTime.TryParse(text3, out var result3)) ? new DateTime?(result3) : ((DateTime?)null));
			if (Date.HasValue)
			{
				Type = CustomFieldType.DateTime;
			}
			break;
		}
		case JsonValueType.String:
			Id = json.String;
			break;
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		bool flag = false;
		JsonObject jsonObject = new JsonObject();
		switch (Type)
		{
		case CustomFieldType.Text:
			if (Text == null)
			{
				flag = true;
			}
			else
			{
				jsonObject["text"] = Text;
			}
			break;
		case CustomFieldType.DropDown:
			if (Selected == null)
			{
				flag = true;
			}
			else
			{
				jsonObject["idValue"] = Selected.Id;
			}
			break;
		case CustomFieldType.CheckBox:
			if (!Checked.HasValue)
			{
				flag = true;
			}
			else
			{
				jsonObject["checked"] = Checked.ToLowerString();
			}
			break;
		case CustomFieldType.DateTime:
			if (!Date.HasValue)
			{
				flag = true;
			}
			else
			{
				jsonObject["date"] = serializer.Serialize(Date);
			}
			break;
		case CustomFieldType.Number:
			if (!Number.HasValue)
			{
				flag = true;
			}
			else
			{
				jsonObject["number"] = string.Format(CultureInfo.InvariantCulture, "{0}", Number);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (!flag)
		{
			return jsonObject;
		}
		return string.Empty;
	}
}
