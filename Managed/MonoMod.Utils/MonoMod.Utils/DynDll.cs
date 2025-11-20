using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoMod.Utils;

public static class DynDll
{
	public static Dictionary<string, DynDllMapping> Mappings = new Dictionary<string, DynDllMapping>();

	private const int RTLD_LAZY = 1;

	private const int RTLD_NOW = 2;

	private const int RTLD_LOCAL = 0;

	private const int RTLD_GLOBAL = 256;

	[DllImport("kernel32", SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32", SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32", SetLastError = true)]
	private static extern bool FreeLibrary(IntPtr hLibModule);

	[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPTStr)] string filename, int flags);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool dlclose(IntPtr handle);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPTStr)] string symbol);

	[DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr dlerror();

	private static T _CheckError<T>(T valueIn)
	{
		if (!_CheckError(valueIn, out var valueOut, out var e))
		{
			throw e;
		}
		return valueOut;
	}

	private static bool _CheckError<T>(T valueIn, out T valueOut, out Exception e)
	{
		if (PlatformHelper.Is(Platform.Windows))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				valueOut = default(T);
				e = new Win32Exception(lastWin32Error);
				return false;
			}
		}
		else
		{
			IntPtr intPtr = dlerror();
			if (intPtr != IntPtr.Zero)
			{
				valueOut = default(T);
				e = new Win32Exception(Marshal.PtrToStringAnsi(intPtr));
				return false;
			}
		}
		valueOut = valueIn;
		e = null;
		return true;
	}

	public static IntPtr OpenLibrary(string name, bool skipMapping = false, int? flags = null)
	{
		if (!_CheckError(_OpenLibrary(name, skipMapping, flags), out var valueOut, out var e))
		{
			throw e;
		}
		return valueOut;
	}

	public static bool TryOpenLibrary(string name, out IntPtr lib, bool skipMapping = false, int? flags = null)
	{
		Exception e;
		return _CheckError(_OpenLibrary(name, skipMapping, flags), out lib, out e);
	}

	public static IntPtr _OpenLibrary(string name, bool skipMapping, int? flags)
	{
		if (name != null && !skipMapping && Mappings.TryGetValue(name, out var value))
		{
			name = value.ResolveAs ?? name;
			flags = value.Flags ?? flags;
		}
		if (PlatformHelper.Is(Platform.Windows))
		{
			if (name == null)
			{
				return GetModuleHandle(name);
			}
			return LoadLibrary(name);
		}
		int flags2 = flags ?? 258;
		IntPtr intPtr = dlopen(name, flags2);
		if (intPtr == IntPtr.Zero && File.Exists(name))
		{
			intPtr = dlopen(Path.GetFullPath(name), flags2);
		}
		return intPtr;
	}

	public static bool CloseLibrary(IntPtr lib)
	{
		if (PlatformHelper.Is(Platform.Windows))
		{
			return _CheckError(CloseLibrary(lib));
		}
		return _CheckError(dlclose(lib));
	}

	public static IntPtr GetFunction(this IntPtr lib, string name)
	{
		if (!_CheckError(_GetFunction(lib, name), out var valueOut, out var e))
		{
			throw e;
		}
		return valueOut;
	}

	public static bool TryGetFunction(this IntPtr lib, string name, out IntPtr ptr)
	{
		Exception e;
		return _CheckError(_GetFunction(lib, name), out ptr, out e);
	}

	private static IntPtr _GetFunction(IntPtr lib, string name)
	{
		if (lib == IntPtr.Zero)
		{
			throw new ArgumentNullException("lib");
		}
		if (PlatformHelper.Is(Platform.Windows))
		{
			return GetProcAddress(lib, name);
		}
		return dlsym(lib, name);
	}

	public static T AsDelegate<T>(this IntPtr s) where T : class
	{
		return Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
	}

	public static void ResolveDynDllImports(this Type type, Dictionary<string, DynDllMapping> mappings = null)
	{
		_ResolveDynDllImports(type, null, mappings);
	}

	public static void ResolveDynDllImports(object instance, Dictionary<string, DynDllMapping> mappings = null)
	{
		_ResolveDynDllImports(instance.GetType(), instance, mappings);
	}

	private static void _ResolveDynDllImports(Type type, object instance, Dictionary<string, DynDllMapping> mappings)
	{
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
		bindingFlags = ((instance != null) ? (bindingFlags | BindingFlags.Instance) : (bindingFlags | BindingFlags.Static));
		FieldInfo[] fields = type.GetFields(bindingFlags);
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag = true;
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DynDllImportAttribute), inherit: true);
			for (int j = 0; j < customAttributes.Length; j++)
			{
				DynDllImportAttribute dynDllImportAttribute = (DynDllImportAttribute)customAttributes[j];
				flag = false;
				bool skipMapping = false;
				string text = dynDllImportAttribute.DLL;
				int? num = null;
				if (mappings != null && (skipMapping = mappings.TryGetValue(text, out var value)))
				{
					text = value.ResolveAs ?? text;
					num = value.Flags ?? num;
				}
				if (!TryOpenLibrary(text, out var lib, skipMapping, num))
				{
					continue;
				}
				foreach (string item in dynDllImportAttribute.EntryPoints.Concat(new string[2]
				{
					fieldInfo.Name,
					fieldInfo.FieldType.Name
				}))
				{
					if (lib.TryGetFunction(item, out var ptr))
					{
						fieldInfo.SetValue(instance, Marshal.GetDelegateForFunctionPointer(ptr, fieldInfo.FieldType));
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				throw new EntryPointNotFoundException("No matching entry point found for " + fieldInfo.Name + " in " + fieldInfo.DeclaringType.FullName);
			}
		}
	}
}
