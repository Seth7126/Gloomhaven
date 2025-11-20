using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.IO;

/// <summary>Provides the base class for both <see cref="T:System.IO.FileInfo" /> and <see cref="T:System.IO.DirectoryInfo" /> objects.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public abstract class FileSystemInfo : MarshalByRefObject, ISerializable
{
	private Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA _data;

	private int _dataInitialized = -1;

	/// <summary>Represents the fully qualified path of the directory or file.</summary>
	/// <exception cref="T:System.IO.PathTooLongException">The fully qualified path is 260 or more characters.</exception>
	protected string FullPath;

	/// <summary>The path originally specified by the user, whether relative or absolute.</summary>
	protected string OriginalPath;

	internal string _name;

	/// <summary>Gets or sets the attributes for the current file or directory.</summary>
	/// <returns>
	///   <see cref="T:System.IO.FileAttributes" /> of the current <see cref="T:System.IO.FileSystemInfo" />.</returns>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file does not exist. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">The caller attempts to set an invalid file attribute. -or-The user attempts to set an attribute value but does not have write permission.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public FileAttributes Attributes
	{
		get
		{
			EnsureDataInitialized();
			return (FileAttributes)_data.dwFileAttributes;
		}
		set
		{
			FileSystem.SetAttributes(FullPath, value);
			_dataInitialized = -1;
		}
	}

	internal bool ExistsCore
	{
		get
		{
			if (_dataInitialized == -1)
			{
				Refresh();
			}
			if (_dataInitialized != 0)
			{
				return false;
			}
			if (_data.dwFileAttributes != -1)
			{
				return this is DirectoryInfo == ((_data.dwFileAttributes & 0x10) == 16);
			}
			return false;
		}
	}

	internal DateTimeOffset CreationTimeCore
	{
		get
		{
			EnsureDataInitialized();
			return _data.ftCreationTime.ToDateTimeOffset();
		}
		set
		{
			FileSystem.SetCreationTime(FullPath, value, this is DirectoryInfo);
			_dataInitialized = -1;
		}
	}

	internal DateTimeOffset LastAccessTimeCore
	{
		get
		{
			EnsureDataInitialized();
			return _data.ftLastAccessTime.ToDateTimeOffset();
		}
		set
		{
			FileSystem.SetLastAccessTime(FullPath, value, this is DirectoryInfo);
			_dataInitialized = -1;
		}
	}

	internal DateTimeOffset LastWriteTimeCore
	{
		get
		{
			EnsureDataInitialized();
			return _data.ftLastWriteTime.ToDateTimeOffset();
		}
		set
		{
			FileSystem.SetLastWriteTime(FullPath, value, this is DirectoryInfo);
			_dataInitialized = -1;
		}
	}

	internal long LengthCore
	{
		get
		{
			EnsureDataInitialized();
			return (long)(((ulong)_data.nFileSizeHigh << 32) | ((ulong)_data.nFileSizeLow & 0xFFFFFFFFuL));
		}
	}

	internal string NormalizedPath
	{
		get
		{
			if (!PathInternal.EndsWithPeriodOrSpace(FullPath))
			{
				return FullPath;
			}
			return PathInternal.EnsureExtendedPrefix(FullPath);
		}
	}

	/// <summary>Gets the full path of the directory or file.</summary>
	/// <returns>A string containing the full path.</returns>
	/// <exception cref="T:System.IO.PathTooLongException">The fully qualified path and file name is 260 or more characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string FullName => FullPath;

	/// <summary>Gets the string representing the extension part of the file.</summary>
	/// <returns>A string containing the <see cref="T:System.IO.FileSystemInfo" /> extension.</returns>
	/// <filterpriority>1</filterpriority>
	public string Extension
	{
		get
		{
			int length = FullPath.Length;
			int num = length;
			while (--num >= 0)
			{
				char c = FullPath[num];
				if (c == '.')
				{
					return FullPath.Substring(num, length - num);
				}
				if (PathInternal.IsDirectorySeparator(c) || c == Path.VolumeSeparatorChar)
				{
					break;
				}
			}
			return string.Empty;
		}
	}

	/// <summary>For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists. Otherwise, the Name property gets the name of the directory.</summary>
	/// <returns>A string that is the name of the parent directory, the name of the last directory in the hierarchy, or the name of a file, including the file name extension.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual string Name => _name;

	/// <summary>Gets a value indicating whether the file or directory exists.</summary>
	/// <returns>true if the file or directory exists; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual bool Exists
	{
		get
		{
			try
			{
				return ExistsCore;
			}
			catch
			{
				return false;
			}
		}
	}

	/// <summary>Gets or sets the creation time of the current file or directory.</summary>
	/// <returns>The creation date and time of the current <see cref="T:System.IO.FileSystemInfo" /> object.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid creation time.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime CreationTime
	{
		get
		{
			return CreationTimeUtc.ToLocalTime();
		}
		set
		{
			CreationTimeUtc = value.ToUniversalTime();
		}
	}

	/// <summary>Gets or sets the creation time, in coordinated universal time (UTC), of the current file or directory.</summary>
	/// <returns>The creation date and time in UTC format of the current <see cref="T:System.IO.FileSystemInfo" /> object.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid access time.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime CreationTimeUtc
	{
		get
		{
			return CreationTimeCore.UtcDateTime;
		}
		set
		{
			CreationTimeCore = File.GetUtcDateTimeOffset(value);
		}
	}

	/// <summary>Gets or sets the time the current file or directory was last accessed.</summary>
	/// <returns>The time that the current file or directory was last accessed.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid access time</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime LastAccessTime
	{
		get
		{
			return LastAccessTimeUtc.ToLocalTime();
		}
		set
		{
			LastAccessTimeUtc = value.ToUniversalTime();
		}
	}

	/// <summary>Gets or sets the time, in coordinated universal time (UTC), that the current file or directory was last accessed.</summary>
	/// <returns>The UTC time that the current file or directory was last accessed.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid access time.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime LastAccessTimeUtc
	{
		get
		{
			return LastAccessTimeCore.UtcDateTime;
		}
		set
		{
			LastAccessTimeCore = File.GetUtcDateTimeOffset(value);
		}
	}

	/// <summary>Gets or sets the time when the current file or directory was last written to.</summary>
	/// <returns>The time the current file was last written.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid write time.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime LastWriteTime
	{
		get
		{
			return LastWriteTimeUtc.ToLocalTime();
		}
		set
		{
			LastWriteTimeUtc = value.ToUniversalTime();
		}
	}

	/// <summary>Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.</summary>
	/// <returns>The UTC time when the current file was last written to.</returns>
	/// <exception cref="T:System.IO.IOException">
	///   <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid write time.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DateTime LastWriteTimeUtc
	{
		get
		{
			return LastWriteTimeCore.UtcDateTime;
		}
		set
		{
			LastWriteTimeCore = File.GetUtcDateTimeOffset(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileSystemInfo" /> class.</summary>
	protected FileSystemInfo()
	{
	}

	internal unsafe static FileSystemInfo Create(string fullPath, ref FileSystemEntry findData)
	{
		FileSystemInfo obj = (findData.IsDirectory ? ((FileSystemInfo)new DirectoryInfo(fullPath, null, new string(findData.FileName), isNormalized: true)) : ((FileSystemInfo)new FileInfo(fullPath, null, new string(findData.FileName), isNormalized: true)));
		obj.Init(findData._info);
		return obj;
	}

	internal void Invalidate()
	{
		_dataInitialized = -1;
	}

	internal unsafe void Init(Interop.NtDll.FILE_FULL_DIR_INFORMATION* info)
	{
		_data.dwFileAttributes = (int)info->FileAttributes;
		_data.ftCreationTime = *(Interop.Kernel32.FILE_TIME*)(&info->CreationTime);
		_data.ftLastAccessTime = *(Interop.Kernel32.FILE_TIME*)(&info->LastAccessTime);
		_data.ftLastWriteTime = *(Interop.Kernel32.FILE_TIME*)(&info->LastWriteTime);
		_data.nFileSizeHigh = (uint)(info->EndOfFile >> 32);
		_data.nFileSizeLow = (uint)info->EndOfFile;
		_dataInitialized = 0;
	}

	private void EnsureDataInitialized()
	{
		if (_dataInitialized == -1)
		{
			_data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
			Refresh();
		}
		if (_dataInitialized != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(_dataInitialized, FullPath);
		}
	}

	/// <summary>Refreshes the state of the object.</summary>
	/// <exception cref="T:System.IO.IOException">A device such as a disk drive is not ready. </exception>
	/// <filterpriority>1</filterpriority>
	public void Refresh()
	{
		_dataInitialized = FileSystem.FillAttributeInfo(FullPath, ref _data, returnErrorOnNotFound: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileSystemInfo" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> is null.</exception>
	protected FileSystemInfo(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		FullPath = Path.GetFullPathInternal(info.GetString("FullPath"));
		OriginalPath = info.GetString("OriginalPath");
		_name = info.GetString("Name");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the file name and additional exception information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[ComVisible(false)]
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("OriginalPath", OriginalPath, typeof(string));
		info.AddValue("FullPath", FullPath, typeof(string));
		info.AddValue("Name", Name, typeof(string));
	}

	/// <summary>Deletes a file or directory.</summary>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive.</exception>
	/// <exception cref="T:System.IO.IOException">There is an open handle on the file or directory, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
	/// <filterpriority>2</filterpriority>
	public abstract void Delete();

	public override string ToString()
	{
		return OriginalPath ?? string.Empty;
	}
}
