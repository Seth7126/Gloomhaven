using System.Collections.Generic;
using Unity;

namespace System.Data;

/// <summary>This API supports the .NET Framework infrastructure and is not intended to be used directly from your code. Represents a collection of ordered <see cref="T:System.Data.DataRow" /> objects returned from a query.</summary>
/// <typeparam name="TRow">The type of objects in the source sequence, typically <see cref="T:System.Data.DataRow" />.</typeparam>
public sealed class OrderedEnumerableRowCollection<TRow> : EnumerableRowCollection<TRow>
{
	internal OrderedEnumerableRowCollection(EnumerableRowCollection<TRow> enumerableTable, IEnumerable<TRow> enumerableRows)
		: base(enumerableTable, enumerableRows, (Func<TRow, TRow>)null)
	{
	}

	internal OrderedEnumerableRowCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
