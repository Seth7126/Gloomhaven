using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class EnemyCurrentTurnStatPanel : Singleton<EnemyCurrentTurnStatPanel>
{
	[SerializeField]
	private MonsterBaseUI _currentTurnCard;

	private UIWindow _window;

	private CEnemyActor _currentShownEnemy;

	private RequestCounter _hideCounter;

	protected override void Awake()
	{
		base.Awake();
		_window = GetComponent<UIWindow>();
		_hideCounter = new RequestCounter(UpdateShown, UpdateShown);
	}

	public void Show(CEnemyActor enemyActor)
	{
		CMonsterAbilityCard roundAbilityCard = enemyActor.MonsterClass.RoundAbilityCard;
		if (roundAbilityCard != null && _currentShownEnemy != enemyActor)
		{
			_currentShownEnemy = enemyActor;
			_currentTurnCard.GenerateCard(roundAbilityCard, enemyActor);
			UpdateShown();
		}
	}

	public void Hide()
	{
		_currentShownEnemy = null;
		UpdateShown();
	}

	private void UpdateShown()
	{
		if (_currentShownEnemy != null && _hideCounter.Empty)
		{
			_window.Show();
		}
		else
		{
			_window.Hide();
		}
	}

	public void Hide(object requester)
	{
		_hideCounter.Add(requester);
	}

	public void CancelHide(object requester)
	{
		_hideCounter.Remove(requester);
	}
}
