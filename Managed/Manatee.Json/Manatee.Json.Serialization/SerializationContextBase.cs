using System;
using System.Collections.Generic;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization.Internal;

namespace Manatee.Json.Serialization;

public class SerializationContextBase
{
	private readonly Stack<Type> _requestedTypes = new Stack<Type>();

	private readonly Stack<Type> _inferredTypes = new Stack<Type>();

	public Type InferredType { get; private set; }

	public Type RequestedType { get; private set; }

	public JsonPointer CurrentLocation { get; } = new JsonPointer("#");

	public JsonSerializer RootSerializer { get; }

	internal SerializationReferenceCache SerializationMap { get; }

	internal SerializationContextBase(JsonSerializer rootSerializer)
	{
		SerializationMap = new SerializationReferenceCache();
		RootSerializer = rootSerializer;
	}

	internal void OverrideInferredType(Type type)
	{
		_inferredTypes.Pop();
		_inferredTypes.Push(InferredType);
		InferredType = type;
	}

	private protected void PushDetails(Type inferredType, Type requestedType, string propertyName)
	{
		_inferredTypes.Push(InferredType);
		InferredType = inferredType;
		_requestedTypes.Push(RequestedType);
		RequestedType = requestedType;
		CurrentLocation.Add(propertyName);
	}

	private protected void PopDetails()
	{
		InferredType = _inferredTypes.Pop();
		RequestedType = _requestedTypes.Pop();
		CurrentLocation.RemoveAt(CurrentLocation.Count - 1);
	}
}
