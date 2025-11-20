using System;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using UnityEngine;

public class UIIntroduceLevelUp : UIIntroduceConcept
{
	[SerializeField]
	private IntroductionConfigUI configAfterCardSelected;

	public override void Show(Action onFinished = null)
	{
		if (AdventureState.MapState.MapParty.HasIntroduced(concept.ToString()))
		{
			onFinished?.Invoke();
		}
		else
		{
			base.Show(onFinished);
		}
	}

	public void ShowAfterCardSelected(Action onFinished = null)
	{
		string id = concept.ToString();
		if (AdventureState.MapState.MapParty.HasIntroduced(id))
		{
			onFinished?.Invoke();
			return;
		}
		Show(configAfterCardSelected, onFinished);
		AdventureState.MapState.MapParty.MarkIntroDone(id);
	}
}
