using System;
using System.Collections.Generic;
using SM.Gamepad;
using ScenarioRuleLibrary;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationPhasesSeparator : CustomizableNavigationHandlerBehaviour
{
	[Serializable]
	public class PhasesHolder
	{
		[SerializeField]
		private List<CPhase.PhaseType> _phases;

		[SerializeField]
		private CustomizableNavigationHandlerBehaviour _handler;

		public List<CPhase.PhaseType> Phases => _phases;

		public CustomizableNavigationHandlerBehaviour Handler => _handler;
	}

	[SerializeField]
	private PhasesHolder[] _phasesHolders;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _anyOtherPhaseHandler;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		bool flag = false;
		PhasesHolder[] phasesHolders = _phasesHolders;
		foreach (PhasesHolder phasesHolder in phasesHolders)
		{
			if (phasesHolder.Phases.Contains(PhaseManager.PhaseType))
			{
				flag = phasesHolder.Handler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			proceededNodes.Add(inNode);
			return _anyOtherPhaseHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
		}
		return true;
	}
}
