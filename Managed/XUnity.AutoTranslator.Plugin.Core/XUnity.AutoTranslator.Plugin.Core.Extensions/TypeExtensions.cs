using System;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

internal static class TypeExtensions
{
	private static Assembly _assemblyCsharp;

	public static Type UnwrapNullable(this Type type)
	{
		return Nullable.GetUnderlyingType(type) ?? type;
	}

	public static bool IsAssemblyCsharp(this Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		if (_assemblyCsharp != null)
		{
			return _assemblyCsharp.Equals(assembly);
		}
		bool num = assembly.GetName().Name.Equals("Assembly-CSharp");
		if (num)
		{
			_assemblyCsharp = assembly;
		}
		return num;
	}

	public static bool IsAssemblyCsharpFirstpass(this Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		return assembly.GetName().Name.Equals("Assembly-CSharp-firstpass");
	}
}
