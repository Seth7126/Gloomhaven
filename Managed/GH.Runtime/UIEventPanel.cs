#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Script.Networking.Tokens;
using Chronos;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Events;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.SpecialStates;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIEventPanel : Singleton<UIEventPanel>
{
	[SerializeField]
	private TextLocalizedListener eventBanner;

	[SerializeField]
	private TextLocalizedListener eventTitle;

	[SerializeField]
	private TMP_Text eventDescription;

	[SerializeField]
	private Image eventImage;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private GUIAnimator screenRevealAnimation;

	[SerializeField]
	private GUIAnimator screenTransitionAnimation;

	[SerializeField]
	private string screenRevealAudioItem = "PlaySound_UIMapSelectButton";

	[SerializeField]
	private GUIAnimator hideAnimation;

	[SerializeField]
	private string showAudioItem = "PlaySound_UIMapEncounter";

	[SerializeField]
	private string finsihAudioItem = "PlaySound_UITravelCartStartDrum";

	[Header("City Encounter")]
	[SerializeField]
	private GUIAnimator showCityAnimation;

	[SerializeField]
	private float delayToPlayScreenRevealInCity;

	[Header("Road Encounter")]
	[SerializeField]
	private GUIAnimator showRoadAnimation;

	[SerializeField]
	private float delayToPlayScreenRevealInRoad = 2f;

	[Header("Rewards")]
	[SerializeField]
	private UICampaignReward rewardPrefab;

	[SerializeField]
	private List<UICampaignReward> rewardsPool;

	[SerializeField]
	private RectTransform rewardsContainer;

	[SerializeField]
	private UIIntroductionRewardsProcess rewardIntroduction;

	[Header("Options")]
	[SerializeField]
	private EventButton optionPrefab;

	[SerializeField]
	private ScrollRect optionsScroll;

	[SerializeField]
	private ContentSizeFitter optionsContentSizeFitter;

	[SerializeField]
	private List<EventButton> optionsPool;

	[Header("Leave options")]
	[SerializeField]
	private string leaveFormat = "<size=32><color=#F3DDABFF><font=\"MarcellusSC-Regular SDF\">{0}";

	private const string cRoadEventType = "RoadEvent";

	private UIWindow myWindow;

	private CRoadEvent eventData;

	private CRoadEventScreen currentScreen;

	private Action onEventCompleted;

	private List<EventButton> eventButtons = new List<EventButton>();

	private List<RewardGroup> rolledTreasureTables = new List<RewardGroup>();

	private List<RewardGroup> appliedRewards = new List<RewardGroup>();

	private List<Reward> shownRewards = new List<Reward>();

	private int buttonIDCounter;

	private Sprite loadedImage;

	private bool isPlayingShowAnimation;

	private int RollResult;

	public bool IsOpen => myWindow.IsOpen;

	protected override void Awake()
	{
		base.Awake();
		myWindow = GetComponent<UIWindow>();
		myWindow.onShown.AddListener(PlayShowAnimation);
		myWindow.onHidden.AddListener(OnFinishedEvent);
		showCityAnimation.OnAnimationFinished.AddListener(OnShowAnimationFinished);
		showRoadAnimation.OnAnimationFinished.AddListener(OnShowAnimationFinished);
		showRoadAnimation.OnAnimationStarted.AddListener(delegate
		{
			Debug.LogGUI("Show road animation starts");
		});
		hideAnimation.OnAnimationFinished.AddListener(delegate
		{
			Debug.LogGUI("Event hide animation finished " + myWindow.IsOpen);
			if (eventData != null)
			{
				myWindow.Hide(eventData.EventType == "RoadEvent" && AdventureState.MapState.IsCampaign);
			}
		});
		screenRevealAnimation.OnAnimationFinished.AddListener(delegate
		{
			foreach (EventButton eventButton in eventButtons)
			{
				eventButton.ShowAnswer(instant: false);
			}
			if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt, ActionPhaseType.MapEvent);
			}
			isPlayingShowAnimation = false;
			if (!controllerArea.IsFocused)
			{
				controllerArea.Focus();
			}
			else
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.MapEvent);
			}
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
	}

	public void StartEvent(CRoadEvent eventData, Action onEventCompleted)
	{
		if (eventData != null)
		{
			this.eventData = eventData;
			this.onEventCompleted = onEventCompleted;
			isPlayingShowAnimation = true;
			currentScreen = eventData.Screens[0];
			buttonIDCounter = 0;
			appliedRewards.Clear();
			shownRewards.Clear();
			HelperTools.NormalizePool(ref rewardsPool, rewardPrefab.gameObject, rewardsContainer, 0);
			RollTreasureTables();
			eventBanner.SetTextKey((eventData.EventType == "RoadEvent") ? "GUI_ENCOUNTER" : "GUI_CITY_ENCOUNTER");
			eventTitle.SetTextKey((eventData.EventType == "RoadEvent") ? "GUI_ENCOUNTER" : "GUI_CITY_ENCOUNTER");
			if (currentScreen.TreasureConditions.Count > 0)
			{
				foreach (CRoadEventTreasureCondition treasureCondition in currentScreen.TreasureConditions)
				{
					if (CRoadEventTreasureCondition.DoGroupsMatch(appliedRewards, treasureCondition, RollResult))
					{
						currentScreen = treasureCondition.Screen;
						break;
					}
				}
			}
			UpdateScreen(setActionProcessorStateIfOnline: false, forceAnimation: true);
			Debug.LogGUI("Show event screen " + myWindow.IsOpen);
			myWindow.Show(instant: true);
			controllerArea.Enable();
			if (FFSNetwork.IsOnline && !FFSNetwork.IsClient)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MapEvent);
			}
		}
		else
		{
			onEventCompleted?.Invoke();
		}
	}

	private void LoadImage()
	{
		if (eventData.NarrativeImageId.IsNOTNullOrEmpty())
		{
			NarrativeConfigUI narrativeImageConfig = UIInfoTools.Instance.GetNarrativeImageConfig(eventData.NarrativeImageId);
			if (narrativeImageConfig == null)
			{
				eventImage.sprite = ((eventData.EventType == "RoadEvent") ? UIInfoTools.Instance.defaultEventImage : UIInfoTools.Instance.defaultCityEventImage);
				return;
			}
			if (narrativeImageConfig.picture != null)
			{
				eventImage.sprite = narrativeImageConfig.picture;
				return;
			}
			string path = Path.Combine(Application.streamingAssetsPath, narrativeImageConfig.pathPicture + ".png");
			if (File.Exists(path))
			{
				byte[] data = File.ReadAllBytes(path);
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(data);
				eventImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
		}
		else
		{
			eventImage.sprite = ((eventData.EventType == "RoadEvent") ? UIInfoTools.Instance.defaultEventImage : UIInfoTools.Instance.defaultCityEventImage);
		}
	}

	private void OnShowAnimationFinished()
	{
		Debug.LogGUI("OnShowAnimationFinished");
		LoadImage();
		StartCoroutine(RevealScreen());
	}

	private IEnumerator RevealScreen()
	{
		float num = ((eventData.EventType == "RoadEvent") ? delayToPlayScreenRevealInRoad : delayToPlayScreenRevealInCity);
		Debug.LogGUI("Reveal event screen " + num + " " + screenRevealAnimation.IsPlaying);
		if (num > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(num);
		}
		AudioControllerUtils.PlaySound(screenRevealAudioItem);
		screenRevealAnimation.Play();
	}

	private void RollTreasureTables()
	{
		rolledTreasureTables.Clear();
		string text = "";
		foreach (CRoadEventScreen screen in eventData.Screens)
		{
			foreach (Tuple<string, TreasureTable, string> treasureTable in GetTreasureTables(screen))
			{
				Debug.Log("EVENT TREASURE TABLE: " + treasureTable.Item1 + " and " + treasureTable.Item2.Name);
				List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, new List<TreasureTable> { treasureTable.Item2 }, AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
				rolledTreasureTables.Add(new RewardGroup(list, treasureTable.Item2.Name, screen.Name, treasureTable.Item1, treasureTable.Item3));
				foreach (RewardGroup item in list)
				{
					text = text + item.TreasureType + " ";
				}
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (Roll Event Treasure): " + AdventureState.MapState.PeekMapRNG + "Rewards: " + text);
	}

	private void UpdateScreen(bool setActionProcessorStateIfOnline = true, bool forceAnimation = false)
	{
		bool flag = currentScreen == eventData.Screens[0];
		if (flag || forceAnimation)
		{
			screenTransitionAnimation.Play();
		}
		if (optionsContentSizeFitter != null)
		{
			optionsContentSizeFitter.enabled = true;
		}
		List<Reward> list = new List<Reward>();
		if (currentScreen.TreasureTables.Count > 0)
		{
			RollTreasureTables();
			foreach (TreasureTable treasureTable in currentScreen.TreasureTables)
			{
				RewardGroup rewardGroup = rolledTreasureTables.Find((RewardGroup x) => x.TreasureTable == treasureTable.Name && x.TreasureType == "Screen");
				if (rewardGroup == null || rewardGroup.Rewards.IsNullOrEmpty())
				{
					continue;
				}
				if (currentScreen.Player != 0)
				{
					foreach (Reward reward in rewardGroup.Rewards)
					{
						reward.GiveToCharacterID = AdventureState.MapState.MapParty.SelectedCharacters.ElementAt(currentScreen.Player - 1).CharacterID;
						reward.GiveToCharacterType = EGiveToCharacterType.Give;
						reward.TreasureDistributionType = ETreasureDistributionType.None;
						if (reward.IsVisibleInUI())
						{
							list.Add(reward);
						}
					}
				}
				appliedRewards.Add(rewardGroup);
			}
		}
		string text = "";
		if (!string.IsNullOrEmpty(currentScreen.Header))
		{
			text = string.Format(LocalizationManager.GetTranslation(currentScreen.Header), "<b>" + AdventureState.MapState.MapParty.SelectedCharacters.ElementAt(currentScreen.Player - 1).CharacterName + "</b>", RollResult, 100) + " \n\n<b>";
		}
		eventDescription.text = ProcessTreasureTableLookups((currentScreen.Text == null) ? null : (text + LocalizationManager.GetTranslation(currentScreen.Text)), currentScreen.Name);
		if (currentScreen.End != CRoadEventScreen.ActionAtEventEnd.None)
		{
			FinalScreen();
		}
		else if (currentScreen.Next != null)
		{
			CRoadEventScreen cRoadEventScreen = eventData.Screens.SingleOrDefault((CRoadEventScreen s) => s.Name == currentScreen.Next);
			if (cRoadEventScreen == null)
			{
				Debug.LogError("Unable to find screen named " + currentScreen.Next);
				FinalScreen();
				return;
			}
			ShowRewards(list);
			shownRewards.AddRange(list);
			RollResult = AdventureState.MapState.MapRNG.Next(100) + 1;
			HandleNextScreenTreasureConditions(cRoadEventScreen);
			UpdateScreen();
		}
		else if (currentScreen.Options != null && currentScreen.Options.Count > 0)
		{
			ShowRewards(list);
			shownRewards.AddRange(list);
			eventButtons.Clear();
			HelperTools.NormalizePool(ref optionsPool, optionPrefab.gameObject, optionsScroll.content, 0);
			int num = 0;
			foreach (CRoadEventOption option in currentScreen.Options)
			{
				EventButton button;
				if (CheckOptionConditions(option, currentScreen.Name))
				{
					button = GetEventButtonFromPool(num++);
					eventButtons.Add(button);
					button.SetAvailableOption(buttonIDCounter++, option.Name, ProcessTreasureTableLookups((option.Text == null) ? null : LocalizationManager.GetTranslation(option.Text), currentScreen.Name), ProcessTreasureTableLookups((option.Subtext == null) ? null : LocalizationManager.GetTranslation(option.Subtext), currentScreen.Name), delegate
					{
						ContinueEvent(button);
					}, delegate(bool hovered)
					{
						OnHoveredOption(button, hovered);
					}, !flag && !forceAnimation);
					continue;
				}
				Debug.Log("Show event screen failed condition");
				if (option.IfUnavailable != CRoadEventOption.ERoadEventUnavailableOptionVisibility.NA && option.IfUnavailable != CRoadEventOption.ERoadEventUnavailableOptionVisibility.Hide)
				{
					Debug.Log("Show event screen failed condition and not hide");
					button = GetEventButtonFromPool(num++);
					eventButtons.Add(button);
					button.SetUnvailableOption(buttonIDCounter++, option.Name, ProcessTreasureTableLookups((option.Text == null) ? null : LocalizationManager.GetTranslation(option.Text), currentScreen.Name), ProcessTreasureTableLookups((option.Subtext == null) ? null : LocalizationManager.GetTranslation(option.Subtext), currentScreen.Name), delegate
					{
						ContinueEvent(button);
					}, delegate(bool hovered)
					{
						OnHoveredOption(button, hovered);
					}, ProcessTreasureTableLookups(option.UnavailableText.IsNullOrEmpty() ? null : LocalizationManager.GetTranslation(option.UnavailableText), currentScreen.Name), !flag && !forceAnimation);
				}
			}
			optionsScroll.verticalNormalizedPosition = 1f;
			if (optionsContentSizeFitter != null)
			{
				optionsContentSizeFitter.enabled = num > 3 || num < 2;
				(optionsContentSizeFitter.transform as RectTransform).sizeDelta = Vector2.zero;
			}
			if (FFSNetwork.IsClient && setActionProcessorStateIfOnline)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt, ActionPhaseType.MapEvent);
			}
			if (controllerArea.IsFocused)
			{
				EnableNavigation();
			}
		}
		else
		{
			Debug.LogError("Unable to update event screen.");
			FinalScreen();
		}
		Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
	}

	private void ShowRewards(List<Reward> rewards)
	{
		HelperTools.NormalizePool(ref rewardsPool, rewardPrefab.gameObject, rewardsContainer, rewards.Count);
		for (int i = 0; i < rewards.Count; i++)
		{
			rewardsPool[i].ShowReward(rewards[i]);
		}
		rewardIntroduction.Process(rewards);
	}

	private EventButton GetEventButtonFromPool(int pos)
	{
		if (optionsPool.Count <= pos)
		{
			optionsPool.Add(UnityEngine.Object.Instantiate(optionPrefab, optionsScroll.content, worldPositionStays: false));
		}
		return optionsPool[pos];
	}

	private bool CheckOptionConditions(CRoadEventOption option, string screen)
	{
		if (option.Conditions != null)
		{
			foreach (KeyValuePair<CRoadEventOption.ERoadEventOptionCondition, List<string>> optionCondition in option.Conditions)
			{
				switch (optionCondition.Key)
				{
				case CRoadEventOption.ERoadEventOptionCondition.MinGold:
				{
					if (optionCondition.Value[0].Contains("Any") && ParseInt(optionCondition.Value[0].Replace("Any", string.Empty), out var minGold))
					{
						switch (AdventureState.MapState.GoldMode)
						{
						case EGoldMode.PartyGold:
							if (AdventureState.MapState.MapParty.PartyGold < minGold)
							{
								return false;
							}
							break;
						case EGoldMode.CharacterGold:
							if (AdventureState.MapState.MapParty.SelectedCharacters.All((CMapCharacter it) => it.CharacterGold < minGold))
							{
								return false;
							}
							break;
						}
					}
					else
					{
						if (!ParseInt(optionCondition.Value[0], out minGold) && !ParseInt(ProcessTreasureTableLookups("¬" + optionCondition.Value[0] + "¬", screen), out minGold, logErrors: true))
						{
							break;
						}
						switch (AdventureState.MapState.GoldMode)
						{
						case EGoldMode.PartyGold:
							if (AdventureState.MapState.MapParty.PartyGold < minGold)
							{
								return false;
							}
							break;
						case EGoldMode.CharacterGold:
							if (AdventureState.MapState.MapParty.TotalSelectedCharactersGold < minGold)
							{
								return false;
							}
							break;
						}
					}
					break;
				}
				case CRoadEventOption.ERoadEventOptionCondition.MaxGold:
				{
					if (!ParseInt(optionCondition.Value[0], out var output) && !ParseInt(ProcessTreasureTableLookups("¬" + optionCondition.Value[0] + "¬", screen), out output, logErrors: true))
					{
						break;
					}
					switch (AdventureState.MapState.GoldMode)
					{
					case EGoldMode.PartyGold:
						if (AdventureState.MapState.MapParty.PartyGold >= output)
						{
							return false;
						}
						break;
					case EGoldMode.CharacterGold:
						if (AdventureState.MapState.MapParty.TotalSelectedCharactersGold >= output)
						{
							return false;
						}
						break;
					}
					break;
				}
				case CRoadEventOption.ERoadEventOptionCondition.InParty:
					if (!AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter x) => optionCondition.Value.Contains(x.CharacterID)))
					{
						return false;
					}
					break;
				case CRoadEventOption.ERoadEventOptionCondition.NotInParty:
					if (AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter x) => optionCondition.Value.Contains(x.CharacterID)))
					{
						return false;
					}
					break;
				case CRoadEventOption.ERoadEventOptionCondition.HaveItem:
				{
					bool flag2 = true;
					foreach (string itemString in optionCondition.Value)
					{
						if (!AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter x) => x.AllCharacterItems.Any((CItem y) => y.YMLData.StringID == itemString)))
						{
							flag2 = false;
						}
					}
					if (!flag2)
					{
						return false;
					}
					break;
				}
				case CRoadEventOption.ERoadEventOptionCondition.HaveItemSlotEquipped:
				{
					bool flag = true;
					foreach (string item in optionCondition.Value)
					{
						if (Enum.TryParse<CItem.EItemSlot>(item, out var slotType) && AdventureState.MapState.MapParty.SelectedCharacters.All((CMapCharacter x) => x.CheckEquippedItems.All((CItem item) => item.YMLData.Slot != slotType)))
						{
							flag = false;
						}
					}
					if (!flag)
					{
						return false;
					}
					break;
				}
				}
			}
		}
		return true;
	}

	public static bool ParseInt(string input, out int output, bool logErrors = false)
	{
		output = 0;
		if (int.TryParse(input, out output))
		{
			return true;
		}
		if (logErrors)
		{
			Debug.LogError("Couldn't parse int value in event from input " + input);
		}
		return false;
	}

	private string ProcessTreasureTableLookups(string text, string screen)
	{
		if (text == null)
		{
			return string.Empty;
		}
		while (text.Contains('¬'))
		{
			string[] key = CardLayoutRow.GetKey(text, '¬').Split('.');
			if (key.Length != 2)
			{
				Debug.LogError("Invalid key " + key?.ToString() + " in event text.");
			}
			else
			{
				RewardGroup rewardGroup = rolledTreasureTables.FirstOrDefault((RewardGroup x) => x.TreasureTable == key[0]);
				if (rewardGroup != null)
				{
					text = CardLayoutRow.ReplaceKey(text, '¬', rewardGroup.GetAbsValueAsString(key[1]));
					continue;
				}
				Debug.LogError("Unable to find treasure table with name " + key[0]);
			}
			text = CardLayoutRow.ReplaceKey(text, '¬', string.Empty);
		}
		return text;
	}

	private void ContinueEvent(EventButton eventButton)
	{
		if (FFSNetwork.IsHost)
		{
			if (eventData != null && currentScreen != null)
			{
				int iD = eventButton.ID;
				IProtocolToken supplementaryDataToken = new RoadEventToken(eventData.ID, currentScreen.Name);
				Synchronizer.SendGameAction(GameActionType.ContinueRoadEvent, ActionPhaseType.MapEvent, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, iD, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			else
			{
				Synchronizer.SendGameAction(GameActionType.ContinueRoadEvent, ActionPhaseType.MapEvent, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, eventButton.ID);
			}
		}
		if (currentScreen.Options == null || currentScreen.Options.Count == 0)
		{
			UpdateScreen();
			return;
		}
		CRoadEventOption option = currentScreen.Options.Single((CRoadEventOption x) => x.Name == eventButton.Name);
		SimpleLog.AddToSimpleLog("RoadEvent (" + eventData.ID + ") option chosen: " + option.Name);
		if (option.TreasureTables.Count > 0)
		{
			RollTreasureTables();
			foreach (TreasureTable treasureTable in option.TreasureTables)
			{
				RewardGroup rewardGroup = rolledTreasureTables.Find((RewardGroup x) => x.TreasureTable == treasureTable.Name && x.Screen == currentScreen.Name && x.Option == option.Name);
				if (rewardGroup != null && !rewardGroup.Rewards.IsNullOrEmpty())
				{
					appliedRewards.Add(rewardGroup);
				}
			}
		}
		CRoadEventScreen cRoadEventScreen = eventData.Screens.SingleOrDefault((CRoadEventScreen s) => s.Name == option.Next);
		if (cRoadEventScreen != null)
		{
			RollResult = AdventureState.MapState.MapRNG.Next(100) + 1;
			HandleNextScreenTreasureConditions(cRoadEventScreen);
			UpdateScreen();
		}
		else
		{
			Debug.LogError("Unable to find screen named " + option.Next);
			FinalScreen();
		}
	}

	public void HandleNextScreenTreasureConditions(CRoadEventScreen nextScreen)
	{
		currentScreen = nextScreen;
		if (nextScreen.TreasureConditions.Count <= 0)
		{
			return;
		}
		foreach (CRoadEventTreasureCondition treasureCondition in nextScreen.TreasureConditions)
		{
			if (CRoadEventTreasureCondition.DoGroupsMatch(appliedRewards, treasureCondition, RollResult))
			{
				HandleNextScreenTreasureConditions(treasureCondition.Screen);
			}
		}
	}

	private void FinalScreen()
	{
		SimpleLog.AddToSimpleLog("Finished showing event card");
		if (AdventureState.MapState.IsCampaign)
		{
			bool flag = eventData.EventType == "RoadEvent";
			CCardDeck cCardDeck = ((!flag) ? AdventureState.MapState.MapParty.CityEventDeck : AdventureState.MapState.MapParty.RoadEventDeck);
			if (currentScreen.End == CRoadEventScreen.ActionAtEventEnd.Reuse)
			{
				SimpleLog.AddToSimpleLog(flag ? ("[ROAD EVENT DECK]: Removing event card: " + eventData.ID) : ("[CITY EVENT DECK]: Removing event card: " + eventData.ID));
				cCardDeck.RemoveCard(eventData.ID);
				SimpleLog.AddToSimpleLog(flag ? ("[ROAD EVENT DECK]: Adding event card back to bottom: " + eventData.ID) : ("[CITY EVENT DECK]: Adding event card bottom: " + eventData.ID));
				cCardDeck.AddCard(eventData.ID, CCardDeck.EAddCard.Bottom);
			}
			else
			{
				SimpleLog.AddToSimpleLog(flag ? ("[ROAD EVENT DECK]: Removing event card: " + eventData.ID) : ("[CITY EVENT DECK]: Removing event card: " + eventData.ID));
				cCardDeck.RemoveCard(eventData.ID);
			}
		}
		HelperTools.NormalizePool(ref optionsPool, optionPrefab.gameObject, optionsScroll.content, 1);
		EventButton button = optionsPool[0];
		eventButtons.Add(button);
		button.SetAvailableOption(buttonIDCounter, "Leave", string.Format(leaveFormat, LocalizationManager.GetTranslation("GUI_LEAVE")), null, delegate
		{
			CompleteEvent(button);
		}, delegate(bool hovered)
		{
			OnHoveredOption(button, hovered);
		});
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
		if (AdventureState.MapState.IsCampaign)
		{
			ShowRewards(appliedRewards.SelectMany((RewardGroup it) => from reward in it.Rewards.Except(shownRewards)
				where reward.IsVisibleInUI()
				select reward).ToList());
		}
		else
		{
			HelperTools.NormalizePool(ref rewardsPool, rewardPrefab.gameObject, rewardsContainer, 0);
		}
		if (FFSNetwork.IsClient)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessOneAndHalt, ActionPhaseType.MapEvent);
		}
	}

	private void OnHoveredOption(EventButton option, bool hovered)
	{
		if (hovered && InputManager.GamePadInUse)
		{
			optionsScroll.ScrollToFit(option.transform as RectTransform);
		}
	}

	private void CompleteEvent(EventButton buttonPressed)
	{
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.ContinueRoadEvent, ActionPhaseType.MapEvent, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, buttonPressed.ID);
		}
		hideAnimation.Play();
	}

	private void PlayShowAnimation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(SpecialStateTag.Lock);
		if (eventData.EventType == "RoadEvent" && AdventureState.MapState.IsCampaign)
		{
			showRoadAnimation.Play();
		}
		else
		{
			showCityAnimation.Play();
		}
	}

	public void PlayShowAudio()
	{
		AudioControllerUtils.PlaySound(showAudioItem);
	}

	private void OnFinishedEvent()
	{
		if (eventData == null)
		{
			return;
		}
		OnHidden();
		List<Reward> rewards = appliedRewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
		if (AdventureState.MapState.IsCampaign)
		{
			if (eventData.EventType == "RoadEvent")
			{
				Singleton<CampaignRewardsManager>.Instance.ShowAppliedEffects(rewards, delegate
				{
					DistributeRewards(rewards);
				});
				return;
			}
			AdventureState.MapState.ApplyRewards(rewards.FindAll((Reward it) => it.Type == ETreasureType.Prosperity), "party");
			Singleton<CampaignRewardsManager>.Instance.ShowAppliedEffects(rewards, delegate
			{
				DistributeRewards(rewards.FindAll((Reward it) => it.Type != ETreasureType.Prosperity));
			});
		}
		else
		{
			Singleton<UIRewardsManager>.Instance.StartRewardsShowcase(appliedRewards, hideBlackOverlayInstantly: false, networkProcessIfServer: true, delegate
			{
				DistributeRewards(rewards);
			});
		}
	}

	private void DistributeRewards(List<Reward> rewards)
	{
		bool isRoadEvent = eventData.EventType == "RoadEvent";
		bool readyUp = isRoadEvent && rewards.Count > 0;
		if (readyUp)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapEvent);
		}
		Singleton<UIDistributeRewardManager>.Instance.Process(rewards, delegate
		{
			if (readyUp)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MapEvent);
			}
			if (isRoadEvent && NewPartyDisplayUI.PartyDisplay.IsOpen)
			{
				NewPartyDisplayUI.PartyDisplay.Hide(this, instant: false, OnDistributedRewards);
			}
			else
			{
				OnDistributedRewards();
			}
		}, delegate
		{
			NewPartyDisplayUI.PartyDisplay.Show(this);
		}, readyUp);
	}

	private void OnDistributedRewards()
	{
		eventData = null;
		AudioControllerUtils.PlaySound(finsihAudioItem);
		onEventCompleted?.Invoke();
	}

	private void OnDisable()
	{
		OnHidden();
	}

	private void OnHidden()
	{
		StopAllCoroutines();
		screenTransitionAnimation.Stop();
		showCityAnimation.Stop();
		showRoadAnimation.Stop();
		hideAnimation.Stop();
		screenRevealAnimation.Stop();
		if (loadedImage != null)
		{
			Resources.UnloadAsset(loadedImage);
			loadedImage = null;
		}
		controllerArea.Destroy();
	}

	private List<Tuple<string, TreasureTable, string>> GetTreasureTables(CRoadEventScreen screen)
	{
		List<Tuple<string, TreasureTable, string>> list = new List<Tuple<string, TreasureTable, string>>();
		foreach (CRoadEventTreasureCondition treasureCondition in screen.TreasureConditions)
		{
			list.AddRange(GetTreasureTables(treasureCondition.Screen));
		}
		foreach (TreasureTable treasureTable in screen.TreasureTables)
		{
			list.Add(new Tuple<string, TreasureTable, string>(screen.Name, treasureTable, "Screen"));
		}
		foreach (CRoadEventOption option in screen.Options)
		{
			foreach (TreasureTable treasureTable2 in option.TreasureTables)
			{
				list.Add(new Tuple<string, TreasureTable, string>(option.Name, treasureTable2, "Option"));
			}
		}
		return list;
	}

	private void EnableNavigation()
	{
		if (!isPlayingShowAnimation)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.MapEvent);
		}
	}

	private void DisableNavigation()
	{
	}

	public void ClientContinueRoadEvent(GameAction action)
	{
		EventButton eventButton = eventButtons.FirstOrDefault((EventButton x) => x.ID == action.SupplementaryDataIDMed);
		if (eventButton != null)
		{
			Debug.Log("ClientContinueRoadEvent");
			eventButton.Click();
			return;
		}
		throw new Exception("Error continuing the road event. No button with ID " + action.SupplementaryDataIDMed + " found.");
	}
}
