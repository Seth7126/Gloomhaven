using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class PropagationPaths
{
	[Flags]
	public enum Type
	{
		None = 0,
		TrickleDown = 1,
		BubbleUp = 2
	}

	private static readonly ObjectPool<PropagationPaths> s_Pool = new ObjectPool<PropagationPaths>();

	public readonly List<VisualElement> trickleDownPath;

	public readonly List<VisualElement> targetElements;

	public readonly List<VisualElement> bubbleUpPath;

	private const int k_DefaultPropagationDepth = 16;

	private const int k_DefaultTargetCount = 4;

	public PropagationPaths()
	{
		trickleDownPath = new List<VisualElement>(16);
		targetElements = new List<VisualElement>(4);
		bubbleUpPath = new List<VisualElement>(16);
	}

	public PropagationPaths(PropagationPaths paths)
	{
		trickleDownPath = new List<VisualElement>(paths.trickleDownPath);
		targetElements = new List<VisualElement>(paths.targetElements);
		bubbleUpPath = new List<VisualElement>(paths.bubbleUpPath);
	}

	internal static PropagationPaths Copy(PropagationPaths paths)
	{
		PropagationPaths propagationPaths = s_Pool.Get();
		propagationPaths.trickleDownPath.AddRange(paths.trickleDownPath);
		propagationPaths.targetElements.AddRange(paths.targetElements);
		propagationPaths.bubbleUpPath.AddRange(paths.bubbleUpPath);
		return propagationPaths;
	}

	public static PropagationPaths Build(VisualElement elem, EventBase evt, Type pathTypesRequested)
	{
		if (elem == null || pathTypesRequested == Type.None)
		{
			return null;
		}
		PropagationPaths propagationPaths = s_Pool.Get();
		propagationPaths.targetElements.Add(elem);
		while (elem.hierarchy.parent != null)
		{
			elem = elem.hierarchy.parent;
			if (elem.isCompositeRoot && !evt.ignoreCompositeRoots)
			{
				propagationPaths.targetElements.Add(elem);
				continue;
			}
			if ((pathTypesRequested & Type.TrickleDown) == Type.TrickleDown && elem.HasTrickleDownHandlers())
			{
				propagationPaths.trickleDownPath.Add(elem);
			}
			if ((pathTypesRequested & Type.BubbleUp) == Type.BubbleUp && elem.HasBubbleUpHandlers())
			{
				propagationPaths.bubbleUpPath.Add(elem);
			}
		}
		return propagationPaths;
	}

	public void Release()
	{
		bubbleUpPath.Clear();
		targetElements.Clear();
		trickleDownPath.Clear();
		s_Pool.Release(this);
	}
}
