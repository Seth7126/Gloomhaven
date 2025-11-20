using System;
using Script.GUI.SMNavigation;
using UnityEngine;

public class SandboxUI : MonoBehaviour, IShowActivity
{
	public GameObject ResumeButton;

	public GameObject ResumeButtonDummy;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	public bool IsActive => base.gameObject.activeSelf;

	private void OnDestroy()
	{
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
	}

	private void OnEnable()
	{
		bool flag = SaveData.Instance.Global.AllSingleScenarios.Exists((PartyAdventureData s) => s.PartyName == "SandboxScenario" && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID) || s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID) && SaveData.Instance.Global.ResumeSingleScenario != null);
		ResumeButton.SetActive(flag);
		ResumeButtonDummy.SetActive(!flag);
		controllerArea.Enable();
		OnShow?.Invoke();
		OnActivityChanged?.Invoke(obj: true);
	}

	public void ResumeSandbox()
	{
		SaveData.Instance.Global.ResumeSingleScenarioName = "SandboxScenario";
		SaveData.Instance.Global.CurrentHostAccountID = PlatformLayer.UserData.PlatformAccountID;
		SaveData.Instance.Global.CurrentHostNetworkAccountID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
		SceneController.Instance.ResumeSingleScenario();
	}

	private void OnDisable()
	{
		controllerArea.Destroy();
		OnHide?.Invoke();
		OnActivityChanged?.Invoke(obj: false);
	}
}
