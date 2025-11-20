using System.Collections;
using System.ComponentModel;
using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Represents a collection of parameters relevant to an <see cref="T:System.Data.OleDb.OleDbCommand" /> as well as their respective mappings to columns in a <see cref="T:System.Data.DataSet" />. </summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public class OleDbParameterCollection : DbParameterCollection
{
	/// <summary>Returns an integer that contains the number of elements in the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />. Read-only. </summary>
	/// <returns>The number of elements in the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> as an integer.</returns>
	public override int Count
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> has a fixed size. Read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> has a fixed size; otherwise false.</returns>
	public override bool IsFixedSize
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> is read only; otherwise false.</returns>
	public override bool IsReadOnly
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> is synchronized. Read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> is synchronized; otherwise false.</returns>
	public override bool IsSynchronized
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.OleDb.OleDbParameter" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.OleDb.OleDbParameter" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index specified does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public new OleDbParameter this[int index]
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.OleDb.OleDbParameter" /> with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.OleDb.OleDbParameter" /> with the specified name.</returns>
	/// <param name="parameterName">The name of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The name specified does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public new OleDbParameter this[string parameterName]
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />. Read-only.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</returns>
	public override object SyncRoot
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	internal OleDbParameterCollection()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Data.OleDb.OleDbParameter" /> to add to the collection.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Data.OleDb.OleDbParameter" /> specified in the <paramref name="value" /> parameter is already added to this or another <see cref="T:System.Data.OleDb.OleDbParameterCollection" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter Add(OleDbParameter value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> object to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object in the collection.</returns>
	/// <param name="value">A <see cref="T:System.Object" />.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int Add(object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an <see cref="T:System.Data.OleDb.OleDbParameter" /> to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />, given the parameter name and data type.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="oleDbType">One of the <see cref="T:System.Data.OleDb.OleDbType" /> values. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter Add(string parameterName, OleDbType oleDbType)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an <see cref="T:System.Data.OleDb.OleDbParameter" /> to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> given the parameter name, data type, and column length.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="oleDbType">One of the <see cref="T:System.Data.OleDb.OleDbType" /> values. </param>
	/// <param name="size">The length of the column. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter Add(string parameterName, OleDbType oleDbType, int size)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an <see cref="T:System.Data.OleDb.OleDbParameter" /> to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> given the parameter name, data type, column length, and source column name.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="oleDbType">One of the <see cref="T:System.Data.OleDb.OleDbType" /> values. </param>
	/// <param name="size">The length of the column. </param>
	/// <param name="sourceColumn">The name of the source column. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter Add(string parameterName, OleDbType oleDbType, int size, string sourceColumn)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an <see cref="T:System.Data.OleDb.OleDbParameter" /> to the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> given the parameter name and value.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="value">The <see cref="P:System.Data.OleDb.OleDbParameter.Value" /> of the <see cref="T:System.Data.OleDb.OleDbParameter" /> to add to the collection. </param>
	/// <exception cref="T:System.InvalidCastException">The <paramref name="value" /> parameter is not an <see cref="T:System.Data.OleDb.OleDbParameter" />. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter Add(string parameterName, object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an array of values to the end of the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <param name="values">The <see cref="T:System.Array" /> values to add.</param>
	public override void AddRange(Array values)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds an array of <see cref="T:System.Data.OleDb.OleDbParameter" /> values to the end of the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <param name="values">The <see cref="T:System.Data.OleDbParameter" /> values to add.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(OleDbParameter[] values)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds a value to the end of the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <param name="value">The value to be added.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbParameter AddWithValue(string parameterName, object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes all <see cref="T:System.Data.OleDb.OleDbParameter" /> objects from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	public override void Clear()
	{
		throw ADP.OleDb();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> is in this <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>true if the <see cref="P:System.Data.OleDb.OleDbParameter" /> is in the collection; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Data.OleDb.OleDbParameter" /> value.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(OleDbParameter value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is in this <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> contains <paramref name="value" />; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> value.</param>
	public override bool Contains(object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Determines whether the specified <see cref="T:System.String" /> is in this <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> contains the value; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.String" /> value.</param>
	public override bool Contains(string value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> to the specified one-dimensional <see cref="T:System.Array" /> starting at the specified destination <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Array" /> at which copying starts.</param>
	public override void CopyTo(Array array, int index)
	{
		throw ADP.OleDb();
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> to the specified <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> starting at the specified destination index.</summary>
	/// <param name="array">The <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> that is the destination of the elements copied from the current <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> at which copying starts.</param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(OleDbParameter[] array, int index)
	{
		throw ADP.OleDb();
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator" /> for the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</returns>
	public override IEnumerator GetEnumerator()
	{
		throw ADP.OleDb();
	}

	protected override DbParameter GetParameter(int index)
	{
		throw ADP.OleDb();
	}

	protected override DbParameter GetParameter(string parameterName)
	{
		throw ADP.OleDb();
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> that is a <see cref="T:System.Data.OleDb.OleDbParameter" /> within the collection.</returns>
	/// <param name="value">The <see cref="T:System.Data.OleDb.OleDbParameter" /> object in the collection to find.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(OleDbParameter value)
	{
		throw ADP.OleDb();
	}

	/// <summary>The location of the specified <see cref="T:System.Object" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Object" /> that is a <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> within the collection.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to find.</param>
	public override int IndexOf(object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> with the specified name.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.OleDb.OleDbParameter" /> with the specified case-sensitive name.</returns>
	/// <param name="parameterName">The case-sensitive name of the <see cref="T:System.Data.OleDb.OleDbParameter" /> to find.</param>
	public override int IndexOf(string parameterName)
	{
		throw ADP.OleDb();
	}

	/// <summary>Inserts a <see cref="T:System.Data.OleDb.OleDbParameter" /> object into the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which value should be inserted.</param>
	/// <param name="value">An <see cref="T:System.Data.OleDb.OleDbParameter" /> object to be inserted in the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Insert(int index, OleDbParameter value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Inserts a <see cref="T:System.Object" /> into the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which value should be inserted.</param>
	/// <param name="value">A <see cref="T:System.Object" /> to be inserted in the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</param>
	public override void Insert(int index, object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes the <see cref="T:System.Data.OleDb.OleDbParameter" /> from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <param name="value">An <see cref="T:System.Data.OleDb.OleDbParameter" /> object to remove from the collection.</param>
	/// <exception cref="T:System.InvalidCastException">The parameter is not a <see cref="T:System.Data.OleDb.OleDbParameter" />. </exception>
	/// <exception cref="T:System.SystemException">The parameter does not exist in the collection. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(OleDbParameter value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes the <see cref="T:System.Object" /> object from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> to be removed from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</param>
	public override void Remove(object value)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes the <see cref="T:System.Data.OleDb.OleDbParameter" /> from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.OleDb.OleDbParameter" /> object to remove.</param>
	public override void RemoveAt(int index)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes the <see cref="T:System.Data.OleDb.OleDbParameter" /> from the <see cref="T:System.Data.OleDb.OleDbParameterCollection" /> at the specified parameter name.</summary>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.OleDb.OleDbParameter" /> object to remove.</param>
	public override void RemoveAt(string parameterName)
	{
		throw ADP.OleDb();
	}

	protected override void SetParameter(int index, DbParameter value)
	{
		throw ADP.OleDb();
	}

	protected override void SetParameter(string parameterName, DbParameter value)
	{
		throw ADP.OleDb();
	}
}
