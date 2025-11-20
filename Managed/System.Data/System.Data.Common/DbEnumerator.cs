using System.Collections;
using System.ComponentModel;
using System.Data.ProviderBase;

namespace System.Data.Common;

/// <summary>Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator" /> method, which supports a simple iteration over a collection by a .NET Framework data provider.</summary>
/// <filterpriority>2</filterpriority>
public class DbEnumerator : IEnumerator
{
	private sealed class DbColumnDescriptor : PropertyDescriptor
	{
		private int _ordinal;

		private Type _type;

		public override Type ComponentType => typeof(IDataRecord);

		public override bool IsReadOnly => true;

		public override Type PropertyType => _type;

		internal DbColumnDescriptor(int ordinal, string name, Type type)
			: base(name, null)
		{
			_ordinal = ordinal;
			_type = type;
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override object GetValue(object component)
		{
			return ((IDataRecord)component)[_ordinal];
		}

		public override void ResetValue(object component)
		{
			throw ADP.NotSupported();
		}

		public override void SetValue(object component, object value)
		{
			throw ADP.NotSupported();
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}
	}

	internal IDataReader _reader;

	internal DbDataRecord _current;

	internal SchemaInfo[] _schemaInfo;

	internal PropertyDescriptorCollection _descriptors;

	private FieldNameLookup _fieldNameLookup;

	private bool _closeReader;

	/// <summary>Gets the current element in the collection.</summary>
	/// <returns>The current element in the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
	/// <filterpriority>2</filterpriority>
	public object Current => _current;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbEnumerator" /> class using the specified DataReader.</summary>
	/// <param name="reader">The DataReader through which to iterate. </param>
	public DbEnumerator(IDataReader reader)
	{
		if (reader == null)
		{
			throw ADP.ArgumentNull("reader");
		}
		_reader = reader;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbEnumerator" /> class using the specified DataReader, and indicates whether to automatically close the DataReader after iterating through its data.</summary>
	/// <param name="reader">The DataReader through which to iterate. </param>
	/// <param name="closeReader">true to automatically close the DataReader after iterating through its data; otherwise, false. </param>
	public DbEnumerator(IDataReader reader, bool closeReader)
	{
		if (reader == null)
		{
			throw ADP.ArgumentNull("reader");
		}
		_reader = reader;
		_closeReader = closeReader;
	}

	public DbEnumerator(DbDataReader reader)
		: this((IDataReader)reader)
	{
	}

	public DbEnumerator(DbDataReader reader, bool closeReader)
		: this((IDataReader)reader, closeReader)
	{
	}

	/// <summary>Advances the enumerator to the next element of the collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool MoveNext()
	{
		if (_schemaInfo == null)
		{
			BuildSchemaInfo();
		}
		_current = null;
		if (_reader.Read())
		{
			object[] values = new object[_schemaInfo.Length];
			_reader.GetValues(values);
			_current = new DataRecordInternal(_schemaInfo, values, _descriptors, _fieldNameLookup);
			return true;
		}
		if (_closeReader)
		{
			_reader.Close();
		}
		return false;
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Reset()
	{
		throw ADP.NotSupported();
	}

	private void BuildSchemaInfo()
	{
		int fieldCount = _reader.FieldCount;
		string[] array = new string[fieldCount];
		for (int i = 0; i < fieldCount; i++)
		{
			array[i] = _reader.GetName(i);
		}
		ADP.BuildSchemaTableInfoTableNames(array);
		SchemaInfo[] array2 = new SchemaInfo[fieldCount];
		PropertyDescriptor[] array3 = new PropertyDescriptor[_reader.FieldCount];
		for (int j = 0; j < array2.Length; j++)
		{
			SchemaInfo schemaInfo = new SchemaInfo
			{
				name = _reader.GetName(j),
				type = _reader.GetFieldType(j),
				typeName = _reader.GetDataTypeName(j)
			};
			array3[j] = new DbColumnDescriptor(j, array[j], schemaInfo.type);
			array2[j] = schemaInfo;
		}
		_schemaInfo = array2;
		_fieldNameLookup = new FieldNameLookup(_reader, -1);
		_descriptors = new PropertyDescriptorCollection(array3);
	}
}
