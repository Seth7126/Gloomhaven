using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface ICustomField : ICacheable, IRefreshable
{
	ICustomFieldDefinition Definition { get; }

	event Action<ICustomField, IEnumerable<string>> Updated;
}
public interface ICustomField<T> : ICustomField, ICacheable, IRefreshable
{
	T Value { get; set; }
}
