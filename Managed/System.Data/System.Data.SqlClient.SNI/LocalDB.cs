using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Data.SqlClient.SNI;

internal sealed class LocalDB
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int LocalDBStartInstance([In][MarshalAs(UnmanagedType.LPWStr)] string localDBInstanceName, [In] int flags, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder sqlConnectionDataSource, [In][Out] ref int bufferLength);

	internal enum LocalDBErrorState
	{
		NO_INSTALLATION,
		INVALID_CONFIG,
		NO_SQLUSERINSTANCEDLL_PATH,
		INVALID_SQLUSERINSTANCEDLL_PATH,
		NONE
	}

	private static readonly LocalDB Instance = new LocalDB();

	private const string LocalDBInstalledVersionRegistryKey = "SOFTWARE\\Microsoft\\Microsoft SQL Server Local DB\\Installed Versions\\";

	private const string InstanceAPIPathValueName = "InstanceAPIPath";

	private const string ProcLocalDBStartInstance = "LocalDBStartInstance";

	private const int MAX_LOCAL_DB_CONNECTION_STRING_SIZE = 260;

	private IntPtr _startInstanceHandle = IntPtr.Zero;

	private LocalDBStartInstance localDBStartInstanceFunc;

	private volatile Microsoft.Win32.SafeHandles.SafeLibraryHandle _sqlUserInstanceLibraryHandle;

	private LocalDB()
	{
	}

	internal static string GetLocalDBConnectionString(string localDbInstance)
	{
		if (!Instance.LoadUserInstanceDll())
		{
			return null;
		}
		return Instance.GetConnectionString(localDbInstance);
	}

	internal static IntPtr GetProcAddress(string functionName)
	{
		if (!Instance.LoadUserInstanceDll())
		{
			return IntPtr.Zero;
		}
		return global::Interop.Kernel32.GetProcAddress(Instance._sqlUserInstanceLibraryHandle, functionName);
	}

	private string GetConnectionString(string localDbInstance)
	{
		StringBuilder stringBuilder = new StringBuilder(261);
		int bufferLength = stringBuilder.Capacity;
		localDBStartInstanceFunc(localDbInstance, 0, stringBuilder, ref bufferLength);
		return stringBuilder.ToString();
	}

	internal static uint MapLocalDBErrorStateToCode(LocalDBErrorState errorState)
	{
		return errorState switch
		{
			LocalDBErrorState.NO_INSTALLATION => 52u, 
			LocalDBErrorState.INVALID_CONFIG => 53u, 
			LocalDBErrorState.NO_SQLUSERINSTANCEDLL_PATH => 54u, 
			LocalDBErrorState.INVALID_SQLUSERINSTANCEDLL_PATH => 55u, 
			LocalDBErrorState.NONE => 0u, 
			_ => 53u, 
		};
	}

	private bool LoadUserInstanceDll()
	{
		if (_sqlUserInstanceLibraryHandle != null)
		{
			return true;
		}
		lock (this)
		{
			if (_sqlUserInstanceLibraryHandle != null)
			{
				return true;
			}
			LocalDBErrorState errorState;
			string userInstanceDllPath = GetUserInstanceDllPath(out errorState);
			if (userInstanceDllPath == null)
			{
				SNILoadHandle.SingletonInstance.LastError = new SNIError(SNIProviders.INVALID_PROV, 0u, MapLocalDBErrorStateToCode(errorState), string.Empty);
				return false;
			}
			if (string.IsNullOrWhiteSpace(userInstanceDllPath))
			{
				SNILoadHandle.SingletonInstance.LastError = new SNIError(SNIProviders.INVALID_PROV, 0u, 55u, string.Empty);
				return false;
			}
			Microsoft.Win32.SafeHandles.SafeLibraryHandle safeLibraryHandle = global::Interop.Kernel32.LoadLibraryExW(userInstanceDllPath.Trim(), IntPtr.Zero, 0u);
			if (safeLibraryHandle.IsInvalid)
			{
				SNILoadHandle.SingletonInstance.LastError = new SNIError(SNIProviders.INVALID_PROV, 0u, 56u, string.Empty);
				safeLibraryHandle.Dispose();
				return false;
			}
			_startInstanceHandle = global::Interop.Kernel32.GetProcAddress(safeLibraryHandle, "LocalDBStartInstance");
			if (_startInstanceHandle == IntPtr.Zero)
			{
				SNILoadHandle.SingletonInstance.LastError = new SNIError(SNIProviders.INVALID_PROV, 0u, 57u, string.Empty);
				safeLibraryHandle.Dispose();
				return false;
			}
			localDBStartInstanceFunc = (LocalDBStartInstance)Marshal.GetDelegateForFunctionPointer(_startInstanceHandle, typeof(LocalDBStartInstance));
			if (localDBStartInstanceFunc == null)
			{
				SNILoadHandle.SingletonInstance.LastError = new SNIError(SNIProviders.INVALID_PROV, 0u, 57u, string.Empty);
				safeLibraryHandle.Dispose();
				_startInstanceHandle = IntPtr.Zero;
				return false;
			}
			_sqlUserInstanceLibraryHandle = safeLibraryHandle;
			return true;
		}
	}

	private string GetUserInstanceDllPath(out LocalDBErrorState errorState)
	{
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server Local DB\\Installed Versions\\");
		if (registryKey == null)
		{
			errorState = LocalDBErrorState.NO_INSTALLATION;
			return null;
		}
		Version version = new Version();
		Version version2 = version;
		string[] subKeyNames = registryKey.GetSubKeyNames();
		for (int i = 0; i < subKeyNames.Length; i++)
		{
			if (!Version.TryParse(subKeyNames[i], out var result))
			{
				errorState = LocalDBErrorState.INVALID_CONFIG;
				return null;
			}
			if (version2.CompareTo(result) < 0)
			{
				version2 = result;
			}
		}
		if (version2.Equals(version))
		{
			errorState = LocalDBErrorState.INVALID_CONFIG;
			return null;
		}
		using RegistryKey registryKey2 = registryKey.OpenSubKey(version2.ToString());
		object value = registryKey2.GetValue("InstanceAPIPath");
		if (value == null)
		{
			errorState = LocalDBErrorState.NO_SQLUSERINSTANCEDLL_PATH;
			return null;
		}
		if (registryKey2.GetValueKind("InstanceAPIPath") != RegistryValueKind.String)
		{
			errorState = LocalDBErrorState.INVALID_SQLUSERINSTANCEDLL_PATH;
			return null;
		}
		string result2 = (string)value;
		errorState = LocalDBErrorState.NONE;
		return result2;
	}
}
