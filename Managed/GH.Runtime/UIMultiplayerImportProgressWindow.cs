using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMultiplayerImportProgressWindow : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI confirmImportText;

	[SerializeField]
	private UIMultiplayerUser playerInfo;

	[SerializeField]
	private List<UIMultiplayerImportCharacter> importSlotPool;

	[SerializeField]
	private ExtendedScrollRect slotScroll;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private Dictionary<ICharacterImportData, UIMultiplayerImportCharacter> assignedSlots;

	private CallbackPromise<List<ICharacterImportData>> promise;

	private void OnEnable()
	{
		confirmButton.onClick.AddListener(ConfirmImport);
		controllerArea.OnFocusedArea.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerAreaUnfocused);
	}

	private void OnDisable()
	{
		confirmButton.onClick.RemoveListener(ConfirmImport);
		controllerArea.OnFocusedArea.RemoveListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.RemoveListener(OnControllerAreaUnfocused);
	}

	public ICallbackPromise<List<ICharacterImportData>> Show<T>(NetworkPlayer player, List<T> importData) where T : ICharacterImportData
	{
		promise = new CallbackPromise<List<ICharacterImportData>>();
		slotScroll.ScrollToTop();
		playerInfo.Show(player);
		confirmButton.interactable = FFSNetwork.IsHost;
		base.gameObject.SetActive(value: true);
		Decorate(importData);
		controllerArea.Enable();
		return promise;
	}

	private void Decorate<T>(List<T> questCompletionList) where T : ICharacterImportData
	{
		assignedSlots = new Dictionary<ICharacterImportData, UIMultiplayerImportCharacter>();
		HelperTools.NormalizePool(ref importSlotPool, importSlotPool[0].gameObject, slotScroll.content, questCompletionList.Count);
		for (int i = 0; i < questCompletionList.Count; i++)
		{
			Decorate(questCompletionList[i], importSlotPool[i]);
			if (controllerArea.IsFocused)
			{
				importSlotPool[i].EnableNavigation(i == 0);
			}
		}
		RefreshToggledImport();
	}

	private void Decorate(ICharacterImportData data, UIMultiplayerImportCharacter slot)
	{
		slot.OnImportToggled.RemoveListener(OnToggledImportChanged);
		slot.OnImportToggled.AddListener(OnToggledImportChanged);
		slot.OnSelected.RemoveListener(NavigateTo);
		slot.OnSelected.AddListener(NavigateTo);
		slot.SetCharacter(data, FFSNetwork.IsHost);
		assignedSlots[data] = slot;
	}

	private void NavigateTo(UIMultiplayerImportCharacter slot)
	{
		if (controllerArea.IsFocused)
		{
			slotScroll.ScrollToFit(slot.transform as RectTransform);
		}
	}

	private void OnToggledImportChanged(ICharacterImportData data, bool toggled)
	{
		RefreshToggledImport();
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			IProtocolToken supplementaryDataToken = new IdListToken((from it in assignedSlots
				where it.Value.EnabledImport
				select it.Key.Id).ToList());
			Synchronizer.SendGameAction(GameActionType.UpdatedImportSettings, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
	}

	private void RefreshToggledImport()
	{
		confirmImportText.text = string.Format("{0} {1}/{2}", LocalizationManager.GetTranslation("GUI_CONFIRM"), assignedSlots.Values.Count((UIMultiplayerImportCharacter it) => it.EnabledImport), assignedSlots.Count((KeyValuePair<ICharacterImportData, UIMultiplayerImportCharacter> it) => it.Key.CanImport()));
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			controllerArea.Destroy();
			promise = null;
		}
	}

	public void ConfirmImport()
	{
		List<ICharacterImportData> list = (from it in assignedSlots
			where it.Value.EnabledImport
			select it.Key).ToList();
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			IProtocolToken supplementaryDataToken = new IdListToken(list.Select((ICharacterImportData it) => it.Id).ToList());
			Synchronizer.SendGameAction(GameActionType.ImportSettings, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		promise.Resolve(list);
	}

	public void EnableImportData(string[] dataIds)
	{
		foreach (KeyValuePair<ICharacterImportData, UIMultiplayerImportCharacter> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.SetImport(dataIds.Contains(assignedSlot.Key.Id));
		}
		RefreshToggledImport();
	}

	private void OnControllerAreaFocused()
	{
		bool flag = false;
		foreach (UIMultiplayerImportCharacter value in assignedSlots.Values)
		{
			if (value.IsInteractable)
			{
				value.EnableNavigation(!flag);
				flag = true;
			}
		}
		if (!flag)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private void OnControllerAreaUnfocused()
	{
		foreach (UIMultiplayerImportCharacter value in assignedSlots.Values)
		{
			value.DisableNavigation();
		}
	}
}
