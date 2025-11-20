using System.Collections.Generic;
using SM.Gamepad;
using ScenarioRuleLibrary;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationPhaseCondition : UiNavigationCondition
{
	[SerializeField]
	private CPhase.PhaseType _phaseType;

	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return PhaseManager.PhaseType == _phaseType;
	}
}
