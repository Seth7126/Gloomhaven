using System.Collections.Generic;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public class TextSelectorLanguageOption : LanguageOption
{
	[SerializeField]
	private Selectable _selectable;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private InputListener _inputListener;

	[SerializeField]
	private Hotkey _confirmHotkey;

	[SerializeField]
	private LongConfirmHandler _confirmHandler;

	[SerializeField]
	private CanvasGroup _confirmView;

	[SerializeField]
	private StringListSelector _selector;

	private SimpleKeyActionHandlerBlocker _confirmBlocker;

	public override Selectable Selectable => _selectable;

	[UsedImplicitly]
	private void OnDestroy()
	{
		_selector.OnSelectedChanged -= OnLanguageSelected;
		_confirmHotkey.Deinitialize();
	}

	private void Awake()
	{
		_confirmHandler.gameObject.SetActive(value: true);
	}

	public override void EnableNavigation()
	{
		base.EnableNavigation();
		_confirmBlocker.SetBlock(value: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CONFIRM_ACTION_BUTTON, StartConfirm, delegate
		{
			DisplayConfirm(active: true);
		}, delegate
		{
			DisplayConfirm(active: false);
		}).AddBlocker(_confirmBlocker));
		_inputListener.Register();
		_confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
	}

	public override void DisableNavigation()
	{
		base.DisableNavigation();
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CONFIRM_ACTION_BUTTON, StartConfirm);
		_inputListener.UnRegister();
		_confirmHotkey.Deinitialize();
		DisplayConfirm(active: false);
	}

	public override void SetInteractable(bool interactable, Color textColor)
	{
		_selectable.interactable = interactable;
		_text.color = textColor;
	}

	public override void Initialize(List<string> languages)
	{
		_confirmBlocker = new SimpleKeyActionHandlerBlocker();
		_selector.OnSelectedChanged += OnLanguageSelected;
		_selector.SetElements(languages);
	}

	public override void SelectWithoutNotify(string language)
	{
		_selector.SelectWithoutNotify(language);
	}

	private void OnLanguageSelected(string language)
	{
		UpdateConfirmView(language);
	}

	private void StartConfirm()
	{
		_confirmHandler.Pressed(ApplyLanguage);
	}

	private void ApplyLanguage()
	{
		string selectedElement = _selector.SelectedElement;
		_settings.ApplyLanguage(selectedElement);
		UpdateConfirmView(selectedElement);
		if (SaveData.Instance.Global.GameMode != EGameMode.Campaign && SaveData.Instance.Global.GameMode != EGameMode.Guildmaster)
		{
			return;
		}
		if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				SceneController.Instance.CampaignResume(SaveData.Instance.Global.CurrentAdventureData, isJoiningMPClient: false);
			}
			else
			{
				SceneController.Instance.GuildmasterResume(SaveData.Instance.Global.CurrentAdventureData, isJoiningMPClient: false, regenerateMonsterCards: true);
			}
		}
		else
		{
			SceneController.Instance.LoadMainMenu();
		}
	}

	private void UpdateConfirmView(string language)
	{
		bool flag = CanChangeLanguage(language);
		_confirmBlocker.SetBlock(!flag);
	}

	private void DisplayConfirm(bool active)
	{
		_confirmView.alpha = (active ? 1 : 0);
	}
}
