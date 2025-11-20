using Platforms.Social;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gloomhaven;

public class GeneralSettings : MonoBehaviour
{
	[SerializeField]
	private ButtonSwitch crossPlayToggle;

	[SerializeField]
	private ButtonSwitch scenarioSpeedUpToggle;

	[SerializeField]
	private GameObject crossPlayObject;

	[SerializeField]
	private ControllerInputAreaLocal m_ControllerArea;

	private void Awake()
	{
		m_ControllerArea.OnFocusedArea.AddListener(EnableNavigation);
		m_ControllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
	}

	private void OnEnable()
	{
		crossPlayObject.SetActive(!FFSNetwork.IsOnline);
		crossPlayToggle.IsOn = SaveData.Instance.Global.CrossplayEnabled;
		scenarioSpeedUpToggle.IsOn = SaveData.Instance.Global.SpeedUpToggle;
		crossPlayToggle.OnValueChanged.AddListener(ToggleCrossPlay);
		scenarioSpeedUpToggle.OnValueChanged.AddListener(ToggleScenarioSpeedUp);
		if (!FFSNetwork.IsOnline && PlatformLayer.Instance.IsDelayedInit && SaveData.Instance.Global.CrossplayEnabled)
		{
			PlatformLayer.Networking.GetCurrentUserPrivilegesAsync(OnGetCurrentUserPrivilegesAsync, PrivilegePlatform.AllExceptSwitch, Privilege.CrossPlay);
		}
		void OnGetCurrentUserPrivilegesAsync(OperationResult operationResult, bool isPrivilegeProvided)
		{
			if (!isPrivilegeProvided)
			{
				SaveData.Instance.Global.CrossplayEnabled = false;
				SaveData.Instance.SaveGlobalData();
			}
			crossPlayToggle.IsOn = isPrivilegeProvided;
		}
	}

	private void OnDestroy()
	{
		m_ControllerArea.OnFocusedArea.RemoveListener(EnableNavigation);
		m_ControllerArea.OnUnfocusedArea.RemoveListener(DisableNavigation);
	}

	private void OnDisable()
	{
		crossPlayToggle.OnValueChanged.RemoveListener(ToggleCrossPlay);
		scenarioSpeedUpToggle.OnValueChanged.RemoveListener(ToggleScenarioSpeedUp);
		crossPlayToggle.DisableNavigation();
		scenarioSpeedUpToggle.DisableNavigation();
	}

	private void EnableNavigation()
	{
		crossPlayToggle.SetNavigation(Navigation.Mode.Vertical);
		EventSystem.current.SetSelectedGameObject(crossPlayToggle.gameObject);
	}

	private void DisableNavigation()
	{
		crossPlayToggle.DisableNavigation();
	}

	public void ToggleScenarioSpeedUp(bool enable)
	{
		SaveData.Instance.Global.SpeedUpToggle = enable;
		SaveData.Instance.SaveGlobalData();
	}

	private void ToggleCrossPlay(bool enabled)
	{
		if (enabled)
		{
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.CrossPlay, OnCheckPrivilegeValidity, PrivilegePlatform.AllExceptSwitch);
				return;
			}
			SaveData.Instance.Global.CrossplayEnabled = true;
			SaveData.Instance.SaveGlobalData();
		}
		else
		{
			SaveData.Instance.Global.CrossplayEnabled = false;
			SaveData.Instance.SaveGlobalData();
		}
		void OnCheckPrivilegeValidity(bool isCrossPlayValid)
		{
			crossPlayToggle.SetValue(isCrossPlayValid);
			SaveData.Instance.Global.CrossplayEnabled = isCrossPlayValid;
			SaveData.Instance.SaveGlobalData();
		}
	}
}
