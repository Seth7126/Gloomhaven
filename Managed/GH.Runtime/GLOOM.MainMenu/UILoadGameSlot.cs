using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using JetBrains.Annotations;
using MapRuleLibrary.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UILoadGameSlot : MonoBehaviour
{
	public class GameSlotEvent : UnityEvent<IGameSaveData>
	{
	}

	public class GameDLCEvent : UnityEvent<IGameSaveData, DLCRegistry.EDLCKey>
	{
	}

	[Serializable]
	private class TextStateColor
	{
		public TextMeshProUGUI text;

		public Color defaultColor;

		[SerializeField]
		private Color invalidColor;

		public void SetValid(bool valid)
		{
			text.color = (valid ? defaultColor : invalidColor);
		}
	}

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TextMeshProUGUI partyName;

	[SerializeField]
	private UIMultiplayerSaveOwner saveOwner;

	[SerializeField]
	private TextMeshProUGUI date;

	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm";

	[SerializeField]
	private TextMeshProUGUI questName;

	[SerializeField]
	private TextMeshProUGUI partyGold;

	[SerializeField]
	private GameObject wealthHeader;

	[SerializeField]
	private TextMeshProUGUI wealth;

	[SerializeField]
	private UILoadGameCharacter[] characterSlots;

	[SerializeField]
	private ExtendedDropdown dlcDropdown;

	[SerializeField]
	private List<UILoadGameOwnedDLC> ownedDLCSlot;

	[SerializeField]
	private TextMeshProUGUI _noDLCActivated;

	[SerializeField]
	private ExtendedButton loadButton;

	[SerializeField]
	private Button deleteButton;

	[SerializeField]
	private List<Graphic> validMasks;

	[SerializeField]
	private List<TextStateColor> validTexts;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private Selectable dropdownSelectable;

	[SerializeField]
	private GameObject _dark;

	public Action<IGameSaveData, DLCRegistry.EDLCKey> OnDLCEnabled;

	public GameSlotEvent OnLoadData = new GameSlotEvent();

	public GameSlotEvent OnDeleteData = new GameSlotEvent();

	public GameDLCEvent OnEnableDLC = new GameDLCEvent();

	private SelectorWrapper<DLCRegistry.EDLCKey> dlcSelector;

	private UINavigationSelectable _uiNavigationSelectable;

	private UICreateGameDLCStep _dlcWindow;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private bool _isSaveInvalid;

	private bool _hasAllDLCs;

	private IGameSaveData data;

	private const string c_NewQuestString = "GUI_LOAD_ADVENTURE_START_QUEST";

	private const string c_DLCQuest = "GUI_LOAD_GAME_DLC_QUEST";

	private UnityAction onClickLoad;

	public UnityEvent OnHovered => button.onMouseEnter;

	private void Awake()
	{
		_uiNavigationSelectable = GetComponent<UINavigationSelectable>();
		loadButton.onClick.AddListener(delegate
		{
			onClickLoad?.Invoke();
		});
		deleteButton.onClick.AddListener(Delete);
		dlcSelector = new DLCSelector(dlcDropdown);
		dlcSelector.OnValuedChanged.AddListener(EnableDLC);
		_dlcWindow = Singleton<UILoadGameWindow>.Instance.UICreateGameDlcStep;
		SubscribeOnGamepadEvents();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		loadButton.onClick.RemoveAllListeners();
		deleteButton.onClick.RemoveAllListeners();
		OnLoadData.RemoveAllListeners();
		OnDeleteData.RemoveAllListeners();
		OnEnableDLC.RemoveAllListeners();
		button.onMouseEnter.RemoveAllListeners();
	}

	private void SubscribeOnGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent += UpdateHotkeys;
			_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Load).AddBlocker(new NavigationSelectableSelectKeyActionHandlerBlocker(_uiNavigationSelectable)).AddBlocker(_simpleKeyActionHandlerBlocker));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DELETE_CHARACTER, Delete).AddBlocker(new NavigationSelectableSelectKeyActionHandlerBlocker(_uiNavigationSelectable)));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_MENU_ALT1, OnDLCDropdownClicked).AddBlocker(new NavigationSelectableSelectKeyActionHandlerBlocker(_uiNavigationSelectable)));
		}
	}

	private void EnableDLC(DLCRegistry.EDLCKey dlc)
	{
		if (data != null)
		{
			dlcSelector.ResetSelection();
			OnEnableDLC.Invoke(data, dlc);
		}
	}

	private void Load()
	{
		if (data != null)
		{
			OnLoadData.Invoke(data);
		}
	}

	private void Delete()
	{
		if (data != null)
		{
			OnDeleteData.Invoke(data);
		}
	}

	private void OnDLCDropdownClicked()
	{
		if (!_hasAllDLCs)
		{
			_dlcWindow.ShowDLCForLoadGame(data, OnDLCEnabled);
		}
	}

	public void SetData(IGameSaveData data)
	{
		this.data = data;
		partyName.SetTextCensored(data.DisplayGameName);
		date.text = data.LastSavedTimeStamp.ToString(formatDate);
		saveOwner.SetPlayer(data.Owner);
		if (!data.Wealth.HasValue)
		{
			wealthHeader.SetActive(value: false);
		}
		else
		{
			wealth.text = string.Format("{0}: {1}", LocalizationManager.GetTranslation("GUI_PartyProsperity"), data.Wealth);
			wealthHeader.SetActive(value: true);
		}
		if (InputManager.GamePadInUse)
		{
			partyGold.alignment = (data.Reputation.HasValue ? TextAlignmentOptions.Left : TextAlignmentOptions.Right);
		}
		if (data.Reputation.HasValue)
		{
			partyGold.text = string.Format("{0}: {1}", LocalizationManager.GetTranslation("GUI_PartyReputation"), data.Reputation);
		}
		else if (data.GoldMode == EGoldMode.PartyGold)
		{
			partyGold.text = string.Format("<color=#{0}>{1}: <size=26><sprite name=\"Gold_Icon_White\" color=#{2}></color></size> <color=#{3}>{4}</color>", UIInfoTools.Instance.goldColor.ToHex(), LocalizationManager.GetTranslation("GUI_OPT_SHARED_GOLD"), UIInfoTools.Instance.goldColor.ToHex(), UIInfoTools.Instance.goldTextColor.ToHex(), data.PartyGold);
		}
		else if (data.GoldMode == EGoldMode.CharacterGold)
		{
			partyGold.text = "<color=#" + UIInfoTools.Instance.goldColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_OPT_SPLIT_GOLD") + ": <size=26><sprite name=\"Gold_Icon_White\" color=#" + UIInfoTools.Instance.goldColor.ToHex() + "></color></size>";
		}
		else
		{
			partyGold.text = string.Empty;
		}
		List<DLCRegistry.EDLCKey> list = new List<DLCRegistry.EDLCKey>
		{
			DLCRegistry.EDLCKey.DLC1,
			DLCRegistry.EDLCKey.DLC2
		};
		DecorateDLCOptions(list);
		List<DLCRegistry.EDLCKey> invalidDLCs;
		bool flag = data.HasInvalidDLCs(out invalidDLCs);
		for (int i = 0; i < validMasks.Count; i++)
		{
			validMasks[i].material = (flag ? UIInfoTools.Instance.greyedOutMaterial : null);
		}
		for (int j = 0; j < validTexts.Count; j++)
		{
			validTexts[j].SetValid(!flag);
		}
		loadButton.TextLanguageKey = (flag ? "Consoles/LEARN_MORE" : "GUI_LOAD");
		if (flag)
		{
			tooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_LOAD_SAVE_MISSING_DLC"), LocalizationManager.GetTranslation(invalidDLCs[0].ToString())));
			tooltip.enabled = true;
			onClickLoad = delegate
			{
				PlatformLayer.DLC.OpenPlatformStoreDLCOverlay(invalidDLCs[0]);
			};
		}
		else
		{
			tooltip.enabled = false;
			onClickLoad = Load;
		}
		if (_dark != null)
		{
			_dark.SetActive(flag);
		}
		_isSaveInvalid = flag;
		SetCharacters(data.SelectedCharacters);
		string Translation;
		if (string.IsNullOrEmpty(data.CurrentQuest))
		{
			questName.text = LocalizationManager.GetTranslation("GUI_LOAD_ADVENTURE_START_QUEST");
		}
		else if (LocalizationManager.TryGetTranslation(data.CurrentQuest, out Translation))
		{
			questName.text = Translation;
		}
		else
		{
			questName.text = LocalizationManager.GetTranslation("GUI_LOAD_GAME_DLC_QUEST");
		}
		if (_noDLCActivated != null)
		{
			_noDLCActivated.gameObject.SetActive(list.FindAll((DLCRegistry.EDLCKey it) => data.IsDLCActive(it) && it != DLCRegistry.EDLCKey.None).Count == 0);
		}
		if (InputManager.GamePadInUse)
		{
			loadButton.gameObject.SetActive(value: false);
		}
	}

	private void DecorateDLCOptions(List<DLCRegistry.EDLCKey> dlcs)
	{
		List<DLCRegistry.EDLCKey> ownedDLCs = dlcs.FindAll((DLCRegistry.EDLCKey it) => data.IsDLCActive(it) && it != DLCRegistry.EDLCKey.None);
		_hasAllDLCs = ownedDLCs.Count == dlcs.Count;
		if (dlcs.Count == 0 || _hasAllDLCs)
		{
			dlcDropdown.gameObject.SetActive(value: false);
			HelperTools.NormalizePool(ref ownedDLCSlot, ownedDLCSlot[0].gameObject, ownedDLCSlot[0].transform.parent, 0);
		}
		else
		{
			List<DLCRegistry.EDLCKey> list = dlcs.Where((DLCRegistry.EDLCKey it) => PlatformLayer.DLC.UserInstalledDLC(it)).ToList();
			IEnumerable<DLCRegistry.EDLCKey> source = list.Where((DLCRegistry.EDLCKey x) => ownedDLCs.Contains(x));
			if (list.Count == source.Count())
			{
				dlcDropdown.gameObject.SetActive(value: false);
			}
			else
			{
				dlcSelector.SetOptions(list.ConvertAll((Converter<DLCRegistry.EDLCKey, SelectorOptData<DLCRegistry.EDLCKey>>)((DLCRegistry.EDLCKey it) => new DLCSelectorOpt(it, ownedDLCs.Contains(it)))));
				dlcDropdown.gameObject.SetActive(value: true);
			}
		}
		HelperTools.NormalizePool(ref ownedDLCSlot, ownedDLCSlot[0].gameObject, ownedDLCSlot[0].transform.parent, ownedDLCs.Count);
		for (int num = 0; num < ownedDLCs.Count; num++)
		{
			ownedDLCSlot[num].SetDLC(ownedDLCs[num]);
		}
		if (InputManager.GamePadInUse)
		{
			dlcDropdown.gameObject.SetActive(value: false);
		}
	}

	private void UpdateHotkeys(IUiNavigationSelectable selectable)
	{
		_simpleKeyActionHandlerBlocker.SetBlock(_isSaveInvalid);
		Singleton<UILoadGameWindow>.Instance.SelectHotkey.gameObject.SetActive(!_isSaveInvalid);
		Singleton<UILoadGameWindow>.Instance.ActivateDLCHotkey.gameObject.SetActive(!_hasAllDLCs);
	}

	private void SetCharacters(List<IGameSaveCharacterData> characters, List<DLCRegistry.EDLCKey> invalidDLCs = null)
	{
		for (int i = 0; i < characterSlots.Length; i++)
		{
			if (i < characters.Count)
			{
				DLCRegistry.EDLCKey eDLCKey = characters[i].BelongsToDLC(invalidDLCs);
				if (eDLCKey != DLCRegistry.EDLCKey.None)
				{
					characterSlots[i].ShowMissing(eDLCKey);
				}
				else
				{
					characterSlots[i].ShowCharacter(characters[i].CharacterModel, characters[i].Level, characters[i].CharacterName, characters[i].Gold, invalidDLCs.IsNullOrEmpty());
				}
			}
			else
			{
				characterSlots[i].ShowEmpty();
			}
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent -= UpdateHotkeys;
			DisableNavigation();
		}
	}

	public void ClearTempAvatar()
	{
		saveOwner.Dispose();
	}

	public void EnableNavigation()
	{
		button.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}
}
