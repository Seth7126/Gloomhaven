using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIHouseRulesSelector : MonoBehaviour
{
	[Serializable]
	public class HouseRulesSelectorEvent : UnityEvent<StateShared.EHouseRulesFlag>
	{
	}

	[SerializeField]
	private List<UIHouseRuleSelector> rules;

	public HouseRulesSelectorEvent OnRulesChanged;

	private void Awake()
	{
		for (int i = 0; i < rules.Count; i++)
		{
			rules[i].OnModeChanged.AddListener(delegate
			{
				OnModeChanged();
			});
		}
	}

	private void OnModeChanged()
	{
		if (rules.All((UIHouseRuleSelector it) => it.HasSelectedMode()))
		{
			OnRulesChanged.Invoke(GetValue());
		}
	}

	public void SetValue(StateShared.EHouseRulesFlag flag)
	{
		for (int i = 0; i < rules.Count; i++)
		{
			rules[i].SetMode(flag);
		}
	}

	public StateShared.EHouseRulesFlag GetValue()
	{
		StateShared.EHouseRulesFlag eHouseRulesFlag = StateShared.EHouseRulesFlag.None;
		for (int i = 0; i < rules.Count; i++)
		{
			eHouseRulesFlag |= rules[i].GetSelectedMode();
		}
		return eHouseRulesFlag;
	}

	public void SetTooltip(string tooltip)
	{
		for (int i = 0; i < rules.Count; i++)
		{
			rules[i].SetTooltip(tooltip);
		}
	}
}
