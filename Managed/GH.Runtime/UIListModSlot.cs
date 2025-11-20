using System;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIListModSlot : UIModSlot
{
	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private TextLocalizedListener sourceText;

	[SerializeField]
	private TextLocalizedListener typeText;

	[SerializeField]
	private ExtendedButton updateButton;

	[SerializeField]
	private Button uploadButton;

	[SerializeField]
	private Button validateButton;

	[SerializeField]
	private Button levelEditorButton;

	private Action<IMod> onUpdateMod;

	private Action<IMod> onUploadMod;

	private Action<IMod> onValidateMod;

	private Action<IMod> onOpenLevelEditor;

	private void Awake()
	{
		uploadButton.onClick.AddListener(UploadMod);
		updateButton.onClick.AddListener(UpdateMod);
		validateButton.onClick.AddListener(ValidateMod);
		levelEditorButton.onClick.AddListener(OpenLevelEditor);
	}

	private void OnDestroy()
	{
		uploadButton.onClick.RemoveAllListeners();
		updateButton.onClick.RemoveAllListeners();
		validateButton.onClick.RemoveAllListeners();
		levelEditorButton.onClick.RemoveAllListeners();
	}

	public override void SetMod(IMod modData)
	{
		SetMod(modData, null, null, null, null);
	}

	public void SetMod(IMod modData, Action<IMod> onUpdateMod, Action<IMod> onUploadMod, Action<IMod> onValidateMod, Action<IMod> onOpenLevelEditor)
	{
		base.SetMod(modData);
		this.onUpdateMod = onUpdateMod;
		this.onUploadMod = onUploadMod;
		this.onValidateMod = onValidateMod;
		this.onOpenLevelEditor = onOpenLevelEditor;
		sourceText.SetTextKey(modData.IsCustomMod ? "GUI_MOD_SOURCE_CUSTOM" : "GUI_MOD_SOURCE_WORKSHOP");
		switch (modData.ModType)
		{
		case GHModMetaData.EModType.Campaign:
			typeText.SetTextKey("GUI_MOD_CAMPAIGN_TYPE");
			break;
		case GHModMetaData.EModType.Guildmaster:
			typeText.SetTextKey("GUI_MOD_GUILDMASTER_TYPE");
			break;
		case GHModMetaData.EModType.CustomLevels:
			typeText.SetTextKey("GUI_MOD_CUSTOMLEVELS_TYPE");
			break;
		case GHModMetaData.EModType.Language:
			typeText.SetTextKey("GUI_MOD_LANGUAGE_TYPE");
			break;
		case GHModMetaData.EModType.Global:
			typeText.SetTextKey("GUI_MOD_GLOBAL_TYPE");
			break;
		default:
			typeText.SetTextKey("GUI_MOD_INVALID_TYPE");
			break;
		}
		updateButton.TextLanguageKey = (modData.IsCustomMod ? "GUI_EDIT" : "GUI_MOD_UPDATE");
		RefreshVersion();
		RefreshUploadOption();
	}

	public void RefreshUpload()
	{
		RefreshUploadOption();
		RefreshRanking();
	}

	public void RefreshUploadOption()
	{
		uploadButton.gameObject.SetActive(modData.IsCustomMod && modData.IsValid);
	}

	public void RefreshUpdateOption()
	{
		updateButton.gameObject.SetActive(modData.IsCustomMod);
	}

	public void RefreshValidateOption()
	{
		validateButton.gameObject.SetActive(!modData.IsValid);
	}

	public void RefreshLevelEditorOption()
	{
		levelEditorButton.gameObject.SetActive((modData.IsValid && modData.ModType == GHModMetaData.EModType.Campaign) || modData.ModType == GHModMetaData.EModType.Guildmaster);
	}

	public void RefreshValidate()
	{
		RefreshValidateOption();
		RefreshLevelEditorOption();
		RefreshUploadOption();
	}

	public void RefreshVersion()
	{
		versionText.text = string.Format(LocalizationManager.GetTranslation("GUI_MOD_VERSION"), modData.Version);
		RefreshUpdateOption();
		RefreshValidate();
	}

	public void RefreshOptions()
	{
		RefreshUploadOption();
		RefreshUpdateOption();
		RefreshValidateOption();
		RefreshLevelEditorOption();
	}

	private void UpdateMod()
	{
		onUpdateMod?.Invoke(modData);
	}

	private void UploadMod()
	{
		onUploadMod?.Invoke(modData);
	}

	private void ValidateMod()
	{
		onValidateMod?.Invoke(modData);
	}

	private void OpenLevelEditor()
	{
		onOpenLevelEditor?.Invoke(modData);
	}
}
