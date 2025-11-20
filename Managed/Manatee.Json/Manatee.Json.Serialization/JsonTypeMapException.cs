using System;

namespace Manatee.Json.Serialization;

public class JsonTypeMapException : Exception
{
	internal JsonTypeMapException(Type abstractType, Type concreteType)
		: base($"Cannot create map from type '{abstractType}' to type '{concreteType}' because the destination type is either abstract or an interface.")
	{
	}
}
public class JsonTypeMapException<TAbstract, TConcrete> : JsonTypeMapException
{
	internal JsonTypeMapException()
		: base(typeof(TAbstract), typeof(TConcrete))
	{
	}
}
