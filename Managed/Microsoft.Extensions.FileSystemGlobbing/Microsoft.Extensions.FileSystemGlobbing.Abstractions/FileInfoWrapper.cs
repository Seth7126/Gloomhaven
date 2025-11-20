using System.IO;

namespace Microsoft.Extensions.FileSystemGlobbing.Abstractions;

public class FileInfoWrapper : FileInfoBase
{
	private readonly FileInfo _fileInfo;

	public override string Name => _fileInfo.Name;

	public override string FullName => _fileInfo.FullName;

	public override DirectoryInfoBase ParentDirectory => new DirectoryInfoWrapper(_fileInfo.Directory);

	public FileInfoWrapper(FileInfo fileInfo)
	{
		_fileInfo = fileInfo;
	}
}
