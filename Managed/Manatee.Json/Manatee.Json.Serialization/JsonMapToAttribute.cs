using System;

namespace Manatee.Json.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonMapToAttribute : Attribute
{
	public string MapToKey { get; }

	public JsonMapToAttribute(string key)
	{
		MapToKey = key;
	}
}
