namespace System.EnterpriseServices;

/// <summary>Specifies the level of impersonation allowed when calling targets of a server application.</summary>
[Serializable]
public enum ImpersonationLevelOption
{
	/// <summary>The client is anonymous to the server. The server process can impersonate the client, but the impersonation token does not contain any information about the client.</summary>
	Anonymous = 1,
	/// <summary>Uses the default impersonation level for the specified authentication service. In COM+, this setting is provided by the DefaultImpersonationLevel property in the LocalComputer collection.</summary>
	Default = 0,
	/// <summary>The most powerful impersonation level. When this level is selected, the server (whether local or remote) can impersonate the client's security context while acting on behalf of the client </summary>
	Delegate = 4,
	/// <summary>The system default level. The server can obtain the client's identity, and the server can impersonate the client to do ACL checks.</summary>
	Identify = 2,
	/// <summary>The server can impersonate the client's security context while acting on behalf of the client. The server can access local resources as the client.</summary>
	Impersonate = 3
}
