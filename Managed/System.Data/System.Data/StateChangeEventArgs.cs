namespace System.Data;

/// <summary>Provides data for the state change event of a .NET Framework data provider.</summary>
/// <filterpriority>2</filterpriority>
public sealed class StateChangeEventArgs : EventArgs
{
	private ConnectionState _originalState;

	private ConnectionState _currentState;

	/// <summary>Gets the new state of the connection. The connection object will be in the new state already when the event is fired.</summary>
	/// <returns>One of the <see cref="T:System.Data.ConnectionState" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	public ConnectionState CurrentState => _currentState;

	/// <summary>Gets the original state of the connection.</summary>
	/// <returns>One of the <see cref="T:System.Data.ConnectionState" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	public ConnectionState OriginalState => _originalState;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.StateChangeEventArgs" /> class, when given the original state and the current state of the object.</summary>
	/// <param name="originalState">One of the <see cref="T:System.Data.ConnectionState" /> values. </param>
	/// <param name="currentState">One of the <see cref="T:System.Data.ConnectionState" /> values. </param>
	public StateChangeEventArgs(ConnectionState originalState, ConnectionState currentState)
	{
		_originalState = originalState;
		_currentState = currentState;
	}
}
