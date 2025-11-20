using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Describes the type of access to system data for a user-defined method or function.</summary>
[Serializable]
public enum SystemDataAccessKind
{
	/// <summary>The method or function does not access system data. </summary>
	None,
	/// <summary>The method or function reads system data.</summary>
	Read
}
