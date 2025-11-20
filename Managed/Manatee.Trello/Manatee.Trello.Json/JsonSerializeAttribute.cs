using System;

namespace Manatee.Trello.Json;

[AttributeUsage(AttributeTargets.Property)]
public class JsonSerializeAttribute : Attribute
{
	public bool IsRequired { get; set; }
}
