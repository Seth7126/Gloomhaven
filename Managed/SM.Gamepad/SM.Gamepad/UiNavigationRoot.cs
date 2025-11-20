using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SM.Gamepad;

[DefaultExecutionOrder(1)]
public class UiNavigationRoot : UiNavigationGroup, IUiNavigationRoot, IUiNavigationNode, IUiNavigationElement
{
	protected override IUiNavigationRoot RootOrSelf => this;

	public event Action<IUiNavigationRoot> OnRootElementEnabledEvent = delegate
	{
	};

	public event Action<IUiNavigationRoot> OnRootElementDisabledEvent = delegate
	{
	};

	[UsedImplicitly]
	protected override void Awake()
	{
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		SM.Gamepad.UiNavigationManager.Instance.RegisterRoot(this);
		this.OnRootElementEnabledEvent(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		this.OnRootElementDisabledEvent(this);
		SM.Gamepad.UiNavigationManager.Instance.UnRegisterRoot(this);
	}

	[UsedImplicitly]
	protected override void OnTransformParentChanged()
	{
	}

	[UsedImplicitly]
	protected override void OnBeforeTransformParentChanged()
	{
		SM.Gamepad.UiNavigationManager.Instance.ClearRootCache(this);
		base.OnBeforeTransformParentChanged();
	}

	public void InitializeRootElement(IUiNavigationManager uiNavigationManager, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		NavigationManager = uiNavigationManager;
		InitializeBaseElement(uiNavigationManager, null, elementsByNameMap, knownSelectablesMap);
		InitializeOldFashionNavigationHandlers();
		if (!knownElements.Contains(this))
		{
			knownElements.Add(this);
		}
		UiNavigationElementsUtils.InitElements(base.Elements, base.UiNavigationManager, this, knownElements, elementsByNameMap, knownSelectablesMap);
	}

	public override void InitializeNodeElement(IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		InitializeRootElement(uiNavigationManager, knownElements, elementsByNameMap, knownSelectablesMap);
	}

	public override void RefreshParent()
	{
		foreach (IUiNavigationElement item in new List<IUiNavigationElement>(base.Elements))
		{
			item.RefreshParent();
		}
	}
}
