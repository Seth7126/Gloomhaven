using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Collects information relevant to a warning or error returned by the data source.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbError
{
	/// <summary>Gets a short description of the error.</summary>
	/// <returns>A short description of the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Message
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets the database-specific error information.</summary>
	/// <returns>The database-specific error information.</returns>
	/// <filterpriority>2</filterpriority>
	public int NativeError
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets the name of the provider that generated the error.</summary>
	/// <returns>The name of the provider that generated the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Source
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets the five-character error code following the ANSI SQL standard for the database.</summary>
	/// <returns>The five-character error code, which identifies the source of the error, if the error can be issued from more than one place.</returns>
	/// <filterpriority>2</filterpriority>
	public string SQLState
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	internal OleDbError()
	{
	}

	/// <summary>Gets the complete text of the error message.</summary>
	/// <returns>The complete text of the error.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		throw ADP.OleDb();
	}
}
