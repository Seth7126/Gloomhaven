using System.ComponentModel;

namespace System.Data.Common;

/// <summary>Implements <see cref="T:System.Data.IDataRecord" /> and <see cref="T:System.ComponentModel.ICustomTypeDescriptor" />, and provides data binding support for <see cref="T:System.Data.Common.DbEnumerator" />.</summary>
/// <filterpriority>1</filterpriority>
public abstract class DbDataRecord : ICustomTypeDescriptor, IDataRecord
{
	/// <summary>Indicates the number of fields within the current record. This property is read-only.</summary>
	/// <returns>The number of fields within the current record.</returns>
	/// <exception cref="T:System.NotSupportedException">Not connected to a data source to read from. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract int FieldCount { get; }

	/// <summary>Indicates the value at the specified column in its native format given the column ordinal. This property is read-only.</summary>
	/// <returns>The value at the specified column in its native format.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract object this[int i] { get; }

	/// <summary>Indicates the value at the specified column in its native format given the column name. This property is read-only.</summary>
	/// <returns>The value at the specified column in its native format.</returns>
	/// <param name="name">The column name. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract object this[string name] { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbDataRecord" /> class.</summary>
	protected DbDataRecord()
	{
	}

	/// <summary>Returns the value of the specified column as a Boolean.</summary>
	/// <returns>true if the Boolean is true; otherwise false.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract bool GetBoolean(int i);

	/// <summary>Returns the value of the specified column as a byte.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract byte GetByte(int i);

	/// <summary>Returns the value of the specified column as a byte array.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The zero-based column ordinal.</param>
	/// <param name="dataIndex">The index within the field from which to start the read operation.</param>
	/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
	/// <param name="bufferIndex">The index for <paramref name="buffer" /> to start the read operation.</param>
	/// <param name="length">The number of bytes to read.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length);

	/// <summary>Returns the value of the specified column as a character.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract char GetChar(int i);

	/// <summary>Returns the value of the specified column as a character array.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">Column ordinal. </param>
	/// <param name="dataIndex">Buffer to copy data into. </param>
	/// <param name="buffer">Maximum length to copy into the buffer. </param>
	/// <param name="bufferIndex">Point to start from within the buffer. </param>
	/// <param name="length">Point to start from within the source data. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length);

	/// <summary>Not currently supported.</summary>
	/// <returns>Not currently supported.</returns>
	/// <param name="i">Not currently supported.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public IDataReader GetData(int i)
	{
		return GetDbDataReader(i);
	}

	/// <summary>Returns a <see cref="T:System.Data.Common.DbDataReader" /> object for the requested column ordinal that can be overridden with a provider-specific implementation.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbDataReader" /> object.</returns>
	/// <param name="i">The zero-based column ordinal.</param>
	protected virtual DbDataReader GetDbDataReader(int i)
	{
		throw ADP.NotSupported();
	}

	/// <summary>Returns the name of the back-end data type.</summary>
	/// <returns>The name of the back-end data type.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract string GetDataTypeName(int i);

	/// <summary>Returns the value of the specified column as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract DateTime GetDateTime(int i);

	/// <summary>Returns the value of the specified column as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract decimal GetDecimal(int i);

	/// <summary>Returns the value of the specified column as a double-precision floating-point number.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract double GetDouble(int i);

	/// <summary>Returns the <see cref="T:System.Type" /> that is the data type of the object.</summary>
	/// <returns>The <see cref="T:System.Type" /> that is the data type of the object.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>2</filterpriority>
	public abstract Type GetFieldType(int i);

	/// <summary>Returns the value of the specified column as a single-precision floating-point number.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract float GetFloat(int i);

	/// <summary>Returns the GUID value of the specified field.</summary>
	/// <returns>The GUID value of the specified field.</returns>
	/// <param name="i">The index of the field to return. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract Guid GetGuid(int i);

	/// <summary>Returns the value of the specified column as a 16-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>2</filterpriority>
	public abstract short GetInt16(int i);

	/// <summary>Returns the value of the specified column as a 32-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract int GetInt32(int i);

	/// <summary>Returns the value of the specified column as a 64-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>2</filterpriority>
	public abstract long GetInt64(int i);

	/// <summary>Returns the name of the specified column.</summary>
	/// <returns>The name of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract string GetName(int i);

	/// <summary>Returns the column ordinal, given the name of the column.</summary>
	/// <returns>The column ordinal.</returns>
	/// <param name="name">The name of the column. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract int GetOrdinal(string name);

	/// <summary>Returns the value of the specified column as a string.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract string GetString(int i);

	/// <summary>Returns the value at the specified column in its native format.</summary>
	/// <returns>The value to return.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract object GetValue(int i);

	/// <summary>Populates an array of objects with the column values of the current record.</summary>
	/// <returns>The number of instances of <see cref="T:System.Object" /> in the array.</returns>
	/// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public abstract int GetValues(object[] values);

	/// <summary>Used to indicate nonexistent values.</summary>
	/// <returns>true if the specified column is equivalent to <see cref="T:System.DBNull" />; otherwise false.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	public abstract bool IsDBNull(int i);

	/// <summary>Returns a collection of custom attributes for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.AttributeCollection" /> that contains the attributes for this object. </returns>
	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return new AttributeCollection((Attribute[])null);
	}

	/// <summary>Returns the class name of this instance of a component.</summary>
	/// <returns>The class name of the object, or null if the class does not have a name.</returns>
	string ICustomTypeDescriptor.GetClassName()
	{
		return null;
	}

	/// <summary>Returns the name of this instance of a component.</summary>
	/// <returns>The name of the object, or null if the object does not have a name.</returns>
	string ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	/// <summary>Returns a type converter for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that is the converter for this object, or null if there is no <see cref="T:System.ComponentModel.TypeConverter" /> for this object.</returns>
	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return null;
	}

	/// <summary>Returns the default event for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptor" /> that represents the default event for this object, or null if this object does not have events.</returns>
	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return null;
	}

	/// <summary>Returns the default property for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the default property for this object, or null if this object does not have properties.</returns>
	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return null;
	}

	/// <summary>Returns an editor of the specified type for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.Object" /> of the specified type that is the editor for this object, or null if the editor cannot be found.</returns>
	/// <param name="editorBaseType">A <see cref="T:System.Type" /> that represents the editor for this object.</param>
	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return null;
	}

	/// <summary>Returns the events for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the events for this component instance.</returns>
	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return new EventDescriptorCollection(null);
	}

	/// <summary>Returns the events for this instance of a component using the specified attribute array as a filter.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the filtered events for this component instance.</returns>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return new EventDescriptorCollection(null);
	}

	/// <summary>Returns the properties for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the properties for this component instance.</returns>
	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return ((ICustomTypeDescriptor)this).GetProperties((Attribute[])null);
	}

	/// <summary>Returns the properties for this instance of a component using the attribute array as a filter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the filtered properties for this component instance.</returns>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return new PropertyDescriptorCollection(null);
	}

	/// <summary>Returns an object that contains the property described by the specified property descriptor.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the owner of the specified property.</returns>
	/// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the property whose owner is to be found.</param>
	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}
}
