using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public abstract class UiNavigationCondition : MonoBehaviour
{
	public abstract bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection);
}
