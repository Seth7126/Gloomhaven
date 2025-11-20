using System;

namespace MonoMod.ModInterop;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public sealed class ModImportNameAttribute : Attribute
{
	public string Name;

	public ModImportNameAttribute(string name)
	{
		Name = name;
	}
}
