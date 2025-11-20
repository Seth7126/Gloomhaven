using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicLinq;

internal class Signature : IEquatable<Signature>
{
	public DynamicProperty[] properties;

	public int hashCode;

	public Signature(IEnumerable<DynamicProperty> properties)
	{
		this.properties = properties.ToArray();
		hashCode = 0;
		foreach (DynamicProperty property in properties)
		{
			hashCode ^= property.Name.GetHashCode() ^ property.Type.GetHashCode();
		}
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Signature))
		{
			return false;
		}
		return Equals((Signature)obj);
	}

	public bool Equals(Signature other)
	{
		if (properties.Length != other.properties.Length)
		{
			return false;
		}
		for (int i = 0; i < properties.Length; i++)
		{
			if (properties[i].Name != other.properties[i].Name || properties[i].Type != other.properties[i].Type)
			{
				return false;
			}
		}
		return true;
	}
}
