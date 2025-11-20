#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using GLOOM.MainMenu;
using SM.Gamepad;
using Script.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIOptionsWindow : Singleton<UIOptionsWindow>
{
	[Serializable]
	private class OptionTab
	{
		public UIMainMenuOption OptionToggle;

		public UISubmenuGOWindow TabWindow;
	}

	[SerializeField]
	private VerticalPointerUI m_VerticalPointer;

	[SerializeField]
	private ToggleGroup m_ToggleGroup;

	[SerializeField]
	private List<OptionTab> m_Tabs;

	[SerializeField]
	private LocalHotkeys m_optionHotkeys;

	[SerializeField]
	private LocalHotkeys m_tabHotkeys;

	[SerializeField]
	private UIMainMenuOption m_LanguageOption;

	[SerializeField]
	private UIMainMenuOption m_DifficultyOption;

	[SerializeField]
	private UIMainMenuOption m_HouseRulesOption;

	[SerializeField]
	private UIMainMenuOption m_DisplayOption;

	[SerializeField]
	private UIMainMenuOption m_PerfomanceOption;

	[SerializeField]
	private UIMainMenuOption m_EpicStoreOption;

	[SerializeField]
	private TextLocalizedListener m_EpicStoreOptionText;

	[SerializeField]
	private ControllerInputAreaLocal m_ControllerArea;

	private UIWindow m_Window;

	private Action m_OnHidden;

	private IHotkeySession m_hotkeySessionOptions;

	private IHotkeySession m_hotkeySessionTabs;

	private INetworkLoginService epicLoginService;

	public LocalHotkeys TabHotkeys => m_tabHotkeys;

	protected override void Awake()
	{
		base.Awake();
		epicLoginService = new DummyNetworkLoginService(signedIn: false, 5f);
		m_Window = GetComponent<UIWindow>();
		m_Window.onShown.AddListener(OnShow);
		m_Window.onHidden.AddListener(OnHidden);
		m_ControllerArea.OnFocusedArea.AddListener(EnableNavigation);
		m_ControllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		RefreshLogIn();
		for (int i = 0; i < m_Tabs.Count; i++)
		{
			InitializeOption(m_Tabs[i].OptionToggle, m_Tabs[i].TabWindow);
		}
		if (PlatformLayer.Instance.IsConsole)
		{
			m_DisplayOption.gameObject.SetActive(value: false);
			DeviceType currentPlatform = PlatformLayer.Instance.GetCurrentPlatform();
			if (currentPlatform != DeviceType.PlayStation5 && currentPlatform != DeviceType.XboxSeriesX && currentPlatform != DeviceType.XboxSeriesS)
			{
				m_PerfomanceOption.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_PerfomanceOption.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			Debug.LogWarning("Options menu can't be destroyed " + base.transform.parent?.name);
		}
		m_Window.onShown.RemoveListener(OnShow);
		m_Window.onHidden.RemoveListener(OnHidden);
		m_ControllerArea.OnFocusedArea.RemoveListener(EnableNavigation);
		m_ControllerArea.OnUnfocusedArea.RemoveListener(DisableNavigation);
		m_LanguageOption = null;
		m_DifficultyOption = null;
		m_HouseRulesOption = null;
		m_DisplayOption = null;
		m_ControllerArea = null;
		base.OnDestroy();
	}

	private void EnableNavigation()
	{
		UIMainMenuOption uIMainMenuOption = null;
		for (int i = 0; i < m_Tabs.Count; i++)
		{
			m_Tabs[i].OptionToggle.EnableNavigation();
			if (uIMainMenuOption == null && m_Tabs[i].OptionToggle.IsSelected)
			{
				uIMainMenuOption = m_Tabs[i].OptionToggle;
			}
		}
		ShowOptionHotkeys();
		m_EpicStoreOption.EnableNavigation();
		EventSystem.current.SetSelectedGameObject((!(uIMainMenuOption == null)) ? uIMainMenuOption.gameObject : m_Tabs.OrderBy((OptionTab it) => it.OptionToggle.transform.GetSiblingIndex()).FirstOrDefault((OptionTab it) => it.OptionToggle.gameObject.activeSelf)?.OptionToggle?.gameObject);
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < m_Tabs.Count; i++)
		{
			m_Tabs[i].OptionToggle.DisableNavigation();
		}
		HideOptionHotkeys();
		m_EpicStoreOption.DisableNavigation();
	}

	private void InitializeOption(UIMainMenuOption option, UISubmenuGOWindow tab)
	{
		tab?.OnHidden.AddListener(option.Deselect);
		option.Init(delegate
		{
			SetFocused(focused: false);
			m_Window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
			ShowTabHotkeys();
			tab.Show();
		}, delegate
		{
			m_Window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
			m_ControllerArea.Focus();
			tab.Hide();
			HideTabHotkeys();
			SetFocused(focused: true);
		});
	}

	private bool IsScenarioOptionsAvailable()
	{
		bool flag = SaveData.Instance.Global.CurrentGameState == EGameState.None || (Singleton<MapChoreographer>.Instance != null && Singleton<MapChoreographer>.Instance.PartyAtHQ);
		if (FFSNetwork.IsShuttingDown || FFSNetwork.IsStartingUp)
		{
			flag = false;
		}
		return flag && ((!FFSNetwork.IsHost) ? (!FFSNetwork.IsOnline) : (PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0));
	}

	private void SetFocused(bool focused)
	{
		for (int i = 0; i < m_Tabs.Count; i++)
		{
			m_Tabs[i].OptionToggle.SetFocused(focused);
		}
	}

	public void Show(RectTransform fromPointer, Action onHidden = null)
	{
		m_OnHidden = onHidden;
		m_VerticalPointer.PointAt(fromPointer);
		m_Window.Show();
		m_ControllerArea.Enable();
	}

	public void Hide()
	{
		m_Window.Hide();
	}

	private void OnShow()
	{
		CloseWindows();
		m_DifficultyOption.gameObject.SetActive(SaveData.Instance?.Global != null && (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign));
		m_HouseRulesOption.gameObject.SetActive(SaveData.Instance?.Global != null && (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign));
		if (IsScenarioOptionsAvailable())
		{
			EnableScenarioOptions();
		}
		else
		{
			DisableScenarioOptions();
		}
		m_EpicStoreOption.gameObject.SetActive(PlatformLayer.UserData.CanLogOutEpicStore());
		RefreshLogIn();
		if (m_HouseRulesOption.gameObject.activeSelf)
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
			{
				m_HouseRulesOption.SetTooltip(enable: false);
			}
			else
			{
				m_HouseRulesOption.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_HOUSE_RULES_SCENARIO_WARNING"));
			}
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu)
		{
			m_PerfomanceOption.IsInteractable = true;
		}
		else
		{
			m_PerfomanceOption.IsInteractable = false;
			m_PerfomanceOption.ResetInteractionMasks();
		}
		SetFocused(focused: true);
	}

	private void ReinitializeTabsBeforeShow()
	{
		foreach (OptionTab tab in m_Tabs)
		{
			if (tab.OptionToggle.gameObject.activeInHierarchy)
			{
				tab.OptionToggle.gameObject.SetActive(value: false);
				tab.OptionToggle.gameObject.SetActive(value: true);
			}
		}
	}

	private void DisableScenarioOptions()
	{
		m_LanguageOption.IsInteractable = false;
		m_LanguageOption.ResetInteractionMasks();
		m_DifficultyOption.IsInteractable = false;
		m_DifficultyOption.ResetInteractionMasks();
		m_HouseRulesOption.IsInteractable = false;
		m_HouseRulesOption.ResetInteractionMasks();
	}

	private void EnableScenarioOptions()
	{
		m_LanguageOption.IsInteractable = true;
		m_DifficultyOption.IsInteractable = true;
		m_HouseRulesOption.IsInteractable = true;
	}

	private void OnHidden()
	{
		CloseWindows();
		m_ControllerArea.Destroy();
		m_OnHidden?.Invoke();
		m_OnHidden = null;
	}

	public void CloseWindows()
	{
		m_Window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		m_ToggleGroup.SetAllTogglesOff();
		SetFocused(focused: true);
	}

	public void Cancel()
	{
		if (m_ToggleGroup.AnyTogglesOn())
		{
			CloseWindows();
		}
		else
		{
			Hide();
		}
	}

	private void ShowOptionHotkeys()
	{
		m_hotkeySessionOptions = m_optionHotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back", "Select");
	}

	private void ShowTabHotkeys()
	{
		m_hotkeySessionTabs = m_tabHotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back");
	}

	private void HideOptionHotkeys()
	{
		m_hotkeySessionOptions.Dispose();
	}

	private void HideTabHotkeys()
	{
		m_hotkeySessionTabs.Dispose();
	}

	private void RefreshLogIn()
	{
		if (m_EpicStoreOption.gameObject.activeSelf)
		{
			string format = "<color=#{1}>{0}";
			if (epicLoginService.IsSignedIn())
			{
				m_EpicStoreOptionText.Text.text = string.Format(format, LocalizationManager.GetTranslation("GUI_LOGOUT"), UIInfoTools.Instance.warningColor.ToHex());
				m_EpicStoreOption.Init(AskLogOut);
			}
			else
			{
				m_EpicStoreOptionText.Text.text = string.Format(format, LocalizationManager.GetTranslation("GUI_LOGIN"), UIInfoTools.Instance.basicTextColor.ToHex());
				m_EpicStoreOption.Init(LogIn);
			}
			m_EpicStoreOption.IsInteractable = !epicLoginService.IsSigningIn() && !epicLoginService.IsSigningOut();
		}
	}

	private void AskLogOut()
	{
		if (!epicLoginService.IsSigningOut())
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_LOG_OUT_EPICSTORE_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_LOG_OUT_EPICSTORE"), LogOut, m_EpicStoreOption.Deselect);
		}
	}

	private void LogOut()
	{
		m_EpicStoreOption.Deselect();
		m_EpicStoreOption.IsInteractable = false;
		epicLoginService.SignOut().Done(delegate
		{
			if (m_Window.IsOpen)
			{
				RefreshLogIn();
			}
		});
	}

	private void LogIn()
	{
		m_EpicStoreOption.Deselect();
		m_EpicStoreOption.IsInteractable = false;
		epicLoginService.SignIn().Done(delegate
		{
			if (m_Window.IsOpen)
			{
				RefreshLogIn();
			}
		});
	}
}
