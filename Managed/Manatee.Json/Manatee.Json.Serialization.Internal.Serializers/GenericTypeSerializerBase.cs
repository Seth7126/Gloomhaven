using System;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal.Serializers;

internal abstract class GenericTypeSerializerBase : IPrioritizedSerializer, ISerializer
{
	private readonly MethodInfo _encodeMethod;

	private readonly MethodInfo _decodeMethod;

	public virtual int Priority => 2;

	public virtual bool ShouldMaintainReferences => false;

	protected GenericTypeSerializerBase()
	{
		_encodeMethod = GetType().GetTypeInfo().GetDeclaredMethod("_Encode") ?? throw new NotImplementedException("Serializer must implement an _Encode method");
		_decodeMethod = GetType().GetTypeInfo().GetDeclaredMethod("_Decode") ?? throw new NotImplementedException("Serializer must implement a _Decode method");
	}

	public abstract bool Handles(SerializationContextBase context);

	public JsonValue Serialize(SerializationContext context)
	{
		object obj = PrepSource(context);
		if (obj != null)
		{
			context.OverrideSource(obj);
		}
		Type[] typeArguments = GetTypeArguments(context.Source.GetType());
		MethodInfo methodInfo = _encodeMethod;
		if (methodInfo.IsGenericMethod)
		{
			methodInfo = methodInfo.MakeGenericMethod(typeArguments);
		}
		return (JsonValue)methodInfo.Invoke(null, new object[1] { context });
	}

	public object Deserialize(DeserializationContext context)
	{
		Type[] typeArguments = GetTypeArguments(context.InferredType);
		MethodInfo methodInfo = _decodeMethod;
		if (methodInfo.IsGenericMethod)
		{
			methodInfo = methodInfo.MakeGenericMethod(typeArguments);
		}
		return methodInfo.Invoke(null, new object[1] { context });
	}

	protected virtual Type[] GetTypeArguments(Type type)
	{
		if (!type.GetTypeInfo().IsGenericType)
		{
			return new Type[1] { type };
		}
		return type.GetTypeArguments();
	}

	protected virtual object? PrepSource(SerializationContext context)
	{
		return null;
	}
}
