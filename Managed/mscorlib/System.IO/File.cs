using System.Buffers;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Provides static methods for the creation, copying, deletion, moving, and opening of files, and aids in the creation of <see cref="T:System.IO.FileStream" /> objects.</summary>
/// <filterpriority>1</filterpriority>
public static class File
{
	private const int MaxByteArrayLength = 2147483591;

	private static Encoding s_UTF8NoBOM;

	internal const int DefaultBufferSize = 4096;

	private static Encoding UTF8NoBOM => s_UTF8NoBOM ?? (s_UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));

	/// <summary>Opens an existing UTF-8 encoded text file for reading.</summary>
	/// <returns>A <see cref="T:System.IO.StreamReader" /> on the specified path.</returns>
	/// <param name="path">The file to be opened for reading. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamReader OpenText(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return new StreamReader(path);
	}

	/// <summary>Creates or opens a file for writing UTF-8 encoded text.</summary>
	/// <returns>A <see cref="T:System.IO.StreamWriter" /> that writes to the specified file using UTF-8 encoding.</returns>
	/// <param name="path">The file to be opened for writing. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamWriter CreateText(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return new StreamWriter(path, append: false);
	}

	/// <summary>Creates a <see cref="T:System.IO.StreamWriter" /> that appends UTF-8 encoded text to an existing file, or to a new file if the specified file does not exist.</summary>
	/// <returns>A stream writer that appends UTF-8 encoded text to the specified file or to a new file.</returns>
	/// <param name="path">The path to the file to append to. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static StreamWriter AppendText(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return new StreamWriter(path, append: true);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is not allowed.</summary>
	/// <param name="sourceFileName">The file to copy. </param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists.-or- An I/O error has occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Copy(string sourceFileName, string destFileName)
	{
		Copy(sourceFileName, destFileName, overwrite: false);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is allowed.</summary>
	/// <param name="sourceFileName">The file to copy. </param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory. </param>
	/// <param name="overwrite">true if the destination file can be overwritten; otherwise, false. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. -or-<paramref name="destFileName" /> is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists and <paramref name="overwrite" /> is false.-or- An I/O error has occurred. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName", "File name cannot be null.");
		}
		if (destFileName == null)
		{
			throw new ArgumentNullException("destFileName", "File name cannot be null.");
		}
		if (sourceFileName.Length == 0)
		{
			throw new ArgumentException("Empty file name is not legal.", "sourceFileName");
		}
		if (destFileName.Length == 0)
		{
			throw new ArgumentException("Empty file name is not legal.", "destFileName");
		}
		FileSystem.CopyFile(Path.GetFullPath(sourceFileName), Path.GetFullPath(destFileName), overwrite);
	}

	/// <summary>Creates or overwrites a file in the specified path.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <param name="path">The path and name of the file to create. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Create(string path)
	{
		return Create(path, 4096);
	}

	/// <summary>Creates or overwrites the specified file.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> with the specified buffer size that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Create(string path, int bufferSize)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
	}

	/// <summary>Creates or overwrites the specified file, specifying a buffer size and a <see cref="T:System.IO.FileOptions" /> value that describes how to create or overwrite the file.</summary>
	/// <returns>A new file with the specified buffer size.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. -or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" /> and file encryption is not supported on the current platform.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	public static FileStream Create(string path, int bufferSize, FileOptions options)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Deletes the specified file. </summary>
	/// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">The specified file is in use. -or-There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> is a directory.-or- <paramref name="path" /> specified a read-only file. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Delete(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		FileSystem.DeleteFile(Path.GetFullPath(path));
	}

	/// <summary>Determines whether the specified file exists.</summary>
	/// <returns>true if the caller has the required permissions and <paramref name="path" /> contains the name of an existing file; otherwise, false. This method also returns false if <paramref name="path" /> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of <paramref name="path" />.</returns>
	/// <param name="path">The file to check. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool Exists(string path)
	{
		try
		{
			if (path == null)
			{
				return false;
			}
			if (path.Length == 0)
			{
				return false;
			}
			path = Path.GetFullPath(path);
			if (path.Length > 0 && PathInternal.IsDirectorySeparator(path[path.Length - 1]))
			{
				return false;
			}
			return FileSystem.FileExists(path);
		}
		catch (ArgumentException)
		{
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		return false;
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path with read/write access.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> opened in the specified mode and path, with read/write access and not shared.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode)
	{
		return Open(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, with the specified mode and access.</summary>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> that provides access to the specified file, with the specified mode and access.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="access" /> specified Read and <paramref name="mode" /> specified Create, CreateNew, Truncate, or Append. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not Read.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> or <paramref name="access" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode, FileAccess access)
	{
		return Open(path, mode, access, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
	/// <param name="path">The file to open. </param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten. </param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file. </param>
	/// <param name="share">A <see cref="T:System.IO.FileShare" /> value specifying the type of access other threads have to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or- <paramref name="access" /> specified Read and <paramref name="mode" /> specified Create, CreateNew, Truncate, or Append. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not Read.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. -or-<paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> specified an invalid value. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		return new FileStream(path, mode, access, share);
	}

	internal static DateTimeOffset GetUtcDateTimeOffset(DateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKind.Unspecified)
		{
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
		}
		return dateTime.ToUniversalTime();
	}

	/// <summary>Sets the date and time the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information. </param>
	/// <param name="creationTime">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetCreationTime(string path, DateTime creationTime)
	{
		FileSystem.SetCreationTime(Path.GetFullPath(path), creationTime, asDirectory: false);
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information. </param>
	/// <param name="creationTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		FileSystem.SetCreationTime(Path.GetFullPath(path), GetUtcDateTimeOffset(creationTimeUtc), asDirectory: false);
	}

	/// <summary>Returns the creation date and time of the specified file or directory.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain creation date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetCreationTime(string path)
	{
		return FileSystem.GetCreationTime(Path.GetFullPath(path)).LocalDateTime;
	}

	/// <summary>Returns the creation date and time, in coordinated universal time (UTC), of the specified file or directory.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain creation date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetCreationTimeUtc(string path)
	{
		return FileSystem.GetCreationTime(Path.GetFullPath(path)).UtcDateTime;
	}

	/// <summary>Sets the date and time the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information. </param>
	/// <param name="lastAccessTime">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		FileSystem.SetLastAccessTime(Path.GetFullPath(path), lastAccessTime, asDirectory: false);
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information. </param>
	/// <param name="lastAccessTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		FileSystem.SetLastAccessTime(Path.GetFullPath(path), GetUtcDateTimeOffset(lastAccessTimeUtc), asDirectory: false);
	}

	/// <summary>Returns the date and time the specified file or directory was last accessed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain access date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastAccessTime(string path)
	{
		return FileSystem.GetLastAccessTime(Path.GetFullPath(path)).LocalDateTime;
	}

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain access date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastAccessTimeUtc(string path)
	{
		return FileSystem.GetLastAccessTime(Path.GetFullPath(path)).UtcDateTime;
	}

	/// <summary>Sets the date and time that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information. </param>
	/// <param name="lastWriteTime">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in local time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		FileSystem.SetLastWriteTime(Path.GetFullPath(path), lastWriteTime, asDirectory: false);
	}

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information. </param>
	/// <param name="lastWriteTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in UTC time. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		FileSystem.SetLastWriteTime(Path.GetFullPath(path), GetUtcDateTimeOffset(lastWriteTimeUtc), asDirectory: false);
	}

	/// <summary>Returns the date and time the specified file or directory was last written to.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
	/// <param name="path">The file or directory for which to obtain write date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastWriteTime(string path)
	{
		return FileSystem.GetLastWriteTime(Path.GetFullPath(path)).LocalDateTime;
	}

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
	/// <param name="path">The file or directory for which to obtain write date and time information. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DateTime GetLastWriteTimeUtc(string path)
	{
		return FileSystem.GetLastWriteTime(Path.GetFullPath(path)).UtcDateTime;
	}

	/// <summary>Gets the <see cref="T:System.IO.FileAttributes" /> of the file on the path.</summary>
	/// <returns>The <see cref="T:System.IO.FileAttributes" /> of the file on the path.</returns>
	/// <param name="path">The path to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, or contains invalid characters. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="path" /> represents a file and is invalid, such as being on an unmapped drive, or the file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> represents a directory and is invalid, such as being on an unmapped drive, or the directory cannot be found.</exception>
	/// <exception cref="T:System.IO.IOException">This file is being used by another process.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileAttributes GetAttributes(string path)
	{
		return FileSystem.GetAttributes(Path.GetFullPath(path));
	}

	/// <summary>Sets the specified <see cref="T:System.IO.FileAttributes" /> of the file on the specified path.</summary>
	/// <param name="path">The path to the file. </param>
	/// <param name="fileAttributes">A bitwise combination of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, contains invalid characters, or the file attribute is invalid. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetAttributes(string path, FileAttributes fileAttributes)
	{
		if ((fileAttributes & (FileAttributes)(-2147483648)) != 0)
		{
			Path.Validate(path);
			if (!MonoIO.SetFileAttributes(path, fileAttributes, out var error))
			{
				throw MonoIO.GetException(path, error);
			}
		}
		else
		{
			FileSystem.SetAttributes(Path.GetFullPath(path), fileAttributes);
		}
	}

	/// <summary>Opens an existing file for reading.</summary>
	/// <returns>A read-only <see cref="T:System.IO.FileStream" /> on the specified path.</returns>
	/// <param name="path">The file to be opened for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream OpenRead(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Opens an existing file or creates a new file for writing.</summary>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> object on the specified path with <see cref="F:System.IO.FileAccess.Write" /> access.</returns>
	/// <param name="path">The file to be opened for writing. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a read-only file or directory. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileStream OpenWrite(string path)
	{
		return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
	}

	/// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
	/// <returns>A string containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ReadAllText(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return InternalReadAllText(path, Encoding.UTF8);
	}

	/// <summary>Opens a file, reads all lines of the file with the specified encoding, and then closes the file.</summary>
	/// <returns>A string containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <param name="encoding">The encoding applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ReadAllText(string path, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return InternalReadAllText(path, encoding);
	}

	private static string InternalReadAllText(string path, Encoding encoding)
	{
		using StreamReader streamReader = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks: true);
		return streamReader.ReadToEnd();
	}

	/// <summary>Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or <paramref name="contents" /> is empty.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllText(string path, string contents)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		using StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(contents);
	}

	/// <summary>Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string to write to the file. </param>
	/// <param name="encoding">The encoding to apply to the string.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or <paramref name="contents" /> is empty. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		using StreamWriter streamWriter = new StreamWriter(path, append: false, encoding);
		streamWriter.Write(contents);
	}

	/// <summary>Opens a binary file, reads the contents of the file into a byte array, and then closes the file.</summary>
	/// <returns>A byte array containing the contents of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static byte[] ReadAllBytes(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1);
		long length = fileStream.Length;
		if (length > int.MaxValue)
		{
			throw new IOException("The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size.");
		}
		if (length == 0L)
		{
			return ReadAllBytesUnknownLength(fileStream);
		}
		int num = 0;
		int num2 = (int)length;
		byte[] array = new byte[num2];
		while (num2 > 0)
		{
			int num3 = fileStream.Read(array, num, num2);
			if (num3 == 0)
			{
				throw Error.GetEndOfFile();
			}
			num += num3;
			num2 -= num3;
		}
		return array;
	}

	private static byte[] ReadAllBytesUnknownLength(FileStream fs)
	{
		byte[] array = null;
		Span<byte> span = stackalloc byte[512];
		try
		{
			int num = 0;
			while (true)
			{
				if (num == span.Length)
				{
					uint num2 = (uint)(span.Length * 2);
					if (num2 > 2147483591)
					{
						num2 = (uint)Math.Max(2147483591, span.Length + 1);
					}
					byte[] array2 = ArrayPool<byte>.Shared.Rent((int)num2);
					span.CopyTo(array2);
					if (array != null)
					{
						ArrayPool<byte>.Shared.Return(array);
					}
					span = (array = array2);
				}
				int num3 = fs.Read(span.Slice(num));
				if (num3 == 0)
				{
					break;
				}
				num += num3;
			}
			return span.Slice(0, num).ToArray();
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}

	/// <summary>Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="bytes">The bytes to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null or the byte array is empty. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllBytes(string path, byte[] bytes)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path", "Path cannot be null.");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		InternalWriteAllBytes(path, bytes);
	}

	private static void InternalWriteAllBytes(string path, byte[] bytes)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
		fileStream.Write(bytes, 0, bytes.Length);
	}

	/// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string[] ReadAllLines(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return InternalReadAllLines(path, Encoding.UTF8);
	}

	/// <summary>Opens a file, reads all lines of the file with the specified encoding, and then closes the file.</summary>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <param name="path">The file to open for reading. </param>
	/// <param name="encoding">The encoding applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return InternalReadAllLines(path, encoding);
	}

	private static string[] InternalReadAllLines(string path, Encoding encoding)
	{
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(path, encoding))
		{
			string item;
			while ((item = streamReader.ReadLine()) != null)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	/// <summary>Reads the lines of a file.</summary>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <param name="path">The file to read.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static IEnumerable<string> ReadLines(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return ReadLinesIterator.CreateIterator(path, Encoding.UTF8);
	}

	/// <summary>Read the lines of a file that has a specified encoding.</summary>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <param name="path">The file to read.</param>
	/// <param name="encoding">The encoding that is applied to the contents of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		return ReadLinesIterator.CreateIterator(path, encoding);
	}

	/// <summary>Creates a new file, write the specified string array to the file, and then closes the file. </summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string array to write to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is null.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllLines(string path, string[] contents)
	{
		WriteAllLines(path, (IEnumerable<string>)contents);
	}

	/// <summary>Creates a new file, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path " />or <paramref name="contents" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		InternalWriteAllLines(new StreamWriter(path), contents);
	}

	/// <summary>Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file. </summary>
	/// <param name="path">The file to write to. </param>
	/// <param name="contents">The string array to write to the file. </param>
	/// <param name="encoding">An <see cref="T:System.Text.Encoding" /> object that represents the character encoding applied to the string array.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is null.  </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		WriteAllLines(path, (IEnumerable<string>)contents, encoding);
	}

	/// <summary>Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path" />,<paramref name=" contents" />, or <paramref name="encoding" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		InternalWriteAllLines(new StreamWriter(path, append: false, encoding), contents);
	}

	private static void InternalWriteAllLines(TextWriter writer, IEnumerable<string> contents)
	{
		using (writer)
		{
			foreach (string content in contents)
			{
				writer.WriteLine(content);
			}
		}
	}

	/// <summary>Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.</summary>
	/// <param name="path">The file to append the specified string to. </param>
	/// <param name="contents">The string to append to the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void AppendAllText(string path, string contents)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		using StreamWriter streamWriter = new StreamWriter(path, append: true);
		streamWriter.Write(contents);
	}

	/// <summary>Appends the specified string to the file, creating the file if it does not already exist.</summary>
	/// <param name="path">The file to append the specified string to. </param>
	/// <param name="contents">The string to append to the file. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn’t exist or it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		using StreamWriter streamWriter = new StreamWriter(path, append: true, encoding);
		streamWriter.Write(contents);
	}

	/// <summary>Appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path " />or <paramref name="contents" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn’t exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to write to the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.</exception>
	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		InternalWriteAllLines(new StreamWriter(path, append: true), contents);
	}

	/// <summary>Appends lines to a file by using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either<paramref name=" path" />, <paramref name="contents" />, or <paramref name="encoding" /> is null.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn’t exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters and file names must be less than 260 characters.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.-or-This operation is not supported on the current platform.-or-<paramref name="path" /> is a directory.-or-The caller does not have the required permission.</exception>
	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		InternalWriteAllLines(new StreamWriter(path, append: true, encoding), contents);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.-or-The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.-or-The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.- or -The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.-or- This operation is not supported on the current platform.-or- Source or destination parameters specify a directory instead of a file.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
	{
		Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file and optionally ignores merge errors.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <param name="ignoreMetadataErrors">true to ignore merge errors (such as attributes and access control lists (ACLs)) from the replaced file to the replacement file; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.-or-The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.-or-The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.- or -The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.-or- This operation is not supported on the current platform.-or- Source or destination parameters specify a directory instead of a file.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (destinationFileName == null)
		{
			throw new ArgumentNullException("destinationFileName");
		}
		FileSystem.ReplaceFile(Path.GetFullPath(sourceFileName), Path.GetFullPath(destinationFileName), (destinationBackupFileName != null) ? Path.GetFullPath(destinationBackupFileName) : null, ignoreMetadataErrors);
	}

	/// <summary>Moves a specified file to a new location, providing the option to specify a new file name.</summary>
	/// <param name="sourceFileName">The name of the file to move. </param>
	/// <param name="destFileName">The new path for the file. </param>
	/// <exception cref="T:System.IO.IOException">The destination file already exists.-or-<paramref name="sourceFileName" /> was not found. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains invalid characters as defined in <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid, (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Move(string sourceFileName, string destFileName)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName", "File name cannot be null.");
		}
		if (destFileName == null)
		{
			throw new ArgumentNullException("destFileName", "File name cannot be null.");
		}
		if (sourceFileName.Length == 0)
		{
			throw new ArgumentException("Empty file name is not legal.", "sourceFileName");
		}
		if (destFileName.Length == 0)
		{
			throw new ArgumentException("Empty file name is not legal.", "destFileName");
		}
		string fullPath = Path.GetFullPath(sourceFileName);
		string fullPath2 = Path.GetFullPath(destFileName);
		if (!FileSystem.FileExists(fullPath))
		{
			throw new FileNotFoundException(SR.Format("Could not find file '{0}'.", fullPath), fullPath);
		}
		FileSystem.MoveFile(fullPath, fullPath2);
	}

	/// <summary>Encrypts a file so that only the account used to encrypt the file can decrypt it.</summary>
	/// <param name="path">A path that describes a file to encrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.-or-This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Encrypt(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		throw new PlatformNotSupportedException("File encryption is not supported on this platform.");
	}

	/// <summary>Decrypts a file that was encrypted by the current account using the <see cref="M:System.IO.File.Encrypt(System.String)" /> method.</summary>
	/// <param name="path">A path that describes a file to decrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. For example, the encrypted file is already open. -or-This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Decrypt(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		throw new PlatformNotSupportedException("File encryption is not supported on this platform.");
	}

	private static StreamReader AsyncStreamReader(string path, Encoding encoding)
	{
		return new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan), encoding, detectEncodingFromByteOrderMarks: true);
	}

	private static StreamWriter AsyncStreamWriter(string path, Encoding encoding, bool append)
	{
		return new StreamWriter(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan), encoding);
	}

	public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
	}

	public static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return InternalReadAllTextAsync(path, encoding, cancellationToken);
		}
		return Task.FromCanceled<string>(cancellationToken);
	}

	private static async Task<string> InternalReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
	{
		char[] buffer = null;
		StreamReader sr = AsyncStreamReader(path, encoding);
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			buffer = ArrayPool<char>.Shared.Rent(sr.CurrentEncoding.GetMaxCharCount(4096));
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				int num = await sr.ReadAsync(new Memory<char>(buffer), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					break;
				}
				sb.Append(buffer, 0, num);
			}
			return sb.ToString();
		}
		finally
		{
			sr.Dispose();
			if (buffer != null)
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}
	}

	public static Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
	{
		return WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);
	}

	public static Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		if (string.IsNullOrEmpty(contents))
		{
			new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read).Dispose();
			return Task.CompletedTask;
		}
		return InternalWriteAllTextAsync(AsyncStreamWriter(path, encoding, append: false), contents, cancellationToken);
	}

	public static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<byte[]>(cancellationToken);
		}
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.Asynchronous | FileOptions.SequentialScan);
		bool flag = false;
		try
		{
			long length = fileStream.Length;
			if (length > int.MaxValue)
			{
				return Task.FromException<byte[]>(new IOException("The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size."));
			}
			flag = true;
			return (length > 0) ? InternalReadAllBytesAsync(fileStream, (int)length, cancellationToken) : InternalReadAllBytesUnknownLengthAsync(fileStream, cancellationToken);
		}
		finally
		{
			if (!flag)
			{
				fileStream.Dispose();
			}
		}
	}

	private static async Task<byte[]> InternalReadAllBytesAsync(FileStream fs, int count, CancellationToken cancellationToken)
	{
		using (fs)
		{
			int index = 0;
			byte[] bytes = new byte[count];
			do
			{
				int num = await fs.ReadAsync(new Memory<byte>(bytes, index, count - index), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					throw Error.GetEndOfFile();
				}
				index += num;
			}
			while (index < count);
			return bytes;
		}
	}

	private static async Task<byte[]> InternalReadAllBytesUnknownLengthAsync(FileStream fs, CancellationToken cancellationToken)
	{
		byte[] rentedArray = ArrayPool<byte>.Shared.Rent(512);
		try
		{
			int bytesRead = 0;
			while (true)
			{
				if (bytesRead == rentedArray.Length)
				{
					uint num = (uint)(rentedArray.Length * 2);
					if (num > 2147483591)
					{
						num = (uint)Math.Max(2147483591, rentedArray.Length + 1);
					}
					byte[] array = ArrayPool<byte>.Shared.Rent((int)num);
					Buffer.BlockCopy(rentedArray, 0, array, 0, bytesRead);
					ArrayPool<byte>.Shared.Return(rentedArray);
					rentedArray = array;
				}
				int num2 = await fs.ReadAsync(rentedArray.AsMemory(bytesRead), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (num2 == 0)
				{
					break;
				}
				bytesRead += num2;
			}
			return rentedArray.AsSpan(0, bytesRead).ToArray();
		}
		finally
		{
			fs.Dispose();
			ArrayPool<byte>.Shared.Return(rentedArray);
		}
	}

	public static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path", "Path cannot be null.");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return InternalWriteAllBytesAsync(path, bytes, cancellationToken);
		}
		return Task.FromCanceled(cancellationToken);
	}

	private static async Task InternalWriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
	{
		using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
		await fs.WriteAsync(new ReadOnlyMemory<byte>(bytes), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await fs.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);
	}

	public static Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return InternalReadAllLinesAsync(path, encoding, cancellationToken);
		}
		return Task.FromCanceled<string[]>(cancellationToken);
	}

	private static async Task<string[]> InternalReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken)
	{
		using StreamReader sr = AsyncStreamReader(path, encoding);
		cancellationToken.ThrowIfCancellationRequested();
		List<string> lines = new List<string>();
		string item;
		while ((item = await sr.ReadLineAsync().ConfigureAwait(continueOnCapturedContext: false)) != null)
		{
			lines.Add(item);
			cancellationToken.ThrowIfCancellationRequested();
		}
		return lines.ToArray();
	}

	public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
	{
		return WriteAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);
	}

	public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return InternalWriteAllLinesAsync(AsyncStreamWriter(path, encoding, append: false), contents, cancellationToken);
		}
		return Task.FromCanceled(cancellationToken);
	}

	private static async Task InternalWriteAllLinesAsync(TextWriter writer, IEnumerable<string> contents, CancellationToken cancellationToken)
	{
		using (writer)
		{
			foreach (string content in contents)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await writer.WriteLineAsync(content).ConfigureAwait(continueOnCapturedContext: false);
			}
			cancellationToken.ThrowIfCancellationRequested();
			await writer.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private static async Task InternalWriteAllTextAsync(StreamWriter sw, string contents, CancellationToken cancellationToken)
	{
		char[] buffer = null;
		try
		{
			buffer = ArrayPool<char>.Shared.Rent(4096);
			int count = contents.Length;
			int batchSize;
			for (int index = 0; index < count; index += batchSize)
			{
				batchSize = Math.Min(4096, count - index);
				contents.CopyTo(index, buffer, 0, batchSize);
				await sw.WriteAsync(new ReadOnlyMemory<char>(buffer, 0, batchSize), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			cancellationToken.ThrowIfCancellationRequested();
			await sw.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			sw.Dispose();
			if (buffer != null)
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}
	}

	public static Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
	{
		return AppendAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);
	}

	public static Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		if (string.IsNullOrEmpty(contents))
		{
			new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read).Dispose();
			return Task.CompletedTask;
		}
		return InternalWriteAllTextAsync(AsyncStreamWriter(path, encoding, append: true), contents, cancellationToken);
	}

	public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
	{
		return AppendAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);
	}

	public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.", "path");
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return InternalWriteAllLinesAsync(AsyncStreamWriter(path, encoding, append: true), contents, cancellationToken);
		}
		return Task.FromCanceled(cancellationToken);
	}

	/// <summary>Creates or overwrites the specified file with the specified buffer size, file options, and file security.</summary>
	/// <returns>A new file with the specified buffer size, file options, and file security.</returns>
	/// <param name="path">The name of the file. </param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file. </param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <param name="fileSecurity">One of the <see cref="T:System.Security.AccessControl.FileSecurity" /> values that determines the access control and audit security for the file.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only.-or-<see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" /> and file encryption is not supported on the current platform. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or- <paramref name="path" /> specified a file that is read-only. </exception>
	public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control list (ACL) entries for a specified file.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.     </returns>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileSecurity GetAccessControl(string path)
	{
		return GetAccessControl(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the specified type of access control list (ACL) entries for a particular file.</summary>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.     </returns>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> values that specifies the type of access control list (ACL) information to receive.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
	{
		return new FileSecurity(path, includeSections);
	}

	/// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.FileSecurity" /> object to the specified file.</summary>
	/// <param name="path">A file to add or remove access control list (ACL) entries from.</param>
	/// <param name="fileSecurity">A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes an ACL entry to apply to the file described by the <paramref name="path" /> parameter.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is null.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.-or- This operation is not supported on the current platform.-or- The <paramref name="path" /> parameter specified a directory.-or- The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileSecurity" /> parameter is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void SetAccessControl(string path, FileSecurity fileSecurity)
	{
		if (fileSecurity == null)
		{
			throw new ArgumentNullException("fileSecurity");
		}
		fileSecurity.PersistModifications(path);
	}
}
