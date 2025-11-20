using System;

namespace Manatee.Json.Serialization;

public class TypeInstantiationException : Exception
{
	public TypeInstantiationException(Type type)
		: base($"Manatee.Json cannot create an instance of type '{type}' through the default resolver." + " You may need to implement your own IResolver to instantiate this type.")
	{
	}

	public TypeInstantiationException(Type type, Exception innerException)
		: base($"Manatee.Json cannot create an instance of type '{type}' through the default resolver." + " You may need to implement your own IResolver to instantiate this type.", innerException)
	{
	}
}
