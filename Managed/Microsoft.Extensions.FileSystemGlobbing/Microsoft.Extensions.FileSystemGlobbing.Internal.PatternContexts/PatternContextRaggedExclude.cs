using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

public class PatternContextRaggedExclude : PatternContextRagged
{
	public PatternContextRaggedExclude(IRaggedPattern pattern)
		: base(pattern)
	{
	}

	public override bool Test(DirectoryInfoBase directory)
	{
		if (IsStackEmpty())
		{
			throw new InvalidOperationException(SR.CannotTestDirectory);
		}
		if (Frame.IsNotApplicable)
		{
			return false;
		}
		if (IsEndingGroup() && TestMatchingGroup(directory))
		{
			return true;
		}
		if (base.Pattern.EndsWith.Count == 0 && Frame.SegmentGroupIndex == base.Pattern.Contains.Count - 1 && TestMatchingGroup(directory))
		{
			return true;
		}
		return false;
	}
}
