using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Used to mark a method definition in an assembly as a trigger in SQL Server. The properties on the attribute reflect the physical attributes used when the type is registered with SQL Server. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SqlTriggerAttribute : Attribute
{
	private string m_fName;

	private string m_fTarget;

	private string m_fEvent;

	/// <summary>The name of the trigger.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the name of the trigger.</returns>
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

	/// <summary>The table to which the trigger applies.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the table name.</returns>
	public string Target
	{
		get
		{
			return m_fTarget;
		}
		set
		{
			m_fTarget = value;
		}
	}

	/// <summary>The type of trigger and what data manipulation language (DML) action activates the trigger.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the type of trigger and what data manipulation language (DML) action activates the trigger.</returns>
	public string Event
	{
		get
		{
			return m_fEvent;
		}
		set
		{
			m_fEvent = value;
		}
	}

	/// <summary>An attribute on a method definition in an assembly, used to mark the method as a trigger in SQL Server.</summary>
	public SqlTriggerAttribute()
	{
		m_fName = null;
		m_fTarget = null;
		m_fEvent = null;
	}
}
