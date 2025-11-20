using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.FileSystemGlobbing.Util;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal;

public class MatcherContext
{
	private readonly DirectoryInfoBase _root;

	private readonly List<IPatternContext> _includePatternContexts;

	private readonly List<IPatternContext> _excludePatternContexts;

	private readonly List<FilePatternMatch> _files;

	private readonly HashSet<string> _declaredLiteralFolderSegmentInString;

	private readonly HashSet<LiteralPathSegment> _declaredLiteralFolderSegments = new HashSet<LiteralPathSegment>();

	private readonly HashSet<LiteralPathSegment> _declaredLiteralFileSegments = new HashSet<LiteralPathSegment>();

	private bool _declaredParentPathSegment;

	private bool _declaredWildcardPathSegment;

	private readonly StringComparison _comparisonType;

	public MatcherContext(IEnumerable<IPattern> includePatterns, IEnumerable<IPattern> excludePatterns, DirectoryInfoBase directoryInfo, StringComparison comparison)
	{
		_root = directoryInfo;
		_files = new List<FilePatternMatch>();
		_comparisonType = comparison;
		_includePatternContexts = includePatterns.Select((IPattern pattern) => pattern.CreatePatternContextForInclude()).ToList();
		_excludePatternContexts = excludePatterns.Select((IPattern pattern) => pattern.CreatePatternContextForExclude()).ToList();
		_declaredLiteralFolderSegmentInString = new HashSet<string>(StringComparisonHelper.GetStringComparer(comparison));
	}

	public PatternMatchingResult Execute()
	{
		_files.Clear();
		Match(_root, null);
		return new PatternMatchingResult(_files, _files.Count > 0);
	}

	private void Match(DirectoryInfoBase directory, string parentRelativePath)
	{
		PushDirectory(directory);
		Declare();
		List<FileSystemInfoBase> list = new List<FileSystemInfoBase>();
		if (_declaredWildcardPathSegment || _declaredLiteralFileSegments.Any())
		{
			list.AddRange(directory.EnumerateFileSystemInfos());
		}
		else
		{
			IEnumerable<DirectoryInfoBase> enumerable = directory.EnumerateFileSystemInfos().OfType<DirectoryInfoBase>();
			foreach (DirectoryInfoBase item in enumerable)
			{
				if (_declaredLiteralFolderSegmentInString.Contains(item.Name))
				{
					list.Add(item);
				}
			}
		}
		if (_declaredParentPathSegment)
		{
			list.Add(directory.GetDirectory(".."));
		}
		List<DirectoryInfoBase> list2 = new List<DirectoryInfoBase>();
		foreach (FileSystemInfoBase item2 in list)
		{
			if (item2 is FileInfoBase fileInfoBase)
			{
				PatternTestResult patternTestResult = MatchPatternContexts(fileInfoBase, (IPatternContext pattern, FileInfoBase file) => pattern.Test(file));
				if (patternTestResult.IsSuccessful)
				{
					_files.Add(new FilePatternMatch(CombinePath(parentRelativePath, fileInfoBase.Name), patternTestResult.Stem));
				}
			}
			else if (item2 is DirectoryInfoBase directoryInfoBase && MatchPatternContexts(directoryInfoBase, (IPatternContext pattern, DirectoryInfoBase dir) => pattern.Test(dir)))
			{
				list2.Add(directoryInfoBase);
			}
		}
		foreach (DirectoryInfoBase item3 in list2)
		{
			string parentRelativePath2 = CombinePath(parentRelativePath, item3.Name);
			Match(item3, parentRelativePath2);
		}
		PopDirectory();
	}

	private void Declare()
	{
		_declaredLiteralFileSegments.Clear();
		_declaredLiteralFolderSegments.Clear();
		_declaredParentPathSegment = false;
		_declaredWildcardPathSegment = false;
		foreach (IPatternContext includePatternContext in _includePatternContexts)
		{
			includePatternContext.Declare(DeclareInclude);
		}
	}

	private void DeclareInclude(IPathSegment patternSegment, bool isLastSegment)
	{
		if (patternSegment is LiteralPathSegment literalPathSegment)
		{
			if (isLastSegment)
			{
				_declaredLiteralFileSegments.Add(literalPathSegment);
				return;
			}
			_declaredLiteralFolderSegments.Add(literalPathSegment);
			_declaredLiteralFolderSegmentInString.Add(literalPathSegment.Value);
		}
		else if (patternSegment is ParentPathSegment)
		{
			_declaredParentPathSegment = true;
		}
		else if (patternSegment is WildcardPathSegment)
		{
			_declaredWildcardPathSegment = true;
		}
	}

	internal static string CombinePath(string left, string right)
	{
		if (string.IsNullOrEmpty(left))
		{
			return right;
		}
		return left + "/" + right;
	}

	private bool MatchPatternContexts<TFileInfoBase>(TFileInfoBase fileinfo, Func<IPatternContext, TFileInfoBase, bool> test)
	{
		return MatchPatternContexts(fileinfo, (IPatternContext ctx, TFileInfoBase file) => test(ctx, file) ? PatternTestResult.Success(string.Empty) : PatternTestResult.Failed).IsSuccessful;
	}

	private PatternTestResult MatchPatternContexts<TFileInfoBase>(TFileInfoBase fileinfo, Func<IPatternContext, TFileInfoBase, PatternTestResult> test)
	{
		PatternTestResult result = PatternTestResult.Failed;
		foreach (IPatternContext includePatternContext in _includePatternContexts)
		{
			PatternTestResult patternTestResult = test(includePatternContext, fileinfo);
			if (patternTestResult.IsSuccessful)
			{
				result = patternTestResult;
				break;
			}
		}
		if (!result.IsSuccessful)
		{
			return PatternTestResult.Failed;
		}
		foreach (IPatternContext excludePatternContext in _excludePatternContexts)
		{
			if (test(excludePatternContext, fileinfo).IsSuccessful)
			{
				return PatternTestResult.Failed;
			}
		}
		return result;
	}

	private void PopDirectory()
	{
		foreach (IPatternContext excludePatternContext in _excludePatternContexts)
		{
			excludePatternContext.PopDirectory();
		}
		foreach (IPatternContext includePatternContext in _includePatternContexts)
		{
			includePatternContext.PopDirectory();
		}
	}

	private void PushDirectory(DirectoryInfoBase directory)
	{
		foreach (IPatternContext includePatternContext in _includePatternContexts)
		{
			includePatternContext.PushDirectory(directory);
		}
		foreach (IPatternContext excludePatternContext in _excludePatternContexts)
		{
			excludePatternContext.PushDirectory(directory);
		}
	}
}
