using Unity;

namespace System.Data.SqlClient;

/// <summary>Provides data for the <see cref="E:System.Data.SqlClient.SqlConnection.InfoMessage" /> event.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlInfoMessageEventArgs : EventArgs
{
	private SqlException _exception;

	/// <summary>Gets the collection of warnings sent from the server.</summary>
	/// <returns>The collection of warnings sent from the server.</returns>
	/// <filterpriority>2</filterpriority>
	public SqlErrorCollection Errors => _exception.Errors;

	/// <summary>Gets the full text of the error sent from the database.</summary>
	/// <returns>The full text of the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Message => _exception.Message;

	/// <summary>Gets the name of the object that generated the error.</summary>
	/// <returns>The name of the object that generated the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Source => _exception.Source;

	internal SqlInfoMessageEventArgs(SqlException exception)
	{
		_exception = exception;
	}

	private bool ShouldSerializeErrors()
	{
		if (_exception != null)
		{
			return 0 < _exception.Errors.Count;
		}
		return false;
	}

	/// <summary>Retrieves a string representation of the <see cref="E:System.Data.SqlClient.SqlConnection.InfoMessage" /> event.</summary>
	/// <returns>A string representing the <see cref="E:System.Data.SqlClient.SqlConnection.InfoMessage" /> event.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return Message;
	}

	internal SqlInfoMessageEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
