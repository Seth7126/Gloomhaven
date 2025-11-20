using System;
using MapRuleLibrary.State;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;

namespace GLOOM.MainMenu;

public class UIGuildmasterCreateGameWindow : UICreateGameDataStep
{
	[Header("Gold")]
	[SerializeField]
	private UIGoldModeSelector m_GoldSelector;

	private Action _onCanceled;

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCanceled = null)
	{
		m_GoldSelector.SetMode((gameData.GoldMode == EGoldMode.None) ? EGoldMode.PartyGold : gameData.GoldMode);
		_onCanceled = onCanceled;
		base.Setup(service, gameData, onConfirmed, (Action)OnCancel);
	}

	protected override GameData BuildGameModel(GameData data)
	{
		base.BuildGameModel(data);
		data.GoldMode = m_GoldSelector.GetSelectedMode();
		return data;
	}

	private void OnCancel()
	{
		_onCanceled?.Invoke();
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.SubMenuOptionsWithSelected);
		}
	}
}
