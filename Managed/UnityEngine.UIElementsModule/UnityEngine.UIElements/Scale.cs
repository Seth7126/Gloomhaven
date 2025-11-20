using System;

namespace UnityEngine.UIElements;

public struct Scale : IEquatable<Scale>
{
	private Vector3 m_Scale;

	private bool m_IsNone;

	public Vector3 value
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

	public Scale(Vector3 scale)
	{
		m_Scale = scale;
		m_IsNone = false;
	}

	internal static Scale Initial()
	{
		return new Scale(Vector3.one);
	}

	public static Scale None()
	{
		Scale result = Initial();
		result.m_IsNone = true;
		return result;
	}

	internal bool IsNone()
	{
		return m_IsNone;
	}

	public static bool operator ==(Scale lhs, Scale rhs)
	{
		return lhs.m_Scale == rhs.m_Scale;
	}

	public static bool operator !=(Scale lhs, Scale rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(Scale other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is Scale other && Equals(other);
	}

	public override int GetHashCode()
	{
		return m_Scale.GetHashCode() * 793;
	}

	public override string ToString()
	{
		return m_Scale.ToString();
	}
}
