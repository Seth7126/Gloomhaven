using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;

namespace System.Data.Odbc;

/// <summary>Represents a collection of parameters relevant to an <see cref="T:System.Data.Odbc.OdbcCommand" /> and their respective mappings to columns in a <see cref="T:System.Data.DataSet" />. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcParameterCollection : DbParameterCollection
{
	private bool _rebindCollection;

	private static Type s_itemType = typeof(OdbcParameter);

	private List<OdbcParameter> _items;

	internal bool RebindCollection
	{
		get
		{
			return _rebindCollection;
		}
		set
		{
			_rebindCollection = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcParameter" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.Odbc.OdbcParameter" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index specified does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public new OdbcParameter this[int index]
	{
		get
		{
			return (OdbcParameter)GetParameter(index);
		}
		set
		{
			SetParameter(index, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcParameter" /> with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.Odbc.OdbcParameter" /> with the specified name.</returns>
	/// <param name="parameterName">The name of the parameter to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The name specified does not exist. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new OdbcParameter this[string parameterName]
	{
		get
		{
			return (OdbcParameter)GetParameter(parameterName);
		}
		set
		{
			SetParameter(parameterName, value);
		}
	}

	/// <summary>Returns an Integer that contains the number of elements in the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />. Read-only.</summary>
	/// <returns>The number of elements in the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> as an Integer.</returns>
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

	private List<OdbcParameter> InnerList
	{
		get
		{
			List<OdbcParameter> list = _items;
			if (list == null)
			{
				list = (_items = new List<OdbcParameter>());
			}
			return list;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> has a fixed size. Read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> has a fixed size, otherwise false.</returns>
	public override bool IsFixedSize => ((IList)InnerList).IsFixedSize;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> is read only, otherwise, false.</returns>
	public override bool IsReadOnly => ((IList)InnerList).IsReadOnly;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> is synchronized. Read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> is synchronized; otherwise, false.</returns>
	public override bool IsSynchronized => ((ICollection)InnerList).IsSynchronized;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />. Read-only.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</returns>
	public override object SyncRoot => ((ICollection)InnerList).SyncRoot;

	internal OdbcParameterCollection()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Data.Odbc.OdbcParameter" /> to add to the collection. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Data.Odbc.OdbcParameter" /> specified in the <paramref name="value" /> parameter is already added to this or another <see cref="T:System.Data.Odbc.OdbcParameterCollection" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OdbcParameter Add(OdbcParameter value)
	{
		Add((object)value);
		return value;
	}

	/// <summary>Adds an <see cref="T:System.Data.Odbc.OdbcParameter" /> to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> given the parameter name and value.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="value">The <see cref="P:System.Data.OleDb.OleDbParameter.Value" /> of the <see cref="T:System.Data.Odbc.OdbcParameter" /> to add to the collection. </param>
	/// <exception cref="T:System.InvalidCastException">The <paramref name="value" /> parameter is not an <see cref="T:System.Data.Odbc.OdbcParameter" />. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value).  http://go.microsoft.com/fwlink/?linkid=14202", false)]
	public OdbcParameter Add(string parameterName, object value)
	{
		return Add(new OdbcParameter(parameterName, value));
	}

	/// <summary>Adds a value to the end of the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />. </summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <param name="value">The value to be added.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OdbcParameter AddWithValue(string parameterName, object value)
	{
		return Add(new OdbcParameter(parameterName, value));
	}

	/// <summary>Adds an <see cref="T:System.Data.Odbc.OdbcParameter" /> to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />, given the parameter name and data type.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="odbcType">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OdbcParameter Add(string parameterName, OdbcType odbcType)
	{
		return Add(new OdbcParameter(parameterName, odbcType));
	}

	/// <summary>Adds an <see cref="T:System.Data.Odbc.OdbcParameter" /> to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />, given the parameter name, data type, and column length.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="odbcType">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <param name="size">The length of the column. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OdbcParameter Add(string parameterName, OdbcType odbcType, int size)
	{
		return Add(new OdbcParameter(parameterName, odbcType, size));
	}

	/// <summary>Adds an <see cref="T:System.Data.Odbc.OdbcParameter" /> to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> given the parameter name, data type, column length, and source column name.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <param name="parameterName">The name of the parameter. </param>
	/// <param name="odbcType">One of the <see cref="T:System.Data.Odbc.OdbcType" /> values. </param>
	/// <param name="size">The length of the column. </param>
	/// <param name="sourceColumn">The name of the source column. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OdbcParameter Add(string parameterName, OdbcType odbcType, int size, string sourceColumn)
	{
		return Add(new OdbcParameter(parameterName, odbcType, size, sourceColumn));
	}

	/// <summary>Adds an array of <see cref="T:System.Data.Odbc.OdbcParameter" /> values to the end of the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <param name="values">An array of <see cref="T:System.Data.Odbc.OdbcParameter" /> objects to add to the collection.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddRange(OdbcParameter[] values)
	{
		AddRange((Array)values);
	}

	internal void Bind(OdbcCommand command, CMDWrapper cmdWrapper, CNativeBuffer parameterBuffer)
	{
		for (int i = 0; i < Count; i++)
		{
			this[i].Bind(cmdWrapper.StatementHandle, command, checked((short)(i + 1)), parameterBuffer, allowReentrance: true);
		}
		_rebindCollection = false;
	}

	internal int CalcParameterBufferSize(OdbcCommand command)
	{
		int parameterBufferSize = 0;
		for (int i = 0; i < Count; i++)
		{
			if (_rebindCollection)
			{
				this[i].HasChanged = true;
			}
			this[i].PrepareForBind(command, (short)(i + 1), ref parameterBufferSize);
			parameterBufferSize = (parameterBufferSize + (IntPtr.Size - 1)) & ~(IntPtr.Size - 1);
		}
		return parameterBufferSize;
	}

	internal void ClearBindings()
	{
		for (int i = 0; i < Count; i++)
		{
			this[i].ClearBinding();
		}
	}

	/// <summary>Gets a value indicating whether an <see cref="T:System.Data.Odbc.OdbcParameter" /> object with the specified parameter name exists in the collection.</summary>
	/// <returns>true if the collection contains the parameter; otherwise, false.</returns>
	/// <param name="value">The name of the <see cref="T:System.Data.Odbc.OdbcParameter" /> object to find. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool Contains(string value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> is in this <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcParameter" /> is in the collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Data.Odbc.OdbcParameter" /> value.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool Contains(OdbcParameter value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> to the specified <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> starting at the specified destination index.</summary>
	/// <param name="array">The <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> that is the destination of the elements copied from the current <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> at which copying starts.</param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(OdbcParameter[] array, int index)
	{
		CopyTo((Array)array, index);
	}

	private void OnChange()
	{
		_rebindCollection = true;
	}

	internal void GetOutputValues(CMDWrapper cmdWrapper)
	{
		if (!_rebindCollection)
		{
			CNativeBuffer nativeParameterBuffer = cmdWrapper._nativeParameterBuffer;
			for (int i = 0; i < Count; i++)
			{
				this[i].GetOutputValue(nativeParameterBuffer);
			}
		}
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> within the collection.</returns>
	/// <param name="value">The <see cref="T:System.Data.Odbc.OdbcParameter" /> object in the collection to find.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public int IndexOf(OdbcParameter value)
	{
		return IndexOf((object)value);
	}

	/// <summary>Inserts a <see cref="T:System.Data.Odbc.OdbcParameter" /> object into the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which the object should be inserted.</param>
	/// <param name="value">A <see cref="T:System.Data.Odbc.OdbcParameter" /> object to be inserted in the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Insert(int index, OdbcParameter value)
	{
		Insert(index, (object)value);
	}

	/// <summary>Removes the <see cref="T:System.Data.Odbc.OdbcParameter" /> from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <param name="value">A <see cref="T:System.Data.Odbc.OdbcParameter" /> object to remove from the collection.</param>
	/// <exception cref="T:System.InvalidCastException">The parameter is not a <see cref="T:System.Data.Odbc.OdbcParameter" />.</exception>
	/// <exception cref="T:System.SystemException">The parameter does not exist in the collection.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void Remove(OdbcParameter value)
	{
		Remove((object)value);
	}

	/// <summary>Adds the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> object to the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>The index of the new <see cref="T:System.Data.Odbc.OdbcParameter" /> object in the collection.</returns>
	/// <param name="value">A <see cref="T:System.Object" />.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int Add(object value)
	{
		OnChange();
		ValidateType(value);
		Validate(-1, value);
		InnerList.Add((OdbcParameter)value);
		return Count - 1;
	}

	/// <summary>Adds an array of values to the end of the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
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
		foreach (OdbcParameter value2 in values)
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

	/// <summary>Removes all <see cref="T:System.Data.Odbc.OdbcParameter" /> objects from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	public override void Clear()
	{
		OnChange();
		List<OdbcParameter> innerList = InnerList;
		if (innerList == null)
		{
			return;
		}
		foreach (OdbcParameter item in innerList)
		{
			item.ResetParent();
		}
		innerList.Clear();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is in this <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> contains the value otherwise false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> value.</param>
	public override bool Contains(object value)
	{
		return -1 != IndexOf(value);
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> to the specified one-dimensional <see cref="T:System.Array" /> starting at the specified destination <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in the <see cref="T:System.Array" /> at which copying starts.</param>
	public override void CopyTo(Array array, int index)
	{
		((ICollection)InnerList).CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator" /> for the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</returns>
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
			foreach (OdbcParameter item in items)
			{
				if (parameterName == item.ParameterName)
				{
					return num;
				}
				num++;
			}
			num = 0;
			foreach (OdbcParameter item2 in items)
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

	/// <summary>Gets the location of the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> with the specified name.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Data.Odbc.OdbcParameter" /> with the specified case-sensitive name.</returns>
	/// <param name="parameterName">The case-sensitive name of the <see cref="T:System.Data.Odbc.OdbcParameter" /> to find.</param>
	public override int IndexOf(string parameterName)
	{
		return IndexOf(InnerList, parameterName);
	}

	/// <summary>Gets the location of the specified <see cref="T:System.Object" /> within the collection.</summary>
	/// <returns>The zero-based location of the specified <see cref="T:System.Object" /> that is a <see cref="T:System.Data.Odbc.OdbcParameter" /> within the collection.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to find.</param>
	public override int IndexOf(object value)
	{
		if (value != null)
		{
			ValidateType(value);
			List<OdbcParameter> innerList = InnerList;
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

	/// <summary>Inserts a <see cref="T:System.Object" /> into the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which the object should be inserted.</param>
	/// <param name="value">A <see cref="T:System.Object" /> to be inserted in the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</param>
	public override void Insert(int index, object value)
	{
		OnChange();
		ValidateType(value);
		Validate(-1, (OdbcParameter)value);
		InnerList.Insert(index, (OdbcParameter)value);
	}

	private void RangeCheck(int index)
	{
		if (index < 0 || Count <= index)
		{
			throw ADP.ParametersMappingIndex(index, this);
		}
	}

	/// <summary>Removes the <see cref="T:System.Object" /> object from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <param name="value">A <see cref="T:System.Object" /> to be removed from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</param>
	public override void Remove(object value)
	{
		OnChange();
		ValidateType(value);
		int num = IndexOf(value);
		if (-1 != num)
		{
			RemoveIndex(num);
		}
		else if (this != ((OdbcParameter)value).CompareExchangeParent(null, this))
		{
			throw ADP.CollectionRemoveInvalidObject(s_itemType, this);
		}
	}

	/// <summary>Removes the <see cref="T:System.Data.Odbc.OdbcParameter" /> from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> at the specified index.</summary>
	/// <param name="index">The zero-based index of the <see cref="T:System.Data.Odbc.OdbcParameter" /> object to remove.</param>
	public override void RemoveAt(int index)
	{
		OnChange();
		RangeCheck(index);
		RemoveIndex(index);
	}

	/// <summary>Removes the <see cref="T:System.Data.Odbc.OdbcParameter" /> from the <see cref="T:System.Data.Odbc.OdbcParameterCollection" /> with the specified parameter name.</summary>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.Odbc.OdbcParameter" /> object to remove.</param>
	public override void RemoveAt(string parameterName)
	{
		OnChange();
		int index = CheckName(parameterName);
		RemoveIndex(index);
	}

	private void RemoveIndex(int index)
	{
		List<OdbcParameter> innerList = InnerList;
		OdbcParameter odbcParameter = innerList[index];
		innerList.RemoveAt(index);
		odbcParameter.ResetParent();
	}

	private void Replace(int index, object newValue)
	{
		List<OdbcParameter> innerList = InnerList;
		ValidateType(newValue);
		Validate(index, newValue);
		OdbcParameter odbcParameter = innerList[index];
		innerList[index] = (OdbcParameter)newValue;
		odbcParameter.ResetParent();
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
		object obj = ((OdbcParameter)value).CompareExchangeParent(this, null);
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
		string parameterName = ((OdbcParameter)value).ParameterName;
		if (parameterName.Length == 0)
		{
			index = 1;
			do
			{
				parameterName = "Parameter" + index.ToString(CultureInfo.CurrentCulture);
				index++;
			}
			while (-1 != IndexOf(parameterName));
			((OdbcParameter)value).ParameterName = parameterName;
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
}
