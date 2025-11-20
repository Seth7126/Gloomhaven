using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Used to mark a method definition in an assembly as a stored procedure. The properties on the attribute reflect the physical characteristics used when the type is registered with SQL Server. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SqlProcedureAttribute : Attribute
{
	private string m_fName;

	/// <summary>The name of the stored procedure.</summary>
	/// <returns>A <see cref="T:System.String" /> representing the name of the stored procedure.</returns>
	public string Name
	{
		get
		{
			return m_fName;
		}
		set
		{
			m_fName = value;
		}
	}

	/// <summary>An attribute on a method definition in an assembly, used to indicate that the given method should be registered as a stored procedure in SQL Server.</summary>
	public SqlProcedureAttribute()
	{
		m_fName = null;
	}
}
