using System;

namespace MonoMod.Utils;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DynDllImportAttribute : Attribute
{
	public string DLL;

	public string[] EntryPoints;

	[Obsolete("Pass the entry points as parameters instead.")]
	public string EntryPoint
	{
		set
		{
			EntryPoints = new string[1] { value };
		}
	}

	public DynDllImportAttribute(string dll, params string[] entryPoints)
	{
		DLL = dll;
		EntryPoints = entryPoints;
	}
}
