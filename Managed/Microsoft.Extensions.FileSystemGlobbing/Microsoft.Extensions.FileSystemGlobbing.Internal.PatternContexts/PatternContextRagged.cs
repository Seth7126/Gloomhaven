using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;

public abstract class PatternContextRagged : PatternContext<PatternContextRagged.FrameData>
{
	public struct FrameData
	{
		public bool IsNotApplicable;

		public int SegmentGroupIndex;

		public IList<IPathSegment> SegmentGroup;

		public int BacktrackAvailable;

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

	protected IRaggedPattern Pattern { get; }

	public PatternContextRagged(IRaggedPattern pattern)
	{
		Pattern = pattern;
	}

	public override PatternTestResult Test(FileInfoBase file)
	{
		if (IsStackEmpty())
		{
			throw new InvalidOperationException(SR.CannotTestFile);
		}
		if (!Frame.IsNotApplicable && IsEndingGroup() && TestMatchingGroup(file))
		{
			return PatternTestResult.Success(CalculateStem(file));
		}
		return PatternTestResult.Failed;
	}

	public sealed override void PushDirectory(DirectoryInfoBase directory)
	{
		FrameData frame = Frame;
		if (IsStackEmpty())
		{
			frame.SegmentGroupIndex = -1;
			frame.SegmentGroup = Pattern.StartsWith;
		}
		else if (!Frame.IsNotApplicable)
		{
			if (IsStartingGroup())
			{
				if (!TestMatchingSegment(directory.Name))
				{
					frame.IsNotApplicable = true;
				}
				else
				{
					frame.SegmentIndex++;
				}
			}
			else if (!IsStartingGroup() && directory.Name == "..")
			{
				frame.IsNotApplicable = true;
			}
			else if (!IsStartingGroup() && !IsEndingGroup() && TestMatchingGroup(directory))
			{
				frame.SegmentIndex = Frame.SegmentGroup.Count;
				frame.BacktrackAvailable = 0;
			}
			else
			{
				frame.BacktrackAvailable++;
			}
		}
		if (frame.InStem)
		{
			frame.StemItems.Add(directory.Name);
		}
		while (frame.SegmentIndex == frame.SegmentGroup.Count && frame.SegmentGroupIndex != Pattern.Contains.Count)
		{
			frame.SegmentGroupIndex++;
			frame.SegmentIndex = 0;
			if (frame.SegmentGroupIndex < Pattern.Contains.Count)
			{
				frame.SegmentGroup = Pattern.Contains[frame.SegmentGroupIndex];
			}
			else
			{
				frame.SegmentGroup = Pattern.EndsWith;
			}
			frame.InStem = true;
		}
		PushDataFrame(frame);
	}

	public override void PopDirectory()
	{
		base.PopDirectory();
		if (Frame.StemItems.Count > 0)
		{
			Frame.StemItems.RemoveAt(Frame.StemItems.Count - 1);
		}
	}

	protected bool IsStartingGroup()
	{
		return Frame.SegmentGroupIndex == -1;
	}

	protected bool IsEndingGroup()
	{
		return Frame.SegmentGroupIndex == Pattern.Contains.Count;
	}

	protected bool TestMatchingSegment(string value)
	{
		if (Frame.SegmentIndex >= Frame.SegmentGroup.Count)
		{
			return false;
		}
		return Frame.SegmentGroup[Frame.SegmentIndex].Match(value);
	}

	protected bool TestMatchingGroup(FileSystemInfoBase value)
	{
		int count = Frame.SegmentGroup.Count;
		int num = Frame.BacktrackAvailable + 1;
		if (num < count)
		{
			return false;
		}
		FileSystemInfoBase fileSystemInfoBase = value;
		for (int i = 0; i != count; i++)
		{
			IPathSegment pathSegment = Frame.SegmentGroup[count - i - 1];
			if (!pathSegment.Match(fileSystemInfoBase.Name))
			{
				return false;
			}
			fileSystemInfoBase = fileSystemInfoBase.ParentDirectory;
		}
		return true;
	}

	protected string CalculateStem(FileInfoBase matchedFile)
	{
		return MatcherContext.CombinePath(Frame.Stem, matchedFile.Name);
	}
}
