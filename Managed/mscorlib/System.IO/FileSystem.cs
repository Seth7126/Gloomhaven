using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.IO;

internal static class FileSystem
{
	internal const int GENERIC_READ = int.MinValue;

	public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite)
	{
		int num = Interop.Kernel32.CopyFile(sourceFullPath, destFullPath, !overwrite);
		if (num == 0)
		{
			return;
		}
		string path = destFullPath;
		if (num != 80)
		{
			using (SafeFileHandle safeFileHandle = Interop.Kernel32.CreateFile(sourceFullPath, int.MinValue, FileShare.Read, FileMode.Open, 0))
			{
				if (safeFileHandle.IsInvalid)
				{
					path = sourceFullPath;
				}
			}
			if (num == 5 && DirectoryExists(destFullPath))
			{
				throw new IOException(SR.Format("The target file '{0}' is a directory, not a file.", destFullPath), 5);
			}
		}
		throw Win32Marshal.GetExceptionForWin32Error(num, path);
	}

	public static void ReplaceFile(string sourceFullPath, string destFullPath, string destBackupFullPath, bool ignoreMetadataErrors)
	{
		int dwReplaceFlags = (ignoreMetadataErrors ? 2 : 0);
		if (!Interop.Kernel32.ReplaceFile(destFullPath, sourceFullPath, destBackupFullPath, dwReplaceFlags, IntPtr.Zero, IntPtr.Zero))
		{
			throw Win32Marshal.GetExceptionForWin32Error(Marshal.GetLastWin32Error());
		}
	}

	public static void CreateDirectory(string fullPath)
	{
		if (DirectoryExists(fullPath))
		{
			return;
		}
		List<string> list = new List<string>();
		bool flag = false;
		int num = fullPath.Length;
		if (num >= 2 && PathInternal.EndsInDirectorySeparator(fullPath))
		{
			num--;
		}
		int rootLength = PathInternal.GetRootLength(fullPath);
		if (num > rootLength)
		{
			int num2 = num - 1;
			while (num2 >= rootLength && !flag)
			{
				string text = fullPath.Substring(0, num2 + 1);
				if (!DirectoryExists(text))
				{
					list.Add(text);
				}
				else
				{
					flag = true;
				}
				while (num2 > rootLength && !PathInternal.IsDirectorySeparator(fullPath[num2]))
				{
					num2--;
				}
				num2--;
			}
		}
		int count = list.Count;
		Interop.Kernel32.SECURITY_ATTRIBUTES lpSecurityAttributes = default(Interop.Kernel32.SECURITY_ATTRIBUTES);
		bool flag2 = true;
		int num3 = 0;
		string path = fullPath;
		while (list.Count > 0)
		{
			string text2 = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			flag2 = Interop.Kernel32.CreateDirectory(text2, ref lpSecurityAttributes);
			if (!flag2 && num3 == 0)
			{
				int lastError = Marshal.GetLastWin32Error();
				if (lastError != 183)
				{
					num3 = lastError;
				}
				else if (FileExists(text2) || (!DirectoryExists(text2, out lastError) && lastError == 5))
				{
					num3 = lastError;
					path = text2;
				}
			}
		}
		if (count == 0 && !flag)
		{
			string text3 = Directory.InternalGetDirectoryRoot(fullPath);
			if (!DirectoryExists(text3))
			{
				throw Win32Marshal.GetExceptionForWin32Error(3, text3);
			}
		}
		else if (!flag2 && num3 != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(num3, path);
		}
	}

	public static void DeleteFile(string fullPath)
	{
		if (!Interop.Kernel32.DeleteFile(fullPath))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 2)
			{
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
			}
		}
	}

	public static bool DirectoryExists(string fullPath)
	{
		int lastError;
		return DirectoryExists(fullPath, out lastError);
	}

	private static bool DirectoryExists(string path, out int lastError)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		lastError = FillAttributeInfo(path, ref data, returnErrorOnNotFound: true);
		if (lastError == 0 && data.dwFileAttributes != -1)
		{
			return (data.dwFileAttributes & 0x10) != 0;
		}
		return false;
	}

	internal static int FillAttributeInfo(string path, ref Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data, bool returnErrorOnNotFound)
	{
		int num = 0;
		path = PathInternal.TrimEndingDirectorySeparator(path);
		using (DisableMediaInsertionPrompt.Create())
		{
			if (!Interop.Kernel32.GetFileAttributesEx(path, Interop.Kernel32.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, ref data))
			{
				num = Marshal.GetLastWin32Error();
				if (num != 2 && num != 3 && num != 21 && num != 123 && num != 161 && num != 53 && num != 67 && num != 87 && num != 1231)
				{
					Interop.Kernel32.WIN32_FIND_DATA data2 = default(Interop.Kernel32.WIN32_FIND_DATA);
					using SafeFindHandle safeFindHandle = Interop.Kernel32.FindFirstFile(path, ref data2);
					if (safeFindHandle.IsInvalid)
					{
						num = Marshal.GetLastWin32Error();
					}
					else
					{
						num = 0;
						data.PopulateFrom(ref data2);
					}
				}
			}
		}
		if (num != 0 && !returnErrorOnNotFound && ((uint)(num - 2) <= 1u || num == 21))
		{
			data.dwFileAttributes = -1;
			return 0;
		}
		return num;
	}

	public static bool FileExists(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		if (FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true) == 0 && data.dwFileAttributes != -1)
		{
			return (data.dwFileAttributes & 0x10) == 0;
		}
		return false;
	}

	public static FileAttributes GetAttributes(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true);
		if (num != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
		return (FileAttributes)data.dwFileAttributes;
	}

	public static DateTimeOffset GetCreationTime(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
		if (num != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
		return data.ftCreationTime.ToDateTimeOffset();
	}

	public static FileSystemInfo GetFileSystemInfo(string fullPath, bool asDirectory)
	{
		if (!asDirectory)
		{
			return new FileInfo(fullPath, null, null, isNormalized: false);
		}
		return new DirectoryInfo(fullPath, null, null, isNormalized: false);
	}

	public static DateTimeOffset GetLastAccessTime(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
		if (num != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
		return data.ftLastAccessTime.ToDateTimeOffset();
	}

	public static DateTimeOffset GetLastWriteTime(string fullPath)
	{
		Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
		int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
		if (num != 0)
		{
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
		return data.ftLastWriteTime.ToDateTimeOffset();
	}

	public static void MoveDirectory(string sourceFullPath, string destFullPath)
	{
		if (!Interop.Kernel32.MoveFile(sourceFullPath, destFullPath))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			switch (lastWin32Error)
			{
			case 2:
				throw Win32Marshal.GetExceptionForWin32Error(3, sourceFullPath);
			case 5:
				throw new IOException(SR.Format("Access to the path '{0}' is denied.", sourceFullPath), Win32Marshal.MakeHRFromErrorCode(lastWin32Error));
			default:
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
		}
	}

	public static void MoveFile(string sourceFullPath, string destFullPath)
	{
		if (!Interop.Kernel32.MoveFile(sourceFullPath, destFullPath))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error();
		}
	}

	private static SafeFileHandle OpenHandle(string fullPath, bool asDirectory)
	{
		string text = fullPath.Substring(0, PathInternal.GetRootLength(fullPath));
		if (text == fullPath && text[1] == Path.VolumeSeparatorChar)
		{
			throw new ArgumentException("Path must not be a drive.", "path");
		}
		SafeFileHandle safeFileHandle = Interop.Kernel32.CreateFile(fullPath, 1073741824, FileShare.ReadWrite | FileShare.Delete, FileMode.Open, asDirectory ? 33554432 : 0);
		if (safeFileHandle.IsInvalid)
		{
			int num = Marshal.GetLastWin32Error();
			if (!asDirectory && num == 3 && fullPath.Equals(Directory.GetDirectoryRoot(fullPath)))
			{
				num = 5;
			}
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
		return safeFileHandle;
	}

	public static void RemoveDirectory(string fullPath, bool recursive)
	{
		if (!recursive)
		{
			RemoveDirectoryInternal(fullPath, topLevel: true);
			return;
		}
		Interop.Kernel32.WIN32_FIND_DATA findData = default(Interop.Kernel32.WIN32_FIND_DATA);
		GetFindData(fullPath, ref findData);
		if (IsNameSurrogateReparsePoint(ref findData))
		{
			RemoveDirectoryInternal(fullPath, topLevel: true);
			return;
		}
		fullPath = PathInternal.EnsureExtendedPrefix(fullPath);
		RemoveDirectoryRecursive(fullPath, ref findData, topLevel: true);
	}

	private static void GetFindData(string fullPath, ref Interop.Kernel32.WIN32_FIND_DATA findData)
	{
		using SafeFindHandle safeFindHandle = Interop.Kernel32.FindFirstFile(PathInternal.TrimEndingDirectorySeparator(fullPath), ref findData);
		if (safeFindHandle.IsInvalid)
		{
			int num = Marshal.GetLastWin32Error();
			if (num == 2)
			{
				num = 3;
			}
			throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
		}
	}

	private static bool IsNameSurrogateReparsePoint(ref Interop.Kernel32.WIN32_FIND_DATA data)
	{
		if ((data.dwFileAttributes & 0x400) != 0)
		{
			return (data.dwReserved0 & 0x20000000) != 0;
		}
		return false;
	}

	private static void RemoveDirectoryRecursive(string fullPath, ref Interop.Kernel32.WIN32_FIND_DATA findData, bool topLevel)
	{
		Exception ex = null;
		using (SafeFindHandle safeFindHandle = Interop.Kernel32.FindFirstFile(Path.Join(fullPath, "*"), ref findData))
		{
			if (safeFindHandle.IsInvalid)
			{
				throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
			}
			int lastWin32Error;
			do
			{
				if ((findData.dwFileAttributes & 0x10) == 0)
				{
					string stringFromFixedBuffer = findData.cFileName.GetStringFromFixedBuffer();
					if (!Interop.Kernel32.DeleteFile(Path.Combine(fullPath, stringFromFixedBuffer)) && ex == null)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error != 2)
						{
							ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer);
						}
					}
				}
				else
				{
					if (findData.cFileName.FixedBufferEqualsString(".") || findData.cFileName.FixedBufferEqualsString(".."))
					{
						continue;
					}
					string stringFromFixedBuffer2 = findData.cFileName.GetStringFromFixedBuffer();
					if (!IsNameSurrogateReparsePoint(ref findData))
					{
						try
						{
							RemoveDirectoryRecursive(Path.Combine(fullPath, stringFromFixedBuffer2), ref findData, topLevel: false);
						}
						catch (Exception ex2)
						{
							if (ex == null)
							{
								ex = ex2;
							}
						}
						continue;
					}
					if (findData.dwReserved0 == 2684354563u && !Interop.Kernel32.DeleteVolumeMountPoint(Path.Join(fullPath, stringFromFixedBuffer2, "\\")) && ex == null)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error != 0 && lastWin32Error != 3)
						{
							ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer2);
						}
					}
					if (!Interop.Kernel32.RemoveDirectory(Path.Combine(fullPath, stringFromFixedBuffer2)) && ex == null)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error != 3)
						{
							ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer2);
						}
					}
				}
			}
			while (Interop.Kernel32.FindNextFile(safeFindHandle, ref findData));
			if (ex != null)
			{
				throw ex;
			}
			lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0 && lastWin32Error != 18)
			{
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
			}
		}
		RemoveDirectoryInternal(fullPath, topLevel, allowDirectoryNotEmpty: true);
	}

	private static void RemoveDirectoryInternal(string fullPath, bool topLevel, bool allowDirectoryNotEmpty = false)
	{
		if (Interop.Kernel32.RemoveDirectory(fullPath))
		{
			return;
		}
		int num = Marshal.GetLastWin32Error();
		switch (num)
		{
		case 2:
			num = 3;
			goto case 3;
		case 3:
			if (!topLevel)
			{
				return;
			}
			break;
		case 145:
			if (allowDirectoryNotEmpty)
			{
				return;
			}
			break;
		case 5:
			throw new IOException(SR.Format("Access to the path '{0}' is denied.", fullPath));
		}
		throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
	}

	public static void SetAttributes(string fullPath, FileAttributes attributes)
	{
		if (!Interop.Kernel32.SetFileAttributes(fullPath, (int)attributes))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error == 87)
			{
				throw new ArgumentException("Invalid File or Directory attributes value.", "attributes");
			}
			throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
		}
	}

	public static void SetCreationTime(string fullPath, DateTimeOffset time, bool asDirectory)
	{
		using SafeFileHandle hFile = OpenHandle(fullPath, asDirectory);
		if (!Interop.Kernel32.SetFileTime(hFile, time.ToFileTime(), -1L, -1L, -1L))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
		}
	}

	public static void SetLastAccessTime(string fullPath, DateTimeOffset time, bool asDirectory)
	{
		using SafeFileHandle hFile = OpenHandle(fullPath, asDirectory);
		if (!Interop.Kernel32.SetFileTime(hFile, -1L, time.ToFileTime(), -1L, -1L))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
		}
	}

	public static void SetLastWriteTime(string fullPath, DateTimeOffset time, bool asDirectory)
	{
		using SafeFileHandle hFile = OpenHandle(fullPath, asDirectory);
		if (!Interop.Kernel32.SetFileTime(hFile, -1L, -1L, time.ToFileTime(), -1L))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
		}
	}

	public static string[] GetLogicalDrives()
	{
		return DriveInfoInternal.GetLogicalDrives();
	}
}
