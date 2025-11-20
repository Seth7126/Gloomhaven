#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chronos;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.MapState;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LevelEventsController : Singleton<LevelEventsController>
{
	public static LevelEventsController s_Instance;

	private bool m_CurrentlyActive;

	private CCustomLevelData m_CurrentlyPlayedLevel;

	private CMapScenarioState m_CurrentlyPlayedMapState;

	private List<CLevelMessage> m_MessagesToShow = new List<CLevelMessage>();

	private List<CLevelMessage> m_AlreadyShownMessages = new List<CLevelMessage>();

	private List<CLevelEvent> m_LevelEventsToShow = new List<CLevelEvent>();

	private List<CLevelEvent> m_AlreadyShowLevelEvents = new List<CLevelEvent>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_CustomWinObjectivesToTrigger = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_AlreadyTriggeredCustomWinObjectives = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_CustomLoseObjectivesToTrigger = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_AlreadyTriggeredCustomLoseObjectives = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<SEvent> m_SEventsPostedToProcess = new List<SEvent>();

	private List<UIEvent> m_UIEventsPostedToProcess = new List<UIEvent>();

	private IEnumerator m_SEventRetreivalRoutine;

	private IEnumerator m_EventProcessingRoutine;

	private Dictionary<CLevelMessage, UnityAction> m_PendingInteractionLimiting = new Dictionary<CLevelMessage, UnityAction>();

	private UnityAction<CLevelMessage> m_ActionForNextMessageDismissal;

	private int m_LastFrameSEventCount;

	private bool m_DebugLog;

	private string m_ShortRestCardDataPending;

	public GameObject StoryControllerPrefab;

	private GameObject m_StoryControllerSceneObject;

	public static bool s_EventsControllerActive
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.m_CurrentlyActive;
			}
			return false;
		}
	}

	public int NumberOfAlreadyDisplayedMessages => m_AlreadyShownMessages.Count;

	public bool IsBusy
	{
		get
		{
			if ((m_UIEventsPostedToProcess?.Count ?? 0) <= 0)
			{
				return (m_SEventsPostedToProcess?.Count ?? 0) > 0;
			}
			return true;
		}
	}

	[UsedImplicitly]
	protected override void Awake()
	{
		base.Awake();
		s_Instance = this;
		if (ScenarioManager.CurrentScenarioState != null)
		{
			SEventLog.ClearEventLog();
		}
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		InteractabilityManager.s_Instance?.LoadProfile(null);
		UIEventManager.OnEventLogged -= UIEventLogged;
		s_Instance = null;
		base.OnDestroy();
	}

	public void Start()
	{
		m_CurrentlyPlayedLevel = SaveData.Instance.Global.CurrentCustomLevelData;
		if (m_CurrentlyPlayedLevel == null)
		{
			m_CurrentlyPlayedLevel = SaveData.Instance.Global.CurrentAutoTestDataCopy;
		}
		bool num = m_CurrentlyPlayedLevel != null && m_CurrentlyPlayedLevel.HasScriptedEvents;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState != null)
		{
			m_CurrentlyPlayedMapState = SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState;
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState != null && SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.HasPredefinedMessages)
		{
			m_CurrentlyPlayedMapState = SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
		}
		else
		{
			m_CurrentlyPlayedMapState = null;
		}
		if (!num)
		{
			m_CurrentlyPlayedLevel = null;
		}
		if (num || m_CurrentlyPlayedMapState != null)
		{
			m_CurrentlyActive = true;
			Debug.Log("[LevelEventsController] - set to active");
			UIEventManager.OnEventLogged += UIEventLogged;
			StartListeningForEvents();
		}
	}

	public void StartListeningForEvents()
	{
		m_SEventsPostedToProcess.Clear();
		m_UIEventsPostedToProcess.Clear();
		m_MessagesToShow.Clear();
		m_AlreadyShownMessages.Clear();
		m_LevelEventsToShow.Clear();
		m_AlreadyShowLevelEvents.Clear();
		m_CustomWinObjectivesToTrigger.Clear();
		m_AlreadyTriggeredCustomWinObjectives.Clear();
		m_CustomLoseObjectivesToTrigger.Clear();
		m_AlreadyTriggeredCustomLoseObjectives.Clear();
		m_LastFrameSEventCount = 0;
		m_ActionForNextMessageDismissal = null;
		if (m_SEventRetreivalRoutine != null)
		{
			StopCoroutine(m_SEventRetreivalRoutine);
			m_SEventRetreivalRoutine = null;
		}
		m_SEventRetreivalRoutine = RetrieveEventsPostedSinceLastFrame();
		StartCoroutine(m_SEventRetreivalRoutine);
		if (m_EventProcessingRoutine != null)
		{
			StopCoroutine(m_EventProcessingRoutine);
			m_EventProcessingRoutine = null;
		}
		m_EventProcessingRoutine = ProcessEventsLogged();
		StartCoroutine(m_EventProcessingRoutine);
		if (m_CurrentlyPlayedLevel != null)
		{
			if (m_CurrentlyPlayedLevel.LevelMessages != null && m_CurrentlyPlayedLevel.LevelMessages.Count > 0)
			{
				m_MessagesToShow = m_CurrentlyPlayedLevel.LevelMessages.ToList();
			}
			if (m_CurrentlyPlayedLevel.LevelEvents != null && m_CurrentlyPlayedLevel.LevelEvents.Count > 0)
			{
				m_LevelEventsToShow = m_CurrentlyPlayedLevel.LevelEvents.ToList();
			}
			for (int i = 0; i < m_CurrentlyPlayedLevel.ScenarioState.WinObjectives.Count; i++)
			{
				if (m_CurrentlyPlayedLevel.ScenarioState.WinObjectives[i].ObjectiveType == EObjectiveType.CustomTrigger)
				{
					m_CustomWinObjectivesToTrigger.Add(new Tuple<CObjective_CustomTrigger, int>(m_CurrentlyPlayedLevel.ScenarioState.WinObjectives[i] as CObjective_CustomTrigger, i));
				}
			}
			for (int j = 0; j < m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives.Count; j++)
			{
				if (m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives[j].ObjectiveType == EObjectiveType.CustomTrigger)
				{
					m_CustomLoseObjectivesToTrigger.Add(new Tuple<CObjective_CustomTrigger, int>(m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives[j] as CObjective_CustomTrigger, j));
				}
			}
		}
		if (m_CurrentlyPlayedMapState == null)
		{
			return;
		}
		if (m_CurrentlyPlayedMapState.ScenarioStartMessage != null)
		{
			m_MessagesToShow.Add(m_CurrentlyPlayedMapState.ScenarioStartMessage);
		}
		if (m_CurrentlyPlayedMapState.ScenarioCompleteMessage != null)
		{
			m_MessagesToShow.Add(m_CurrentlyPlayedMapState.ScenarioCompleteMessage);
		}
		if (m_CurrentlyPlayedMapState.ScenarioRoomRevealMessages != null)
		{
			m_MessagesToShow.AddRange(m_CurrentlyPlayedMapState.ScenarioRoomRevealMessages);
		}
		if (SaveData.Instance.Global.GameMode != EGameMode.Campaign)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		List<string> list = new List<string>();
		while (!flag)
		{
			num++;
			string text = string.Format(m_CurrentlyPlayedMapState.LocalisedStartKey, num);
			if (LocalizationManager.TryGetTranslation(text, out var _))
			{
				list.Add(text);
			}
			else
			{
				flag = true;
			}
		}
		if (list.Count > 0)
		{
			m_MessagesToShow.Add(CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioStart));
		}
		flag = false;
		num = 0;
		list.Clear();
		while (!flag)
		{
			num++;
			string text2 = string.Format(m_CurrentlyPlayedMapState.LocalisedSuccessKey, num);
			if (LocalizationManager.TryGetTranslation(text2, out var _))
			{
				list.Add(text2);
			}
			else
			{
				flag = true;
			}
		}
		if (list.Count > 0)
		{
			m_MessagesToShow.Add(CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioEnd));
		}
		foreach (CMap map in m_CurrentlyPlayedMapState.CurrentState.Maps)
		{
			flag = false;
			num = 0;
			list.Clear();
			while (!flag)
			{
				num++;
				string text3 = string.Format(m_CurrentlyPlayedMapState.LocalisedOpenRoomKey, map.RoomName, num);
				if (LocalizationManager.TryGetTranslation(text3, out var _))
				{
					list.Add(text3);
				}
				else
				{
					flag = true;
				}
			}
			if (list.Count > 0)
			{
				m_MessagesToShow.Add(CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.RoomRevealed, map.MapGuid));
			}
		}
	}

	private void UIEventLogged(UIEvent eventToProcess)
	{
		if (!m_CurrentlyActive)
		{
			Debug.LogWarningFormat("LevelEventsController received UIEvent while it shouldn't have. EventType received:{0}", eventToProcess.EventType.ToString());
		}
		else
		{
			m_UIEventsPostedToProcess.Add(eventToProcess);
		}
	}

	private void DebugLogSEvent(SEvent eventToLog)
	{
		string text = $"LEVEL EVENTS CONTROLLER SEVENT OF TYPE [{eventToLog.Type.ToString()}] FROM ROUND [{eventToLog.Round}] PROCESSED IN ROUND [{ScenarioManager.CurrentScenarioState.RoundNumber}]:";
		switch (eventToLog.Type)
		{
		case ESEType.Internal:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventInternal).SubType;
			break;
		case ESEType.Phase:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventPhase).SubType;
			text = text + "\nPHASE: " + (eventToLog as SEventPhase).SubTypePhase;
			break;
		case ESEType.Actor:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventActor).ActorSubType;
			text = text + "\nACTOR: " + (eventToLog as SEventActor).ActorClassID;
			break;
		case ESEType.Item:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventItem).ItemSubType;
			text = text + "\nITEM: " + (eventToLog as SEventItem).ItemType;
			break;
		case ESEType.Action:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventAction).SubTypeAction;
			text = text + "\nTEXT: " + (eventToLog as SEventAction).Text;
			break;
		case ESEType.Ability:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventAbility).AbilitySubType;
			text = text + "\nABILITY: " + (eventToLog as SEventAbility).AbilityType;
			if ((eventToLog as SEventAbility).AbilityType == CAbility.EAbilityType.Move)
			{
				text = text + "\nMOVESTATE: " + (eventToLog as SEventAbilityMove).MoveState;
			}
			else if (eventToLog is SEventAbilityAttack sEventAbilityAttack)
			{
				text = text + "\nATTACKSTATE: " + sEventAbilityAttack.AttackState;
			}
			break;
		case ESEType.AttackModifier:
			text = text + "\nTEXT: " + (eventToLog as SEventAttackModifier).Text;
			break;
		}
	}

	private void DebugLogUIEvent(UIEvent eventToLog)
	{
		string text = "LEVEL EVENTS CONTROLLER UIEVENT PROCESSED IN ROUND [" + eventToLog.RoundInt + "]:\nTYPE:" + eventToLog.EventType.ToString() + "\nPHASE:" + eventToLog.CurrentPhaseType.ToString() + "\nCURRENT PHASE ACTOR:" + eventToLog.CurrentPhaseActorName;
		switch (eventToLog.EventType)
		{
		case UIEvent.EUIEventType.AbilityCardSelected:
		case UIEvent.EUIEventType.CardTopHalfSelected:
		case UIEvent.EUIEventType.CardBottomHalfSelected:
			text = text + "\nABILITY: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.LevelMessageDismissed:
			text = text + "\nMESSAGE NAME: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.TileSelectedForAbility:
			text = text + "\nABILITY: " + eventToLog.ContextID;
			text = text + "\nTILE: X: " + eventToLog.TileForEvent.X + " Y:" + eventToLog.TileForEvent.Y;
			break;
		case UIEvent.EUIEventType.InitiativeAvatarHovered:
			text = text + "\nACTOR HOVERED: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.ConsumeElementCardTop:
		case UIEvent.EUIEventType.ConsumeElementCardBottom:
			text = text + "\nABILITY: " + eventToLog.ContextID + ((eventToLog.EventType == UIEvent.EUIEventType.ConsumeElementCardTop) ? " [TOP]" : " [BOTTOM]");
			text = text + "\nINDEX: " + ((eventToLog.ContextInt < 0) ? "ANY" : eventToLog.ContextInt.ToString());
			break;
		}
	}

	private IEnumerator RetrieveEventsPostedSinceLastFrame()
	{
		while (m_CurrentlyActive)
		{
			try
			{
				if (ScenarioManager.CurrentScenarioState != null)
				{
					int count = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Count;
					if (count == 0)
					{
						m_LastFrameSEventCount = 0;
					}
					if (count > m_LastFrameSEventCount)
					{
						m_SEventsPostedToProcess.AddRange(ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.GetRange(m_LastFrameSEventCount, count - m_LastFrameSEventCount));
						m_LastFrameSEventCount = count;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error in SEvent retreival code:" + ex.Message);
			}
			yield return null;
		}
	}

	private IEnumerator ProcessEventsLogged()
	{
		while (m_CurrentlyActive)
		{
			bool eventProcessed = false;
			if (m_SEventsPostedToProcess.Count > 0)
			{
				if (m_SEventsPostedToProcess[0] != null)
				{
					ProcessEvent(m_SEventsPostedToProcess[0]);
				}
				m_SEventsPostedToProcess.RemoveAt(0);
				eventProcessed = true;
				yield return null;
			}
			if (m_UIEventsPostedToProcess.Count > 0)
			{
				if (m_UIEventsPostedToProcess[0] != null)
				{
					ProcessEvent(null, m_UIEventsPostedToProcess[0]);
				}
				m_UIEventsPostedToProcess.RemoveAt(0);
				eventProcessed = true;
				yield return null;
			}
			if (!eventProcessed)
			{
				yield return null;
			}
		}
	}

	private void ProcessEvent(SEvent SEventToProcess = null, UIEvent UIEventToProcess = null)
	{
		bool flag = SEventToProcess != null;
		if (m_DebugLog)
		{
			if (flag)
			{
				DebugLogSEvent(SEventToProcess);
			}
			else
			{
				DebugLogUIEvent(UIEventToProcess);
			}
		}
		if (LevelMessagesUIHandler.s_Instance?.CurrentlyDisplayedBoxMessage != null)
		{
			CActor contextActor = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedBoxMessage.DismissTrigger, out contextActor) : ShouldEventCauseTrigger(UIEventToProcess, LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedBoxMessage.DismissTrigger, out contextActor))
			{
				LevelMessagesUIHandler.s_Instance.HideCurrentlyShownBoxMessage();
			}
		}
		if (LevelMessagesUIHandler.s_Instance?.CurrentlyDisplayedHelpTextMessage != null)
		{
			CActor contextActor2 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedHelpTextMessage.DismissTrigger, out contextActor2) : ShouldEventCauseTrigger(UIEventToProcess, LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedHelpTextMessage.DismissTrigger, out contextActor2))
			{
				LevelMessagesUIHandler.s_Instance.HideCurrentlyShownHelpTextMessage();
			}
		}
		List<CLevelMessage> list = new List<CLevelMessage>();
		foreach (CLevelMessage item in m_MessagesToShow)
		{
			CActor contextActor3 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item.DisplayTrigger, out contextActor3) : ShouldEventCauseTrigger(UIEventToProcess, item.DisplayTrigger, out contextActor3))
			{
				list.Add(item);
			}
		}
		foreach (CLevelMessage item2 in list)
		{
			ShowLevelMessage(item2);
			m_AlreadyShownMessages.Add(item2);
			m_MessagesToShow.Remove(item2);
		}
		List<Tuple<CLevelEvent, CActor>> list2 = new List<Tuple<CLevelEvent, CActor>>();
		foreach (CLevelEvent item3 in m_LevelEventsToShow)
		{
			CActor contextActor4 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item3.DisplayTrigger, out contextActor4) : ShouldEventCauseTrigger(UIEventToProcess, item3.DisplayTrigger, out contextActor4))
			{
				list2.Add(new Tuple<CLevelEvent, CActor>(item3, contextActor4));
			}
		}
		foreach (Tuple<CLevelEvent, CActor> item4 in list2)
		{
			RunLevelEvent(item4.Item1, item4.Item2);
			if (!m_AlreadyShowLevelEvents.Contains(item4.Item1))
			{
				m_AlreadyShowLevelEvents.Add(item4.Item1);
			}
			if (!item4.Item1.Repeats)
			{
				m_LevelEventsToShow.Remove(item4.Item1);
			}
		}
		List<Tuple<CObjective_CustomTrigger, int>> list3 = new List<Tuple<CObjective_CustomTrigger, int>>();
		foreach (Tuple<CObjective_CustomTrigger, int> item5 in m_CustomWinObjectivesToTrigger)
		{
			CActor contextActor5 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item5.Item1.CustomTrigger, out contextActor5) : ShouldEventCauseTrigger(UIEventToProcess, item5.Item1.CustomTrigger, out contextActor5))
			{
				list3.Add(item5);
			}
		}
		bool flag2 = false;
		foreach (Tuple<CObjective_CustomTrigger, int> item6 in list3)
		{
			SetCustomObjectiveTriggered(item6, isWinObjective: true);
			flag2 = true;
			m_AlreadyTriggeredCustomWinObjectives.Add(item6);
			m_CustomWinObjectivesToTrigger.Remove(item6);
		}
		List<Tuple<CObjective_CustomTrigger, int>> list4 = new List<Tuple<CObjective_CustomTrigger, int>>();
		foreach (Tuple<CObjective_CustomTrigger, int> item7 in m_CustomLoseObjectivesToTrigger)
		{
			CActor contextActor6 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item7.Item1.CustomTrigger, out contextActor6) : ShouldEventCauseTrigger(UIEventToProcess, item7.Item1.CustomTrigger, out contextActor6))
			{
				list4.Add(item7);
			}
		}
		foreach (Tuple<CObjective_CustomTrigger, int> item8 in list4)
		{
			SetCustomObjectiveTriggered(item8, isWinObjective: false);
			flag2 = true;
			m_AlreadyTriggeredCustomLoseObjectives.Add(item8);
			m_CustomLoseObjectivesToTrigger.Remove(item8);
		}
		if (flag2)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	private bool ShouldEventCauseTrigger(SEvent eventToCheck, CLevelTrigger triggerToCheck, out CActor contextActor)
	{
		contextActor = null;
		if (triggerToCheck.IsUIEventTypeTrigger)
		{
			return false;
		}
		if (triggerToCheck.IsTriggeredByDismiss)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerRound != 0 && triggerToCheck.EventTriggerRound != eventToCheck.Round)
		{
			return false;
		}
		if (eventToCheck.Type != ESEType.Actor && !string.IsNullOrEmpty(triggerToCheck.EventTriggerActorGuid) && !string.IsNullOrEmpty(eventToCheck.CurrentPhaseActorGuid) && !string.Equals(triggerToCheck.EventTriggerActorGuid, eventToCheck.CurrentPhaseActorGuid, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!CheckRequiredMessagesPlayed(triggerToCheck))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventAliveActorGUID))
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(triggerToCheck.EventAliveActorGUID);
			if (cActor == null || cActor.IsDead)
			{
				return false;
			}
		}
		if (eventToCheck.Type == (ESEType)triggerToCheck.EventTriggerTypeInt)
		{
			try
			{
				switch (eventToCheck.Type)
				{
				case ESEType.Internal:
					if ((eventToCheck as SEventInternal).SubType == (ESESubTypeInternal)triggerToCheck.EventTriggerSubTypeInt)
					{
						return true;
					}
					return false;
				case ESEType.Phase:
					if (((eventToCheck as SEventPhase).SubTypePhase == (ESESubTypePhase)triggerToCheck.EventTriggerSubTypeInt || triggerToCheck.EventTriggerSubTypeInt == 0) && ((eventToCheck as SEventPhase).SubType == (CPhase.PhaseType)triggerToCheck.EventTriggerContextTypeInt || triggerToCheck.EventTriggerContextTypeInt == 9))
					{
						return true;
					}
					return false;
				case ESEType.Item:
					if ((eventToCheck as SEventItem).ItemSubType == (ESESubTypeItem)triggerToCheck.EventTriggerSubTypeInt && string.Equals((eventToCheck as SEventItem).ItemStringID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					return false;
				case ESEType.Action:
					if ((eventToCheck as SEventAction).SubTypeAction == (ESESubTypeAction)triggerToCheck.EventTriggerSubTypeInt)
					{
						return true;
					}
					return false;
				case ESEType.Ability:
					if ((eventToCheck as SEventAbility).AbilitySubType == (ESESubTypeAbility)triggerToCheck.EventTriggerSubTypeInt && (eventToCheck as SEventAbility).AbilityType == (CAbility.EAbilityType)triggerToCheck.EventTriggerContextTypeInt)
					{
						if ((eventToCheck as SEventAbility).AbilityType == CAbility.EAbilityType.Move)
						{
							if (triggerToCheck.EventTriggerContextSubTypeInt == 0 || (eventToCheck as SEventAbilityMove).MoveState == (CAbilityMove.EMoveState)triggerToCheck.EventTriggerContextSubTypeInt)
							{
								return true;
							}
							return false;
						}
						if ((eventToCheck as SEventAbility).AbilityType == CAbility.EAbilityType.Attack)
						{
							if (triggerToCheck.EventTriggerContextSubTypeInt == 0 || (eventToCheck as SEventAbilityAttack).AttackState == (CAbilityAttack.EAttackState)triggerToCheck.EventTriggerContextSubTypeInt)
							{
								return true;
							}
							return false;
						}
						return true;
					}
					return false;
				case ESEType.Actor:
					if ((eventToCheck as SEventActor).ActorSubType == (ESESubTypeActor)triggerToCheck.EventTriggerSubTypeInt && (string.IsNullOrEmpty(triggerToCheck.EventTriggerActorGuid) || (eventToCheck as SEventActor).ActorGuid == triggerToCheck.EventTriggerActorGuid))
					{
						return true;
					}
					return false;
				case ESEType.AttackModifier:
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception matching TutorialMessageTrigger to SEvent[" + eventToCheck.Type.ToString() + "]:\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		return false;
	}

	private bool ShouldEventCauseTrigger(UIEvent eventToCheck, CLevelTrigger triggerToCheck, out CActor contextActor)
	{
		contextActor = null;
		if (!triggerToCheck.IsUIEventTypeTrigger)
		{
			return false;
		}
		if (triggerToCheck.IsTriggeredByDismiss)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerRound != 0 && triggerToCheck.EventTriggerRound != eventToCheck.RoundInt)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerContextTypeInt != 9 && eventToCheck.CurrentPhaseType != CPhase.PhaseType.None && triggerToCheck.EventTriggerContextTypeInt != (int)eventToCheck.CurrentPhaseType)
		{
			return false;
		}
		if (eventToCheck.EventType != UIEvent.EUIEventType.InitiativeAvatarHovered && !string.IsNullOrEmpty(triggerToCheck.EventTriggerActorName) && !string.IsNullOrEmpty(eventToCheck.CurrentPhaseActorName) && !string.Equals(triggerToCheck.EventTriggerActorName, eventToCheck.CurrentPhaseActorName, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!CheckRequiredMessagesPlayed(triggerToCheck))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(eventToCheck.ContextActorGUID))
		{
			contextActor = ScenarioManager.Scenario.FindActor(eventToCheck.ContextActorGUID);
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventAliveActorGUID))
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(triggerToCheck.EventAliveActorGUID);
			if (cActor == null || cActor.IsDead)
			{
				return false;
			}
		}
		if (eventToCheck.EventType == UIEvent.EUIEventType.ActorEndedTurnOnTile && triggerToCheck.EventTriggerTile != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == eventToCheck.ContextID);
			if (actorState == null || triggerToCheck.EventFilter == null || triggerToCheck.EventFilter.IsValidTarget(actorState))
			{
				if (triggerToCheck.EventTriggerTypeInt == 32)
				{
					if (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)
					{
						return true;
					}
					return false;
				}
				if (triggerToCheck.EventTriggerTypeInt == 33)
				{
					if (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)
					{
						return false;
					}
					return true;
				}
			}
		}
		if (eventToCheck.EventType == (UIEvent.EUIEventType)triggerToCheck.EventTriggerTypeInt)
		{
			switch (eventToCheck.EventType)
			{
			case UIEvent.EUIEventType.AbilityCardSelected:
			case UIEvent.EUIEventType.CardTopHalfSelected:
			case UIEvent.EUIEventType.CardBottomHalfSelected:
			case UIEvent.EUIEventType.ActorSelected:
			case UIEvent.EUIEventType.LevelMessageDismissed:
			case UIEvent.EUIEventType.RoomRevealed:
			case UIEvent.EUIEventType.ObjectiveCompleted:
			case UIEvent.EUIEventType.PropActivated:
			case UIEvent.EUIEventType.PropDestroyed:
			case UIEvent.EUIEventType.PropDeactivated:
			case UIEvent.EUIEventType.DoorUnlocked:
				if (string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerContextId) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase) || eventToCheck.ContextID.ToLower().Contains(triggerToCheck.EventTriggerContextId.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty)))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.TileSelectedForAbility:
				if ((eventToCheck.TileForEvent == null || triggerToCheck.EventTriggerTile == null || (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)) && (string.IsNullOrEmpty(eventToCheck.ContextID) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase) || eventToCheck.ContextID.ToLower().Contains(triggerToCheck.EventTriggerContextId.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty))))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.InitiativeAvatarHovered:
				if (string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerActorName) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerActorName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.ConsumeElementCardTop:
			case UIEvent.EUIEventType.ConsumeElementCardBottom:
				if ((string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerContextId) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase)) && (eventToCheck.ContextInt < 0 || triggerToCheck.EventTriggerContextIndex < 0 || eventToCheck.ContextInt == triggerToCheck.EventTriggerContextIndex))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.AllActorsDeadFiltered:
			{
				CObjectiveFilter eventFilter = triggerToCheck.EventFilter;
				if (eventFilter == null)
				{
					return false;
				}
				bool result = true;
				foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
				{
					if ((!(actorState2 is EnemyState enemyState) || enemyState.ConfigPerPartySize[ScenarioManager.CurrentScenarioState.Players.Count] != ScenarioManager.EPerPartySizeConfig.Hidden) && eventFilter.IsValidTarget(actorState2))
					{
						CActor cActor2 = ScenarioManager.FindActor(actorState2.ActorGuid);
						if (cActor2 != null)
						{
							if (!cActor2.IsDead)
							{
								result = false;
								break;
							}
						}
						else if (!actorState2.IsDead)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
			case UIEvent.EUIEventType.ActorEndedTurnOnTile:
			case UIEvent.EUIEventType.ActorDidntEndTurnOnTile:
				return false;
			default:
				return true;
			}
		}
		return false;
	}

	private bool CheckRequiredMessagesPlayed(CLevelTrigger triggerToCheck)
	{
		if (!string.IsNullOrEmpty(triggerToCheck.EventTriggerPlayedMessageReq))
		{
			List<string> list = triggerToCheck.EventTriggerPlayedMessageReq.Split(',').ToList();
			list.TrimAll();
			if (triggerToCheck.EventTriggerPlayedMessageReqOR)
			{
				bool flag = false;
				foreach (string msg in list)
				{
					if (m_AlreadyShownMessages.Any((CLevelMessage m) => string.Equals(m.MessageName, msg, StringComparison.OrdinalIgnoreCase)))
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else
			{
				foreach (string msg2 in list)
				{
					if (!m_AlreadyShownMessages.Any((CLevelMessage m) => string.Equals(m.MessageName, msg2, StringComparison.OrdinalIgnoreCase)))
					{
						return false;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventTriggerNotPlayedMessageReq))
		{
			List<string> list2 = triggerToCheck.EventTriggerNotPlayedMessageReq.Split(',').ToList();
			list2.TrimAll();
			if (triggerToCheck.EventTriggerNotPlayedMessageReqOR)
			{
				bool flag2 = false;
				foreach (string msg3 in list2)
				{
					if (!m_AlreadyShownMessages.Any((CLevelMessage m) => string.Equals(m.MessageName, msg3, StringComparison.OrdinalIgnoreCase)))
					{
						flag2 = true;
					}
					if (flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					return false;
				}
			}
			else
			{
				foreach (string msg4 in list2)
				{
					if (m_AlreadyShownMessages.Any((CLevelMessage m) => string.Equals(m.MessageName, msg4, StringComparison.OrdinalIgnoreCase)))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void ShowLevelMessage(CLevelMessage message)
	{
		StartCoroutine(ShowLevelMessageRoutine(message));
	}

	private IEnumerator ShowLevelMessageRoutine(CLevelMessage message)
	{
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			yield return new WaitUntil(() => TransitionManager.s_Instance.TransitionDone);
		}
		if (message.LayoutType != CLevelMessage.ELevelMessageLayoutType.EmptyMessageFlag)
		{
			if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.StoryDialog)
			{
				Singleton<StoryController>.Instance.Show(message);
			}
			else if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.HelpText)
			{
				LevelMessagesUIHandler.s_Instance.ShowHelpText(message);
			}
			else
			{
				LevelMessagesUIHandler.s_Instance.ShowBoxMessage(message);
			}
		}
	}

	public void CompleteLevel(bool completionSuccess, bool userQuit = false, UnityAction postCompleteAction = null)
	{
		if (!m_CurrentlyActive)
		{
			Debug.LogWarning("Tried to complete null Level");
			return;
		}
		if (m_EventProcessingRoutine != null)
		{
			StopCoroutine(m_EventProcessingRoutine);
			m_EventProcessingRoutine = null;
		}
		if (m_SEventRetreivalRoutine != null)
		{
			StopCoroutine(m_SEventRetreivalRoutine);
			m_SEventRetreivalRoutine = null;
		}
		if (AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			SEventLog.ClearEventLog();
		}
		UnityAction<string> EndEventControllerAction = delegate(string loggingString)
		{
			m_CurrentlyPlayedLevel = null;
			m_CurrentlyActive = false;
			Debug.LogFormat("[LevelEventsController] - set to inactive. REASON: {0}", loggingString);
		};
		bool flag = false;
		if (!userQuit)
		{
			if (completionSuccess)
			{
				CLevelMessage cLevelMessage = m_MessagesToShow.FirstOrDefault((CLevelMessage m) => m.DisplayTrigger.IsUIEventTypeTrigger && m.DisplayTrigger.EventTriggerTypeInt == 30);
				if (cLevelMessage != null && CheckRequiredMessagesPlayed(cLevelMessage.DisplayTrigger))
				{
					ShowLevelMessage(cLevelMessage);
					m_AlreadyShownMessages.Add(cLevelMessage);
					m_MessagesToShow.Remove(cLevelMessage);
					flag = true;
				}
			}
			else
			{
				CLevelMessage cLevelMessage2 = m_MessagesToShow.FirstOrDefault((CLevelMessage m) => m.DisplayTrigger.IsUIEventTypeTrigger && m.DisplayTrigger.EventTriggerTypeInt == 31);
				if (cLevelMessage2 != null && CheckRequiredMessagesPlayed(cLevelMessage2.DisplayTrigger))
				{
					ShowLevelMessage(cLevelMessage2);
					m_AlreadyShownMessages.Add(cLevelMessage2);
					m_MessagesToShow.Remove(cLevelMessage2);
					flag = true;
				}
			}
		}
		if (flag)
		{
			m_ActionForNextMessageDismissal = delegate(CLevelMessage messageDismissed)
			{
				postCompleteAction?.Invoke();
				EndEventControllerAction?.Invoke(string.Format("Ended when dismissing message with MessageName {0}. ScenarioCompletionSuccess:{1} | UserQuitManually {2}", messageDismissed?.MessageName ?? "NULL", completionSuccess.ToString(), userQuit.ToString()));
			};
		}
		else
		{
			postCompleteAction?.Invoke();
			EndEventControllerAction?.Invoke($"Ended via the CompleteLevel() method. ScenarioCompletionSuccess:{completionSuccess.ToString()} | UserQuitManually {userQuit.ToString()}");
			m_ActionForNextMessageDismissal = null;
		}
	}

	public void MessageWasDisplayed(CLevelMessage messageDisplayed, bool fromStoryController = false)
	{
		UnityAction unityAction = delegate
		{
			if (!fromStoryController)
			{
				InteractabilityManager.s_Instance.ConfigureDefaultControlsForMessage(messageDisplayed);
				LevelMessagesUIHandler.s_Instance.ConfigureIsolatedUIControlForBoxMessage(messageDisplayed);
			}
			if (messageDisplayed.InteractabilityProfileForMessage != null && messageDisplayed.InteractabilityProfileForMessage.ControlsToAllow.Count > 0)
			{
				InteractabilityManager.s_Instance.LoadProfile(messageDisplayed.InteractabilityProfileForMessage);
			}
			else if (messageDisplayed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDisplayed.DismissTrigger.IsTriggeredByDismiss)
			{
				InteractabilityManager.s_Instance.LoadDefaultDismissMessageProfile();
			}
		};
		if (InteractabilityManager.s_Instance.CurrentlyLoadedProfileIsNullOrDefault)
		{
			unityAction?.Invoke();
		}
		else
		{
			m_PendingInteractionLimiting.Add(messageDisplayed, unityAction);
		}
		if (messageDisplayed.ShouldPauseGame)
		{
			Main.Pause3DWorld();
		}
		if (messageDisplayed.CameraProfile != null && messageDisplayed.CameraProfile.ShouldForceCamera)
		{
			CameraController.s_CameraController.SetCameraWithMessageProfile(messageDisplayed.CameraProfile);
		}
	}

	public void MessageWasDismissed(CLevelMessage messageDismissed)
	{
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.LevelMessageDismissed, null, messageDismissed.MessageName));
		if ((messageDismissed.InteractabilityProfileForMessage != null && messageDismissed.InteractabilityProfileForMessage.ControlsToAllow.Count > 0) || (messageDismissed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDismissed.DismissTrigger.IsTriggeredByDismiss))
		{
			if (m_CurrentlyPlayedLevel != null && m_CurrentlyPlayedLevel.ShouldPreventUnspecifiedInteraction)
			{
				InteractabilityManager.s_Instance.LoadDefaultMessagelessProfile();
			}
			else
			{
				InteractabilityManager.s_Instance.LoadProfile(null);
			}
		}
		if (messageDismissed.ShouldPauseGame)
		{
			Main.Unpause3DWorld();
		}
		m_ActionForNextMessageDismissal?.Invoke(messageDismissed);
		m_ActionForNextMessageDismissal = null;
		ResolvePendingInteractionProfiles();
	}

	private void ResolvePendingInteractionProfiles()
	{
		if (m_PendingInteractionLimiting.Count == 0)
		{
			return;
		}
		List<CLevelMessage> list = new List<CLevelMessage>();
		foreach (CLevelMessage key in m_PendingInteractionLimiting.Keys)
		{
			list.Add(key);
			if (LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedBoxMessage == key || LevelMessagesUIHandler.s_Instance.CurrentlyDisplayedHelpTextMessage == key)
			{
				m_PendingInteractionLimiting[key]?.Invoke();
				break;
			}
		}
		foreach (CLevelMessage item in list)
		{
			m_PendingInteractionLimiting.Remove(item);
		}
	}

	private void RunLevelEvent(CLevelEvent levelEvent, CActor overrideContextActor = null)
	{
		switch (levelEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.RevealCharacterAfterAnim:
		{
			if (!ScenarioManager.CurrentScenarioState.Players.Any((PlayerState p) => p.ClassID == levelEvent.EventResource))
			{
				break;
			}
			PlayerState playerToReveal = ScenarioManager.CurrentScenarioState.Players.Single((PlayerState p) => p.ClassID == levelEvent.EventResource);
			UnityAction actionToPerform = delegate
			{
				if (LevelEditorController.RevealHiddenCharacterInState(playerToReveal))
				{
					Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
					InitiativeTrack.Instance.UpdateInitiativeTrack(Choreographer.s_Choreographer.m_ClientPlayers, Choreographer.s_Choreographer.ClientMonsterObjects, playersSelectable: true, enemiesSelectable: false, forceSelectFirstActor: true);
				}
			};
			StartCoroutine(WaitBeforePerformingAction(levelEvent.EventFloatResource, actionToPerform));
			if (string.IsNullOrEmpty(levelEvent.EventSecondResource))
			{
				break;
			}
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[playerToReveal.Location.X, playerToReveal.Location.Y];
			GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Player, levelEvent.EventSecondResource);
			if (characterPrefabFromBundle != null)
			{
				try
				{
					GameObject obj = UnityEngine.Object.Instantiate(characterPrefabFromBundle, cClientTile.m_GameObject.transform.parent);
					obj.transform.position = cClientTile.m_GameObject.transform.position;
					PlayableDirector componentInChildren2 = obj.GetComponentInChildren<PlayableDirector>();
					TimelineAsset asset = obj.GetComponentInChildren<TimelineAssets>().FindTimelineAsset("RevealCharacterTimeline");
					componentInChildren2.Play(asset);
					break;
				}
				catch (Exception ex)
				{
					Debug.LogError("Reveal Character anim not setup as expected. Failed with exception:" + ex.Message);
					break;
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetShortRestCard:
			m_ShortRestCardDataPending = levelEvent.EventResource;
			break;
		case CLevelEvent.ELevelEventType.SetCharacterSelectedRoundCards:
		{
			if (!ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor p) => p.CharacterClass.CharacterID == levelEvent.EventResource))
			{
				break;
			}
			Debug.Log("Found player in scenario to set selected cards");
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.Single((CPlayerActor p) => p.CharacterClass.CharacterID == levelEvent.EventResource);
			if (cPlayerActor.CharacterClass.SelectedAbilityCards.Any((CAbilityCard c) => c.Name == levelEvent.EventSecondResource))
			{
				Debug.Log("Found card [" + levelEvent.EventSecondResource + "] in scenario to set selected");
				CAbilityCard cAbilityCard = cPlayerActor.CharacterClass.SelectedAbilityCards.Single((CAbilityCard c) => c.Name == levelEvent.EventSecondResource);
				uint num = ScenarioRuleClient.MoveAbilityCard(cPlayerActor.CharacterClass, cAbilityCard, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction: false);
				long num2 = DateTime.Now.Ticks / 10000;
				while (num > ScenarioRuleClient.s_SRLLastProcessedMessageID && DateTime.Now.Ticks / 10000 - num2 < 1000)
				{
					Thread.Sleep(10);
				}
				cPlayerActor.CharacterClass.SetInitiativeAbilityCard(cAbilityCard);
			}
			if (cPlayerActor.CharacterClass.SelectedAbilityCards.Any((CAbilityCard c) => c.Name == levelEvent.EventThirdResource))
			{
				Debug.Log("Found card [" + levelEvent.EventThirdResource + "] in scenario to set selected");
				CAbilityCard cAbilityCard2 = cPlayerActor.CharacterClass.SelectedAbilityCards.Single((CAbilityCard c) => c.Name == levelEvent.EventThirdResource);
				uint num3 = ScenarioRuleClient.MoveAbilityCard(cPlayerActor.CharacterClass, cAbilityCard2, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction: false);
				long num4 = DateTime.Now.Ticks / 10000;
				while (num3 > ScenarioRuleClient.s_SRLLastProcessedMessageID && DateTime.Now.Ticks / 10000 - num4 < 1000)
				{
					Thread.Sleep(10);
				}
				cPlayerActor.CharacterClass.SetSubInitiativeAbilityCard(cAbilityCard2);
			}
			ScenarioRuleClient.SortInitiative();
			InitiativeTrack.Instance?.UpdateInitiativeTrack(GameState.InitiativeSortedActors, playersSelectable: false, enemiesSelectable: false, selectActor: false);
			break;
		}
		case CLevelEvent.ELevelEventType.SetCameraPosition:
			if (levelEvent.EventCameraProfile != null)
			{
				CameraController.s_CameraController.SetCameraWithMessageProfile(levelEvent.EventCameraProfile);
			}
			break;
		case CLevelEvent.ELevelEventType.WinScenario:
			Choreographer.s_Choreographer.WinScenario();
			break;
		case CLevelEvent.ELevelEventType.LoseScenario:
			Choreographer.s_Choreographer.LoseScenario();
			break;
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner s) => s.SpawnerGuid == levelEvent.EventResource);
			if (cSpawner == null || cSpawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) == ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				break;
			}
			switch (CLevelEvent.LevelEventSpawnerEditTypes.SingleOrDefault((CLevelEvent.ELevelEventSpawnerEditType t) => t.ToString() == levelEvent.EventSecondResource))
			{
			case CLevelEvent.ELevelEventSpawnerEditType.OnlyActivateSpawner:
				cSpawner.SetActive(active: true, forceDontSpawn: true);
				cSpawner.WillNewEnemyExistNextRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.ActivateSpawner:
				cSpawner.SetActive(active: true);
				cSpawner.WillNewEnemyExistNextRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.DeactivateSpawner:
				cSpawner.SetActive(active: false);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.TriggerSpawn:
				cSpawner.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: true);
				ScenarioRuleClient.SortInitiative();
				break;
			}
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
			if (levelEvent.EventSecondResource.Equals("true"))
			{
				ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: true);
			}
			else
			{
				ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: true, unlockOnly: true);
			}
			break;
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			bool lockDoor = levelEvent.EventSecondResource.Equals("true");
			ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: false, unlockOnly: false, lockDoor);
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
			ScenarioRuleClient.ToggleScenarioModifierActivation(levelEvent.EventResource, activate: false);
			break;
		case CLevelEvent.ELevelEventType.ActivateModifier:
			ScenarioRuleClient.ToggleScenarioModifierActivation(levelEvent.EventResource, activate: true);
			break;
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
			ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == levelEvent.EventResource)?.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber, forceActivate: true);
			break;
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		{
			CObjective cObjective = ScenarioManager.CurrentScenarioState.WinObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			if (cObjective == null)
			{
				cObjective = ScenarioManager.CurrentScenarioState.LoseObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			}
			if (cObjective != null)
			{
				cObjective.SetActivation(active: false);
			}
			else
			{
				Debug.LogError("Unable to find objective with Event Identifier: " + levelEvent.EventResource + " to deactivate.");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			CObjective cObjective2 = ScenarioManager.CurrentScenarioState.WinObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			if (cObjective2 == null)
			{
				cObjective2 = ScenarioManager.CurrentScenarioState.LoseObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			}
			if (cObjective2 != null)
			{
				cObjective2.SetActivation(active: true);
			}
			else
			{
				Debug.LogError("Unable to find objective with Event Identifier: " + levelEvent.EventResource + " to activate.");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		{
			CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventSecondResource);
			if (cActor == null || MonsterClassManager.Find(levelEvent.EventResource) == null)
			{
				break;
			}
			CTile cTile = null;
			if (cActor.IsDead)
			{
				cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
			}
			else
			{
				List<CTile> list = ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y]).FindAll((CTile x) => CAbilityFilter.IsValidTile(x, CAbilityFilter.EFilterTile.EmptyHex)).ToList();
				cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
			}
			if (cTile != null)
			{
				LevelEditorController.SpawnMonsterAtLocation(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y], levelEvent.EventResource, ScenarioManager.CurrentScenarioState.Level, CActor.EType.Enemy, playSummonedAnim: true);
			}
			else
			{
				Debug.LogError("Unable to find tile to spawn actor to for SpawnMonsterOnActor level event");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			CActor cActor4 = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventSecondResource);
			if (cActor4 != null)
			{
				CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor4.ArrayIndex.X, cActor4.ArrayIndex.Y];
				LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { item2 }, levelEvent.EventResource);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			bool flag3 = false;
			CObjectProp cObjectProp2 = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
			if (cObjectProp2 == null)
			{
				cObjectProp2 = ScenarioManager.CurrentScenarioState.DestroyedProps.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
				if (cObjectProp2 != null)
				{
					flag3 = true;
				}
			}
			if (cObjectProp2 != null)
			{
				if (!flag3)
				{
					cObjectProp2.DestroyProp();
				}
				CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cObjectProp2.ArrayIndex.X, cObjectProp2.ArrayIndex.Y];
				LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { item }, levelEvent.EventResource);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
			if (cObjectProp == null || MonsterClassManager.Find(levelEvent.EventResource) == null)
			{
				break;
			}
			bool flag = true;
			string text = levelEvent.EventResource;
			CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y];
			List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource = LevelEditorEventsPanel.GetPerPartySizeConfigsFromResource(levelEvent.EventThirdResource);
			if (perPartySizeConfigsFromResource.Count == 4)
			{
				ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
				if (currentScenarioState != null)
				{
					_ = currentScenarioState.Players.Count;
					if (true)
					{
						switch (perPartySizeConfigsFromResource[ScenarioManager.CurrentScenarioState.Players.Count - 1])
						{
						case ScenarioManager.EPerPartySizeConfig.ToElite:
						{
							CMonsterClass cMonsterClass = MonsterClassManager.FindEliteVariantOfClass(MonsterClassManager.Find(text).ID);
							if (cMonsterClass != null)
							{
								text = cMonsterClass.ID;
							}
							break;
						}
						case ScenarioManager.EPerPartySizeConfig.Hidden:
							flag = false;
							break;
						}
					}
				}
			}
			if (flag)
			{
				cObjectProp.DestroyProp();
				LevelEditorController.SpawnMonsterAtLocation(tile, text, ScenarioManager.CurrentScenarioState.Level, CActor.EType.Enemy, playSummonedAnim: true);
				ScenarioRuleClient.SortInitiative();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			CScenarioModifier cScenarioModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier d) => d.EventIdentifier == levelEvent.EventResource);
			if (cScenarioModifier != null)
			{
				bool hiddenState = levelEvent.EventSecondResource.Equals("true");
				cScenarioModifier.SetHiddenState(hiddenState);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
		{
			CActor actor2 = overrideContextActor ?? GameState.InternalCurrentActor;
			CheckForCurrentActorOverride(levelEvent, ref actor2);
			{
				foreach (CActiveBonus item3 in CActiveBonus.FindApplicableActiveBonuses(actor2))
				{
					if (item3.Ability.Name == levelEvent.EventResource || item3.Ability.Name == levelEvent.EventSecondResource || item3.Ability.Name == levelEvent.EventThirdResource)
					{
						CClass.CancelActiveBonus(item3);
					}
				}
				break;
			}
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			CActor actor = overrideContextActor ?? GameState.InternalCurrentActor;
			CheckForCurrentActorOverride(levelEvent, ref actor);
			ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == levelEvent.EventResource)?.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, actor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate: true);
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
		{
			CObjectProp cObjectProp3 = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == levelEvent.EventResource);
			if (cObjectProp3 != null && cObjectProp3 is CObjectDifficultTerrain cObjectDifficultTerrain)
			{
				cObjectDifficultTerrain.RemoveDifficultTerrain();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.LoseGoalChestRewardChoice:
			ScenarioManager.CurrentScenarioState.GoalChestRewards.Clear();
			break;
		case CLevelEvent.ELevelEventType.KillActor:
		{
			CActor cActor3 = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventResource);
			GameState.KillActor(cActor3, cActor3, CActor.ECauseOfDeath.Suicide, out var _);
			break;
		}
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
		{
			CObjectActor cObjectActor = ScenarioManager.Scenario.AllObjects.FirstOrDefault((CObjectActor s) => s.AttachedProp?.PropGuid == levelEvent.EventResource);
			if (cObjectActor != null)
			{
				GameState.KillActor(cObjectActor, cObjectActor, CActor.ECauseOfDeath.Suicide, out var _);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			CObjectProp lastDestroyedObstacle = GameState.LastDestroyedObstacle;
			if (lastDestroyedObstacle == null || MonsterClassManager.Find(levelEvent.EventResource) == null)
			{
				break;
			}
			bool flag2 = true;
			string text2 = levelEvent.EventResource;
			CClientTile tile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[lastDestroyedObstacle.ArrayIndex.X, lastDestroyedObstacle.ArrayIndex.Y];
			List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource2 = LevelEditorEventsPanel.GetPerPartySizeConfigsFromResource(levelEvent.EventThirdResource);
			if (perPartySizeConfigsFromResource2.Count == 4)
			{
				ScenarioState currentScenarioState2 = ScenarioManager.CurrentScenarioState;
				if (currentScenarioState2 != null)
				{
					_ = currentScenarioState2.Players.Count;
					if (true)
					{
						switch (perPartySizeConfigsFromResource2[ScenarioManager.CurrentScenarioState.Players.Count - 1])
						{
						case ScenarioManager.EPerPartySizeConfig.ToElite:
						{
							CMonsterClass cMonsterClass2 = MonsterClassManager.FindEliteVariantOfClass(MonsterClassManager.Find(text2).ID);
							if (cMonsterClass2 != null)
							{
								text2 = cMonsterClass2.ID;
							}
							break;
						}
						case ScenarioManager.EPerPartySizeConfig.Hidden:
							flag2 = false;
							break;
						}
					}
				}
			}
			if (flag2)
			{
				LevelEditorController.SpawnMonsterAtLocation(tile2, text2, ScenarioManager.CurrentScenarioState.Level, CActor.EType.Enemy, playSummonedAnim: true);
				ScenarioRuleClient.SortInitiative();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
		{
			CActor cActor2 = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventResource);
			if (cActor2 != null)
			{
				string eventSecondResource = levelEvent.EventSecondResource;
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor2);
				if (gameObject != null)
				{
					MF.GetGameObjectAnimator(gameObject).SetBool(eventSecondResource, value: true);
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerPlayableDirectorOnGameObject:
		{
			PlayableDirector componentInChildren = GameObject.Find(levelEvent.EventResource).GetComponentInChildren<PlayableDirector>();
			if (componentInChildren != null)
			{
				componentInChildren.Play();
			}
			break;
		}
		}
	}

	private void CheckForCurrentActorOverride(CLevelEvent levelEvent, ref CActor actor)
	{
		if (!levelEvent.EventFourthResource.IsNOTNullOrEmpty())
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == levelEvent.EventFourthResource);
		if (cActor == null)
		{
			cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x is CObjectActor { IsAttachedToProp: not false } cObjectActor && cObjectActor.AttachedProp.PropGuid == levelEvent.EventFourthResource);
		}
		if (cActor != null)
		{
			actor = cActor;
		}
	}

	private IEnumerator WaitBeforePerformingAction(float timeToWait, UnityAction actionToPerform)
	{
		if (timeToWait > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(timeToWait);
		}
		actionToPerform?.Invoke();
	}

	public void RunActionIfShortRestDataPending(Action<string> dataPendingAction)
	{
		if (!string.IsNullOrEmpty(m_ShortRestCardDataPending))
		{
			dataPendingAction?.Invoke(m_ShortRestCardDataPending);
			m_ShortRestCardDataPending = string.Empty;
		}
	}

	private void SetCustomObjectiveTriggered(Tuple<CObjective_CustomTrigger, int> objectiveTupleToTrigger, bool isWinObjective)
	{
		if (isWinObjective)
		{
			if (ScenarioManager.CurrentScenarioState.WinObjectives[objectiveTupleToTrigger.Item2] is CObjective_CustomTrigger cObjective_CustomTrigger)
			{
				cObjective_CustomTrigger.SetObjectiveTriggered();
			}
		}
		else if (ScenarioManager.CurrentScenarioState.LoseObjectives[objectiveTupleToTrigger.Item2] is CObjective_CustomTrigger cObjective_CustomTrigger2)
		{
			cObjective_CustomTrigger2.SetObjectiveTriggered();
		}
	}
}
