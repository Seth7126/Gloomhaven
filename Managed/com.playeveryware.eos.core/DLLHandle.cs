using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

public class DLLHandle : SafeHandle
{
	public override bool IsInvalid => handle == IntPtr.Zero;

	[Conditional("ENABLE_DLLHANDLE_PRINT")]
	private static void print(string toPrint)
	{
		UnityEngine.Debug.Log(toPrint);
	}

	public static List<string> GetPathsToPlugins()
	{
		Path.Combine(Application.streamingAssetsPath, "..", "..");
		string item = Path.Combine(Application.dataPath, "Plugins");
		string fullPath = Path.GetFullPath(Path.Combine("Packages", EOSPackageInfo.GetPackageName(), "Runtime"));
		List<string> pluginPaths = new List<string>();
		pluginPaths.Add(item);
		pluginPaths.Add(fullPath);
		if (EOSManagerPlatformSpecifics.Instance != null)
		{
			EOSManagerPlatformSpecifics.Instance.AddPluginSearchPaths(ref pluginPaths);
		}
		for (int num = pluginPaths.Count - 1; num >= 0; num--)
		{
			_ = pluginPaths[num];
		}
		for (int num2 = pluginPaths.Count - 1; num2 >= 0; num2--)
		{
			if (!Directory.Exists(pluginPaths[num2]))
			{
				pluginPaths.RemoveAt(num2);
			}
		}
		return pluginPaths;
	}

	public static string GetVersionForLibrary(string libraryName)
	{
		List<string> pathsToPlugins = GetPathsToPlugins();
		string text = ".dll";
		foreach (string item in pathsToPlugins)
		{
			foreach (string item2 in Directory.EnumerateFileSystemEntries(item, libraryName + text, SearchOption.AllDirectories))
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(item2);
				if (versionInfo != null)
				{
					return $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
				}
			}
		}
		return null;
	}

	public static string GetProductVersionForLibrary(string libraryName)
	{
		List<string> pathsToPlugins = GetPathsToPlugins();
		string text = ".dll";
		foreach (string item in pathsToPlugins)
		{
			foreach (string item2 in Directory.EnumerateFileSystemEntries(item, libraryName + text, SearchOption.AllDirectories))
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(item2);
				if (versionInfo != null)
				{
					return string.Format(versionInfo.ProductVersion);
				}
			}
		}
		return null;
	}

	public static string GetPathForLibrary(string libraryName)
	{
		List<string> pathsToPlugins = GetPathsToPlugins();
		string text = ".dll";
		foreach (string item in pathsToPlugins)
		{
			using IEnumerator<string> enumerator2 = Directory.EnumerateFileSystemEntries(item, libraryName + text, SearchOption.AllDirectories).GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return enumerator2.Current;
			}
		}
		return null;
	}

	public static DLLHandle LoadDynamicLibrary(string libraryName)
	{
		List<string> pathsToPlugins = GetPathsToPlugins();
		string text = ((EOSManagerPlatformSpecifics.Instance != null) ? EOSManagerPlatformSpecifics.Instance.GetDynamicLibraryExtension() : ".dll");
		foreach (string item in pathsToPlugins)
		{
			using IEnumerator<string> enumerator2 = Directory.EnumerateFileSystemEntries(item, libraryName + text, SearchOption.AllDirectories).GetEnumerator();
			if (enumerator2.MoveNext())
			{
				string current = enumerator2.Current;
				IntPtr intPtr = SystemDynamicLibrary.Instance.LoadLibraryAtPath(current);
				if (intPtr != IntPtr.Zero)
				{
					return new DLLHandle(intPtr);
				}
				throw new Win32Exception("Searched in : " + string.Join(" ", pathsToPlugins));
			}
		}
		return null;
	}

	public DLLHandle(IntPtr intPtr, bool value = true)
		: base(intPtr, ownsHandle: true)
	{
		SetHandle(intPtr);
	}

	protected override bool ReleaseHandle()
	{
		if (handle == IntPtr.Zero)
		{
			return true;
		}
		bool result = SystemDynamicLibrary.Instance.UnloadLibrary(handle);
		SetHandle(IntPtr.Zero);
		return result;
	}

	public Delegate LoadFunctionAsDelegate(Type functionType, string functionName)
	{
		return LoadFunctionAsDelegate(handle, functionType, functionName);
	}

	public IntPtr LoadFunctionAsIntPtr(string functionName)
	{
		return SystemDynamicLibrary.Instance.LoadFunctionWithName(handle, functionName);
	}

	public void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(Type clazz, Type delegateType, string functionName)
	{
		ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(handle, clazz, delegateType, functionName);
	}

	private static void ConfigureFromLibraryDelegateFieldOnClassWithFunctionName(IntPtr libraryHandle, Type clazz, Type delegateType, string functionName)
	{
		Delegate value = LoadFunctionAsDelegate(libraryHandle, delegateType, functionName);
		clazz.GetField(functionName).SetValue(null, value);
	}

	public static Delegate LoadFunctionAsDelegate(IntPtr libraryHandle, Type functionType, string functionName)
	{
		if (libraryHandle == IntPtr.Zero)
		{
			throw new Exception("libraryHandle is null");
		}
		if (functionType == null)
		{
			throw new Exception("null function type?");
		}
		IntPtr intPtr = SystemDynamicLibrary.Instance.LoadFunctionWithName(libraryHandle, functionName);
		if (intPtr == IntPtr.Zero)
		{
			throw new Exception("Function not found: " + functionName);
		}
		return Marshal.GetDelegateForFunctionPointer(intPtr, functionType);
	}
}
