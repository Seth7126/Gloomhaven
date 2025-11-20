using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;

namespace System.Data.SqlClient;

/// <summary>Represents a collection of parameters associated with a <see cref="T:System.Data.SqlClient.SqlCommand" /> and their respective mappings to columns in a <see cref="T:System.Data.DataSet" />. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlParameterCollection : DbParameterCollection, ICollection, IEnumerable, IList, IDataParameterCollection
{
	private bool _isDirty;

	private static Type s_itemType = typeof(SqlParameter);

	private List<SqlParameter> _items;

	internal bool IsDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			_isDirty = value;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> has a fixed size. </summary>
	/// <returns>Returns true if the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> has a fixed size; otherwise false.</returns>
	public override bool IsFixedSize => ((IList)InnerList).IsFixedSize;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> is read-only. </summary>
	/// <returns>Returns true if the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> is read only; otherwise false.</returns>
	public override bool IsReadOnly => ((IList)InnerList).IsReadOnly;

	/// <summary>Gets the <see cref="T:System.Data.SqlClient.SqlParameter" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.SqlClient.SqlParameter" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The specified index does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public new SqlParameter this[int index]
	{
		get
		{
			return (SqlParameter)GetParameter(index);
		}
		set
		{
			SetParameter(index, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.SqlClient.SqlParameter" /> with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.SqlClient.SqlParameter" /> with the specified name.</returns>
	/// <param name="parameterName">The name of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The specified <paramref name="parameterName" /> is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public new SqlParameter this[string parameterName]
	{
		get
		{
			return (SqlParameter)GetParameter(parameterName);
		}
		set
		{
			SetParameter(parameterName, value);
		}
	}

	/// <summary>Returns an Integer that contains the number of elements in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. Read-only. </summary>
	/// <returns>The number of elements in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> as an Integer.</returns>
	public override int Count
	{
		get
		{
			if (_items == null)
			{
				return 0;
			}
			return _items.Count;
		}
	}

	private List<SqlParameter> InnerList
	{
		get
		{
			List<SqlParameter> list = _items;
			if (list == null)
			{
				list = (_items = new List<SqlParameter>());
			}
			return list;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. </summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</returns>
	public override object SyncRoot => ((ICollection)InnerList).SyncRoot;

	internal SqlParameterCollection()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> object to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlClient.SqlParameter" /> to add to the collection. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Data.SqlClient.SqlParameter" /> specified in the <paramref name="value" /> parameter is already added to this or another <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The parameter passed was not a <see cref="T:System.Data.SqlClient.SqlParameter" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter Add(SqlParameter value)
	{
		Add((object)value);
		return value;
	}

	/// <summary>Adds a value to the end of the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <param name="value">The value to be added. Use <see cref="F:System.DBNull.Value" /> instead of null, to indicate a null value.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter AddWithValue(string parameterName, object value)
	{
		return Add(new SqlParameter(parameterName, value));
	}

	/// <summary>Adds a <see cref="T:System.Data.SqlClient.SqlParameter" /> to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> given the parameter name and the data type.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="sqlDbType">One of the <see cref="T:System.Data.SqlDbType" /> values. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
	{
		return Add(new SqlParameter(parameterName, sqlDbType));
	}

	/// <summary>Adds a <see cref="T:System.Data.SqlClient.SqlParameter" /> to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />, given the specified parameter name, <see cref="T:System.Data.SqlDbType" /> and size.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="sqlDbType">The <see cref="T:System.Data.SqlDbType" /> of the <see cref="T:System.Data.SqlClient.SqlParameter" /> to add to the collection. </param>
	/// <param name="size">The size as an <see cref="T:System.Int32" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size)
	{
		return Add(new SqlParameter(parameterName, sqlDbType, size));
	}

	/// <summary>Adds a <see cref="T:System.Data.SqlClient.SqlParameter" /> to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> with the parameter name, the data type, and the column length.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="sqlDbType">One of the <see cref="T:System.Data.SqlDbType" /> values. </param>
	/// <param name="size">The column length.</param>
	/// <param name="sourceColumn">The name of the source column (<see cref="P:System.Data.SqlClient.SqlParameter.SourceColumn" />) if this <see cref="T:System.Data.SqlClient.SqlParameter" /> is used in a call to <see cref="Overload:System.Data.Common.DbDataAdapter.Update" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
	{
		return Add(new SqlParameter(parameterName, sqlDbType, size, sourceColumn));
	}

	/// <summary>Adds an array of <see cref="T:System.Data.SqlClient.SqlParameter" /> values to the end of the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <param name="values">The <see cref="T:System.Data.SqlClient.SqlParameter" /> values to add.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(SqlParameter[] values)
	{
		AddRange((Array)values);
	}

	/// <summary>Determines whether the specified parameter name is in this <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> contains the value; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.String" /> value.</param>
	public override bool Contains(string value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> is in this <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> contains the value; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlClient.SqlParameter" /> value.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(SqlParameter value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> to the specified <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> starting at the specified destination index.</summary>
	/// <param name="array">The <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> that is the destination of the elements copied from the current <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> at which copying starts.</param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(SqlParameter[] array, int index)
	{
		CopyTo((Array)array, index);
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> that is a <see cref="T:System.Data.SqlClient.SqlParameter" /> within the collection. Returns -1 when the object does not exist in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlClient.SqlParameter" /> to find. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(SqlParameter value)
	{
		return IndexOf((object)value);
	}

	/// <summary>Inserts a <see cref="T:System.Data.SqlClient.SqlParameter" /> object into the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which value should be inserted.</param>
	/// <param name="value">A <see cref="T:System.Data.SqlClient.SqlParameter" /> object to be inserted in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Insert(int index, SqlParameter value)
	{
		Insert(index, (object)value);
	}

	private void OnChange()
	{
		IsDirty = true;
	}

	/// <summary>Removes the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> from the collection.</summary>
	/// <param name="value">A <see cref="T:System.Data.SqlClient.SqlParameter" /> object to remove from the collection. </param>
	/// <exception cref="T:System.InvalidCastException">The parameter is not a <see cref="T:System.Data.SqlClient.SqlParameter" />. </exception>
	/// <exception cref="T:System.SystemException">The parameter does not exist in the collection. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(SqlParameter value)
	{
		Remove((object)value);
	}

	/// <summary>Adds the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> object to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.</returns>
	/// <param name="value">An <see cref="T:System.Object" />.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int Add(object value)
	{
		OnChange();
		ValidateType(value);
		Validate(-1, value);
		InnerList.Add((SqlParameter)value);
		return Count - 1;
	}

	/// <summary>Adds an array of values to the end of the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <param name="values">The <see cref="T:System.Array" /> values to add.</param>
	public override void AddRange(Array values)
	{
		OnChange();
		if (values == null)
		{
			throw ADP.ArgumentNull("values");
		}
		foreach (object value in values)
		{
			ValidateType(value);
		}
		foreach (SqlParameter value2 in values)
		{
			Validate(-1, value2);
			InnerList.Add(value2);
		}
	}

	private int CheckName(string parameterName)
	{
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw ADP.ParametersSourceIndex(parameterName, this, s_itemType);
		}
		return num;
	}

	/// <summary>Removes all the <see cref="T:System.Data.SqlClient.SqlParameter" /> objects from the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	public override void Clear()
	{
		OnChange();
		List<SqlParameter> innerList = InnerList;
		if (innerList == null)
		{
			return;
		}
		foreach (SqlParameter item in innerList)
		{
			item.ResetParent();
		}
		innerList.Clear();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is in this <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> contains the value; otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> value.</param>
	public override bool Contains(object value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> to the specified one-dimensional <see cref="T:System.Array" /> starting at the specified destination <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Array" /> at which copying starts.</param>
	public override void CopyTo(Array array, int index)
	{
		((ICollection)InnerList).CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. </returns>
	public override IEnumerator GetEnumerator()
	{
		return ((IEnumerable)InnerList).GetEnumerator();
	}

	protected override DbParameter GetParameter(int index)
	{
		RangeCheck(index);
		return InnerList[index];
	}

	protected override DbParameter GetParameter(string parameterName)
	{
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw ADP.ParametersSourceIndex(parameterName, this, s_itemType);
		}
		return InnerList[num];
	}

	private static int IndexOf(IEnumerable items, string parameterName)
	{
		if (items != null)
		{
			int num = 0;
			foreach (SqlParameter item in items)
			{
				if (parameterName == item.ParameterName)
				{
					return num;
				}
				num++;
			}
			num = 0;
			foreach (SqlParameter item2 in items)
			{
				if (ADP.DstCompare(parameterName, item2.ParameterName) == 0)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> with the specified name.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> with the specified case-sensitive name. Returns -1 when the object does not exist in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</returns>
	/// <param name="parameterName">The case-sensitive name of the <see cref="T:System.Data.SqlClient.SqlParameter" /> to find.</param>
	public override int IndexOf(string parameterName)
	{
		return IndexOf(InnerList, parameterName);
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Object" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Object" /> that is a <see cref="T:System.Data.SqlClient.SqlParameter" /> within the collection. Returns -1 when the object does not exist in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to find. </param>
	public override int IndexOf(object value)
	{
		if (value != null)
		{
			ValidateType(value);
			List<SqlParameter> innerList = InnerList;
			if (innerList != null)
			{
				int count = innerList.Count;
				for (int i = 0; i < count; i++)
				{
					if (value == innerList[i])
					{
						return i;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>Inserts an <see cref="T:System.Object" /> into the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which value should be inserted.</param>
	/// <param name="value">An <see cref="T:System.Object" /> to be inserted in the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</param>
	public override void Insert(int index, object value)
	{
		OnChange();
		ValidateType(value);
		Validate(-1, (SqlParameter)value);
		InnerList.Insert(index, (SqlParameter)value);
	}

	private void RangeCheck(int index)
	{
		if (index < 0 || Count <= index)
		{
			throw ADP.ParametersMappingIndex(index, this);
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> from the collection.</summary>
	/// <param name="value">The object to remove from the collection. </param>
	public override void Remove(object value)
	{
		OnChange();
		ValidateType(value);
		int num = IndexOf(value);
		if (-1 != num)
		{
			RemoveIndex(num);
		}
		else if (this != ((SqlParameter)value).CompareExchangeParent(null, this))
		{
			throw ADP.CollectionRemoveInvalidObject(s_itemType, this);
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.SqlClient.SqlParameter" /> from the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.SqlClient.SqlParameter" /> object to remove.</param>
	public override void RemoveAt(int index)
	{
		OnChange();
		RangeCheck(index);
		RemoveIndex(index);
	}

	/// <summary>Removes the <see cref="T:System.Data.SqlClient.SqlParameter" /> from the <see cref="T:System.Data.SqlClient.SqlParameterCollection" /> at the specified parameter name.</summary>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.SqlClient.SqlParameter" /> to remove.</param>
	public override void RemoveAt(string parameterName)
	{
		OnChange();
		int index = CheckName(parameterName);
		RemoveIndex(index);
	}

	private void RemoveIndex(int index)
	{
		List<SqlParameter> innerList = InnerList;
		SqlParameter sqlParameter = innerList[index];
		innerList.RemoveAt(index);
		sqlParameter.ResetParent();
	}

	private void Replace(int index, object newValue)
	{
		List<SqlParameter> innerList = InnerList;
		ValidateType(newValue);
		Validate(index, newValue);
		SqlParameter sqlParameter = innerList[index];
		innerList[index] = (SqlParameter)newValue;
		sqlParameter.ResetParent();
	}

	protected override void SetParameter(int index, DbParameter value)
	{
		OnChange();
		RangeCheck(index);
		Replace(index, value);
	}

	protected override void SetParameter(string parameterName, DbParameter value)
	{
		OnChange();
		int num = IndexOf(parameterName);
		if (num < 0)
		{
			throw ADP.ParametersSourceIndex(parameterName, this, s_itemType);
		}
		Replace(num, value);
	}

	private void Validate(int index, object value)
	{
		if (value == null)
		{
			throw ADP.ParameterNull("value", this, s_itemType);
		}
		object obj = ((SqlParameter)value).CompareExchangeParent(this, null);
		if (obj != null)
		{
			if (this != obj)
			{
				throw ADP.ParametersIsNotParent(s_itemType, this);
			}
			if (index != IndexOf(value))
			{
				throw ADP.ParametersIsParent(s_itemType, this);
			}
		}
		string parameterName = ((SqlParameter)value).ParameterName;
		if (parameterName.Length == 0)
		{
			index = 1;
			do
			{
				parameterName = "Parameter" + index.ToString(CultureInfo.CurrentCulture);
				index++;
			}
			while (-1 != IndexOf(parameterName));
			((SqlParameter)value).ParameterName = parameterName;
		}
	}

	private void ValidateType(object value)
	{
		if (value == null)
		{
			throw ADP.ParameterNull("value", this, s_itemType);
		}
		if (!s_itemType.IsInstanceOfType(value))
		{
			throw ADP.InvalidParameterType(this, s_itemType, value);
		}
	}

	/// <summary>Adds the specified <see cref="T:System.Data.SqlClient.SqlParameter" /> object to the <see cref="T:System.Data.SqlClient.SqlParameterCollection" />.</summary>
	/// <returns>A new <see cref="T:System.Data.SqlClient.SqlParameter" /> object.Use caution when you are using this overload of the SqlParameterCollection.Add method to specify integer parameter values. Because this overload takes a <paramref name="value" /> of type <see cref="T:System.Object" />, you must convert the integral value to an <see cref="T:System.Object" /> type when the value is zero, as the following C# example demonstrates. Copy Codeparameters.Add("@pname", Convert.ToInt32(0));If you do not perform this conversion, the compiler assumes that you are trying to call the SqlParameterCollection.Add (string, SqlDbType) overload.</returns>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.SqlClient.SqlParameter" /> to add to the collection.</param>
	/// <param name="value">A <see cref="T:System.Object" />.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Data.SqlClient.SqlParameter" /> specified in the <paramref name="value" /> parameter is already added to this or another <see cref="T:System.Data.SqlClient.SqlParameterCollection" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public SqlParameter Add(string parameterName, object value)
	{
		return Add(new SqlParameter(parameterName, value));
	}
}
