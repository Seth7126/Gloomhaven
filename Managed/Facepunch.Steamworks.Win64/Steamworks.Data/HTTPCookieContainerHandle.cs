using System;

namespace Steamworks.Data;

internal struct HTTPCookieContainerHandle : IEquatable<HTTPCookieContainerHandle>, IComparable<HTTPCookieContainerHandle>
{
	public uint Value;

	public static implicit operator HTTPCookieContainerHandle(uint value)
	{
		return new HTTPCookieContainerHandle
		{
			Value = value
		};
	}

	public static implicit operator uint(HTTPCookieContainerHandle value)
	{
		return value.Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override bool Equals(object p)
	{
		return Equals((HTTPCookieContainerHandle)p);
	}

	public bool Equals(HTTPCookieContainerHandle p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(HTTPCookieContainerHandle a, HTTPCookieContainerHandle b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(HTTPCookieContainerHandle a, HTTPCookieContainerHandle b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(HTTPCookieContainerHandle other)
	{
		return Value.CompareTo(other.Value);
	}
}
