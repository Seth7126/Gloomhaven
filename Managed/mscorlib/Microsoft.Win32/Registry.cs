using System;

namespace Microsoft.Win32;

/// <summary>Provides <see cref="T:Microsoft.Win32.RegistryKey" /> objects that represent the root keys in the Windows registry, and static methods to access key/value pairs.</summary>
public static class Registry
{
	/// <summary>Contains information about the current user preferences. This field reads the Windows registry base key HKEY_CURRENT_USER </summary>
	public static readonly RegistryKey CurrentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);

	/// <summary>Contains the configuration data for the local machine. This field reads the Windows registry base key HKEY_LOCAL_MACHINE.</summary>
	public static readonly RegistryKey LocalMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);

	/// <summary>Defines the types (or classes) of documents and the properties associated with those types. This field reads the Windows registry base key HKEY_CLASSES_ROOT.</summary>
	public static readonly RegistryKey ClassesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);

	/// <summary>Contains information about the default user configuration. This field reads the Windows registry base key HKEY_USERS.</summary>
	public static readonly RegistryKey Users = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);

	/// <summary>Contains performance information for software components. This field reads the Windows registry base key HKEY_PERFORMANCE_DATA.</summary>
	public static readonly RegistryKey PerformanceData = RegistryKey.OpenBaseKey(RegistryHive.PerformanceData, RegistryView.Default);

	/// <summary>Contains configuration information pertaining to the hardware that is not specific to the user. This field reads the Windows registry base key HKEY_CURRENT_CONFIG.</summary>
	public static readonly RegistryKey CurrentConfig = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Default);

	/// <summary>Contains dynamic registry data. This field reads the Windows registry base key HKEY_DYN_DATA.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The operating system does not support dynamic data; that is, it is not Windows 98, Windows 98 Second Edition, or Windows Millennium Edition (Windows Me).</exception>
	[Obsolete("Use PerformanceData instead")]
	public static readonly RegistryKey DynData = RegistryKey.OpenBaseKey(RegistryHive.DynData, RegistryView.Default);

	/// <summary>Retrieves the value associated with the specified name, in the specified registry key. If the name is not found in the specified key, returns a default value that you provide, or null if the specified key does not exist. </summary>
	/// <returns>null if the subkey specified by <paramref name="keyName" /> does not exist; otherwise, the value associated with <paramref name="valueName" />, or <paramref name="defaultValue" /> if <paramref name="valueName" /> is not found.</returns>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="defaultValue">The value to return if <paramref name="valueName" /> does not exist.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public static object GetValue(string keyName, string valueName, object defaultValue)
	{
		string subKeyName;
		using RegistryKey registryKey = GetBaseKeyFromKeyName(keyName, out subKeyName).OpenSubKey(subKeyName);
		return registryKey?.GetValue(valueName, defaultValue);
	}

	/// <summary>Sets the specified name/value pair on the specified registry key. If the specified key does not exist, it is created.</summary>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="value">The value to be stored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root. -or-<paramref name="keyName" /> is longer than the maximum length allowed (255 characters).</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and thus cannot be written to; for example, it is a root-level node. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static void SetValue(string keyName, string valueName, object value)
	{
		SetValue(keyName, valueName, value, RegistryValueKind.Unknown);
	}

	/// <summary>Sets the name/value pair on the specified registry key, using the specified registry data type. If the specified key does not exist, it is created.</summary>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="value">The value to be stored.</param>
	/// <param name="valueKind">The registry data type to use when storing the data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root.-or-<paramref name="keyName" /> is longer than the maximum length allowed (255 characters).-or- The type of <paramref name="value" /> did not match the registry data type specified by <paramref name="valueKind" />, therefore the data could not be converted properly. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and thus cannot be written to; for example, it is a root-level node, or the key has not been opened with write access. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
	{
		string subKeyName;
		using RegistryKey registryKey = GetBaseKeyFromKeyName(keyName, out subKeyName).CreateSubKey(subKeyName);
		registryKey.SetValue(valueName, value, valueKind);
	}

	private static RegistryKey GetBaseKeyFromKeyName(string keyName, out string subKeyName)
	{
		if (keyName == null)
		{
			throw new ArgumentNullException("keyName");
		}
		int num = keyName.IndexOf('\\');
		int num2 = ((num != -1) ? num : keyName.Length);
		RegistryKey registryKey = null;
		switch (num2)
		{
		case 10:
			registryKey = Users;
			break;
		case 17:
			registryKey = ((char.ToUpperInvariant(keyName[6]) == 'L') ? ClassesRoot : CurrentUser);
			break;
		case 18:
			registryKey = LocalMachine;
			break;
		case 19:
			registryKey = CurrentConfig;
			break;
		case 21:
			registryKey = PerformanceData;
			break;
		}
		if (registryKey != null && keyName.StartsWith(registryKey.Name, StringComparison.OrdinalIgnoreCase))
		{
			subKeyName = ((num == -1 || num == keyName.Length) ? string.Empty : keyName.Substring(num + 1, keyName.Length - num - 1));
			return registryKey;
		}
		throw new ArgumentException(SR.Format("Registry key name must start with a valid base key name.", "keyName"), "keyName");
	}
}
