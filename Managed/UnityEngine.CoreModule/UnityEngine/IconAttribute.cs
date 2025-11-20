using System;
using System.Diagnostics;

namespace UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
public class IconAttribute : Attribute
{
	private string m_IconPath;

	public string path => m_IconPath;

	private IconAttribute()
	{
	}

	public IconAttribute(string path)
	{
		m_IconPath = path;
	}
}
