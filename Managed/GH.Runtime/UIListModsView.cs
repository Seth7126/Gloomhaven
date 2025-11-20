using System;
using System.Collections.Generic;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIListModsView : MonoBehaviour
{
	[Serializable]
	public class ListModEvent : UnityEvent<IMod>
	{
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ScrollRect scrolRect;

	[SerializeField]
	private UIListModSlot slotPrefab;

	[SerializeField]
	private UIBackgroundTaskMask validationMask;

	[SerializeField]
	private GameObject levelEditor;

	[SerializeField]
	private GameObject titleMods;

	private List<UIListModSlot> slotPool = new List<UIListModSlot>();

	private Dictionary<IMod, UIListModSlot> assignedSlots = new Dictionary<IMod, UIListModSlot>();

	public ListModEvent OnUpdateModEvent = new ListModEvent();

	public ListModEvent OnUploadModEvent = new ListModEvent();

	private IModdingService service;

	private IRulesetService rulesetService = new RulesetService();

	public void Setup(IModdingService service)
	{
		this.service = service;
		ReloadMods();
	}

	public void ReloadMods()
	{
		assignedSlots.Clear();
		List<IMod> mods = service.GetMods();
		HelperTools.NormalizePool(ref slotPool, slotPrefab.gameObject, scrolRect.content, mods.Count);
		for (int i = 0; i < mods.Count; i++)
		{
			AddNewMod(mods[i]);
		}
	}

	private void OnSelectedUpdateMod(IMod modData)
	{
		service.EditMod(modData);
		OnUpdatedMod(modData);
	}

	private void OnUpdatedMod(IMod modData)
	{
		assignedSlots[modData].RefreshVersion();
		OnUpdateModEvent.Invoke(modData);
	}

	private void OnSelectedUploadMod(IMod modData)
	{
		service.UploadMod(modData);
		OnUploadedMod(modData);
	}

	private void OnUploadedMod(IMod modData)
	{
		assignedSlots[modData].RefreshUpload();
		OnUploadModEvent.Invoke(modData);
	}

	private void ValidateMod(IMod modData)
	{
		validationMask.Show("GUI_MODDING_VALIDATING");
		service.ValidateMod(modData, delegate
		{
			validationMask.Hide();
			assignedSlots[modData].RefreshValidate();
			if (modData.IsValid)
			{
				UIModdingNotifications.ShowValidatedNotification(modData.Name);
			}
			else
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_MOD_FAIL_CONFIRMATION"), delegate
				{
					service.ViewModErrors(modData);
				}, null, "GUI_MODDING_VALIDATION_VIEW_MOD_ERRORS", "GUI_CLOSE", showHeader: false);
			}
		});
	}

	private void OpenLevelEditor(IMod modData)
	{
		service.CreateDummyRuleset(modData);
		GHRuleset ruleset = SceneController.Instance.Modding.LevelEditorRuleset;
		validationMask.Show("GUI_MODDING_COMPILING");
		rulesetService.CompileRuleset(ruleset, delegate(bool isValid)
		{
			if (isValid)
			{
				service.LoadAndDeleteDummyRuleset(ruleset);
				UIModdingNotifications.ShowValidatedNotification(ruleset.Name);
				validationMask.Hide();
				titleMods.SetActive(value: false);
				levelEditor.SetActive(value: true);
			}
			else
			{
				validationMask.Hide();
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_RULESET_FAIL_CONFIRMATION"), "GUI_CLOSE");
			}
		});
	}

	public void Show(IMod newMod = null)
	{
		if (newMod != null)
		{
			AddNewMod(newMod);
		}
		scrolRect.verticalNormalizedPosition = 1f;
		window.Show();
	}

	private void AddNewMod(IMod newMod)
	{
		if (!assignedSlots.ContainsKey(newMod))
		{
			if (slotPool.Count <= assignedSlots.Count)
			{
				HelperTools.NormalizePool(ref slotPool, slotPrefab.gameObject, scrolRect.content, assignedSlots.Count + 1);
			}
			UIListModSlot uIListModSlot = slotPool[assignedSlots.Count];
			assignedSlots[newMod] = uIListModSlot;
			uIListModSlot.transform.SetAsFirstSibling();
			uIListModSlot.SetMod(newMod, OnSelectedUpdateMod, OnSelectedUploadMod, ValidateMod, OpenLevelEditor);
			uIListModSlot.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		window.Hide();
	}
}
