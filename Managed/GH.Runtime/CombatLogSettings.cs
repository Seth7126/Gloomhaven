using System;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatLogSettings : Singleton<CombatLogSettings>
{
	[SerializeField]
	private ButtonSwitch toggleCombatLog;

	[SerializeField]
	private CombatLogFilterSetting[] filterSettings;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private UISubmenuGOWindow submenuGOWindow;

	public static Action OnCombatLogFiltersChanged;

	public static Action OnEnableCombatLog;

	public static Action OnDisableCombatLog;

	protected override void Awake()
	{
		base.Awake();
		filterSettings = GetComponentsInChildren<CombatLogFilterSetting>(includeInactive: true);
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		if (controllerArea.IsEnabled)
		{
			EnableNavigation();
		}
	}

	public void Initialize()
	{
		toggleCombatLog.IsOn = !SaveData.Instance.Global.DisabledCombatLog;
		toggleCombatLog.OnValueChanged.AddListener(ToggleCombatLog);
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].gameObject.SetActive(!SaveData.Instance.Global.DisabledCombatLog);
			filterSettings[i].Init(ToggleFilter);
		}
	}

	public void ToggleFilter(CombatLogFilter filter, bool active)
	{
		bool flag = false;
		if (!active && !SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter))
		{
			flag = true;
			SaveData.Instance.Global.DisabledCombatLogFilters.Add(filter);
		}
		else if (active && SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter))
		{
			flag = true;
			SaveData.Instance.Global.DisabledCombatLogFilters.Remove(filter);
		}
		if (flag)
		{
			SaveData.Instance.SaveGlobalData();
			OnCombatLogFiltersChanged?.Invoke();
		}
	}

	public void ToggleCombatLog(bool enable)
	{
		SaveData.Instance.Global.DisabledCombatLog = !enable;
		SaveData.Instance.SaveGlobalData();
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].gameObject.SetActive(enable);
		}
		if (enable)
		{
			OnEnableCombatLog?.Invoke();
		}
		else
		{
			OnDisableCombatLog?.Invoke();
		}
	}

	private void DisableNavigation()
	{
		toggleCombatLog.DisableNavigation();
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].DisableNavigation();
		}
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].EnableNavigation();
		}
		toggleCombatLog.SetNavigation(Navigation.Mode.Vertical);
		EventSystem.current.SetSelectedGameObject(toggleCombatLog.gameObject);
	}
}
