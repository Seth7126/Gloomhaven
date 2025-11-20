using System;
using Manatee.Json.Internal;
using Manatee.Json.Serialization.Internal;
using Manatee.Json.Serialization.Internal.Serializers;

namespace Manatee.Json.Serialization;

public class JsonSerializer
{
	private int _callCount;

	private JsonSerializerOptions? _options;

	private AbstractionMap? _abstractionMap;

	public JsonSerializerOptions Options
	{
		get
		{
			return _options ?? (_options = new JsonSerializerOptions(JsonSerializerOptions.Default));
		}
		set
		{
			_options = value;
		}
	}

	public AbstractionMap AbstractionMap
	{
		get
		{
			return _abstractionMap ?? (_abstractionMap = new AbstractionMap(Manatee.Json.Serialization.AbstractionMap.Default));
		}
		set
		{
			_abstractionMap = value;
		}
	}

	public JsonValue Serialize<T>(T obj)
	{
		return Serialize(typeof(T), obj);
	}

	public JsonValue Serialize(Type type, object obj)
	{
		SerializationContext serializationContext = new SerializationContext(this);
		serializationContext.Push(obj?.GetType() ?? type, type, null, obj);
		JsonValue result = Serialize(serializationContext);
		serializationContext.Pop();
		return result;
	}

	internal JsonValue Serialize(SerializationContext context)
	{
		_callCount++;
		Log.Serialization(() => $"Serializing {context.CurrentLocation}");
		ISerializer serializer = SerializerFactory.GetSerializer(context);
		JsonValue result = DefaultValueSerializer.Instance.TrySerialize(serializer, context);
		if (--_callCount == 0)
		{
			Log.Serialization(() => "Serialization complete; clearing reference map");
			context.SerializationMap.Clear();
		}
		return result;
	}

	public JsonValue SerializeType<T>()
	{
		return SerializerFactory.GetTypeSerializer().SerializeType<T>(this);
	}

	public JsonValue GenerateTemplate<T>()
	{
		return TemplateGenerator.FromType<T>(this);
	}

	public T Deserialize<T>(JsonValue json)
	{
		return (T)Deserialize(typeof(T), json);
	}

	public object? Deserialize(Type type, JsonValue json)
	{
		DeserializationContext deserializationContext = new DeserializationContext(this, json);
		deserializationContext.Push(type, null, json);
		object? result = Deserialize(deserializationContext);
		deserializationContext.Pop();
		return result;
	}

	internal object? Deserialize(DeserializationContext context)
	{
		_callCount++;
		ISerializer serializer = SerializerFactory.GetSerializer(context);
		object obj = SchemaValidator.Instance.TryDeserialize(serializer, context);
		if (--_callCount == 0)
		{
			Log.Serialization(() => "Primary deserialization complete; processing references");
			if (obj != null)
			{
				context.SerializationMap.Complete(obj);
			}
		}
		return obj;
	}

	public void DeserializeType<T>(JsonValue json)
	{
		SerializerFactory.GetTypeSerializer().DeserializeType<T>(json, this);
	}
}
