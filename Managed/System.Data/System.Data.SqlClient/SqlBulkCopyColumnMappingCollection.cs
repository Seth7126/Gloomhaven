using System.Collections;

namespace System.Data.SqlClient;

/// <summary>Collection of <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> objects that inherits from <see cref="T:System.Collections.CollectionBase" />.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlBulkCopyColumnMappingCollection : CollectionBase
{
	private enum MappingSchema
	{
		Undefined,
		NamesNames,
		NemesOrdinals,
		OrdinalsNames,
		OrdinalsOrdinals
	}

	private MappingSchema _mappingSchema;

	internal bool ReadOnly { get; set; }

	/// <summary>Gets the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object at the specified index.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> to find.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping this[int index] => (SqlBulkCopyColumnMapping)base.List[index];

	internal SqlBulkCopyColumnMappingCollection()
	{
	}

	/// <summary>Adds the specified mapping to the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMappingCollection" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object.</returns>
	/// <param name="bulkCopyColumnMapping">The <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object that describes the mapping to be added to the collection.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping Add(SqlBulkCopyColumnMapping bulkCopyColumnMapping)
	{
		AssertWriteAccess();
		if ((string.IsNullOrEmpty(bulkCopyColumnMapping.SourceColumn) && bulkCopyColumnMapping.SourceOrdinal == -1) || (string.IsNullOrEmpty(bulkCopyColumnMapping.DestinationColumn) && bulkCopyColumnMapping.DestinationOrdinal == -1))
		{
			throw SQL.BulkLoadNonMatchingColumnMapping();
		}
		base.InnerList.Add(bulkCopyColumnMapping);
		return bulkCopyColumnMapping;
	}

	/// <summary>Creates a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> and adds it to the collection, using column names to specify both source and destination columns.</summary>
	/// <returns>A column mapping.</returns>
	/// <param name="sourceColumn">The name of the source column within the data source.</param>
	/// <param name="destinationColumn">The name of the destination column within the destination table.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping Add(string sourceColumn, string destinationColumn)
	{
		AssertWriteAccess();
		return Add(new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn));
	}

	/// <summary>Creates a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> and adds it to the collection, using an ordinal for the source column and a string for the destination column.</summary>
	/// <returns>A column mapping.</returns>
	/// <param name="sourceColumnIndex">The ordinal position of the source column within the data source.</param>
	/// <param name="destinationColumn">The name of the destination column within the destination table.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, string destinationColumn)
	{
		AssertWriteAccess();
		return Add(new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumn));
	}

	/// <summary>Creates a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> and adds it to the collection, using a column name to describe the source column and an ordinal to specify the destination column.</summary>
	/// <returns>A column mapping.</returns>
	/// <param name="sourceColumn">The name of the source column within the data source.</param>
	/// <param name="destinationColumnIndex">The ordinal position of the destination column within the destination table.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping Add(string sourceColumn, int destinationColumnIndex)
	{
		AssertWriteAccess();
		return Add(new SqlBulkCopyColumnMapping(sourceColumn, destinationColumnIndex));
	}

	/// <summary>Creates a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> and adds it to the collection, using ordinals to specify both source and destination columns.</summary>
	/// <returns>A column mapping.</returns>
	/// <param name="sourceColumnIndex">The ordinal position of the source column within the data source.</param>
	/// <param name="destinationColumnIndex">The ordinal position of the destination column within the destination table.</param>
	/// <filterpriority>1</filterpriority>
	public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, int destinationColumnIndex)
	{
		AssertWriteAccess();
		return Add(new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumnIndex));
	}

	private void AssertWriteAccess()
	{
		if (ReadOnly)
		{
			throw SQL.BulkLoadMappingInaccessible();
		}
	}

	/// <summary>Clears the contents of the collection.</summary>
	/// <filterpriority>1</filterpriority>
	public new void Clear()
	{
		AssertWriteAccess();
		base.Clear();
	}

	/// <summary>Gets a value indicating whether a specified <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object exists in the collection.</summary>
	/// <returns>true if the specified mapping exists in the collection; otherwise false.</returns>
	/// <param name="value">A valid <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(SqlBulkCopyColumnMapping value)
	{
		return base.InnerList.Contains(value);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMappingCollection" /> to an array of <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> items, starting at a particular index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> array that is the destination of the elements copied from <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMappingCollection" />. The array must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <filterpriority>1</filterpriority>
	public void CopyTo(SqlBulkCopyColumnMapping[] array, int index)
	{
		base.InnerList.CopyTo(array, index);
	}

	internal void CreateDefaultMapping(int columnCount)
	{
		for (int i = 0; i < columnCount; i++)
		{
			base.InnerList.Add(new SqlBulkCopyColumnMapping(i, i));
		}
	}

	/// <summary>Gets the index of the specified <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object.</summary>
	/// <returns>The zero-based index of the column mapping, or -1 if the column mapping is not found in the collection.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object for which to search.</param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(SqlBulkCopyColumnMapping value)
	{
		return base.InnerList.IndexOf(value);
	}

	/// <summary>Insert a new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> at the index specified.</summary>
	/// <param name="index">Integer value of the location within the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMappingCollection" />  at which to insert the new <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" />.</param>
	/// <param name="value">
	///   <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object to be inserted in the collection.</param>
	/// <filterpriority>1</filterpriority>
	public void Insert(int index, SqlBulkCopyColumnMapping value)
	{
		AssertWriteAccess();
		base.InnerList.Insert(index, value);
	}

	/// <summary>Removes the specified <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> element from the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMappingCollection" />.</summary>
	/// <param name="value">
	///   <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object to be removed from the collection.</param>
	/// <filterpriority>1</filterpriority>
	public void Remove(SqlBulkCopyColumnMapping value)
	{
		AssertWriteAccess();
		base.InnerList.Remove(value);
	}

	/// <summary>Removes the mapping at the specified index from the collection.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.SqlClient.SqlBulkCopyColumnMapping" /> object to be removed from the collection.</param>
	/// <filterpriority>1</filterpriority>
	public new void RemoveAt(int index)
	{
		AssertWriteAccess();
		base.RemoveAt(index);
	}

	internal void ValidateCollection()
	{
		foreach (SqlBulkCopyColumnMapping inner in base.InnerList)
		{
			MappingSchema mappingSchema = ((inner.SourceOrdinal == -1) ? ((inner.DestinationOrdinal == -1) ? MappingSchema.NamesNames : MappingSchema.NemesOrdinals) : ((inner.DestinationOrdinal != -1) ? MappingSchema.OrdinalsOrdinals : MappingSchema.OrdinalsNames));
			if (_mappingSchema == MappingSchema.Undefined)
			{
				_mappingSchema = mappingSchema;
			}
			else if (_mappingSchema != mappingSchema)
			{
				throw SQL.BulkLoadMappingsNamesOrOrdinalsOnly();
			}
		}
	}
}
