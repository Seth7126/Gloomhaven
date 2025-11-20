using System;
using System.Linq;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIRulesetsWindow : MonoBehaviour
{
	[Header("Manage options")]
	[SerializeField]
	private Button createRulesetButton;

	[SerializeField]
	private ExtendedButton playRulesetButton;

	[SerializeField]
	private Button editRulesetButton;

	[SerializeField]
	private Button deleteRulesetButton;

	[SerializeField]
	private Button compileRulesetButton;

	[SerializeField]
	private GameObject rulesetManageOptionsPanel;

	[SerializeField]
	private GameObject PlayCampaignRuleset;

	[SerializeField]
	private GameObject PlayGuildmasterRuleset;

	[Header("Screen elements")]
	[SerializeField]
	private ExtendedButton closeButton;

	[SerializeField]
	private TextLocalizedListener subtitle;

	[SerializeField]
	private UIRulesetInventory rulesetInventory;

	[SerializeField]
	private UIRulesetView rulesetView;

	[SerializeField]
	private Image unfocusedMask;

	[SerializeField]
	private UIBackgroundTaskMask validationMask;

	[Header("Events")]
	[SerializeField]
	private UnityEvent OnClosed = new UnityEvent();

	private IRulesetService service = new RulesetService();

	private Action closeButtonCallback;

	private void Awake()
	{
		closeButton.onClick.AddListener(OnCloseButton);
		playRulesetButton.onClick.AddListener(PlaySelectedRuleset);
		editRulesetButton.onClick.AddListener(OpenSelectedRulesetEditView);
		createRulesetButton.onClick.AddListener(ChooseRulesetTypeToCreate);
		deleteRulesetButton.onClick.AddListener(AskDeleteRuleset);
		compileRulesetButton.onClick.AddListener(CompileSelectedRuleset);
		rulesetInventory.OnHoveredRuleset.AddListener(PreviewRuleset);
		rulesetInventory.OnUnhoveredRuleset.AddListener(delegate
		{
			HideRulesetView();
		});
		rulesetInventory.OnSelectedRuleset.AddListener(OpenVisualizeRulesetView);
		rulesetInventory.OnDeselectedRuleset.AddListener(delegate
		{
			HideRulesetView();
		});
		rulesetView.OnConfirmedRulesetEvent.AddListener(OnConfirmedRulesetChanges);
	}

	private void OnDestroy()
	{
		closeButton.onClick.RemoveAllListeners();
		playRulesetButton.onClick.RemoveAllListeners();
		editRulesetButton.onClick.RemoveAllListeners();
		createRulesetButton.onClick.RemoveAllListeners();
		deleteRulesetButton.onClick.RemoveAllListeners();
		compileRulesetButton.onClick.RemoveAllListeners();
		rulesetInventory.OnHoveredRuleset.RemoveAllListeners();
		rulesetInventory.OnUnhoveredRuleset.RemoveAllListeners();
		rulesetInventory.OnSelectedRuleset.RemoveAllListeners();
		rulesetInventory.OnDeselectedRuleset.RemoveAllListeners();
		rulesetView.OnConfirmedRulesetEvent.RemoveAllListeners();
	}

	public void Show()
	{
		rulesetView.Setup(SceneController.Instance.ModService);
		rulesetInventory.Setup(service.GetRulesets());
		CloseEditCreateRulesetView();
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, OnCloseButton);
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, OnCloseButton);
	}

	public void Close()
	{
		if (base.gameObject.activeSelf)
		{
			Hide();
			OnClosed.Invoke();
		}
	}

	private void FocusOptions(bool focus)
	{
		unfocusedMask.gameObject.SetActive(!focus);
	}

	private void OnCloseButton()
	{
		closeButtonCallback?.Invoke();
	}

	private void PlaySelectedRuleset()
	{
		PlayRuleset(rulesetInventory.SelectedRuleset);
	}

	private void PlayRuleset(IRuleset ruleset)
	{
		GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.SingleOrDefault((GHRuleset s) => s.Name == ruleset.Name);
		if (gHRuleset != null)
		{
			bool flag = true;
			if (ruleset.RulesetType == GHRuleset.ERulesetType.Campaign)
			{
				flag = PlayCampaignRuleset.GetComponent<ModdingCampaign>().Init(gHRuleset);
			}
			else if (ruleset.RulesetType == GHRuleset.ERulesetType.Guildmaster)
			{
				flag = PlayGuildmasterRuleset.GetComponent<ModdingGuildmaster>().Init(gHRuleset);
			}
			if (flag)
			{
				UIModdingNotifications.ShowPlayRulesetNotification(ruleset.Name);
				Hide();
			}
		}
	}

	private void PreviewRuleset(IRuleset ruleset)
	{
		rulesetView.Show(ruleset);
		rulesetManageOptionsPanel.SetActive(value: false);
	}

	private void HideRulesetView()
	{
		rulesetView.Hide();
		rulesetManageOptionsPanel.SetActive(value: false);
	}

	private void OpenVisualizeRulesetView(IRuleset ruleset)
	{
		playRulesetButton.interactable = ruleset.IsCompiled;
		rulesetView.Show(ruleset);
		rulesetManageOptionsPanel.SetActive(value: true);
	}

	private void OpenSelectedRulesetEditView()
	{
		OpenRulesetEditView(rulesetInventory.SelectedRuleset);
	}

	private void OpenRulesetEditView(IRuleset ruleset)
	{
		subtitle.SetTextKey("GUI_MODDING_EDIT_RULESET");
		OpenEditCreateRulesetView(new RulesetDataView(ruleset), "GUI_MODDING_FINISH_EDIT_RULESET", "GUI_MODDING_CANCEL_EDIT_RULESET_CONFIRMATION_TITLE");
	}

	private void ChooseRulesetTypeToCreate()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_CREATE_RULESET_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_CREATE_RULESET_CONFIRMATION"), delegate
		{
			OpenRulesetCreateView(GHRuleset.ERulesetType.Campaign);
		}, delegate
		{
			OpenRulesetCreateView(GHRuleset.ERulesetType.Guildmaster);
		}, "GUI_CAMPAIGN_RULESET", "GUI_GUILDMASTER_RULESET", null, showHeader: false);
	}

	private void OpenRulesetCreateView(GHRuleset.ERulesetType type)
	{
		rulesetInventory.ClearSelectedRuleset();
		subtitle.SetTextKey("GUI_MODDING_CREATE_NEW_RULESET");
		deleteRulesetButton.gameObject.SetActive(value: false);
		OpenEditCreateRulesetView(new RulesetDataView(type), "Create", "GUI_MODDING_CANCEL_CREATE_RULESET_CONFIRMATION_TITLE");
	}

	private void OpenEditCreateRulesetView(RulesetDataView viewData, string confirmationButtonKey, string confirmationCancelKey)
	{
		FocusOptions(focus: false);
		editRulesetButton.gameObject.SetActive(value: false);
		playRulesetButton.gameObject.SetActive(value: false);
		compileRulesetButton.gameObject.SetActive(value: false);
		rulesetManageOptionsPanel.SetActive(value: true);
		closeButton.TextLanguageKey = "GUI_CANCEL";
		closeButtonCallback = delegate
		{
			if (viewData.IsModified)
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation(confirmationCancelKey), LocalizationManager.GetTranslation("GUI_MODDING_RULESET_LOSE_CHANGES_CONFIRMATION"), CloseEditCreateRulesetView, null, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
			}
			else
			{
				CloseEditCreateRulesetView();
			}
		};
		rulesetView.Edit(viewData, service.GetCompatibleMods(viewData.RulesetType), confirmationButtonKey);
	}

	private void CloseEditCreateRulesetView()
	{
		FocusOptions(focus: true);
		closeButton.TextLanguageKey = "GUI_BACK";
		closeButtonCallback = Close;
		subtitle.SetTextKey("GUI_MODDING_LIST_RULESETS");
		editRulesetButton.gameObject.SetActive(value: true);
		deleteRulesetButton.gameObject.SetActive(value: true);
		playRulesetButton.gameObject.SetActive(value: true);
		compileRulesetButton.gameObject.SetActive(value: true);
		if (rulesetInventory.SelectedRuleset != null)
		{
			OpenVisualizeRulesetView(rulesetInventory.SelectedRuleset);
		}
		else
		{
			HideRulesetView();
		}
	}

	private void OnConfirmedRulesetChanges(RulesetDataView rulesetData)
	{
		if (rulesetInventory.SelectedRuleset == null)
		{
			CreateRuleset(rulesetData);
			return;
		}
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_FINISH_EDIT_RULESET"), LocalizationManager.GetTranslation("GUI_MODDING_FINISH_EDIT_RULESET_CONFIRMATION"), delegate
		{
			UpdateRuleset(rulesetInventory.SelectedRuleset, rulesetData);
		});
	}

	private void UpdateRuleset(IRuleset oldRuleset, RulesetDataView rulesetData)
	{
		validationMask.Show("GUI_MODDING_VALIDATING");
		service.UpdateRuleset(oldRuleset, rulesetData, delegate(IRuleset newRuleset)
		{
			validationMask.Hide();
			UIModdingNotifications.ShowValidatedNotification(newRuleset.Name);
			rulesetInventory.Replace(oldRuleset, newRuleset);
			CloseEditCreateRulesetView();
		}, OnValidationFailed);
	}

	private void CreateRuleset(RulesetDataView rulesetData)
	{
		validationMask.Show("GUI_MODDING_VALIDATING");
		service.CreateRuleset(rulesetData, delegate(IRuleset ruleset)
		{
			validationMask.Hide();
			UIModdingNotifications.ShowCreateRulesetNotification(ruleset.Name);
			rulesetInventory.Add(ruleset);
			CloseEditCreateRulesetView();
		}, OnValidationFailed);
	}

	private void OnValidationFailed()
	{
		validationMask.Hide();
		rulesetView.RefreshValidSelectedMods();
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_RULESET_FAIL_CONFIRMATION"), "GUI_CLOSE");
	}

	protected void CompileSelectedRuleset()
	{
		CompileRuleset(rulesetInventory.SelectedRuleset);
	}

	protected void CompileRuleset(IRuleset ruleset)
	{
		validationMask.Show("GUI_MODDING_COMPILING");
		service.CompileRuleset(ruleset, delegate(bool isValid)
		{
			validationMask.Hide();
			if (isValid)
			{
				UIModdingNotifications.ShowValidatedNotification(ruleset.Name);
				playRulesetButton.interactable = true;
			}
			else
			{
				playRulesetButton.interactable = false;
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_RULESET_FAIL_CONFIRMATION"), "GUI_CLOSE");
			}
		});
	}

	private void AskDeleteRuleset()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_DELETE_RULESET"), null, delegate
		{
			DeleteRuleset(rulesetInventory.SelectedRuleset);
		});
	}

	private void DeleteRuleset(IRuleset ruleset)
	{
		service.DeleteRuleset(ruleset);
		UIModdingNotifications.ShowDeleteRulesetNotification(ruleset.Name);
		rulesetInventory.Remove(ruleset);
		CloseEditCreateRulesetView();
	}
}
