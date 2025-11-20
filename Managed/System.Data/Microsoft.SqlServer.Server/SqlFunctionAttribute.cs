using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Used to mark a method definition of a user-defined aggregate as a function in SQL Server. The properties on the attribute reflect the physical characteristics used when the type is registered with SQL Server.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class SqlFunctionAttribute : Attribute
{
	private bool m_fDeterministic;

	private DataAccessKind m_eDataAccess;

	private SystemDataAccessKind m_eSystemDataAccess;

	private bool m_fPrecise;

	private string m_fName;

	private string m_fTableDefinition;

	private string m_FillRowMethodName;

	/// <summary>Indicates whether the user-defined function is deterministic.</summary>
	/// <returns>true if the function is deterministic; otherwise false.</returns>
	public bool IsDeterministic
	{
		get
		{
			return m_fDeterministic;
		}
		set
		{
			m_fDeterministic = value;
		}
	}

	/// <summary>Indicates whether the function involves access to user data stored in the local instance of SQL Server.</summary>
	/// <returns>
	///   <see cref="T:Microsoft.SqlServer.Server.DataAccessKind" />.None: Does not access data. <see cref="T:Microsoft.SqlServer.Server.DataAccessKind" />.Read: Only reads user data.</returns>
	public DataAccessKind DataAccess
	{
		get
		{
			return m_eDataAccess;
		}
		set
		{
			m_eDataAccess = value;
		}
	}

	/// <summary>Indicates whether the function requires access to data stored in the system catalogs or virtual system tables of SQL Server.</summary>
	/// <returns>
	///   <see cref="T:Microsoft.SqlServer.Server.DataAccessKind" />.None: Does not access system data. <see cref="T:Microsoft.SqlServer.Server.DataAccessKind" />.Read: Only reads system data.</returns>
	public SystemDataAccessKind SystemDataAccess
	{
		get
		{
			return m_eSystemDataAccess;
		}
		set
		{
			m_eSystemDataAccess = value;
		}
	}

	/// <summary>Indicates whether the function involves imprecise computations, such as floating point operations.</summary>
	/// <returns>true if the function involves precise computations; otherwise false.</returns>
	public bool IsPrecise
	{
		get
		{
			return m_fPrecise;
		}
		set
		{
			m_fPrecise = value;
		}
	}

	/// <summary>The name under which the function should be registered in SQL Server.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the name under which the function should be registered.</returns>
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

	/// <summary>A string that represents the table definition of the results, if the method is used as a table-valued function (TVF).</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the table definition of the results.</returns>
	public string TableDefinition
	{
		get
		{
			return m_fTableDefinition;
		}
		set
		{
			m_fTableDefinition = value;
		}
	}

	/// <summary>The name of a method in the same class as the table-valued function (TVF) that is used by the TVF contract.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the name of a method used by the TVF contract.</returns>
	public string FillRowMethodName
	{
		get
		{
			return m_FillRowMethodName;
		}
		set
		{
			m_FillRowMethodName = value;
		}
	}

	/// <summary>An optional attribute on a user-defined aggregate, used to indicate that the method should be registered in SQL Server as a function. Also used to set the <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.DataAccess" />, <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.FillRowMethodName" />, <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.IsDeterministic" />, <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.IsPrecise" />, <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.Name" />, <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.SystemDataAccess" />, and <see cref="P:Microsoft.SqlServer.Server.SqlFunctionAttribute.TableDefinition" /> properties of the function attribute.</summary>
	public SqlFunctionAttribute()
	{
		m_fDeterministic = false;
		m_eDataAccess = DataAccessKind.None;
		m_eSystemDataAccess = SystemDataAccessKind.None;
		m_fPrecise = false;
		m_fName = null;
		m_fTableDefinition = null;
		m_FillRowMethodName = null;
	}
}
