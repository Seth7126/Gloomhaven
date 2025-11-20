using System;
using MapRuleLibrary.Adventure;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroduceMapConcept : UIIntroduceConcept
{
	[SerializeField]
	private bool saveIntroDone;

	public override void Show(Action onFinished = null)
	{
		string id = ((conceptConfig == null) ? concept.ToString() : conceptConfig.Concept.ToString());
		if (AdventureState.MapState.MapParty.HasIntroduced(id))
		{
			onFinished?.Invoke();
			return;
		}
		AdventureState.MapState.MapParty.MarkIntroDone(id);
		if (saveIntroDone)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
		base.Show(onFinished);
	}
}
