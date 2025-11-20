using System.Collections.Generic;

namespace System.Runtime.Serialization;

/// <summary>Manages serialization processes at run time. This class cannot be inherited.</summary>
public sealed class SerializationObjectManager
{
	private readonly Dictionary<object, object> _objectSeenTable;

	private readonly StreamingContext _context;

	private SerializationEventHandler _onSerializedHandler;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationObjectManager" /> class. </summary>
	/// <param name="context">An instance of the <see cref="T:System.Runtime.Serialization.StreamingContext" /> class that contains information about the current serialization operation.</param>
	public SerializationObjectManager(StreamingContext context)
	{
		_context = context;
		_objectSeenTable = new Dictionary<object, object>();
	}

	/// <summary>Registers the object upon which events will be raised.</summary>
	/// <param name="obj">The object to register.</param>
	public void RegisterObject(object obj)
	{
		SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
		if (serializationEventsForType.HasOnSerializingEvents && _objectSeenTable.TryAdd(obj, true))
		{
			serializationEventsForType.InvokeOnSerializing(obj, _context);
			AddOnSerialized(obj);
		}
	}

	/// <summary>Invokes the OnSerializing callback event if the type of the object has one; and registers the object for raising the OnSerialized event if the type of the object has one.</summary>
	public void RaiseOnSerializedEvent()
	{
		_onSerializedHandler?.Invoke(_context);
	}

	private void AddOnSerialized(object obj)
	{
		SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
		_onSerializedHandler = serializationEventsForType.AddOnSerialized(obj, _onSerializedHandler);
	}
}
