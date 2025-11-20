using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

internal static class AssemblyLoader
{
	internal static List<Type> GetAllTypesOf<TService>(string directory)
	{
		Directory.CreateDirectory(directory);
		string[] files = Directory.GetFiles(directory, "*.dll");
		HashSet<Type> hashSet = new HashSet<Type>();
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			LoadAssembliesInFile<TService>(array[i], hashSet);
		}
		return hashSet.ToList();
	}

	private static bool LoadAssembliesInFile<TService>(string file, HashSet<Type> allTypes)
	{
		try
		{
			Assembly assembly = LoadAssembly(file);
			if (assembly != null)
			{
				foreach (Type item in GetAllTypesOf<TService>(assembly))
				{
					allTypes.Add(item);
				}
				return true;
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while loading types in assembly: " + file);
		}
		return false;
	}

	private static Assembly LoadAssembly(string file)
	{
		try
		{
			return Assembly.Load(AssemblyName.GetAssemblyName(file));
		}
		catch (BadImageFormatException)
		{
		}
		catch
		{
			try
			{
				return Assembly.LoadFrom(file);
			}
			catch (BadImageFormatException)
			{
			}
		}
		return null;
	}

	internal static List<Type> GetAllTypesOf<TService>(Assembly assembly)
	{
		try
		{
			return (from x in assembly.GetTypes()
				where typeof(TService).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface
				select x).ToList();
		}
		catch (ReflectionTypeLoadException)
		{
			return new List<Type>();
		}
	}
}
