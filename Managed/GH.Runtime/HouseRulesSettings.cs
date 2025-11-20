#define ENABLE_LOGS
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using SharedLibrary.SimpleLog;
using UnityEngine;

public class HouseRulesSettings : MonoBehaviour
{
	[SerializeField]
	private UIHouseRulesSelector selector;

	private IHotkeySession _hotkeySession;

	private void OnEnable()
	{
		_hotkeySession = Singleton<UIOptionsWindow>.Instance?.TabHotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Tips");
		RefreshInteractability();
		selector.SetValue(AdventureState.MapState.HouseRulesSetting);
	}

	private void OnDisable()
	{
		_hotkeySession?.Dispose();
		StateShared.EHouseRulesFlag value = selector.GetValue();
		if (AdventureState.MapState.HouseRulesSetting == value)
		{
			return;
		}
		Debug.LogGUI("Updated rules");
		SimpleLog.AddToSimpleLog("House Rules setting switched to " + value);
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.ChangeHouseRuleSettings, ActionProcessor.CurrentPhase, disableAutoReplication: false, 0, 0, 0, (int)value);
			}
			else
			{
				Debug.LogError("Client changed house rule settings locally somehow");
			}
		}
		else
		{
			AdventureState.MapState.ChangeHouseRules(value);
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	private void RefreshInteractability()
	{
		bool flag = IsInteractable();
		selector.SetTooltip((!flag && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario) ? string.Format("<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_HOUSE_RULES_SCENARIO_WARNING") + "</color>") : null);
	}

	private bool IsInteractable()
	{
		bool flag = SaveData.Instance.Global.CurrentGameState == EGameState.None || (Singleton<MapChoreographer>.Instance != null && Singleton<MapChoreographer>.Instance.PartyAtHQ);
		if (FFSNetwork.IsShuttingDown || FFSNetwork.IsStartingUp)
		{
			flag = false;
		}
		return flag && ((!FFSNetwork.IsHost) ? (!FFSNetwork.IsOnline) : (PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0));
	}

	public static void ProxySetHouseRuleSettings(GameAction action)
	{
		StateShared.EHouseRulesFlag supplementaryDataIDMax = (StateShared.EHouseRulesFlag)action.SupplementaryDataIDMax;
		AdventureState.MapState.ChangeHouseRules(supplementaryDataIDMax);
		SaveData.Instance.SaveCurrentAdventureData();
		Console.LogInfo("House rule setting switched to " + supplementaryDataIDMax);
	}
}
