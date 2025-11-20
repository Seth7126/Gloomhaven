using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Extensions.FileSystemGlobbing.Abstractions;

public class DirectoryInfoWrapper : DirectoryInfoBase
{
	private readonly DirectoryInfo _directoryInfo;

	private readonly bool _isParentPath;

	public override string Name
	{
		get
		{
			if (!_isParentPath)
			{
				return _directoryInfo.Name;
			}
			return "..";
		}
	}

	public override string FullName => _directoryInfo.FullName;

	public override DirectoryInfoBase ParentDirectory => new DirectoryInfoWrapper(_directoryInfo.Parent);

	public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
		: this(directoryInfo, isParentPath: false)
	{
	}

	private DirectoryInfoWrapper(DirectoryInfo directoryInfo, bool isParentPath)
	{
		_directoryInfo = directoryInfo;
		_isParentPath = isParentPath;
	}

	public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos()
	{
		if (!_directoryInfo.Exists)
		{
			yield break;
		}
		IEnumerable<FileSystemInfo> enumerable;
		try
		{
			enumerable = _directoryInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
		}
		catch (DirectoryNotFoundException)
		{
			yield break;
		}
		foreach (FileSystemInfo item in enumerable)
		{
			if (item is DirectoryInfo directoryInfo)
			{
				yield return new DirectoryInfoWrapper(directoryInfo);
			}
			else
			{
				yield return new FileInfoWrapper((FileInfo)item);
			}
		}
	}

	public override DirectoryInfoBase GetDirectory(string name)
	{
		bool flag = string.Equals(name, "..", StringComparison.Ordinal);
		if (flag)
		{
			return new DirectoryInfoWrapper(new DirectoryInfo(Path.Combine(_directoryInfo.FullName, name)), flag);
		}
		DirectoryInfo[] directories = _directoryInfo.GetDirectories(name);
		if (directories.Length == 1)
		{
			return new DirectoryInfoWrapper(directories[0], flag);
		}
		if (directories.Length == 0)
		{
			return null;
		}
		throw new InvalidOperationException("More than one sub directories are found under " + _directoryInfo.FullName + " with name " + name + ".");
	}

	public override FileInfoBase GetFile(string name)
	{
		return new FileInfoWrapper(new FileInfo(Path.Combine(_directoryInfo.FullName, name)));
	}
}
