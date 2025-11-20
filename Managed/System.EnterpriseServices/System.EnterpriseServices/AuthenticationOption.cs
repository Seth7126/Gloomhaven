namespace System.EnterpriseServices;

/// <summary>Specifies the remote procedure call (RPC) authentication mechanism. Applicable only when the <see cref="T:System.EnterpriseServices.ActivationOption" /> is set to Server.</summary>
[Serializable]
public enum AuthenticationOption
{
	/// <summary>Authenticates credentials at the beginning of every call.</summary>
	Call = 3,
	/// <summary>Authenticates credentials only when the connection is made.</summary>
	Connect = 2,
	/// <summary>Uses the default authentication level for the specified authentication service. In COM+, this setting is provided by the DefaultAuthenticationLevel property in the LocalComputer collection.</summary>
	Default = 0,
	/// <summary>Authenticates credentials and verifies that no call data has been modified in transit.</summary>
	Integrity = 5,
	/// <summary>Authentication does not occur.</summary>
	None = 1,
	/// <summary>Authenticates credentials and verifies that all call data is received.</summary>
	Packet = 4,
	/// <summary>Authenticates credentials and encrypts the packet, including the data and the sender's identity and signature.</summary>
	Privacy = 6
}
