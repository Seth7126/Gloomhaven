using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

public class PatternContextRaggedInclude : PatternContextRagged
{
	public PatternContextRaggedInclude(IRaggedPattern pattern)
		: base(pattern)
	{
	}

	public override void Declare(Action<IPathSegment, bool> onDeclare)
	{
		if (IsStackEmpty())
		{
			throw new InvalidOperationException(SR.CannotDeclarePathSegment);
		}
		if (!Frame.IsNotApplicable)
		{
			if (IsStartingGroup() && Frame.SegmentIndex < Frame.SegmentGroup.Count)
			{
				onDeclare(Frame.SegmentGroup[Frame.SegmentIndex], arg2: false);
			}
			else
			{
				onDeclare(WildcardPathSegment.MatchAll, arg2: false);
			}
		}
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
		if (IsStartingGroup() && !TestMatchingSegment(directory.Name))
		{
			return false;
		}
		return true;
	}
}
