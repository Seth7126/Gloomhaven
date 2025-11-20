using System;
using System.Runtime.InteropServices;

public class SystemDynamicLibrary
{
	private static SystemDynamicLibrary s_instance;

	private const string DLLHBinaryName = "DynamicLibraryLoaderHelper";

	private IntPtr DLLHContex;

	public static SystemDynamicLibrary Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new SystemDynamicLibrary();
			}
			return s_instance;
		}
	}

	[DllImport("DynamicLibraryLoaderHelper")]
	private static extern IntPtr DLLH_create_context();

	[DllImport("DynamicLibraryLoaderHelper")]
	private static extern void DLLH_destroy_context(IntPtr context);

	[DllImport("DynamicLibraryLoaderHelper", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr DLLH_load_library_at_path(IntPtr ctx, string library_path);

	[DllImport("DynamicLibraryLoaderHelper")]
	private static extern bool DLLH_unload_library_at_path(IntPtr ctx, IntPtr library_handle);

	[DllImport("DynamicLibraryLoaderHelper", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr DLLH_load_function_with_name(IntPtr ctx, IntPtr library_handle, string function);

	private SystemDynamicLibrary()
	{
		DLLHContex = DLLH_create_context();
	}

	public static IntPtr GetHandleForModule(string moduleName)
	{
		return IntPtr.Zero;
	}

	public IntPtr LoadLibraryAtPath(string libraryPath)
	{
		return DLLH_load_library_at_path(DLLHContex, libraryPath);
	}

	public bool UnloadLibrary(IntPtr libraryHandle)
	{
		return DLLH_unload_library_at_path(DLLHContex, libraryHandle);
	}

	public IntPtr LoadFunctionWithName(IntPtr libraryHandle, string functionName)
	{
		return DLLH_load_function_with_name(DLLHContex, libraryHandle, functionName);
	}
}
