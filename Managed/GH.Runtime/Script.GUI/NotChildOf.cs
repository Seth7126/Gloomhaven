using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class NotChildOf : UiNavigationCondition
{
	[SerializeField]
	private Transform _checkTransform;

	[SerializeField]
	private Transform _parent;

	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return !_checkTransform.IsChildOf(_parent);
	}
}
