using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class DateTimeSerializer : IPrioritizedSerializer, ISerializer
{
	public int Priority => 2;

	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(DateTime);
	}

	public JsonValue Serialize(SerializationContext context)
	{
		DateTime dateTime = (DateTime)context.Source;
		JsonSerializerOptions jsonSerializerOptions = context.RootSerializer.Options ?? JsonSerializerOptions.Default;
		switch (jsonSerializerOptions.DateTimeSerializationFormat)
		{
		case DateTimeSerializationFormat.Iso8601:
			return dateTime.ToString("s");
		case DateTimeSerializationFormat.JavaConstructor:
			return $"/Date({dateTime.Ticks / 10000})/";
		case DateTimeSerializationFormat.Milliseconds:
			return dateTime.Ticks / 10000;
		case DateTimeSerializationFormat.Custom:
			if (string.IsNullOrWhiteSpace(jsonSerializerOptions.CustomDateTimeSerializationFormat))
			{
				throw new ArgumentNullException("JsonSerializerOptions.CustomDateTimeSerializationFormat");
			}
			return dateTime.ToString(jsonSerializerOptions.CustomDateTimeSerializationFormat);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public object Deserialize(DeserializationContext context)
	{
		JsonSerializerOptions jsonSerializerOptions = context.RootSerializer.Options ?? JsonSerializerOptions.Default;
		switch (jsonSerializerOptions.DateTimeSerializationFormat)
		{
		case DateTimeSerializationFormat.Iso8601:
			return DateTime.Parse(context.LocalValue.String);
		case DateTimeSerializationFormat.JavaConstructor:
			return new DateTime(long.Parse(context.LocalValue.String.Substring(6, context.LocalValue.String.Length - 8)) * 10000);
		case DateTimeSerializationFormat.Milliseconds:
			return new DateTime((long)context.LocalValue.Number * 10000);
		case DateTimeSerializationFormat.Custom:
			if (string.IsNullOrWhiteSpace(jsonSerializerOptions.CustomDateTimeSerializationFormat))
			{
				throw new ArgumentNullException("JsonSerializerOptions.CustomDateTimeSerializationFormat");
			}
			return DateTime.ParseExact(context.LocalValue.String, jsonSerializerOptions.CustomDateTimeSerializationFormat, CultureInfo.CurrentCulture, DateTimeStyles.None);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
