using System;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;

namespace GLOOM.MainMenu;

public class UICampaignCreateGameWindow : UICreateGameDataStep
{
	[Header("Enhancement")]
	[SerializeField]
	private UIEnhancementModeSelector m_EnhancementSelector;

	private Action _onCanceled;

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		string key = typeof(EEnhancementMode).ToString();
		m_EnhancementSelector.SetMode((!gameData.HasParam(key)) ? EEnhancementMode.CharacterPersistent : gameData.GetParam<EEnhancementMode>(key));
		_onCanceled = onCancelled;
		base.Setup(service, gameData, onConfirmed, (Action)OnCancel);
	}

	protected override GameData BuildGameModel(GameData data)
	{
		base.BuildGameModel(data);
		data.GoldMode = EGoldMode.CharacterGold;
		data.DLCEnabled = DLCRegistry.EDLCKey.None;
		data.AddParam(typeof(EEnhancementMode).ToString(), m_EnhancementSelector.GetSelectedMode());
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
