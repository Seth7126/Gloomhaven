namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public interface IPattern
{
	IPatternContext CreatePatternContextForInclude();

	IPatternContext CreatePatternContextForExclude();
}
