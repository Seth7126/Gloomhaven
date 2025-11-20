using System;

namespace MonoMod.ModInterop;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ModExportNameAttribute : Attribute
{
	public string Name;

	public ModExportNameAttribute(string name)
	{
		Name = name;
	}
}
