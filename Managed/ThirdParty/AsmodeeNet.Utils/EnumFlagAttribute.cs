using UnityEngine;

namespace AsmodeeNet.Utils;

public class EnumFlagAttribute : PropertyAttribute
{
	public string name;

	public EnumFlagAttribute()
	{
	}

	public EnumFlagAttribute(string name)
	{
		this.name = name;
	}
}
