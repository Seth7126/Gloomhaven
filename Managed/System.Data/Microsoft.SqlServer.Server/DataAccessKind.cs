using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Describes the type of access to user data for a user-defined method or function.</summary>
[Serializable]
public enum DataAccessKind
{
	/// <summary>The method or function does not access user data.</summary>
	None,
	/// <summary>The method or function reads user data.</summary>
	Read
}
