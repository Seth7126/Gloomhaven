using System.Collections.Generic;

namespace Microsoft.Extensions.FileSystemGlobbing.Abstractions;

public abstract class DirectoryInfoBase : FileSystemInfoBase
{
	public abstract IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos();

	public abstract DirectoryInfoBase GetDirectory(string path);

	public abstract FileInfoBase GetFile(string path);
}
