using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationHasInteractableCondition : UiNavigationCondition
{
	[SerializeField]
	private List<UINavigationSelectable> _selectables;

	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		foreach (UINavigationSelectable selectable in _selectables)
		{
			if (selectable.ControlledSelectable.IsInteractable())
			{
				return true;
			}
		}
		return false;
	}
}
