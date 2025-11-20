using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Data;

/// <summary>The exception that is thrown by the <see cref="T:System.Data.Common.DataAdapter" /> during an insert, update, or delete operation if the number of rows affected equals zero.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public sealed class DBConcurrencyException : SystemException
{
	private DataRow[] _dataRows;

	/// <summary>Gets or sets the value of the <see cref="T:System.Data.DataRow" /> that generated the <see cref="T:System.Data.DBConcurrencyException" />.</summary>
	/// <returns>The value of the <see cref="T:System.Data.DataRow" />.</returns>
	/// <filterpriority>1</filterpriority>
	public DataRow Row
	{
		get
		{
			DataRow[] dataRows = _dataRows;
			if (dataRows == null || dataRows.Length == 0)
			{
				return null;
			}
			return dataRows[0];
		}
		set
		{
			_dataRows = new DataRow[1] { value };
		}
	}

	/// <summary>Gets the number of rows whose update failed, generating this exception.</summary>
	/// <returns>An integer containing a count of the number of rows whose update failed.</returns>
	/// <filterpriority>1</filterpriority>
	public int RowCount
	{
		get
		{
			DataRow[] dataRows = _dataRows;
			if (dataRows == null)
			{
				return 0;
			}
			return dataRows.Length;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DBConcurrencyException" /> class.</summary>
	public DBConcurrencyException()
		: this("DB concurrency violation.", null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DBConcurrencyException" /> class.</summary>
	/// <param name="message">The text string describing the details of the exception. </param>
	public DBConcurrencyException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DBConcurrencyException" /> class.</summary>
	/// <param name="message">The text string describing the details of the exception. </param>
	/// <param name="inner">A reference to an inner exception. </param>
	public DBConcurrencyException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = -2146232011;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DBConcurrencyException" /> class.</summary>
	/// <param name="message">The error message that explains the reason for this exception.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
	/// <param name="dataRows">An array containing the <see cref="T:System.Data.DataRow" /> objects whose update failure generated this exception.</param>
	public DBConcurrencyException(string message, Exception inner, DataRow[] dataRows)
		: base(message, inner)
	{
		base.HResult = -2146232011;
		_dataRows = dataRows;
	}

	private DBConcurrencyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Populates the aprcified serialization information object with the data needed to serialize the <see cref="T:System.Data.DBConcurrencyException" />.</summary>
	/// <param name="si">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized data associated with the <see cref="T:System.Data.DBConcurrencyException" />.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the source and destination of the serialized stream associated with the <see cref="T:System.Data.DBConcurrencyException" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, SerializationFormatter" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	/// <summary>Copies the <see cref="T:System.Data.DataRow" /> objects whose update failure generated this exception, to the specified array of <see cref="T:System.Data.DataRow" /> objects.</summary>
	/// <param name="array">The one-dimensional array of <see cref="T:System.Data.DataRow" /> objects to copy the <see cref="T:System.Data.DataRow" /> objects into.</param>
	/// <filterpriority>1</filterpriority>
	public void CopyToRows(DataRow[] array)
	{
		CopyToRows(array, 0);
	}

	/// <summary>Copies the <see cref="T:System.Data.DataRow" /> objects whose update failure generated this exception, to the specified array of <see cref="T:System.Data.DataRow" /> objects, starting at the specified destination array index.</summary>
	/// <param name="array">The one-dimensional array of <see cref="T:System.Data.DataRow" /> objects to copy the <see cref="T:System.Data.DataRow" /> objects into.</param>
	/// <param name="arrayIndex">The destination array index to start copying into.</param>
	/// <filterpriority>1</filterpriority>
	public void CopyToRows(DataRow[] array, int arrayIndex)
	{
		_dataRows?.CopyTo(array, arrayIndex);
	}
}
