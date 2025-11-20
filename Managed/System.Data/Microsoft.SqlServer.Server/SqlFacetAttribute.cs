using System;

namespace Microsoft.SqlServer.Server;

/// <summary>Annotates the returned result of a user-defined type (UDT) with additional information that can be used in Transact-SQL.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
public class SqlFacetAttribute : Attribute
{
	private bool m_IsFixedLength;

	private int m_MaxSize;

	private int m_Scale;

	private int m_Precision;

	private bool m_IsNullable;

	/// <summary>Indicates whether the return type of the user-defined type is of a fixed length.</summary>
	/// <returns>true if the return type is of a fixed length; otherwise false.</returns>
	public bool IsFixedLength
	{
		get
		{
			return m_IsFixedLength;
		}
		set
		{
			m_IsFixedLength = value;
		}
	}

	/// <summary>The maximum size, in logical units, of the underlying field type of the user-defined type.</summary>
	/// <returns>An <see cref="T:System.Int32" /> representing the maximum size, in logical units, of the underlying field type.</returns>
	public int MaxSize
	{
		get
		{
			return m_MaxSize;
		}
		set
		{
			m_MaxSize = value;
		}
	}

	/// <summary>The precision of the return type of the user-defined type.</summary>
	/// <returns>An <see cref="T:System.Int32" /> representing the precision of the return type.</returns>
	public int Precision
	{
		get
		{
			return m_Precision;
		}
		set
		{
			m_Precision = value;
		}
	}

	/// <summary>The scale of the return type of the user-defined type.</summary>
	/// <returns>An <see cref="T:System.Int32" /> representing the scale of the return type.</returns>
	public int Scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			m_Scale = value;
		}
	}

	/// <summary>Indicates whether the return type of the user-defined type can be null.</summary>
	/// <returns>true if the return type of the user-defined type can be null; otherwise false.</returns>
	public bool IsNullable
	{
		get
		{
			return m_IsNullable;
		}
		set
		{
			m_IsNullable = value;
		}
	}

	/// <summary>An optional attribute on a user-defined type (UDT) return type, used to annotate the returned result with additional information that can be used in Transact-SQL.</summary>
	public SqlFacetAttribute()
	{
	}
}
