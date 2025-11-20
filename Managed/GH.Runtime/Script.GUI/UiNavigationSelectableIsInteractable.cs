using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationSelectableIsInteractable : UiNavigationCondition
{
	[SerializeField]
	private UINavigationSelectable _navigationSelectable;

	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return _navigationSelectable.ControlledSelectable.IsInteractable();
	}
}
