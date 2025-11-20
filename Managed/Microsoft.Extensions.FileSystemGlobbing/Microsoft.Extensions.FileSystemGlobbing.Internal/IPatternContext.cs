using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public interface IPatternContext
{
	void Declare(Action<IPathSegment, bool> onDeclare);

	bool Test(DirectoryInfoBase directory);

	PatternTestResult Test(FileInfoBase file);

	void PushDirectory(DirectoryInfoBase directory);

	void PopDirectory();
}
