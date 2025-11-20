using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

public abstract class PatternContextLinear : PatternContext<PatternContextLinear.FrameData>
{
	public struct FrameData
	{
		public bool IsNotApplicable;

		public int SegmentIndex;

		public bool InStem;

		private IList<string> _stemItems;

		public IList<string> StemItems => _stemItems ?? (_stemItems = new List<string>());

		public string Stem
		{
			get
			{
				if (_stemItems != null)
				{
					return string.Join("/", _stemItems);
				}
				return null;
			}
		}
	}

	protected ILinearPattern Pattern { get; }

	public PatternContextLinear(ILinearPattern pattern)
	{
		Pattern = pattern;
	}

	public override PatternTestResult Test(FileInfoBase file)
	{
		if (IsStackEmpty())
		{
			throw new InvalidOperationException(SR.CannotTestFile);
		}
		if (!Frame.IsNotApplicable && IsLastSegment() && TestMatchingSegment(file.Name))
		{
			return PatternTestResult.Success(CalculateStem(file));
		}
		return PatternTestResult.Failed;
	}

	public override void PushDirectory(DirectoryInfoBase directory)
	{
		FrameData frame = Frame;
		if (!IsStackEmpty() && !Frame.IsNotApplicable)
		{
			if (!TestMatchingSegment(directory.Name))
			{
				frame.IsNotApplicable = true;
			}
			else
			{
				IPathSegment pathSegment = Pattern.Segments[Frame.SegmentIndex];
				if (frame.InStem || pathSegment.CanProduceStem)
				{
					frame.InStem = true;
					frame.StemItems.Add(directory.Name);
				}
				frame.SegmentIndex++;
			}
		}
		PushDataFrame(frame);
	}

	protected bool IsLastSegment()
	{
		return Frame.SegmentIndex == Pattern.Segments.Count - 1;
	}

	protected bool TestMatchingSegment(string value)
	{
		if (Frame.SegmentIndex >= Pattern.Segments.Count)
		{
			return false;
		}
		return Pattern.Segments[Frame.SegmentIndex].Match(value);
	}

	protected string CalculateStem(FileInfoBase matchedFile)
	{
		return MatcherContext.CombinePath(Frame.Stem, matchedFile.Name);
	}
}
