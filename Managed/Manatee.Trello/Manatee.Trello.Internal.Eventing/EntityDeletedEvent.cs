using System;
using System.Linq;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Eventing;

internal class EntityDeletedEvent
{
	public static EntityDeletedEvent Create(Type dataType, IJsonCacheable data)
	{
		return (EntityDeletedEvent)typeof(EntityDeletedEvent<>).MakeGenericType(dataType).GetConstructors().First()
			.Invoke(new object[1] { data });
	}
}
internal class EntityDeletedEvent<T> : EntityDeletedEvent where T : IJsonCacheable
{
	public T Data { get; }

	public EntityDeletedEvent(T data)
	{
		Data = data;
	}
}
