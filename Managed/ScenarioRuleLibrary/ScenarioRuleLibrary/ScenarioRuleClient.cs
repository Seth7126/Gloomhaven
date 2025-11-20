#define ENABLE_LOGS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SM.Utils;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class ScenarioRuleClient
{
	public enum EMessageType
	{
		ESTEPCOMPLETEMESSAGE,
		ENEXTPHASEMESSAGE,
		EPASSMESSAGE,
		EENDABILITYSYNCHRONISEMESSAGE,
		EUNDOMESSAGE,
		ESTOPMESSAGE,
		EACTIVATETRAPMESSAGE,
		EENDTURNSYNCHRONISEMESSAGE,
		EPASSSTEPMESSAGE,
		ETILESELECTEDMESSAGE,
		EACTIVATEHAZARDOUSTERRAINMESSAGE,
		ECLEARROUNDABILITYCARDSMESSAGE,
		EMOVEABILITYCARDMESSAGE,
		ETILEDESELECTEDMESSAGE,
		APPLYSINGLETARGETMESSAGE,
		EACTIVATECHESTMESSAGE,
		ETOGGLEACTIONAUGMENTATION,
		EENDROUNDSYNCHRONISEMESSAGE,
		ETOGGLEACTIVEBONUS,
		ECLEARTARGETSMESSAGE,
		ELOCKACTIVEBONUS,
		EUPDATEATTACKPATH,
		EQUEUESPAWNERTRIGGER,
		EUPDATEMOVECARRY,
		ETOGGLESCENARIOMODIFIERACTIVATION,
		ETOGGLEDOOR,
		ETOGGLEITEM,
		ESORTINITIATIVE,
		ESETSTARTROUNDDECKSTATE,
		EGETSTARTROUNDDECKSTATE,
		ESHORTREST
	}

	public enum ToggleState : byte
	{
		Default,
		Toggle,
		Untoggle
	}

	public class CSRLMessage
	{
		public EMessageType m_MessageType;

		public uint m_MessageID;

		public string m_CallerStack;

		public CSRLMessage(EMessageType eMessageType)
		{
			m_MessageType = eMessageType;
		}
	}

	private class CSRLMoveAbilityCardMessage : CSRLMessage
	{
		public CCharacterClass m_CharacterClass;

		public CAbilityCard m_AbilityCard;

		public List<CAbilityCard> m_FromAbilityCardList;

		public List<CAbilityCard> m_ToAbilityCardList;

		public string m_FromAbilityCardListName;

		public string m_ToAbilityCardListName;

		public bool m_SendNetworkSelectedCardsWhenCompleted;

		public CSRLMoveAbilityCardMessage(CCharacterClass characterClass, CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromAbilityCardListName, string toAbilityCardListName, bool sendNetworkSelectedCardsWhenCompleted)
			: base(EMessageType.EMOVEABILITYCARDMESSAGE)
		{
			m_CharacterClass = characterClass;
			m_AbilityCard = abilityCard;
			m_FromAbilityCardList = fromAbilityCardList;
			m_ToAbilityCardList = toAbilityCardList;
			m_FromAbilityCardListName = fromAbilityCardListName;
			m_ToAbilityCardListName = toAbilityCardListName;
			m_SendNetworkSelectedCardsWhenCompleted = sendNetworkSelectedCardsWhenCompleted;
		}
	}

	private class CSRLClearRoundAbilityCardsMessage : CSRLMessage
	{
		public CCharacterClass m_CharacterClass;

		public CSRLClearRoundAbilityCardsMessage(CCharacterClass characterClass)
			: base(EMessageType.ECLEARROUNDABILITYCARDSMESSAGE)
		{
			m_CharacterClass = characterClass;
		}
	}

	private class CSRLTileSelectedMessage : CSRLMessage
	{
		public CTile m_Tile;

		public List<CTile> m_OptionalTileList;

		public CSRLTileSelectedMessage(CTile tile = null, List<CTile> optionalTileList = null)
			: base(EMessageType.ETILESELECTEDMESSAGE)
		{
			m_Tile = tile;
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLTileDeselectedMessage : CSRLMessage
	{
		public CTile m_Tile;

		public List<CTile> m_OptionalTileList;

		public CSRLTileDeselectedMessage(CTile tile = null, List<CTile> optionalTileList = null)
			: base(EMessageType.ETILEDESELECTEDMESSAGE)
		{
			m_Tile = tile;
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLApplySingleTargetMessage : CSRLMessage
	{
		public CActor m_Actor;

		public CSRLApplySingleTargetMessage(CActor actor = null)
			: base(EMessageType.APPLYSINGLETARGETMESSAGE)
		{
			m_Actor = actor;
		}
	}

	private class CSRLToggleActionAugmentationMessage : CSRLMessage
	{
		public CActionAugmentation m_ActionAugmentation;

		public CSRLToggleActionAugmentationMessage(CActionAugmentation actionAugmentation = null)
			: base(EMessageType.ETOGGLEACTIONAUGMENTATION)
		{
			m_ActionAugmentation = actionAugmentation;
		}
	}

	private class CSRLToggleActiveBonusMessage : CSRLMessage
	{
		public CActiveBonus m_ActiveBonus;

		public ElementInfusionBoardManager.EElement? m_Element;

		public CActor m_Actor;

		public object m_ExtraOption;

		public int m_PhaseWhenClickedInt;

		public ToggleState m_toggle;

		public CSRLToggleActiveBonusMessage(CActiveBonus activeBonus, ElementInfusionBoardManager.EElement? element, CActor actor, int phaseWhenClickedInt, object extraOption = null, ToggleState futureToggle = ToggleState.Default)
			: base(EMessageType.ETOGGLEACTIVEBONUS)
		{
			m_ActiveBonus = activeBonus;
			m_Element = element;
			m_Actor = actor;
			m_ExtraOption = extraOption;
			m_PhaseWhenClickedInt = phaseWhenClickedInt;
			m_toggle = futureToggle;
		}
	}

	private class CSRLLockActiveBonusMessage : CSRLMessage
	{
		public CActiveBonus m_ActiveBonus;

		public CSRLLockActiveBonusMessage(CActiveBonus activeBonus)
			: base(EMessageType.ELOCKACTIVEBONUS)
		{
			m_ActiveBonus = activeBonus;
		}
	}

	private class CSRLUpdateAttackPathMessage : CSRLMessage
	{
		public List<CTile> m_OptionalTileList;

		public CSRLUpdateAttackPathMessage(List<CTile> optionalTileList = null)
			: base(EMessageType.EUPDATEATTACKPATH)
		{
			m_OptionalTileList = optionalTileList;
		}
	}

	private class CSRLUpdateMoveCarryMessage : CSRLMessage
	{
		public List<CTile> m_OptionalTileList;

		public bool m_RemoveWaypoint;

		public CSRLUpdateMoveCarryMessage(List<CTile> optionalTileList = null, bool removeWaypoint = false)
			: base(EMessageType.EUPDATEMOVECARRY)
		{
			m_OptionalTileList = optionalTileList;
			m_RemoveWaypoint = removeWaypoint;
		}
	}

	private class CSRLToggleScenarioModifierActivation : CSRLMessage
	{
		public string m_ScenarioModifierEventIdentifier;

		public bool m_Activate;

		public CSRLToggleScenarioModifierActivation(string eventIdentifier, bool activate)
			: base(EMessageType.ETOGGLESCENARIOMODIFIERACTIVATION)
		{
			m_ScenarioModifierEventIdentifier = eventIdentifier;
			m_Activate = activate;
		}
	}

	private class CSRLToggleDoorMessage : CSRLMessage
	{
		public string m_DoorGuid;

		public bool m_OpenDoor;

		public bool m_UnlockOnly;

		public bool m_LockDoor;

		public CSRLToggleDoorMessage(string doorGuid, bool openDoor, bool unlockOnly, bool lockDoor)
			: base(EMessageType.ETOGGLEDOOR)
		{
			m_DoorGuid = doorGuid;
			m_OpenDoor = openDoor;
			m_UnlockOnly = unlockOnly;
			m_LockDoor = lockDoor;
		}
	}

	private class CSRLToggleItemMessage : CSRLMessage
	{
		public CItem m_Item;

		public CActor m_Actor;

		public CSRLToggleItemMessage(CItem item, CActor actor)
			: base(EMessageType.ETOGGLEITEM)
		{
			m_Item = item;
			m_Actor = actor;
		}
	}

	private class CSRLUndoMessage : CSRLMessage
	{
		public List<CActiveBonus> m_ActiveBonusesToUntoggle;

		public CSRLUndoMessage(List<CActiveBonus> activeBonusesToUntoggle)
			: base(EMessageType.EUNDOMESSAGE)
		{
			m_ActiveBonusesToUntoggle = activeBonusesToUntoggle;
		}
	}

	private class CSRLSetStartRoundDeckStateMessage : CSRLMessage
	{
		public CPlayerActor m_PlayerActor;

		public StartRoundCardState m_StartRoundCardState;

		public CSRLSetStartRoundDeckStateMessage(CPlayerActor playerActor, StartRoundCardState startRoundCardState)
			: base(EMessageType.ESETSTARTROUNDDECKSTATE)
		{
			m_PlayerActor = playerActor;
			m_StartRoundCardState = startRoundCardState;
		}
	}

	private class CSRLGetStartRoundDeckStateMessage : CSRLMessage
	{
		public CPlayerActor m_PlayerActor;

		public int m_GameActionID;

		public CSRLGetStartRoundDeckStateMessage(CPlayerActor playerActor, int gameActionID)
			: base(EMessageType.EGETSTARTROUNDDECKSTATE)
		{
			m_PlayerActor = playerActor;
			m_GameActionID = gameActionID;
		}
	}

	private class CSRLShortRestPlayerMessage : CSRLMessage
	{
		public CPlayerActor PlayerActor;

		public CAbilityCard DiscardedAbilityCard;

		public bool LoseHealth;

		public bool FromStateUpdate;

		public bool UpdateScenarioRNG;

		public CSRLShortRestPlayerMessage(CPlayerActor playerActor, CAbilityCard discardedAbilityCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate)
			: base(EMessageType.ESHORTREST)
		{
			PlayerActor = playerActor;
			DiscardedAbilityCard = discardedAbilityCard;
			LoseHealth = loseHealth;
			FromStateUpdate = fromStateUpdate;
			UpdateScenarioRNG = updateScenarioRNG;
		}
	}

	public delegate void MessageHandlerCallback(CMessageData message, bool processImmediately = false);

	private const int IncreasedStackSize = 1048576;

	public const int c_NumberGloomhavenLevels = 8;

	public static volatile uint s_SRLLastProcessedMessageID;

	public static volatile Thread s_WorkThread;

	public static Thread s_MainThread;

	private static volatile bool s_IsAlive;

	private static volatile ConcurrentQueue<CSRLMessage> s_MessageQueue;

	private static volatile BlockingCollection<CSRLMessage> s_BlockingCollection;

	private static volatile ManualResetEventSlim s_manualResetEventSlim;

	private static volatile CancellationTokenSource s_CancellationTokenSource;

	private static volatile bool s_BlockMessageProcessing;

	private static volatile uint s_SRLNextMessageID;

	private static MessageHandlerCallback s_MessageHandler;

	private static volatile bool s_Initialised;

	private static CSRLYML s_SRLYML;

	public static MessageHandlerCallback MessageHandler => s_MessageHandler;

	public static int AttackValueOverride => CAbilityAttack.AttackValueOverride;

	public static int MessageQueueLength => s_BlockingCollection.Count;

	public static List<CSRLMessage> MessageQueueCopy => s_BlockingCollection.ToList();

	public static CSRLYML SRLYML => s_SRLYML;

	public static bool IsMessageProcessingBlocked => s_BlockMessageProcessing;

	public static bool ScenarioRuleClientStopped => !s_IsAlive;

	public static bool IsProcessing { get; private set; }

	public static bool IsProcessingOrMessagesQueued
	{
		get
		{
			if (MessageQueueLength <= 0)
			{
				return IsProcessing;
			}
			return true;
		}
	}

	public static void Initialise(bool reinitialise = false)
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient initialising");
		if (!s_Initialised || reinitialise)
		{
			s_SRLYML = new CSRLYML();
			GameState.OutputAdvantageInfo = false;
			s_Initialised = true;
		}
		GameState.s_Parser.Parse("1 + 1");
		SEventLogMessageHandler.Initialise();
	}

	public static void LoadData()
	{
		CharacterClassManager.Load();
		MonsterClassManager.Load();
		ActionManager.Load();
	}

	public static void ToggleMessageProcessing(bool process)
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient toggling message processing to: " + process + "\n" + Environment.StackTrace);
		s_BlockMessageProcessing = !process;
		if (s_manualResetEventSlim != null)
		{
			if (s_BlockMessageProcessing)
			{
				s_manualResetEventSlim.Reset();
			}
			else
			{
				s_manualResetEventSlim.Set();
			}
		}
	}

	private static void ProcessMessage(CSRLMessage message)
	{
		try
		{
			CSRLQueueStatusLog_MessageData cSRLQueueStatusLog_MessageData = new CSRLQueueStatusLog_MessageData("", null);
			cSRLQueueStatusLog_MessageData.m_SRLMessage = message;
			MessageHandler(cSRLQueueStatusLog_MessageData);
			SimpleLog.AddToSimpleLog("ScenarioRuleClient ProcessMessage: " + message.m_MessageType);
			switch (message.m_MessageType)
			{
			case EMessageType.ESTEPCOMPLETEMESSAGE:
				PhaseManager.StepComplete();
				break;
			case EMessageType.ENEXTPHASEMESSAGE:
				GameState.NextPhase();
				break;
			case EMessageType.EPASSMESSAGE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.CheckForForgoActionActiveBonuses || GameState.CurrentActionInitiator == GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase)
				{
					PhaseManager.NextStep(passing: true);
				}
				else
				{
					GameState.NextPhase();
				}
				break;
			case EMessageType.EPASSSTEPMESSAGE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					PhaseManager.StepComplete(passingStep: true);
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			case EMessageType.EENDABILITYSYNCHRONISEMESSAGE:
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					((CPhaseAction)PhaseManager.Phase).EndAbilitySynchronise();
					break;
				}
				CSRLWrongPhaseException_MessageData message14 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message14);
				break;
			}
			case EMessageType.EENDTURNSYNCHRONISEMESSAGE:
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.EndTurn)
				{
					((CPhaseEndTurn)PhaseManager.Phase).EndTurnSynchronise();
					break;
				}
				CSRLWrongPhaseException_MessageData message3 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message3);
				break;
			}
			case EMessageType.EUNDOMESSAGE:
			{
				CSRLUndoMessage cSRLUndoMessage = (CSRLUndoMessage)message;
				if (PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
				{
					CAbility ability3 = cPhaseAction.CurrentPhaseAbility.m_Ability;
					ability3.Undo();
					bool flag2 = true;
					if (ability3.IsItemAbility)
					{
						cPhaseAction.UndoItemAbility();
					}
					else
					{
						CAbility parentAbility = ability3.ParentAbility;
						if (parentAbility != null && parentAbility.AbilityType == CAbility.EAbilityType.ChooseAbility)
						{
							cPhaseAction.UndoChooseAbility();
							flag2 = false;
						}
						else
						{
							GameState.InternalCurrentActor.Inventory.Undo();
							cPhaseAction.ResetActionAugmentationsToConsume();
							PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
						}
					}
					if (flag2)
					{
						GameState.Undo();
					}
					foreach (CActiveBonus item2 in cSRLUndoMessage.m_ActiveBonusesToUntoggle)
					{
						item2.ToggleActiveBonus(null, null);
					}
					CFinishedProcessingUndo_MessageData cFinishedProcessingUndo_MessageData = new CFinishedProcessingUndo_MessageData();
					cFinishedProcessingUndo_MessageData.m_Ability = ability3;
					MessageHandler(cFinishedProcessingUndo_MessageData);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message13 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message13);
				}
				break;
			}
			case EMessageType.ESTOPMESSAGE:
				GameState.Stop();
				break;
			case EMessageType.EACTIVATETRAPMESSAGE:
			{
				CSRLTrapMessage cSRLTrapMessage = (CSRLTrapMessage)message;
				cSRLTrapMessage.m_TrapProp.AutomaticActivate(cSRLTrapMessage.m_Actor);
				break;
			}
			case EMessageType.EACTIVATECHESTMESSAGE:
			{
				CSRLChestMessage cSRLChestMessage = (CSRLChestMessage)message;
				cSRLChestMessage.m_ChestProp.DelayedActivate(cSRLChestMessage.m_Actor);
				break;
			}
			case EMessageType.ETILESELECTEDMESSAGE:
			{
				CSRLTileSelectedMessage cSRLTileSelectedMessage = (CSRLTileSelectedMessage)message;
				PhaseManager.TileSelected(cSRLTileSelectedMessage.m_Tile, cSRLTileSelectedMessage.m_OptionalTileList);
				break;
			}
			case EMessageType.ETILEDESELECTEDMESSAGE:
			{
				CSRLTileDeselectedMessage cSRLTileDeselectedMessage = (CSRLTileDeselectedMessage)message;
				PhaseManager.TileDeselected(cSRLTileDeselectedMessage.m_Tile, cSRLTileDeselectedMessage.m_OptionalTileList);
				break;
			}
			case EMessageType.EACTIVATEHAZARDOUSTERRAINMESSAGE:
			{
				CSRLHazardousTerrainMessage cSRLHazardousTerrainMessage = (CSRLHazardousTerrainMessage)message;
				cSRLHazardousTerrainMessage.m_HazardousTerrainProp.AutomaticActivate(cSRLHazardousTerrainMessage.m_Actor);
				break;
			}
			case EMessageType.ECLEARROUNDABILITYCARDSMESSAGE:
				((CSRLClearRoundAbilityCardsMessage)message).m_CharacterClass.ClearRoundAbilityCards();
				break;
			case EMessageType.EMOVEABILITYCARDMESSAGE:
			{
				CSRLMoveAbilityCardMessage cSRLMoveAbilityCardMessage = (CSRLMoveAbilityCardMessage)message;
				cSRLMoveAbilityCardMessage.m_CharacterClass.MoveAbilityCard(cSRLMoveAbilityCardMessage.m_AbilityCard, cSRLMoveAbilityCardMessage.m_FromAbilityCardList, cSRLMoveAbilityCardMessage.m_ToAbilityCardList, cSRLMoveAbilityCardMessage.m_FromAbilityCardListName, cSRLMoveAbilityCardMessage.m_ToAbilityCardListName, cSRLMoveAbilityCardMessage.m_SendNetworkSelectedCardsWhenCompleted);
				break;
			}
			case EMessageType.APPLYSINGLETARGETMESSAGE:
				PhaseManager.ApplySingleTarget(((CSRLApplySingleTargetMessage)message).m_Actor);
				break;
			case EMessageType.ETOGGLEACTIONAUGMENTATION:
			{
				CSRLToggleActionAugmentationMessage cSRLToggleActionAugmentationMessage = (CSRLToggleActionAugmentationMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					((CPhaseAction)PhaseManager.Phase).ToggleActionAugmentation(cSRLToggleActionAugmentationMessage.m_ActionAugmentation);
					break;
				}
				CSRLWrongPhaseException_MessageData message4 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message4);
				break;
			}
			case EMessageType.EENDROUNDSYNCHRONISEMESSAGE:
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.EndRound)
				{
					((CPhaseEndRound)PhaseManager.Phase).EndRoundSynchronise();
					break;
				}
				CSRLWrongPhaseException_MessageData message15 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
				MessageHandler(message15);
				break;
			}
			case EMessageType.ETOGGLEACTIVEBONUS:
			{
				CSRLToggleActiveBonusMessage cSRLToggleActiveBonusMessage = (CSRLToggleActiveBonusMessage)message;
				if (cSRLToggleActiveBonusMessage.m_toggle == ToggleState.Default || (cSRLToggleActiveBonusMessage.m_toggle == ToggleState.Toggle && !cSRLToggleActiveBonusMessage.m_ActiveBonus.ToggledBonus) || (cSRLToggleActiveBonusMessage.m_toggle == ToggleState.Untoggle && cSRLToggleActiveBonusMessage.m_ActiveBonus.ToggledBonus))
				{
					cSRLToggleActiveBonusMessage.m_ActiveBonus.ToggleActiveBonus(cSRLToggleActiveBonusMessage.m_Element, cSRLToggleActiveBonusMessage.m_Actor, cSRLToggleActiveBonusMessage.m_ExtraOption);
				}
				CFinishedProcessingActiveBonusToggle_MessageData cFinishedProcessingActiveBonusToggle_MessageData = new CFinishedProcessingActiveBonusToggle_MessageData();
				cFinishedProcessingActiveBonusToggle_MessageData.m_ActiveBonus = cSRLToggleActiveBonusMessage.m_ActiveBonus;
				cFinishedProcessingActiveBonusToggle_MessageData.m_PhaseWhenClickedInt = cSRLToggleActiveBonusMessage.m_PhaseWhenClickedInt;
				cFinishedProcessingActiveBonusToggle_MessageData.m_ActorSpawningMessage = cSRLToggleActiveBonusMessage.m_Actor;
				MessageHandler(cFinishedProcessingActiveBonusToggle_MessageData);
				break;
			}
			case EMessageType.ETOGGLEITEM:
			{
				CSRLToggleItemMessage obj = (CSRLToggleItemMessage)message;
				CAbility ability2 = null;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					ability2 = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability;
				}
				CItem item = obj.m_Item;
				obj.m_Actor.Inventory.ToggleItem(item);
				CFinishedProcessingItemToggle_MessageData cFinishedProcessingItemToggle_MessageData = new CFinishedProcessingItemToggle_MessageData();
				cFinishedProcessingItemToggle_MessageData.m_Ability = ability2;
				MessageHandler(cFinishedProcessingItemToggle_MessageData);
				break;
			}
			case EMessageType.ECLEARTARGETSMESSAGE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					CAbility ability = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability;
					if (ability.CanClearTargets())
					{
						ability.ClearTargets();
					}
					CFinishedProcessingClearTargets_MessageData cFinishedProcessingClearTargets_MessageData = new CFinishedProcessingClearTargets_MessageData();
					cFinishedProcessingClearTargets_MessageData.m_Ability = ability;
					MessageHandler(cFinishedProcessingClearTargets_MessageData);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message8 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message8);
				}
				break;
			case EMessageType.ELOCKACTIVEBONUS:
			{
				CSRLLockActiveBonusMessage cSRLLockActiveBonusMessage = (CSRLLockActiveBonusMessage)message;
				cSRLLockActiveBonusMessage.m_ActiveBonus.ToggleLocked = !cSRLLockActiveBonusMessage.m_ActiveBonus.ToggleLocked;
				break;
			}
			case EMessageType.EUPDATEATTACKPATH:
			{
				CSRLUpdateAttackPathMessage cSRLUpdateAttackPathMessage = (CSRLUpdateAttackPathMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					bool flag = true;
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability is CAbilityAttack cAbilityAttack)
					{
						cAbilityAttack.UpdateAllTargetsOnAttackPath(cSRLUpdateAttackPathMessage.m_OptionalTileList);
						flag = false;
					}
					if (flag && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbilities.Find((CPhaseAction.CPhaseAbility x) => x.m_Ability is CAbilityAttack)?.m_Ability is CAbilityAttack cAbilityAttack2)
					{
						cAbilityAttack2.UpdateAllTargetsOnAttackPath(cSRLUpdateAttackPathMessage.m_OptionalTileList);
						flag = false;
					}
					if (flag)
					{
						CSRLWrongPhaseException_MessageData message11 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
						MessageHandler(message11);
					}
				}
				else
				{
					CSRLWrongPhaseException_MessageData message12 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message12);
				}
				break;
			}
			case EMessageType.EUPDATEMOVECARRY:
			{
				CSRLUpdateMoveCarryMessage cSRLUpdateMoveCarryMessage = (CSRLUpdateMoveCarryMessage)message;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability is CAbilityMove cAbilityMove)
					{
						cAbilityMove.UpdateCarryActors(cSRLUpdateMoveCarryMessage.m_OptionalTileList, cSRLUpdateMoveCarryMessage.m_RemoveWaypoint);
						break;
					}
					CSRLWrongPhaseException_MessageData message9 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message9);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message10 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message10);
				}
				break;
			}
			case EMessageType.ETOGGLESCENARIOMODIFIERACTIVATION:
			{
				CSRLToggleScenarioModifierActivation toggleMessage = (CSRLToggleScenarioModifierActivation)message;
				ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == toggleMessage.m_ScenarioModifierEventIdentifier)?.SetDeactivated(!toggleMessage.m_Activate);
				break;
			}
			case EMessageType.ETOGGLEDOOR:
			{
				CSRLToggleDoorMessage toggleDoorMessage = (CSRLToggleDoorMessage)message;
				CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.FirstOrDefault((CObjectProp d) => d.PropGuid == toggleDoorMessage.m_DoorGuid);
				if (cObjectDoor == null)
				{
					break;
				}
				if (toggleDoorMessage.m_OpenDoor)
				{
					if (toggleDoorMessage.m_UnlockOnly)
					{
						cObjectDoor.UnlockLockedDoorWithoutOpening(null);
					}
					else
					{
						cObjectDoor.SetExtraLockState(lockedStateToSet: false);
					}
				}
				else
				{
					cObjectDoor.CloseOpenedDoor(toggleDoorMessage.m_LockDoor);
				}
				break;
			}
			case EMessageType.ESORTINITIATIVE:
				GameState.SortIntoInitiativeAndIDOrder();
				break;
			case EMessageType.ESETSTARTROUNDDECKSTATE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					CSRLSetStartRoundDeckStateMessage cSRLSetStartRoundDeckStateMessage = (CSRLSetStartRoundDeckStateMessage)message;
					cSRLSetStartRoundDeckStateMessage.m_PlayerActor.CharacterClass.ProxySetStartRoundDeckState(cSRLSetStartRoundDeckStateMessage.m_StartRoundCardState);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message7 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message7);
				}
				break;
			case EMessageType.EGETSTARTROUNDDECKSTATE:
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					CSRLGetStartRoundDeckStateMessage cSRLGetStartRoundDeckStateMessage = (CSRLGetStartRoundDeckStateMessage)message;
					CPlayerActor playerActor = cSRLGetStartRoundDeckStateMessage.m_PlayerActor;
					CReplicateStartRoundCardState_MessageData message5 = new CReplicateStartRoundCardState_MessageData(playerActor, cSRLGetStartRoundDeckStateMessage.m_GameActionID, playerActor.CharacterClass.GetCurrentCardState());
					MessageHandler(message5);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message6 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message6);
				}
				break;
			case EMessageType.ESHORTREST:
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					CSRLShortRestPlayerMessage cSRLShortRestPlayerMessage = (CSRLShortRestPlayerMessage)message;
					GameState.PlayerShortRested(cSRLShortRestPlayerMessage.PlayerActor, cSRLShortRestPlayerMessage.DiscardedAbilityCard, cSRLShortRestPlayerMessage.LoseHealth, cSRLShortRestPlayerMessage.UpdateScenarioRNG, cSRLShortRestPlayerMessage.FromStateUpdate);
				}
				else
				{
					CSRLWrongPhaseException_MessageData message2 = new CSRLWrongPhaseException_MessageData(PhaseManager.PhaseType.ToString(), message.m_MessageType, Environment.StackTrace);
					MessageHandler(message2);
				}
				break;
			}
			s_SRLLastProcessedMessageID = message.m_MessageID;
			SimpleLog.AddToSimpleLog("ScenarioRuleClient Finished ProcessMessage: " + message.m_MessageType);
		}
		catch (Exception ex)
		{
			CSRLException_MessageData message16 = new CSRLException_MessageData(ex.Message, ex.ToString());
			MessageHandler(message16);
		}
	}

	public static void SetMessageHandler(MessageHandlerCallback messageHandler)
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient setting message handler. \n" + Environment.StackTrace);
		s_MessageHandler = messageHandler;
	}

	public static bool InAbility(CAbility.EAbilityType abilityType)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
		{
			return ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == abilityType;
		}
		return false;
	}

	public static void Reset()
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient is resetting. \n" + Environment.StackTrace);
		GameState.Reset();
		PhaseManager.Reset();
		CPhaseAction.Reset();
		foreach (HeroSummonYMLData heroSummon in SRLYML.HeroSummons)
		{
			heroSummon.ResetEnhancementBonuses();
		}
		SEventLogMessageHandler.Reset();
		ResetQueue();
		s_BlockMessageProcessing = false;
		s_manualResetEventSlim.Set();
		s_SRLNextMessageID = 0u;
		s_SRLLastProcessedMessageID = 0u;
	}

	public static void SetNextAttackValueOverride(int value)
	{
		CAbilityAttack.SetNextAttackValueOverride(value);
	}

	private static void Work()
	{
		CancellationToken token = s_CancellationTokenSource.Token;
		try
		{
			while (true)
			{
				token.ThrowIfCancellationRequested();
				s_manualResetEventSlim.Wait(token);
				token.ThrowIfCancellationRequested();
				CSRLMessage message = s_BlockingCollection.Take(token);
				token.ThrowIfCancellationRequested();
				s_manualResetEventSlim.Wait(token);
				token.ThrowIfCancellationRequested();
				IsProcessing = true;
				ProcessMessage(message);
				IsProcessing = false;
			}
		}
		catch (OperationCanceledException)
		{
			LogUtils.Log("OperationCanceledException ThreadEventLogMessageHandler finished");
		}
		catch (ObjectDisposedException)
		{
			LogUtils.Log("ObjectDisposedException ThreadEventLogMessageHandler finished");
		}
		s_IsAlive = false;
	}

	public static void Start()
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient is starting. \n" + Environment.StackTrace);
		GameState.Start();
		if (s_WorkThread != null && s_IsAlive)
		{
			DLLDebug.LogError("ScenarioRuleClient Unable to start work thread as it is already running");
			return;
		}
		s_MainThread = Thread.CurrentThread;
		IsProcessing = false;
		ResetQueue();
		s_CancellationTokenSource = new CancellationTokenSource();
		s_WorkThread = new Thread(Work, 1048576);
		s_IsAlive = true;
		s_WorkThread.Start();
	}

	public static void Stop()
	{
		if (s_WorkThread != null && !s_CancellationTokenSource.IsCancellationRequested)
		{
			SimpleLog.AddToSimpleLog("ScenarioRuleClient is stopping. \n" + Environment.StackTrace);
			s_CancellationTokenSource.Cancel();
			s_WorkThread.Join();
		}
	}

	private static void ResetQueue()
	{
		s_MessageQueue = new ConcurrentQueue<CSRLMessage>();
		s_BlockingCollection = new BlockingCollection<CSRLMessage>(s_MessageQueue);
		if (s_manualResetEventSlim != null)
		{
			s_manualResetEventSlim.Dispose();
		}
		s_manualResetEventSlim = new ManualResetEventSlim(!s_BlockMessageProcessing);
	}

	public static uint AddSRLQueueMessage(CSRLMessage srlMessage, bool processImmediately)
	{
		SimpleLog.AddToSimpleLog("ScenarioRuleClient Add SRL queue message of type: " + srlMessage.m_MessageType.ToString() + " process immediately: " + processImmediately);
		if (s_IsAlive)
		{
			srlMessage.m_MessageID = ++s_SRLNextMessageID;
			srlMessage.m_CallerStack = Environment.StackTrace;
			if (!processImmediately)
			{
				s_BlockingCollection.Add(srlMessage);
				return srlMessage.m_MessageID;
			}
			ProcessMessage(srlMessage);
		}
		else
		{
			SimpleLog.AddToSimpleLog("ScenarioRuleClient thread was not alive when trying to add SRL queue message");
		}
		return 0u;
	}

	public static uint StepComplete(bool processImmediately = false, bool fromSRL = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.ESTEPCOMPLETEMESSAGE), processImmediately);
	}

	public static uint NextPhase(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.ENEXTPHASEMESSAGE), processImmediately);
	}

	public static uint TileSelected(CTile tile, List<CTile> optionalTileList, bool processImmediately = false)
	{
		string text = "Passing tiles to SRL for processing: \nTile: " + tile.m_ArrayIndex.ToString() + "\n";
		if (optionalTileList != null && optionalTileList.Count > 0)
		{
			text += "Optional Tile List: ";
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				CTile cTile = optionalTileList[i];
				text = text + "\nOptional Tile: " + cTile.m_ArrayIndex.ToString();
			}
		}
		SimpleLog.AddToSimpleLog(text);
		return AddSRLQueueMessage(new CSRLTileSelectedMessage(tile, optionalTileList), processImmediately);
	}

	public static uint TileDeselected(CTile tile, List<CTile> optionalTileList, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLTileDeselectedMessage(tile, optionalTileList), processImmediately);
	}

	public static uint ApplySingleTarget(CActor actor, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLApplySingleTargetMessage(actor), processImmediately);
	}

	public static uint ToggleActionAugmentation(CActionAugmentation actionAugmentation, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLToggleActionAugmentationMessage(actionAugmentation), processImmediately);
	}

	public static uint ToggleActiveBonus(CActiveBonus activeBonus, CActor actor, ElementInfusionBoardManager.EElement? element, int phaseWhenClickedInt, object extraOption = null, bool processImmediately = false, ToggleState futureToggle = ToggleState.Default)
	{
		return AddSRLQueueMessage(new CSRLToggleActiveBonusMessage(activeBonus, element, actor, phaseWhenClickedInt, extraOption, futureToggle), processImmediately);
	}

	public static uint LockActiveBonus(CActiveBonus activeBonus, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLLockActiveBonusMessage(activeBonus), processImmediately);
	}

	public static uint ToggleScenarioModifierActivation(string modifierEventIdentifier, bool activate, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLToggleScenarioModifierActivation(modifierEventIdentifier, activate), processImmediately);
	}

	public static uint ToggleDoor(string doorGuid, bool openDoor, bool unlockOnly = false, bool lockDoor = false, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLToggleDoorMessage(doorGuid, openDoor, unlockOnly, lockDoor), processImmediately);
	}

	public static uint ToggleItem(CItem item, CActor actor, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLToggleItemMessage(item, actor), processImmediately);
	}

	public static uint ClearRoundAbilityCards(CCharacterClass characterClass)
	{
		return AddSRLQueueMessage(new CSRLClearRoundAbilityCardsMessage(characterClass), processImmediately: false);
	}

	public static uint MoveAbilityCard(CCharacterClass characterClass, CAbilityCard abilityCard, List<CAbilityCard> fromAbilityCardList, List<CAbilityCard> toAbilityCardList, string fromAbilityCardListName, string toAbilityCardListName, bool networkAction)
	{
		return AddSRLQueueMessage(new CSRLMoveAbilityCardMessage(characterClass, abilityCard, fromAbilityCardList, toAbilityCardList, fromAbilityCardListName, toAbilityCardListName, networkAction), processImmediately: false);
	}

	public static uint Pass(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.EPASSMESSAGE), processImmediately);
	}

	public static uint PassStep(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.EPASSSTEPMESSAGE), processImmediately);
	}

	public static uint EndAbilitySynchronise(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.EENDABILITYSYNCHRONISEMESSAGE), processImmediately);
	}

	public static uint EndTurnSynchronise(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.EENDTURNSYNCHRONISEMESSAGE), processImmediately);
	}

	public static uint EndRoundSynchronise(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.EENDROUNDSYNCHRONISEMESSAGE), processImmediately);
	}

	public static uint Undo(List<CActiveBonus> activeBonusesToUntoggle)
	{
		return AddSRLQueueMessage(new CSRLUndoMessage(activeBonusesToUntoggle), processImmediately: false);
	}

	public static uint ClearTargets(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.ECLEARTARGETSMESSAGE), processImmediately);
	}

	public static uint UpdateAttackpath(List<CTile> optionalTileList, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLUpdateAttackPathMessage(optionalTileList), processImmediately);
	}

	public static uint UpdateMoveCarry(List<CTile> optionalTileList, bool removeWaypoint = false, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLUpdateMoveCarryMessage(optionalTileList, removeWaypoint), processImmediately);
	}

	public static uint SortInitiative(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.ESORTINITIATIVE), processImmediately);
	}

	public static uint ShortRestPlayer(CPlayerActor playerActor, CAbilityCard discardedCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate = false, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLShortRestPlayerMessage(playerActor, discardedCard, loseHealth, updateScenarioRNG, fromStateUpdate), processImmediately);
	}

	public static uint ProxySetStartRoundDeckState(CPlayerActor playerActor, StartRoundCardState startRoundCardState, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLSetStartRoundDeckStateMessage(playerActor, startRoundCardState), processImmediately);
	}

	public static uint GetAndReplicateStartRoundDeckState(CPlayerActor playerActor, int gameActionID, bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLGetStartRoundDeckStateMessage(playerActor, gameActionID), processImmediately);
	}

	public static List<CActor> GetActorsInRange(CActor targetActor, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, bool? canTargetInvisible = false)
	{
		return GameState.GetActorsInRange(targetActor, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, areaEffectTiles, isTargetedAbility, null, canTargetInvisible);
	}

	public static uint Stop(bool processImmediately = false)
	{
		return AddSRLQueueMessage(new CSRLMessage(EMessageType.ESTOPMESSAGE), processImmediately);
	}

	public static double NextDouble(SharedLibrary.Random rnd, double min, double max)
	{
		return rnd.NextDouble() * (max - min) + min;
	}

	public static void FirstAbilityStarted()
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			((CPhaseAction)PhaseManager.Phase).OnFirstAbilityStarted();
		}
	}
}
