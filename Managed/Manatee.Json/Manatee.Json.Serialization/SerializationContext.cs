using System;
using System.Collections.Generic;

namespace Manatee.Json.Serialization;

public class SerializationContext : SerializationContextBase
{
	private readonly Stack<object?> _sources = new Stack<object>();

	public object? Source { get; private set; }

	internal SerializationContext(JsonSerializer rootSerializer)
		: base(rootSerializer)
	{
	}

	public void Push(Type inferredType, Type requestedType, string propertyName, object? source)
	{
		PushDetails(inferredType, requestedType, propertyName);
		_sources.Push(Source);
		Source = source;
	}

	public void Pop()
	{
		PopDetails();
		Source = _sources.Pop();
	}

	internal void OverrideSource(object source)
	{
		_sources.Pop();
		Source = source;
		_sources.Push(source);
	}
}
