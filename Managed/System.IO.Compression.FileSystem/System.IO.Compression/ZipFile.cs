using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Compression;

/// <summary>Provides static methods for creating, extracting, and opening zip archives. </summary>
public static class ZipFile
{
	private const char PathSeparator = '/';

	/// <summary>Opens a zip archive for reading at the specified path.</summary>
	/// <returns>The opened zip archive.</returns>
	/// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="archiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="archiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="archiveFileName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="archiveFileName" /> could not be opened.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="archiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName" />.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="archiveFileName" /> is not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="archiveFileName" /> contains an invalid format.</exception>
	/// <exception cref="T:System.IO.InvalidDataException">
	///   <paramref name="archiveFileName" /> could not be interpreted as a zip archive.</exception>
	public static ZipArchive OpenRead(string archiveFileName)
	{
		return Open(archiveFileName, ZipArchiveMode.Read);
	}

	/// <summary>Opens a zip archive at the specified path and in the specified mode.</summary>
	/// <returns>The opened zip archive.</returns>
	/// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="mode">One of the enumeration values that specifies the actions which are allowed on the entries in the opened archive.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="archiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="archiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="archiveFileName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="archiveFileName" /> could not be opened.-or-<paramref name="mode" /> is set to <see cref="F:System.IO.Compression.ZipArchiveMode.Create" />, but the file specified in <paramref name="archiveFileName" /> already exists.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="archiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="mode" /> is set to <see cref="F:System.IO.Compression.ZipArchiveMode.Read" />, but the file specified in <paramref name="archiveFileName" /> is not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="archiveFileName" /> contains an invalid format.</exception>
	/// <exception cref="T:System.IO.InvalidDataException">
	///   <paramref name="archiveFileName" /> could not be interpreted as a zip archive.-or-<paramref name="mode" /> is <see cref="F:System.IO.Compression.ZipArchiveMode.Update" />, but an entry is missing or corrupt and cannot be read.-or-<paramref name="mode" /> is <see cref="F:System.IO.Compression.ZipArchiveMode.Update" />, but an entry is too large to fit into memory.</exception>
	public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode)
	{
		return Open(archiveFileName, mode, null);
	}

	/// <summary>Opens a zip archive at the specified path, in the specified mode, and by using the specified character encoding for entry names.</summary>
	/// <returns>The opened zip archive.</returns>
	/// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="mode">One of the enumeration values that specifies the actions that are allowed on the entries in the opened archive.</param>
	/// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when an encoding is required for interoperability with zip archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="archiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding" /> is set to a Unicode encoding other than UTF-8.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="archiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="archiveFileName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="archiveFileName" /> could not be opened.-or-<paramref name="mode" /> is set to <see cref="F:System.IO.Compression.ZipArchiveMode.Create" />, but the file specified in <paramref name="archiveFileName" /> already exists.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="archiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="mode" /> is set to <see cref="F:System.IO.Compression.ZipArchiveMode.Read" />, but the file specified in <paramref name="archiveFileName" /> is not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="archiveFileName" /> contains an invalid format.</exception>
	/// <exception cref="T:System.IO.InvalidDataException">
	///   <paramref name="archiveFileName" /> could not be interpreted as a zip archive.-or-<paramref name="mode" /> is <see cref="F:System.IO.Compression.ZipArchiveMode.Update" />, but an entry is missing or corrupt and cannot be read.-or-<paramref name="mode" /> is <see cref="F:System.IO.Compression.ZipArchiveMode.Update" />, but an entry is too large to fit into memory.</exception>
	public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode, Encoding entryNameEncoding)
	{
		FileMode mode2;
		FileAccess access;
		FileShare share;
		switch (mode)
		{
		case ZipArchiveMode.Read:
			mode2 = FileMode.Open;
			access = FileAccess.Read;
			share = FileShare.Read;
			break;
		case ZipArchiveMode.Create:
			mode2 = FileMode.CreateNew;
			access = FileAccess.Write;
			share = FileShare.None;
			break;
		case ZipArchiveMode.Update:
			mode2 = FileMode.OpenOrCreate;
			access = FileAccess.ReadWrite;
			share = FileShare.None;
			break;
		default:
			throw new ArgumentOutOfRangeException("mode");
		}
		FileStream fileStream = new FileStream(archiveFileName, mode2, access, share, 4096, useAsync: false);
		try
		{
			return new ZipArchive(fileStream, mode, leaveOpen: false, entryNameEncoding);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	/// <summary>Creates a zip archive that contains the files and directories from the specified directory.</summary>
	/// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="sourceDirectoryName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destinationArchiveFileName" /> already exists.-or-A file in the specified directory could not be opened.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="destinationArchiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName" /> or the file specified in <paramref name="destinationArchiveFileName" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> contains an invalid format.-or-The zip archive does not support writing.</exception>
	public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
	{
		DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, null, includeBaseDirectory: false, null);
	}

	/// <summary>Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level, and optionally includes the base directory.</summary>
	/// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
	/// <param name="includeBaseDirectory">true to include the directory name from <paramref name="sourceDirectoryName" /> at the root of the archive; false to include only the contents of the directory.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="sourceDirectoryName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destinationArchiveFileName" /> already exists.-or-A file in the specified directory could not be opened.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="destinationArchiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName" /> or the file specified in <paramref name="destinationArchiveFileName" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> contains an invalid format.-or-The zip archive does not support writing.</exception>
	public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory)
	{
		DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory, null);
	}

	/// <summary>Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level and character encoding for entry names, and optionally includes the base directory.</summary>
	/// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
	/// <param name="includeBaseDirectory">true to include the directory name from <paramref name="sourceDirectoryName" /> at the root of the archive; false to include only the contents of the directory.</param>
	/// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when an encoding is required for interoperability with zip archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding" /> is set to a Unicode encoding other than UTF-8.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="sourceDirectoryName" /> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destinationArchiveFileName" /> already exists.-or-A file in the specified directory could not be opened.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="destinationArchiveFileName" /> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName" /> or the file specified in <paramref name="destinationArchiveFileName" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceDirectoryName" /> or <paramref name="destinationArchiveFileName" /> contains an invalid format.-or-The zip archive does not support writing.</exception>
	public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory, Encoding entryNameEncoding)
	{
		DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory, entryNameEncoding);
	}

	/// <summary>Extracts all the files in the specified zip archive to a directory on the file system.</summary>
	/// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
	/// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path in <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="destinationDirectoryName" /> already exists.-or-The name of an entry in the archive is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-Extracting an archive entry would create a file that is outside the directory specified by <paramref name="destinationDirectoryName" />. (For example, this might happen if the entry name contains parent directory accessors.) -or-An archive entry to extract has the same name as an entry that has already been extracted from the same archive.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to access the archive or the destination directory.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> contains an invalid format.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceArchiveFileName" /> was not found.</exception>
	/// <exception cref="T:System.IO.InvalidDataException">The archive specified by <paramref name="sourceArchiveFileName" /> is not a valid zip archive.-or-An archive entry was not found or was corrupt.-or-An archive entry was compressed by using a compression method that is not supported.</exception>
	public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
	{
		ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, null);
	}

	public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwrite)
	{
		ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, null, overwrite);
	}

	/// <summary>Extracts all the files in the specified zip archive to a directory on the file system and uses the specified character encoding for entry names.</summary>
	/// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
	/// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
	/// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when an encoding is required for interoperability with zip archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding" /> is set to a Unicode encoding other than UTF-8.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> is null.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path in <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="destinationDirectoryName" /> already exists.-or-The name of an entry in the archive is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-Extracting an archive entry would create a file that is outside the directory specified by <paramref name="destinationDirectoryName" />. (For example, this might happen if the entry name contains parent directory accessors.) -or-An archive entry to extract has the same name as an entry that has already been extracted from the same archive.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to access the archive or the destination directory.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="destinationDirectoryName" /> or <paramref name="sourceArchiveFileName" /> contains an invalid format.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceArchiveFileName" /> was not found.</exception>
	/// <exception cref="T:System.IO.InvalidDataException">The archive specified by <paramref name="sourceArchiveFileName" /> is not a valid zip archive.-or-An archive entry was not found or was corrupt.-or-An archive entry was compressed by using a compression method that is not supported.</exception>
	public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding)
	{
		ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding, overwrite: false);
	}

	public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding, bool overwrite)
	{
		if (sourceArchiveFileName == null)
		{
			throw new ArgumentNullException("sourceArchiveFileName");
		}
		using ZipArchive source = Open(sourceArchiveFileName, ZipArchiveMode.Read, entryNameEncoding);
		source.ExtractToDirectory(destinationDirectoryName, overwrite);
	}

	private static void DoCreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel? compressionLevel, bool includeBaseDirectory, Encoding entryNameEncoding)
	{
		sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName = Path.GetFullPath(destinationArchiveFileName);
		using ZipArchive zipArchive = Open(destinationArchiveFileName, ZipArchiveMode.Create, entryNameEncoding);
		bool flag = true;
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirectoryName);
		string fullName = directoryInfo.FullName;
		if (includeBaseDirectory && directoryInfo.Parent != null)
		{
			fullName = directoryInfo.Parent.FullName;
		}
		char[] buffer = ArrayPool<char>.Shared.Rent(260);
		try
		{
			foreach (FileSystemInfo item in directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
			{
				flag = false;
				int length = item.FullName.Length - fullName.Length;
				if (item is FileInfo)
				{
					string entryName = EntryFromPath(item.FullName, fullName.Length, length, ref buffer);
					ZipFileExtensions.DoCreateEntryFromFile(zipArchive, item.FullName, entryName, compressionLevel);
				}
				else if (item is DirectoryInfo possiblyEmptyDir && IsDirEmpty(possiblyEmptyDir))
				{
					string entryName2 = EntryFromPath(item.FullName, fullName.Length, length, ref buffer, appendPathSeparator: true);
					zipArchive.CreateEntry(entryName2);
				}
			}
			if (includeBaseDirectory && flag)
			{
				zipArchive.CreateEntry(EntryFromPath(directoryInfo.Name, 0, directoryInfo.Name.Length, ref buffer, appendPathSeparator: true));
			}
		}
		finally
		{
			ArrayPool<char>.Shared.Return(buffer);
		}
	}

	private static string EntryFromPath(string entry, int offset, int length, ref char[] buffer, bool appendPathSeparator = false)
	{
		while (length > 0 && (entry[offset] == Path.DirectorySeparatorChar || entry[offset] == Path.AltDirectorySeparatorChar))
		{
			offset++;
			length--;
		}
		if (length == 0)
		{
			if (!appendPathSeparator)
			{
				return string.Empty;
			}
			return '/'.ToString();
		}
		int num = (appendPathSeparator ? (length + 1) : length);
		EnsureCapacity(ref buffer, num);
		entry.CopyTo(offset, buffer, 0, length);
		for (int i = 0; i < length; i++)
		{
			char c = buffer[i];
			if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
			{
				buffer[i] = '/';
			}
		}
		if (appendPathSeparator)
		{
			buffer[length] = '/';
		}
		return new string(buffer, 0, num);
	}

	private static void EnsureCapacity(ref char[] buffer, int min)
	{
		if (buffer.Length < min)
		{
			int num = buffer.Length * 2;
			if (num < min)
			{
				num = min;
			}
			ArrayPool<char>.Shared.Return(buffer);
			buffer = ArrayPool<char>.Shared.Rent(num);
		}
	}

	private static bool IsDirEmpty(DirectoryInfo possiblyEmptyDir)
	{
		using IEnumerator<string> enumerator = Directory.EnumerateFileSystemEntries(possiblyEmptyDir.FullName).GetEnumerator();
		return !enumerator.MoveNext();
	}
}
