using Unity;

namespace System.Data.Odbc;

/// <summary>Collects information relevant to a warning or error returned by the data source.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class OdbcError
{
	internal string _message;

	internal string _state;

	internal int _nativeerror;

	internal string _source;

	/// <summary>Gets a short description of the error.</summary>
	/// <returns>A description of the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Message
	{
		get
		{
			if (_message == null)
			{
				return string.Empty;
			}
			return _message;
		}
	}

	/// <summary>Gets the five-character error code that follows the ANSI SQL standard for the database.</summary>
	/// <returns>The five-character error code, which identifies the source of the error if the error can be issued from more than one place.</returns>
	/// <filterpriority>2</filterpriority>
	public string SQLState => _state;

	/// <summary>Gets the data source-specific error information.</summary>
	/// <returns>The data source-specific error information.</returns>
	/// <filterpriority>2</filterpriority>
	public int NativeError => _nativeerror;

	/// <summary>Gets the name of the driver that generated the error.</summary>
	/// <returns>The name of the driver that generated the error.</returns>
	/// <filterpriority>2</filterpriority>
	public string Source
	{
		get
		{
			if (_source == null)
			{
				return string.Empty;
			}
			return _source;
		}
	}

	internal OdbcError(string source, string message, string state, int nativeerror)
	{
		_source = source;
		_message = message;
		_state = state;
		_nativeerror = nativeerror;
	}

	internal void SetSource(string Source)
	{
		_source = Source;
	}

	/// <summary>Gets the complete text of the error message.</summary>
	/// <returns>The complete text of the error.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return Message;
	}

	internal OdbcError()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
