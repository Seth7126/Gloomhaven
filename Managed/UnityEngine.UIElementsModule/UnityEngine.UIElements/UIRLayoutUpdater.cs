using Unity.Profiling;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

internal class UIRLayoutUpdater : BaseVisualTreeUpdater
{
	private const int kMaxValidateLayoutCount = 5;

	private static readonly string s_Description = "Update Layout";

	private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker(s_Description);

	public override ProfilerMarker profilerMarker => s_ProfilerMarker;

	public override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		if ((versionChangeType & (VersionChangeType.Hierarchy | VersionChangeType.Layout)) != 0)
		{
			YogaNode yogaNode = ve.yogaNode;
			if (yogaNode != null && yogaNode.IsMeasureDefined)
			{
				yogaNode.MarkDirty();
			}
		}
	}

	public override void Update()
	{
		int num = 0;
		while (base.visualTree.yogaNode.IsDirty)
		{
			if (num > 0)
			{
				base.panel.ApplyStyles();
			}
			base.panel.duringLayoutPhase = true;
			base.visualTree.yogaNode.CalculateLayout();
			base.panel.duringLayoutPhase = false;
			using (new EventDispatcherGate(base.visualTree.panel.dispatcher))
			{
				UpdateSubTree(base.visualTree, num);
			}
			if (num++ >= 5)
			{
				Debug.LogError("Layout update is struggling to process current layout (consider simplifying to avoid recursive layout): " + base.visualTree);
				break;
			}
		}
		base.visualTree.focusController.ReevaluateFocus();
	}

	private void UpdateSubTree(VisualElement ve, int currentLayoutPass, bool isDisplayed = true)
	{
		Rect rect = new Rect(ve.yogaNode.LayoutX, ve.yogaNode.LayoutY, ve.yogaNode.LayoutWidth, ve.yogaNode.LayoutHeight);
		Rect lastPseudoPadding = new Rect(ve.yogaNode.LayoutPaddingLeft, ve.yogaNode.LayoutPaddingTop, ve.yogaNode.LayoutWidth - (ve.yogaNode.LayoutPaddingLeft + ve.yogaNode.LayoutPaddingRight), ve.yogaNode.LayoutHeight - (ve.yogaNode.LayoutPaddingTop + ve.yogaNode.LayoutPaddingBottom));
		Rect lastLayout = ve.lastLayout;
		Rect lastPseudoPadding2 = ve.lastPseudoPadding;
		bool isHierarchyDisplayed = ve.isHierarchyDisplayed;
		VersionChangeType versionChangeType = (VersionChangeType)0;
		bool flag = lastLayout.size != rect.size;
		bool flag2 = lastPseudoPadding2.size != lastPseudoPadding.size;
		if (flag || flag2)
		{
			versionChangeType |= VersionChangeType.Size | VersionChangeType.Repaint;
		}
		bool flag3 = rect.position != lastLayout.position;
		bool flag4 = lastPseudoPadding.position != lastPseudoPadding2.position;
		if (flag3 || flag4)
		{
			versionChangeType |= VersionChangeType.Transform;
		}
		isDisplayed &= ve.resolvedStyle.display != DisplayStyle.None;
		ve.isHierarchyDisplayed = isDisplayed;
		if (versionChangeType != 0)
		{
			ve.IncrementVersion(versionChangeType);
		}
		ve.lastLayout = rect;
		ve.lastPseudoPadding = lastPseudoPadding;
		bool hasNewLayout = ve.yogaNode.HasNewLayout;
		if (hasNewLayout)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UpdateSubTree(ve.hierarchy[i], currentLayoutPass, isDisplayed);
			}
		}
		if (flag || flag3)
		{
			using GeometryChangedEvent geometryChangedEvent = GeometryChangedEvent.GetPooled(lastLayout, rect);
			geometryChangedEvent.layoutPass = currentLayoutPass;
			geometryChangedEvent.target = ve;
			ve.SendEvent(geometryChangedEvent);
		}
		if (hasNewLayout)
		{
			ve.yogaNode.MarkLayoutSeen();
		}
	}
}
