using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.PhaseManager;
using SharedLibrary.Client;
using SharedLibrary.Logger;

namespace MapRuleLibrary.Client;

public class MapRuleLibraryClient : CMessageHandler
{
	private static MapRuleLibraryClient s_Instance;

	private static CMRLYML s_MRLYML;

	private static Action<string, string> s_DebugLogErrorAction;

	public static MapRuleLibraryClient Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new MapRuleLibraryClient();
			}
			return s_Instance;
		}
	}

	public static CMRLYML MRLYML => s_MRLYML;

	public override void Initialise(bool reinitialise = false)
	{
		if (!base.IsInitialised || reinitialise)
		{
			s_MRLYML = new CMRLYML();
		}
		base.Initialise();
	}

	public static void SubscribeAnalyticsLogErrorAction(Action<string, string> logErrorAction)
	{
		s_DebugLogErrorAction = (Action<string, string>)Delegate.Combine(s_DebugLogErrorAction, logErrorAction);
	}

	public static void AnalyticsLogError(string errorCode, string message, string stackTrace = "")
	{
		DLLDebug.LogError(message + "\n" + stackTrace);
		if (errorCode != string.Empty)
		{
			s_DebugLogErrorAction?.Invoke(errorCode, message);
		}
	}

	protected override void ProcessMessage(object message)
	{
		try
		{
			CMapDLLMessage cMapDLLMessage = (CMapDLLMessage)message;
			switch (cMapDLLMessage.m_MessageType)
			{
			case EMapDLLMessageType.Move:
			{
				CMove_MapDLLMessage cMove_MapDLLMessage = (CMove_MapDLLMessage)cMapDLLMessage;
				AdventureState.MapState.SetNextMapPhase(new CMapPhaseMoving(cMove_MapDLLMessage.m_StartLocation, cMove_MapDLLMessage.m_EndLocation));
				break;
			}
			case EMapDLLMessageType.MoveComplete:
			{
				CMoveComplete_MapDLLMessage cMoveComplete_MapDLLMessage = (CMoveComplete_MapDLLMessage)cMapDLLMessage;
				CLocationState locationState = cMoveComplete_MapDLLMessage.m_LocationState;
				if (!(locationState is CHeadquartersState))
				{
					if (!(locationState is CVillageState))
					{
						if (locationState is CMapScenarioState)
						{
							AdventureState.MapState.SetNextMapPhase(new CMapPhaseAtScenario(cMoveComplete_MapDLLMessage.m_LocationState.ID));
						}
					}
					else
					{
						AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.Default));
					}
				}
				else
				{
					AdventureState.MapState.SetNextMapPhase(new CMapPhaseInHQ());
				}
				break;
			}
			case EMapDLLMessageType.EnteredScenario:
				AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InScenario));
				break;
			case EMapDLLMessageType.RoadEvent:
				if (AdventureState.MapState.CurrentMapPhase is CMapPhaseMoving cMapPhaseMoving)
				{
					AdventureState.MapState.SetNextMapPhase(new CMapPhaseRoadEvent(cMapPhaseMoving.EndLocation));
				}
				else
				{
					AdventureState.MapState.SetNextMapPhase(new CMapPhaseRoadEvent());
				}
				break;
			case EMapDLLMessageType.GetMapMessagesForTrigger:
			{
				CGetMapMessagesForTrigger_MapDLLMessage cGetMapMessagesForTrigger_MapDLLMessage = (CGetMapMessagesForTrigger_MapDLLMessage)cMapDLLMessage;
				List<CMapMessageState> list = new List<CMapMessageState>();
				foreach (CMapMessageState mapMessageState in AdventureState.MapState.MapMessageStates)
				{
					bool flag = false;
					if (mapMessageState.MapMessageState == CMapMessageState.EMapMessageState.Unlocked && mapMessageState.MapMessage.MapMessageTrigger == cGetMapMessagesForTrigger_MapDLLMessage.m_MapMessageTrigger)
					{
						flag = true;
					}
					if (flag)
					{
						list.Add(mapMessageState);
					}
				}
				CShowMapMessages_MapClientMessage message2 = new CShowMapMessages_MapClientMessage(cGetMapMessagesForTrigger_MapDLLMessage.m_MapMessageTrigger, list);
				Instance.MessageHandler(message2);
				break;
			}
			case EMapDLLMessageType.EnteredScenarioDebugMode:
				AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InScenarioDebugMode));
				break;
			case EMapDLLMessageType.CheckLockedContent:
				AdventureState.MapState.CheckLockedContent();
				break;
			case EMapDLLMessageType.CheckCurrentJobQuests:
				AdventureState.MapState.CheckCurrentJobQuests(checkTimeout: true);
				break;
			case EMapDLLMessageType.SkipFTUE:
			{
				CSkipFTUE_MapDLLMessage cSkipFTUE_MapDLLMessage = (CSkipFTUE_MapDLLMessage)cMapDLLMessage;
				AdventureState.MapState.SkipFTUE(cSkipFTUE_MapDLLMessage.m_SkipTutorial, cSkipFTUE_MapDLLMessage.m_SkipIntro);
				break;
			}
			case EMapDLLMessageType.UpdateQuestCompletion:
			{
				CUpdateQuestCompletion_MapDLLMessage messageData = (CUpdateQuestCompletion_MapDLLMessage)cMapDLLMessage;
				AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState x) => x.ID == messageData.m_QuestID).UpdateQuestCompletion(messageData.m_AutoComplete);
				break;
			}
			case EMapDLLMessageType.OnMapStateAdventureStarted:
				AdventureState.MapState.OnMapStateAdventureStarted();
				break;
			case EMapDLLMessageType.OnMapLoaded:
			{
				COnMapLoaded_MapDLLMessage cOnMapLoaded_MapDLLMessage = (COnMapLoaded_MapDLLMessage)cMapDLLMessage;
				AdventureState.MapState.OnMapLoaded(cOnMapLoaded_MapDLLMessage.m_IsMPClientJoining);
				break;
			}
			case EMapDLLMessageType.DeleteCharacter:
			{
				CCharacterDeleted_MapDLLMessage cCharacterDeleted_MapDLLMessage = (CCharacterDeleted_MapDLLMessage)cMapDLLMessage;
				AdventureState.MapState.MapParty.DeleteCharacter(cCharacterDeleted_MapDLLMessage.CharacterToDelete);
				break;
			}
			case EMapDLLMessageType.SoloScenarioImportCompletion:
			{
				CSoloScenarioImportCompletion_MapDLLMessage cSoloScenarioImportCompletion_MapDLLMessage = (CSoloScenarioImportCompletion_MapDLLMessage)cMapDLLMessage;
				cSoloScenarioImportCompletion_MapDLLMessage.m_QuestState.SoloScenarioImportCompletion(cSoloScenarioImportCompletion_MapDLLMessage.m_SoloCharacter, cSoloScenarioImportCompletion_MapDLLMessage.m_ScenarioLevel);
				break;
			}
			case EMapDLLMessageType.CheckForDuplicateItems:
			case EMapDLLMessageType.CheckAchievementsForPlatformTrophies:
				break;
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception while processing message.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}
}
