using System;

namespace Manatee.Json.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonIgnoreAttribute : Attribute
{
}
