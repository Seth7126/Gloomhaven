using System;
using System.Globalization;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json;

internal class DateTimeSerializer : Manatee.Json.Serialization.ISerializer
{
	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(DateTime);
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return ((DateTime)context.Source).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
	}

	public object Deserialize(DeserializationContext context)
	{
		if (DateTime.TryParseExact(context.LocalValue.String, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
		{
			return result.ToLocalTime();
		}
		return DateTime.MinValue;
	}
}
