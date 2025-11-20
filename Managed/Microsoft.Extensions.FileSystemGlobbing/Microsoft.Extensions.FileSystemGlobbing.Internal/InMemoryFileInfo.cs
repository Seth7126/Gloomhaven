using System.IO;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

internal sealed class InMemoryFileInfo : FileInfoBase
{
	private InMemoryDirectoryInfo _parent;

	public override string FullName { get; }

	public override string Name { get; }

	public override DirectoryInfoBase ParentDirectory => _parent;

	public InMemoryFileInfo(string file, InMemoryDirectoryInfo parent)
	{
		FullName = file;
		Name = Path.GetFileName(file);
		_parent = parent;
	}
}
