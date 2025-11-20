using System;
using System.Linq;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using Assets.Script.GUI.NewAdventureMode.Guildmaster;
using FFSNet;
using GLOO.Introduction;
using GLOOM;
using I2.Loc;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Locations;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITempleWindow : MonoBehaviour
{
	[SerializeField]
	private UITempleShopInventory shop;

	[SerializeField]
	private ImageProgressBar devotionProgress;

	[SerializeField]
	private TextLocalizedListener devotionLevel;

	[SerializeField]
	private GoldCounter totalDonatedGold;

	[SerializeField]
	private string audioItemBless = "PlaySound_UIReceivedItem";

	[SerializeField]
	private HelpBoxLine helpBox;

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private LocalHotkeys _localHotkeys;

	[SerializeField]
	private BackgroundToggleElement _darkenElement;

	[SerializeField]
	private GuildmasterBannerConfig _guildmasterBannerConfig;

	[SerializeField]
	private UITextTooltipTarget _textTooltipTarget;

	[Header("Partial Hide")]
	[SerializeField]
	private CanvasGroup _inventoryCanvasGroup;

	private TempleShopService service;

	private UIWindow window;

	private ICharacter character;

	private bool _isConfirmationBoxOpened;

	public ControllerInputArea TempleInputArea => controllerArea;

	public UITextTooltipTarget TextTooltipTarget => _textTooltipTarget;

	[CanBeNull]
	public IHotkeyContainer HotkeyContainer => _localHotkeys;

	public IGuildmasterBannerConfig GuildmasterBannerConfig => _guildmasterBannerConfig;

	public UITempleShopInventory Shop => shop;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		Shop.OnBlessingSelected.AddListener(OnSelectedSlot);
		service = new TempleShopService();
		helpBox.Show("GUI_TEMPLE_DESCRIPTION");
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		helpBox.Show("GUI_TEMPLE_DESCRIPTION");
	}

	public void EnterTemple()
	{
		controllerArea.enabled = true;
		character = null;
		Shop.Display(service.GetAvailableBlessings(), service);
		totalDonatedGold.SetCount(service.CalculateTotalGoldDonated());
		devotionProgress.SetAmount(service.DevotionCurrentProgress, service.NextDevotionLevelAmount);
		devotionLevel.SetArguments(service.DevotionLevel.ToString());
		NewPartyDisplayUI.PartyDisplay.EnableSelectionMode(OnSelectedCharacter, disableButtons: false);
		window.Show();
		controllerArea.Focus();
		ShowIntroduction();
	}

	private void ShowIntroduction()
	{
		if (!service.IsTempleIntroductionShown)
		{
			introduction.Show();
			service.SetTempleIntroductionShown();
		}
	}

	public void SetEditMode(bool isEnabled)
	{
		Shop.SetInteractable(isEnabled);
	}

	public void SetDarkenActive(bool value)
	{
		_darkenElement.Toggle(value);
	}

	public void Exit()
	{
		Shop.Hide();
		introduction.Hide();
		NewPartyDisplayUI.PartyDisplay.DisableSelectionMode();
		controllerArea.enabled = false;
		window.Hide();
	}

	private void OnSelectedCharacter(NewPartyCharacterUI characterSlot)
	{
		if (character != characterSlot.Service)
		{
			character = characterSlot.Service;
			Singleton<UIEnhancementConfirmationBox>.Instance.Hide();
			Shop.Refresh(character);
		}
	}

	private void OnSelectedSlot(TempleYML.TempleBlessingDefinition blessing)
	{
		if (!_isConfirmationBoxOpened && service.CanBuy(character.CharacterID, blessing))
		{
			StringBuilder stringBuilder = new StringBuilder(string.Format(GLOOM.LocalizationManager.GetTranslation("GUI_TEMPLE_CONFIRMATION"), GLOOM.LocalizationManager.GetTranslation(character.Class.LocKey), UIInfoTools.Instance.GetCharacterColor(character.Class.Model, character.Class.CustomCharacterConfig).ToHex(), $"AA_{character.Class.Model}"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<color=#{0}>{1} <sprite name=\"Gold_Icon_White\" color=#{0}> {2}</color>", UIInfoTools.Instance.goldColor.ToHex(), GLOOM.LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST"), blessing.GoldCost);
			UIInfoTools.EffectInfo effectInfoCondition;
			string translation;
			if (blessing.TempleBlessingCondition.Type == RewardCondition.EConditionType.Negative)
			{
				effectInfoCondition = UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.NegativeCondition);
				translation = GLOOM.LocalizationManager.GetTranslation(blessing.TempleBlessingCondition.NegativeCondition.ToString());
			}
			else
			{
				effectInfoCondition = UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.PositiveCondition);
				translation = GLOOM.LocalizationManager.GetTranslation(blessing.TempleBlessingCondition.PositiveCondition.ToString());
			}
			_isConfirmationBoxOpened = true;
			Singleton<UIEnhancementConfirmationBox>.Instance.ShowConfirmation(GLOOM.LocalizationManager.GetTranslation("GUI_TEMPLE_CONFIRMATION_TITLE"), stringBuilder.ToString(), (effectInfoCondition.TempleIcon == null) ? effectInfoCondition.Icon : effectInfoCondition.TempleIcon, translation.ToTitleCase() + " " + GLOOM.LocalizationManager.GetTranslation(character.Class.LocKey), delegate
			{
				OnConfirmedBuy(blessing);
			}, "GUI_TEMPLE_CONFIRM", null, delegate
			{
				_isConfirmationBoxOpened = false;
			});
		}
	}

	private void OnConfirmedBuy(TempleYML.TempleBlessingDefinition blessing)
	{
		_isConfirmationBoxOpened = false;
		if (FFSNetwork.IsOnline)
		{
			int actorID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			int supplementaryDataIDMin = service.GetAvailableBlessings().IndexOf(blessing);
			IProtocolToken supplementaryDataToken = new BlessingToken(character.CharacterID);
			Synchronizer.SendGameAction(GameActionType.BuyBlessing, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: true, disableAutoReplication: false, actorID, supplementaryDataIDMin, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			if (FFSNetwork.IsClient)
			{
				return;
			}
		}
		AudioControllerUtils.PlaySound(audioItemBless);
		BuyBlessing(character.CharacterID, blessing);
	}

	private void BuyBlessing(string characterId, TempleYML.TempleBlessingDefinition blessing)
	{
		int num = service.DevotionLevel;
		service.Buy(characterId, blessing);
		Shop.UpdateAvailable(blessing, available: false);
		totalDonatedGold.CountTo(service.CalculateTotalGoldDonated());
		devotionLevel.SetArguments(service.DevotionLevel.ToString());
		int maxProgressCurrentLevel = service.NextDevotionLevelAmount;
		int currentLevelProgress = service.DevotionCurrentProgress;
		if (service.DevotionLevel > num)
		{
			int num2 = service.CalculateDevotionTotalProgress(num);
			devotionProgress.PlayProgressTo(num2, num2, delegate
			{
				devotionProgress.SetAmount(0f, maxProgressCurrentLevel);
				if (currentLevelProgress > 0)
				{
					devotionProgress.PlayProgressTo(service.DevotionCurrentProgress, maxProgressCurrentLevel);
				}
			});
		}
		else
		{
			devotionProgress.PlayProgressTo(currentLevelProgress, maxProgressCurrentLevel);
		}
	}

	public void ProxyBuyBlessing(GameAction action, ref bool actionValid)
	{
		TempleYML.TempleBlessingDefinition blessing = service.GetAvailableBlessings()[action.SupplementaryDataIDMin];
		string text = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
		if (FFSNetwork.IsHost)
		{
			if (service.IsAvailable(text, blessing) && service.CanAfford(text, blessing))
			{
				ProxyBuyBlessing(text, blessing);
				actionValid = true;
			}
			else
			{
				actionValid = false;
			}
		}
		else
		{
			ProxyBuyBlessing(text, blessing);
			actionValid = true;
		}
	}

	private void ProxyBuyBlessing(string targetCharacterID, TempleYML.TempleBlessingDefinition blessing)
	{
		if (window.IsVisible)
		{
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
			int controllableID = (AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
			if (PlayerRegistry.MyPlayer.HasControlOver(controllableID))
			{
				AudioControllerUtils.PlaySound(audioItemBless);
			}
			BuyBlessing(targetCharacterID, blessing);
		}
		else
		{
			service.Buy(targetCharacterID, blessing);
		}
	}

	public void EnablePartialHide()
	{
		_inventoryCanvasGroup.alpha = 0f;
		helpBox.gameObject.SetActive(value: false);
	}

	public void DisablePartialHide()
	{
		_inventoryCanvasGroup.alpha = 1f;
		helpBox.gameObject.SetActive(value: true);
	}
}
