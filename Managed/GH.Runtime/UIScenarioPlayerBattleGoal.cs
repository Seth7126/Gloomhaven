using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioPlayerBattleGoal : MonoBehaviour
{
	[SerializeField]
	private Image characterMarker;

	[SerializeField]
	private UIScenarioBattleGoalProgress progress;

	public void Init(ECharacter character, CBattleGoalState battleGoal, string custom = null)
	{
		characterMarker.sprite = UIInfoTools.Instance.GetCharacterMarker(character, custom);
		progress.SetBattleGoal(battleGoal);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		progress.UpdateProgress();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void UpdateProgress()
	{
		if (progress.gameObject.activeSelf)
		{
			progress.UpdateProgress();
		}
	}
}
