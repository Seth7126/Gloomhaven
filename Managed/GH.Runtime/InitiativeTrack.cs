#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using Chronos;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.Configuration;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InitiativeTrack : MonoBehaviour
{
	[SerializeField]
	private float trackReorderDuration = 0.5f;

	[SerializeField]
	private float delayAnimationDraw = 0.5f;

	[SerializeField]
	private GameObject playerBehaviourPrefab;

	[SerializeField]
	private GameObject enemyBehaviourPrefab;

	[SerializeField]
	private Transform initiativeTrackHolder;

	[SerializeField]
	private HorizontalLayoutGroup layoutGroup;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[SerializeField]
	public HelpBox helpBox;

	[SerializeField]
	private Transform enemyCardsHolder;

	[SerializeField]
	private Graphic enemyCardsBlocker;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private int sortingOrder = 40;

	[SerializeField]
	private string revealEnemiesAudioItem = "PlaySound_EnemyCardDraw";

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private Hotkey previousControllerTip;

	[SerializeField]
	private Hotkey nextControllerTip;

	[SerializeField]
	private InitiativeTrackConfigUI _initiativeTrackConfig;

	private List<InitiativeTrackActorBehaviour> actorsUI = new List<InitiativeTrackActorBehaviour>();

	private List<InitiativeTrackPlayerBehaviour> playersUI = new List<InitiativeTrackPlayerBehaviour>();

	private List<InitiativeTrackEnemyBehaviour> enemiesUI = new List<InitiativeTrackEnemyBehaviour>();

	private InitiativeTrackActorBehaviour selectedActor;

	private bool isShowingDetails;

	private bool isAnimating;

	private bool animationDelayed;

	private List<float> locations = new List<float>();

	private List<LTDescr> moveXAnimations = new List<LTDescr>();

	private const string DebugCancelMove = "Move Potraits";

	private Comparer<InitiativeTrackActorBehaviour> PlayerTypeComparer = Comparer<InitiativeTrackActorBehaviour>.Create(delegate(InitiativeTrackActorBehaviour x, InitiativeTrackActorBehaviour y)
	{
		if (y != null && x != null && x.ActorType() == y.ActorType())
		{
			return x.CompareTo(y);
		}
		if (x != null && x.ActorType() == CActor.EType.Player)
		{
			return 1;
		}
		return (x != null && x.ActorType() == CActor.EType.Enemy) ? (-1) : 0;
	});

	private int _currentElementIndex;

	public static InitiativeTrack Instance { get; private set; }

	public List<InitiativeTrackPlayerBehaviour> PlayersUI => playersUI;

	public bool NeedMinimizeEnemyInitiativeTrackAvatars => actorsUI.Count > _initiativeTrackConfig.ActorsNumberBorder;

	public bool IsSelectable
	{
		get
		{
			if (PhaseManager.PhaseType != CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && selectedActor != null && selectedActor.Actor is CPlayerActor cPlayer)
				{
					return !CardsHandManager.Instance.GetHand(cPlayer).IsImprovedLongResting;
				}
				return true;
			}
			return false;
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		ToggleSortingOrder(value: true);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			InputManager.UnregisterToOnPressed(KeyAction.CONTROL_INITIATVE_TRACK, ControllerFocus);
			previousControllerTip.Deinitialize();
			nextControllerTip.Deinitialize();
		}
		Instance = null;
		foreach (LTDescr moveXAnimation in moveXAnimations)
		{
			LeanTween.cancel(moveXAnimation.id);
		}
		moveXAnimations.Clear();
	}

	private void Start()
	{
		ShowControllerTips();
		controllerArea.OnFocused.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocused.AddListener(OnControllerAreaUnfocused);
		controllerArea.OnEnabledArea.AddListener(CheckShowControllerTip);
		if (InputManager.GamePadInUse)
		{
			previousControllerTip.Initialize(Singleton<UINavigation>.Instance.Input);
			nextControllerTip.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		else
		{
			previousControllerTip.gameObject.SetActive(value: false);
			nextControllerTip.gameObject.SetActive(value: false);
		}
	}

	private void ShowControllerTips()
	{
		if (InputManager.GamePadInUse)
		{
			previousControllerTip.DisplayHotkey(active: true);
			nextControllerTip.DisplayHotkey(active: true);
			previousControllerTip.UpdateHotkeyIcon();
			nextControllerTip.UpdateHotkeyIcon();
		}
	}

	private void CheckShowControllerTip()
	{
	}

	private void Update()
	{
		if (!isShowingDetails && InputManager.GetIsPressed(KeyAction.HIGHLIGHT))
		{
			isShowingDetails = true;
			foreach (InitiativeTrackActorBehaviour item in actorsUI)
			{
				item.ShowDetails(active: true);
			}
		}
		if (isShowingDetails && (!InputManager.GetIsPressed(KeyAction.HIGHLIGHT) || !Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(KeyAction.HIGHLIGHT).Enabled))
		{
			isShowingDetails = false;
			foreach (InitiativeTrackActorBehaviour item2 in actorsUI)
			{
				item2.ShowDetails(active: false);
			}
		}
		if (animationDelayed && !isAnimating)
		{
			animationDelayed = false;
			UpdateActors();
		}
	}

	public void ToggleSortingOrder(bool value)
	{
		if ((bool)canvas)
		{
			canvas.sortingOrder = (value ? sortingOrder : 0);
		}
	}

	public void OnCardHover(bool isActive, AbilityCardUI cardUI, CPlayerActor playerActor)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.RoundAbilityCards.Count == 0)
		{
			InitiativeTrackPlayerBehaviour initiativeTrackPlayerBehaviour = (InitiativeTrackPlayerBehaviour)FindInitiativeTrackActor(playerActor);
			if (initiativeTrackPlayerBehaviour != null)
			{
				initiativeTrackPlayerBehaviour.Avatar.PlayEffect(isActive ? InitiativeTrackActorAvatar.InitiativeEffects.Hover : InitiativeTrackActorAvatar.InitiativeEffects.Active);
			}
		}
	}

	public void UpdateActors()
	{
		if (!isAnimating)
		{
			StopAllCoroutines();
			foreach (InitiativeTrackActorBehaviour item in actorsUI)
			{
				item.RefreshInitiative();
			}
			if (CardsHandManager.Instance.CurrentHand != null)
			{
				CardsHandManager.Instance.CurrentHand.UpdateEffects();
			}
			StartCoroutine(AnimateInitiativeReorder());
		}
		else
		{
			animationDelayed = true;
		}
	}

	private void RebuildLayoutGroupImmediately()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
	}

	private IEnumerator AnimateInitiativeReorder()
	{
		isAnimating = true;
		contentSizeFitter.enabled = false;
		layoutGroup.enabled = false;
		yield return new WaitForEndOfFrame();
		locations.Clear();
		moveXAnimations.Clear();
		InitiativeTrackActorBehaviour[] componentsInChildren = initiativeTrackHolder.GetComponentsInChildren<InitiativeTrackActorBehaviour>(includeInactive: false);
		foreach (InitiativeTrackActorBehaviour initiativeTrackActorBehaviour in componentsInChildren)
		{
			locations.Add(initiativeTrackActorBehaviour.transform.position.x);
		}
		UpdateSortingOrder(updateDisplay: true);
		int num = 0;
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			if (item.gameObject.activeSelf)
			{
				LTDescr anim = LeanTween.moveX(item.gameObject, locations[locations.Count - 1 - num], trackReorderDuration).setEase(LeanTweenType.easeOutQuad);
				anim.setOnComplete((Action)delegate
				{
					moveXAnimations.Remove(anim);
				});
				moveXAnimations.Add(anim);
				num++;
			}
		}
		yield return Timekeeper.instance.WaitForSeconds(trackReorderDuration);
		moveXAnimations.Clear();
		locations.Clear();
		layoutGroup.enabled = true;
		contentSizeFitter.enabled = true;
		ShowSeparatorActorsByType();
		yield return new WaitForEndOfFrame();
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
		isAnimating = false;
	}

	private void StopUpdateActors()
	{
		if (isAnimating)
		{
			int num = 0;
			if (locations.Count > 0)
			{
				foreach (InitiativeTrackActorBehaviour item in actorsUI)
				{
					if (item.gameObject.activeSelf)
					{
						item.transform.position = new Vector3(locations[locations.Count - 1 - num], item.transform.position.y, item.transform.position.z);
						num++;
					}
				}
			}
			isAnimating = false;
			layoutGroup.enabled = true;
			contentSizeFitter.enabled = true;
			if (controllerArea.IsFocused)
			{
				EnableNavigation();
			}
		}
		locations.Clear();
		moveXAnimations.ForEach(delegate(LTDescr it)
		{
			LeanTween.cancel(it.id, "Move Potraits");
		});
		moveXAnimations.Clear();
	}

	public void Select(InitiativeTrackActorBehaviour actorUI)
	{
		if (!IsSelectable)
		{
			Debug.LogGUI("Skip select " + actorUI?.Actor?.GetPrefabName() + " in phase " + PhaseManager.PhaseType);
			return;
		}
		if (selectedActor != null && !actorUI.Actor.IsTakingExtraTurn)
		{
			selectedActor.Deselect();
		}
		selectedActor = actorUI;
		selectedActor.Select();
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ActorSelected, actorUI.Actor.GetPrefabName(), actorUI.Actor.ActorGuid));
		Choreographer.s_Choreographer.ClearHilightedActors();
		GameObject target = Choreographer.s_Choreographer.FindClientActorGameObject(selectedActor.Actor);
		ActorBehaviour.SetHilighted(target, hilight: true);
		CameraController.s_CameraController.SmartFocus(target, pauseDuringTransition: true);
	}

	public bool Select(CPlayerActor playerActor)
	{
		if (!IsSelectable)
		{
			return false;
		}
		foreach (InitiativeTrackPlayerBehaviour item in playersUI)
		{
			if (item.Actor == playerActor)
			{
				if (!InteractabilityManager.ShouldAllowClickForExtendedButton(item.avatarButton))
				{
					Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
					return false;
				}
				Select(item);
				return true;
			}
		}
		return false;
	}

	[ContextMenu("Play draw enemyActor card animation")]
	private void EnemyAbilityCardsAnimation()
	{
		UpdateSortingOrderByPlayerType();
		int num = 0;
		int num2 = 0;
		foreach (InitiativeTrackEnemyBehaviour item in enemiesUI)
		{
			if (item.gameObject.activeSelf)
			{
				num2++;
			}
		}
		AudioControllerUtils.PlaySound(revealEnemiesAudioItem);
		foreach (InitiativeTrackActorBehaviour item2 in actorsUI)
		{
			if (item2.gameObject.activeSelf && item2.Actor.IsEnemyByDefault())
			{
				Action onCompleteCallback = ((num == 0) ? new Action(EndedEnemyAbilityCardsAnimation) : null);
				if (!(item2 is InitiativeTrackEnemyBehaviour))
				{
					Debug.LogErrorFormat("Missing InitiativeTrackEnemyBehaviour for enemyActor {0}", item2.Actor.Class.ID);
				}
				((InitiativeTrackEnemyBehaviour)item2).AnimateCardHighlight(num, num2, onCompleteCallback, delayAnimationDraw * (float)(num2 - 1 - num));
				num++;
			}
		}
		Choreographer.s_Choreographer.readyButton.AlternativeAction(FinishAbilityCardsAnimation, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"), !InputManager.GamePadInUse);
	}

	private void EndedEnemyAbilityCardsAnimation()
	{
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			if (item.gameObject.activeSelf && item.Actor.IsEnemyByDefault())
			{
				((InitiativeTrackEnemyBehaviour)item).OnCardHighlightAnimationComplete();
			}
		}
		StartCoroutine(InitiativeEnemyAbilityCardsAnimation());
	}

	private void ShowSeparatorActorsByType()
	{
		bool flag = true;
		for (int i = 0; i < actorsUI.Count; i++)
		{
			if (!actorsUI[i].gameObject.activeSelf)
			{
				continue;
			}
			if (actorsUI[i].Actor.IsEnemyByDefault())
			{
				flag = false;
			}
			else if (actorsUI[i].Actor.IsPlayerByDefault() && !flag)
			{
				flag = true;
				actorsUI[i].transform.GetSiblingIndex();
				if (InputManager.GamePadInUse && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<EnemyActionScenarioState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<RoundStartScenarioState>() && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<CardSelectionScenarioState>())
				{
					previousControllerTip.transform.SetAsFirstSibling();
					previousControllerTip.gameObject.SetActive(value: true);
					nextControllerTip.transform.SetAsLastSibling();
					nextControllerTip.gameObject.SetActive(value: true);
				}
				break;
			}
		}
	}

	public void ReorderControllerTips(bool isEnemyActionState = false)
	{
		if (isEnemyActionState)
		{
			if (InputManager.GamePadInUse)
			{
				previousControllerTip.transform.SetAsFirstSibling();
				nextControllerTip.transform.SetAsLastSibling();
			}
			return;
		}
		for (int i = 0; i < actorsUI.Count; i++)
		{
			if (actorsUI[i].gameObject.activeSelf && actorsUI[i].Actor.IsPlayerByDefault())
			{
				actorsUI[i].transform.GetSiblingIndex();
				if (InputManager.GamePadInUse)
				{
					previousControllerTip.transform.SetAsFirstSibling();
					nextControllerTip.transform.SetAsLastSibling();
				}
				break;
			}
		}
	}

	public void DisplayControllerTips(bool doShow = true)
	{
		if (InputManager.GamePadInUse)
		{
			previousControllerTip.gameObject.SetActive(doShow);
			nextControllerTip.gameObject.SetActive(doShow);
		}
	}

	private IEnumerator InitiativeEnemyAbilityCardsAnimation()
	{
		yield return AnimateInitiativeReorder();
		Choreographer.s_Choreographer.readyButton.AlternativeAction(PostEnemyCardAnimationProceed, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"), !InputManager.GamePadInUse);
	}

	private void FinishAbilityCardsAnimation()
	{
		int num = 0;
		int num2 = 0;
		foreach (InitiativeTrackEnemyBehaviour item in enemiesUI)
		{
			if (item.gameObject.activeSelf)
			{
				num2++;
			}
		}
		foreach (InitiativeTrackEnemyBehaviour item2 in enemiesUI)
		{
			if (item2.gameObject.activeSelf)
			{
				StopAllCoroutines();
				item2.FinishCardHighlightAnimation(num, num2, delegate
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(PostEnemyCardAnimationProceed, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"));
				});
				num++;
			}
		}
		StopUpdateActors();
		PostEnemyCardAnimationProceed();
	}

	private void PostEnemyCardAnimationProceed()
	{
		if (InputManager.GamePadInUse)
		{
			InputManager.RequestEnableInput(this, KeyAction.CONTROL_INITIATVE_TRACK);
		}
		enemyCardsBlocker.raycastTarget = false;
		foreach (InitiativeTrackEnemyBehaviour item in enemiesUI)
		{
			item.Deselect();
		}
		Choreographer.s_Choreographer.Pass();
	}

	public InitiativeTrackActorBehaviour SelectedActor()
	{
		return selectedActor;
	}

	public InitiativeTrackActorBehaviour FindInitiativeTrackActor(CActor actor)
	{
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			if (item.Actor == actor)
			{
				return item;
			}
		}
		return null;
	}

	private void NormalizeActorsPool(List<CActor> actors)
	{
		actorsUI.Clear();
		int i = 0;
		int j = 0;
		foreach (CActor actor in actors)
		{
			if (actor.IsPlayerByDefault())
			{
				if (i >= playersUI.Count)
				{
					HelperTools.NormalizePool(ref playersUI, playerBehaviourPrefab, initiativeTrackHolder, playersUI.Count + 1);
				}
				actorsUI.Add(playersUI[i]);
				playersUI[i].transform.SetSiblingIndex(i + j);
				i++;
			}
			else if (actor.IsEnemyByDefault() && !actor.IsPropActorByDefault(checkAttachedToProp: false))
			{
				if (j >= enemiesUI.Count)
				{
					NormalizeEnemiesUiPool(enemiesUI.Count + 1);
				}
				actorsUI.Add(enemiesUI[j]);
				enemiesUI[j].transform.SetSiblingIndex(i + j);
				j++;
			}
		}
		for (; i < playersUI.Count; i++)
		{
			playersUI[i].gameObject.SetActive(value: false);
		}
		for (; j < enemiesUI.Count; j++)
		{
			enemiesUI[j].gameObject.SetActive(value: false);
		}
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			item.SetDeadState(value: false);
			item.gameObject.SetActive(value: false);
		}
	}

	public void UpdateInitiativeTrack(List<CActor> actors, bool playersSelectable, bool enemiesSelectable, bool selectActor, bool generateCard = true)
	{
		List<CActor> list = new List<CActor>();
		List<CClass> list2 = new List<CClass>();
		foreach (CActor actor in actors)
		{
			if (!actor.IsHeroSummonByDefault() && !actor.IsPropActorByDefault(checkAttachedToProp: false) && ScenarioManager.Scenario.HasActor(actor) && !list2.Contains(actor.Class))
			{
				list.Add(actor);
				list2.Add(actor.Class);
			}
		}
		if (ScenarioManager.Scenario != null)
		{
			list.AddRange(Enumerable.Reverse(ScenarioManager.Scenario.ExhaustedPlayers));
		}
		NormalizeActorsPool(list);
		int num = 0;
		bool flag = false;
		Debug.Log("UpdateInitiativeTrack selectActor " + selectActor + " isOverridingCurrentActor " + GameState.OverridingCurrentActor + " currentActor " + Choreographer.s_Choreographer.m_CurrentActor?.GetPrefabName() + " " + Choreographer.s_Choreographer.m_CurrentActor?.ID + " " + list.Contains(Choreographer.s_Choreographer.m_CurrentActor));
		foreach (CActor item in list)
		{
			actorsUI[num].gameObject.SetActive(value: true);
			if (item.IsDeadPlayer)
			{
				actorsUI[num].SetDeadState(value: true);
				actorsUI[num].SetAttributesDirect(item, activeIDBackplate: true, activeID: true, item.ID.ToString(), activeInitiative: false, item.Initiative(), 0, 0, isActive: true, activeHilight: false, activeButton: false);
				actorsUI[num].Avatar.SetGrayscale(active: true);
			}
			else
			{
				actorsUI[num].SetAttributes(item, (playersSelectable && item.IsPlayerByDefault()) || (enemiesSelectable && item.IsEnemyByDefault()), changeHilight: true, generateCard);
				if (PhaseManager.PhaseType != CPhase.PhaseType.EndRound && selectActor && item == Choreographer.s_Choreographer.m_CurrentActor && !GameState.OverridingCurrentActor)
				{
					Debug.Log("Select actor " + item.ID);
					flag = true;
					Select(actorsUI[num]);
				}
				actorsUI[num].Avatar.SetGrayscale((PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection || PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.EndTurn) && !flag);
			}
			num++;
		}
		if (controllerArea.IsEnabled)
		{
			CheckShowControllerTip();
		}
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
		ReorderControllerTips();
	}

	public void UpdateSelectable(bool playersSelectable, bool enemiesSelectable)
	{
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			item.SetAttributes(item.Actor, (playersSelectable && item.Actor.IsPlayerByDefault()) || (enemiesSelectable && item.Actor.IsEnemyByDefault()), changeHilight: false);
		}
	}

	private void NormalizeEnemiesUiPool(int newLength)
	{
		HelperTools.NormalizePool(ref enemiesUI, enemyBehaviourPrefab, initiativeTrackHolder, newLength, delegate(InitiativeTrackEnemyBehaviour ui)
		{
			ui.Init(enemyCardsHolder, RebuildLayoutGroupImmediately, _initiativeTrackConfig);
		});
	}

	private void UpdateSortingOrder(bool updateDisplay)
	{
		actorsUI.Sort();
		if (!updateDisplay)
		{
			return;
		}
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			item.transform.SetAsFirstSibling();
		}
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
	}

	private void UpdateSortingOrderByPlayerType()
	{
		bool flag = true;
		actorsUI.Sort(PlayerTypeComparer);
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			if (item.ActorType() == CActor.EType.Enemy)
			{
				flag = false;
			}
			else if (item.ActorType() == CActor.EType.Player && !flag)
			{
				flag = true;
				if (InputManager.GamePadInUse)
				{
					previousControllerTip.gameObject.SetActive(value: false);
					previousControllerTip.transform.SetAsFirstSibling();
					nextControllerTip.gameObject.SetActive(value: false);
					nextControllerTip.transform.SetAsFirstSibling();
				}
			}
			item.transform.SetAsFirstSibling();
		}
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
	}

	public void UpdateInitiativeTrack(List<GameObject> players, List<GameObject> enemies, bool playersSelectable, bool enemiesSelectable, bool forceSelectFirstActor = false)
	{
		List<CActor> list = new List<CActor>();
		foreach (GameObject player in players)
		{
			list.Add(ActorBehaviour.GetActor(player));
		}
		foreach (GameObject enemy in enemies)
		{
			list.Add(ActorBehaviour.GetActor(enemy));
		}
		UpdateInitiativeTrack(list, playersSelectable, enemiesSelectable, selectActor: true);
		UpdateSortingOrder(updateDisplay: true);
		if (forceSelectFirstActor && playersUI.Count > 0)
		{
			InitiativeTrackPlayerBehaviour actorUI = playersUI[0];
			if (FFSNetwork.IsOnline)
			{
				foreach (InitiativeTrackPlayerBehaviour item in playersUI)
				{
					if (item.Actor.IsUnderMyControl)
					{
						actorUI = item;
						break;
					}
				}
			}
			Select(actorUI);
		}
		ShowSeparatorActorsByType();
	}

	private void UpdateActorsList()
	{
		actorsUI.Clear();
		actorsUI.AddRange(playersUI);
		actorsUI.AddRange(enemiesUI);
	}

	public void ShowMonsterClassesForSelectingRoundAbilityCards(List<GameObject> clientEnemies)
	{
		if (InputManager.GamePadInUse)
		{
			InputManager.RequestDisableInput(this, KeyAction.CONTROL_INITIATVE_TRACK);
		}
		enemyCardsBlocker.raycastTarget = true;
		List<CMonsterClass> list = new List<CMonsterClass>();
		List<CEnemyActor> list2 = new List<CEnemyActor>();
		foreach (GameObject clientEnemy in clientEnemies)
		{
			CEnemyActor enemyActor = (CEnemyActor)ActorBehaviour.GetActor(clientEnemy);
			if (list.Find((CMonsterClass x) => x == enemyActor.MonsterClass) == null && enemyActor.MonsterClass.RoundAbilityCard != null)
			{
				list.Add(enemyActor.MonsterClass);
				list2.Add(enemyActor);
			}
		}
		NormalizeEnemiesUiPool(list.Count);
		UpdateActorsList();
		if (controllerArea.IsEnabled)
		{
			CheckShowControllerTip();
		}
		foreach (InitiativeTrackPlayerBehaviour item in playersUI)
		{
			item.Deselect();
			if (FFSNetwork.IsOnline)
			{
				item.RefreshInitiative();
			}
		}
		int num = 0;
		foreach (CMonsterClass item2 in list)
		{
			_ = item2;
			enemiesUI[num].SetAttributesDirect(list2[num], activeIDBackplate: false, activeID: false, "", activeInitiative: true, -1, 0, 0, isActive: true, activeHilight: true, activeButton: true);
			num++;
		}
		EnemyAbilityCardsAnimation();
	}

	public void CheckRoundAbilityCardsOrLongRestSelected()
	{
		CPhase currentPhase = PhaseManager.CurrentPhase;
		if ((currentPhase != null && currentPhase.Type == CPhase.PhaseType.MonsterClassesSelectAbilityCards) || (Choreographer.s_Choreographer.LastMessage != null && Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ChoosePlayerActorToBurnCardToPreventDamage))
		{
			return;
		}
		if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
		{
			Singleton<UIReadyToggle>.Instance?.SetInteractable(interactable: false);
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
			return;
		}
		bool flag = IsCardSelectionReady();
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance?.SetInteractable(flag);
		}
		else
		{
			Choreographer.s_Choreographer.readyButton.SetInteractable(flag);
		}
		if (flag)
		{
			if (!Choreographer.s_Choreographer.IsShowedHelpBox)
			{
				helpBox.Hide();
			}
			return;
		}
		CPhase currentPhase2 = PhaseManager.CurrentPhase;
		if (currentPhase2 != null && currentPhase2.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			helpBox.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_START_TURN_SELECT_CARDS"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)));
		}
	}

	private bool IsCardSelectionReady()
	{
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			if (item.Actor is CPlayerActor playerActor && !playerActor.IsCardSelectionReady())
			{
				return false;
			}
		}
		if (FFSNetwork.IsOnline)
		{
			if (PlayerRegistry.HasPlayerControllables())
			{
				return PlayerRegistry.AllPlayers.Find((NetworkPlayer x) => !x.IsParticipant) == null;
			}
			return false;
		}
		return !CardsHandManager.Instance.CurrentHand.IsShortRestSelected();
	}

	public void OnActiveBonusTriggered(CActor actor, CActiveBonus activeBonus)
	{
		actorsUI.FirstOrDefault((InitiativeTrackActorBehaviour s) => s.Actor == actor && ScenarioManager.Scenario.HasActor(s.Actor))?.OnActiveBonusTriggered(activeBonus);
	}

	public void RefreshActorUI(CActor actor)
	{
		actorsUI.FirstOrDefault((InitiativeTrackActorBehaviour s) => s.Actor == actor && ScenarioManager.Scenario.HasActor(s.Actor))?.RefreshAbilities();
	}

	public void SetActiveBonusInteractable(bool interactable)
	{
		foreach (InitiativeTrackActorBehaviour item in actorsUI)
		{
			item.Avatar.IsActiveBonusInteractable = interactable;
		}
	}

	public void RefreshCharactersController()
	{
		actorsUI.ForEach(delegate(InitiativeTrackActorBehaviour s)
		{
			if (s is InitiativeTrackPlayerBehaviour initiativeTrackPlayerBehaviour && (ScenarioManager.Scenario.HasActor(s.Actor) || s.Actor.IsDeadPlayer))
			{
				initiativeTrackPlayerBehaviour.RefreshPlayerController();
			}
		});
	}

	public void RefreshCharacterController(string characterID)
	{
		InitiativeTrackActorBehaviour initiativeTrackActorBehaviour = actorsUI.FirstOrDefault((InitiativeTrackActorBehaviour it) => it is InitiativeTrackPlayerBehaviour && (ScenarioManager.Scenario.HasActor(it.Actor) || it.Actor.IsDeadPlayer) && ((CPlayerActor)it.Actor).CharacterClass.CharacterID == characterID);
		if (initiativeTrackActorBehaviour != null)
		{
			((InitiativeTrackPlayerBehaviour)initiativeTrackActorBehaviour).RefreshPlayerController();
		}
	}

	private void OnControllerAreaFocused()
	{
		Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_INITIATIVE_TRACK"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.CONTROL_INITIATVE_TRACK)), LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_INITIATIVE_TRACK_TITLE"));
		if (!isAnimating)
		{
			EnableNavigation();
		}
	}

	private void ControllerFocus()
	{
		ControllerInputAreaManager.Instance.FocusArea(controllerArea.Id);
	}

	private void EnableNavigation()
	{
		List<InitiativeTrackActorBehaviour> list = actorsUI.Where((InitiativeTrackActorBehaviour it) => it.gameObject.activeSelf).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Button right = ((num == 0) ? list.Last().avatarButton : list[num - 1].avatarButton);
			Button avatarButton = list[(num + 1) % list.Count].avatarButton;
			list[num].EnableNavigation(avatarButton, right);
		}
		EventSystem.current.SetSelectedGameObject(list.LastOrDefault()?.avatarButton?.gameObject);
	}

	public void GoToPrevious()
	{
		List<InitiativeTrackActorBehaviour> list = actorsUI.Where((InitiativeTrackActorBehaviour it) => it.gameObject.activeSelf && !it.IsDeadState).ToList();
		int index = (_currentElementIndex = ((_currentElementIndex == 0) ? list.IndexOf(list.Last()) : list.IndexOf(list[_currentElementIndex - 1])));
		EventSystem.current.SetSelectedGameObject(list.ElementAt(index).avatarButton.gameObject);
	}

	public void GoToNext()
	{
		List<InitiativeTrackActorBehaviour> list = actorsUI.Where((InitiativeTrackActorBehaviour it) => it.gameObject.activeSelf && !it.IsDeadState).ToList();
		int index = (_currentElementIndex = (_currentElementIndex + 1) % list.Count);
		EventSystem.current.SetSelectedGameObject(list.ElementAt(index).avatarButton.gameObject);
	}

	public void SelectFirst()
	{
		_currentElementIndex = 0;
		List<InitiativeTrackActorBehaviour> source = actorsUI.Where((InitiativeTrackActorBehaviour it) => it.gameObject.activeSelf && !it.IsDeadState).ToList();
		EventSystem.current.SetSelectedGameObject(source.ElementAt(_currentElementIndex).avatarButton.gameObject);
	}

	public void Clear()
	{
		_currentElementIndex = 0;
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void OnControllerAreaUnfocused()
	{
		Singleton<HelpBox>.Instance.ClearOverrideController();
		for (int i = 0; i < actorsUI.Count; i++)
		{
			actorsUI[i].DisableNavigation();
		}
		EventSystem.current.SetSelectedGameObject(null);
	}
}
