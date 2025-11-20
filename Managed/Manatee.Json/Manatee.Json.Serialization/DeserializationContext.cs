using System;
using System.Collections.Generic;

namespace Manatee.Json.Serialization;

public class DeserializationContext : SerializationContextBase
{
	private readonly Stack<JsonValue> _localValues = new Stack<JsonValue>();

	public JsonValue JsonRoot { get; }

	public JsonValue LocalValue { get; private set; }

	public Dictionary<SerializationInfo, object?> ValueMap { get; set; }

	internal DeserializationContext(JsonSerializer rootSerializer, JsonValue jsonRoot)
		: base(rootSerializer)
	{
		JsonRoot = jsonRoot;
	}

	public void Push(Type type, string propertyName, JsonValue localValue)
	{
		PushDetails(type, type, propertyName);
		_localValues.Push(LocalValue);
		LocalValue = localValue;
	}

	public void Pop()
	{
		PopDetails();
		LocalValue = _localValues.Pop();
	}
}
