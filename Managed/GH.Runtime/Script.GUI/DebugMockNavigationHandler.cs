#define ENABLE_LOGS
using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class DebugMockNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private bool _result = true;

	[SerializeField]
	private string _messege = "IT'S ALIVE!!!";

	[SerializeField]
	private bool _asError = true;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (_asError)
		{
			Debug.LogError(_messege, this);
		}
		else
		{
			Debug.Log(_messege, this);
		}
		return _result;
	}
}
