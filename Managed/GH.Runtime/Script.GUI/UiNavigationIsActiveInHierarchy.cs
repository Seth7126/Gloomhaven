using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationIsActiveInHierarchy : UiNavigationCondition
{
	[SerializeField]
	private GameObject _target;

	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (_target == null)
		{
			_target = base.gameObject;
		}
		return _target.activeInHierarchy;
	}
}
