using System.Security.Principal;

namespace Microsoft.SqlServer.Server;

/// <summary>Represents an abstraction of the caller's context, which provides access to the <see cref="T:Microsoft.SqlServer.Server.SqlPipe" />, <see cref="T:Microsoft.SqlServer.Server.SqlTriggerContext" />, and <see cref="T:System.Security.Principal.WindowsIdentity" /> objects. This class cannot be inherited.</summary>
public sealed class SqlContext
{
	/// <summary>Specifies whether the calling code is running within SQL Server, and if the context connection can be accessed.</summary>
	/// <returns>True if the context connection is available and the other <see cref="T:Microsoft.SqlServer.Server.SqlContext" /> members can be accessed.</returns>
	public static bool IsAvailable => false;

	/// <summary>Gets the pipe object that allows the caller to send result sets, messages, and the results of executing commands back to the client.</summary>
	/// <returns>An instance of <see cref="T:Microsoft.SqlServer.Server.SqlPipe" /> if a pipe is available, or null if called in a context where pipe is not available (for example, in a user-defined function).</returns>
	public static SqlPipe Pipe => null;

	/// <summary>Gets the trigger context used to provide the caller with information about what caused the trigger to fire, and a map of the columns that were updated.</summary>
	/// <returns>An instance of <see cref="T:Microsoft.SqlServer.Server.SqlTriggerContext" /> if a trigger context is available, or null if called outside of a trigger invocation.</returns>
	public static SqlTriggerContext TriggerContext => null;

	/// <summary>The Microsoft Windows identity of the caller.</summary>
	/// <returns>A <see cref="T:System.Security.Principal.WindowsIdentity" /> instance representing the Windows identity of the caller, or null if the client was authenticated using SQL Server Authentication. </returns>
	public static WindowsIdentity WindowsIdentity => null;
}
