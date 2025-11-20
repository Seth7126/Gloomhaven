using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

public class PatternContextLinearInclude : PatternContextLinear
{
	public PatternContextLinearInclude(ILinearPattern pattern)
		: base(pattern)
	{
	}

	public override void Declare(Action<IPathSegment, bool> onDeclare)
	{
		if (IsStackEmpty())
		{
			throw new InvalidOperationException(SR.CannotDeclarePathSegment);
		}
		if (!Frame.IsNotApplicable && Frame.SegmentIndex < base.Pattern.Segments.Count)
		{
			onDeclare(base.Pattern.Segments[Frame.SegmentIndex], IsLastSegment());
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
		if (!IsLastSegment())
		{
			return TestMatchingSegment(directory.Name);
		}
		return false;
	}
}
