namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Represents an unstructured log record delivered as a COM+ CrmLogRecordRead structure. This class cannot be inherited.</summary>
public sealed class LogRecord
{
	private LogRecordFlags flags;

	private object record;

	private int sequence;

	/// <summary>Gets a value that indicates when the log record was written.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.LogRecordFlags" /> values which provides information about when this record was written.</returns>
	public LogRecordFlags Flags => flags;

	/// <summary>Gets the log record user data.</summary>
	/// <returns>A single BLOB that contains the user data.</returns>
	public object Record => record;

	/// <summary>The sequence number of the log record.</summary>
	/// <returns>An integer value that specifies the sequence number of the log record.</returns>
	public int Sequence => sequence;

	[System.MonoTODO]
	internal LogRecord()
	{
	}

	[System.MonoTODO]
	internal LogRecord(_LogRecord logRecord)
	{
		flags = (LogRecordFlags)logRecord.dwCrmFlags;
		sequence = logRecord.dwSequenceNumber;
		record = logRecord.blobUserData;
	}
}
