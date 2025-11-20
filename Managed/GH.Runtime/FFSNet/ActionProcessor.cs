#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEC;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FFSNet;

public static class ActionProcessor
{
	public struct ProcessorState
	{
		public ActionProcessorStateType StateType { get; private set; }

		public ActionPhaseType PhaseType { get; private set; }

		public ProcessorState(ActionProcessorStateType stateType, ActionPhaseType phaseType)
		{
			StateType = stateType;
			PhaseType = phaseType;
		}
	}

	private static Queue<GameAction> actionQueue = new Queue<GameAction>();

	private static List<GameAction> processedActionLog = new List<GameAction>();

	private static ProcessorState currentState;

	private static ProcessorState previousState;

	private static ProcessorState savedState;

	private static bool readyToProcessNextAction;

	private static bool processingAction;

	private static bool lockProcessingAction;

	private static CoroutineHandle actionProcessingRoutine;

	private static bool processingRoutineRunning;

	private static int incorrectActionsDetectedCounter;

	public static int MaxConsecutiveIncorrectActionsAllowed { get; set; }

	public static bool IsProcessing => processingAction;

	public static ActionPhaseType CurrentPhase => currentState.PhaseType;

	public static bool HasSavedState => !savedState.Equals(default(ProcessorState));

	public static bool HasSavedStateInSamePhase
	{
		get
		{
			if (!savedState.Equals(default(ProcessorState)))
			{
				return savedState.PhaseType == CurrentPhase;
			}
			return false;
		}
	}

	public static GameAction IsWaitingForTileSelectionFinishedMessage { get; set; }

	public static float QueueDelay { get; set; }

	public static ProcessorState CurrentState => currentState;

	public static ProcessorState PreviousState => previousState;

	public static ProcessorState SavedState => savedState;

	public static bool ReadyToProcessNextAction => readyToProcessNextAction;

	public static Queue<GameAction> ActionQueue => actionQueue;

	public static void SetState(ActionProcessorStateType stateType, ActionPhaseType phaseType = ActionPhaseType.NONE, bool savePreviousState = false, bool processQueueImmediately = false)
	{
		if (stateType == currentState.StateType && phaseType == currentState.PhaseType)
		{
			return;
		}
		previousState = currentState;
		currentState = ((stateType == ActionProcessorStateType.SwitchBackToSavedState) ? savedState : new ProcessorState(stateType, phaseType));
		if (stateType == ActionProcessorStateType.SwitchBackToSavedState)
		{
			savedState = default(ProcessorState);
		}
		else if (savePreviousState && stateType != ActionProcessorStateType.SwitchBackToSavedState)
		{
			savedState = previousState;
		}
		Console.LogCoreInfo("STATE: " + currentState.StateType.ToString() + " @ " + currentState.PhaseType, customFlag: true);
		if (currentState.StateType.In(ActionProcessorStateType.ProcessFreely, ActionProcessorStateType.ProcessOneAndHalt, ActionProcessorStateType.ProcessOneAndSwitchBack))
		{
			readyToProcessNextAction = true;
			if (FFSNetwork.IsOnline)
			{
				StartUpActionProcessing(processQueueImmediately);
			}
		}
		else if (currentState.StateType == ActionProcessorStateType.Halted)
		{
			readyToProcessNextAction = false;
		}
		else
		{
			Console.LogError("ERROR_MULTIPLAYER_00014", "Cannot process state: " + currentState.StateType);
		}
	}

	public static void SetStateOnControllerChanged(GameAction action)
	{
		if (action.ActionTypeID == 78 && action.DataInt != 0)
		{
			if (CurrentState.StateType != ActionProcessorStateType.NONE)
			{
				Console.Log("[TEST] Sending SendBackActionProcessorState.  MyPlayer = " + PlayerRegistry.MyPlayer.PlayerID + " Other Player = " + action.PlayerID + " State = " + CurrentState.StateType);
				Synchronizer.SendSideAction(GameActionType.SendBackActionProcessorState, null, canBeUnreliable: false, sendToHostOnly: false, action.PlayerID, (int)CurrentState.StateType);
			}
			SetState(action);
		}
		else
		{
			Console.LogError("ERROR_MULTIPLAYER_00014", "Incorrect Values sent to SetStateOnControllerChanged.  Action Type '" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + "' State = '" + (ActionProcessorStateType)action.DataInt/*cast due to .constrained prefix*/);
		}
	}

	public static void SetState(GameAction action)
	{
		if ((action.ActionTypeID == 78 || action.ActionTypeID == 79) && action.DataInt != 0)
		{
			currentState = new ProcessorState((ActionProcessorStateType)action.DataInt, currentState.PhaseType);
			Console.LogCoreInfo("STATE: " + currentState.StateType.ToString() + " @ " + currentState.PhaseType, customFlag: true);
			if (currentState.StateType.In(ActionProcessorStateType.ProcessFreely, ActionProcessorStateType.ProcessOneAndHalt, ActionProcessorStateType.ProcessOneAndSwitchBack))
			{
				readyToProcessNextAction = true;
				if (FFSNetwork.IsOnline)
				{
					StartUpActionProcessing(processQueueImmediately: false);
				}
			}
			else if (currentState.StateType == ActionProcessorStateType.Halted)
			{
				readyToProcessNextAction = false;
			}
			else
			{
				Console.LogError("ERROR_MULTIPLAYER_00014", "Cannot process state: " + currentState.StateType);
			}
		}
		else
		{
			Console.LogError("ERROR_MULTIPLAYER_00014", "Incorrect Values sent to SetStateOnControllerChanged.  Action Type '" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + "' State = '" + (ActionProcessorStateType)action.DataInt/*cast due to .constrained prefix*/);
		}
	}

	public static void SaveCurrentState()
	{
		savedState = currentState;
	}

	public static void ClearSavedState()
	{
		savedState = default(ProcessorState);
	}

	public static void ProcessSideAction(GameAction action)
	{
		if (PlayerRegistry.MyPlayer == null)
		{
			GameActionType actionTypeID = (GameActionType)action.ActionTypeID;
			if (actionTypeID != GameActionType.EndOfRoundAllPlayersReady && actionTypeID != GameActionType.EndOfTurnAllPlayersReady && actionTypeID != GameActionType.EndOfAbilityAllPlayersReady)
			{
				ActionProcessorCoroutineHelper.RunCoroutine(ProcessSideActionCoroutine(action));
			}
			return;
		}
		try
		{
			if (action.TargetPlayerID == 0 || action.TargetPlayerID == PlayerRegistry.MyPlayer.PlayerID)
			{
				Console.LogInfo("Processing SideAction (" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + "), DataTOKEN: " + ((action.SupplementaryDataToken != null) ? "YES" : "NO"));
				action.Execute();
				return;
			}
			Console.LogInfo("Ignoring SideAction (" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + ") as TargetPlayerID does not match. TargetPlayerID: " + action.TargetPlayerID + " My PlayerID: " + PlayerRegistry.MyPlayer.PlayerID);
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
			throw;
		}
	}

	private static IEnumerator ProcessSideActionCoroutine(GameAction action)
	{
		while (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer == null)
		{
			yield return null;
		}
		if (FFSNetwork.IsOnline)
		{
			ProcessSideAction(action);
		}
	}

	public static void QueueUpAction(GameAction action, bool processQueueImmediately = false)
	{
		if (QueueDelay > 0f)
		{
			ActionProcessorCoroutineHelper.RunCoroutine(QueueUpActionAfterDelay(action, processQueueImmediately));
			return;
		}
		actionQueue.Enqueue(action);
		Console.LogInfo("New action queued. Queue size: " + actionQueue.Count);
		StartUpActionProcessing(processQueueImmediately);
	}

	private static IEnumerator QueueUpActionAfterDelay(GameAction action, bool processQueueImmediately = false)
	{
		Console.LogInfo("Waiting to add new action queued. Queue size: " + actionQueue.Count);
		yield return new WaitForSeconds(QueueDelay);
		actionQueue.Enqueue(action);
		Console.LogInfo("New action queued. Queue size: " + actionQueue.Count);
		StartUpActionProcessing(processQueueImmediately);
	}

	private static void StartUpActionProcessing(bool processQueueImmediately)
	{
		if ((!processQueueImmediately || !TryProcessNextAction()) && !processingRoutineRunning)
		{
			actionProcessingRoutine = Timing.RunCoroutine(TryProcessActions());
		}
	}

	private static IEnumerator<float> TryProcessActions()
	{
		yield return float.NegativeInfinity;
		if (FFSNetwork.IsOnline)
		{
			Console.LogInfo("Starting a new action processing coroutine.");
		}
		processingRoutineRunning = true;
		while ((readyToProcessNextAction || actionQueue.Count > 0) && !FFSNetwork.HasDesynchronized)
		{
			if (TryProcessNextAction() && actionQueue.Count > 0)
			{
				yield return Timing.WaitForSeconds(1f);
			}
			else
			{
				yield return Timing.WaitForSeconds(FFSNetwork.Manager.ActionQueueProcessingInterval);
			}
		}
		processingRoutineRunning = false;
	}

	public static void UnlockProcessingAction()
	{
		lockProcessingAction = false;
	}

	public static void LockProcessingAction()
	{
		lockProcessingAction = true;
	}

	private static bool TryProcessNextAction()
	{
		try
		{
			if (!lockProcessingAction && readyToProcessNextAction && actionQueue.Count > 0 && !processingAction)
			{
				GameAction action = actionQueue.Peek();
				if (action == null)
				{
					throw new Exception("Error processing action. Action returns null.");
				}
				if (PlayerRegistry.GetPlayer(action.PlayerID) == null)
				{
					Console.LogCoreInfo("Attempting to process message from NULL player (PlayerID " + action.PlayerID + ").  Skipping this action", customFlag: true);
					actionQueue.Dequeue();
					return true;
				}
				if (action.ActionTypeID == 98 && PlayerRegistry.IsSwitchingCharacter && PlayerRegistry.SwitchingPlayerID != action.DataInt)
				{
					Debug.Log("ToggleIsSwitchingCharacter action discarded as another player is already switching.  PlayerRegistry.SwitchingPlayerID: " + PlayerRegistry.SwitchingPlayerID + "  action.DataInt: " + action.DataInt);
					actionQueue.Dequeue();
					return true;
				}
				if (FFSNetwork.IsHost)
				{
					if (action.ActionTypeID == 3 && PlayerRegistry.IsSwitchingCharacter && (action.SupplementaryDataIDMax != PlayerRegistry.SwitchingPlayerID || action.SupplementaryDataIDMed != PlayerRegistry.SwitchingCharacterSlot))
					{
						Debug.Log(((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " action discarded as another player is already switching.  PlayerRegistry.SwitchingPlayerID: " + PlayerRegistry.SwitchingPlayerID + "  action.SupplementaryDataIDMax: " + action.SupplementaryDataIDMax + " PlayerRegistry.SwitchingCharacterSlot: " + PlayerRegistry.SwitchingCharacterSlot + " action.SupplementaryDataIDMed: " + action.SupplementaryDataIDMed);
						actionQueue.Dequeue();
						return true;
					}
					if (action.ActionTypeID == 4 && PlayerRegistry.IsSwitchingCharacter && action.SupplementaryDataIDMax != PlayerRegistry.SwitchingPlayerID)
					{
						Debug.Log(((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " action discarded as another player is already switching.  PlayerRegistry.SwitchingPlayerID: " + PlayerRegistry.SwitchingPlayerID + "  action.SupplementaryDataIDMax: " + action.SupplementaryDataIDMax);
						actionQueue.Dequeue();
						return true;
					}
				}
				if (action.TargetPhaseID == 27 && (currentState.PhaseType == ActionPhaseType.MapHQ || currentState.PhaseType == ActionPhaseType.MapAtLinkedScenario))
				{
					actionQueue.Dequeue();
					return true;
				}
				if ((action.ActionTypeID == 38 || action.ActionTypeID == 39) && action.TargetPhaseID == 8 && currentState.PhaseType == ActionPhaseType.TargetSelection)
				{
					action.TargetPhaseID = 16;
				}
				if (action.TargetPhaseID == 1 && currentState.PhaseType == ActionPhaseType.MapAtLinkedScenario)
				{
					action.TargetPhaseID = 30;
				}
				if (action.TargetPhaseID == 30 && currentState.PhaseType == ActionPhaseType.MapHQ)
				{
					action.TargetPhaseID = 1;
				}
				if (currentState.PhaseType == ActionPhaseType.ScenarioEnded && action.TargetPhaseID != 27)
				{
					actionQueue.Dequeue();
					return true;
				}
				if (currentState.PhaseType == ActionPhaseType.StartOfRound && action.TargetPhaseID == 3 && (action.ActionTypeID == 22 || action.ActionTypeID == 20 || action.ActionTypeID == 21))
				{
					actionQueue.Dequeue();
					return true;
				}
				if (currentState.PhaseType != ActionPhaseType.StartOfRound && action.TargetPhaseID == 4 && (action.ActionTypeID == 20 || action.ActionTypeID == 21 || action.ActionTypeID == 43))
				{
					actionQueue.Dequeue();
					return true;
				}
				if (action.ActionTypeID == 22 && action.TargetPhaseID != (int)CurrentPhase)
				{
					NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == action.TargetPlayerID);
					if (networkPlayer == null || !networkPlayer.IsActive)
					{
						actionQueue.Dequeue();
						return true;
					}
				}
				if (currentState.PhaseType == ActionPhaseType.MapLoadoutScreen && action.TargetPhaseID == 1 && (action.ActionTypeID == 22 || action.ActionTypeID == 20 || action.ActionTypeID == 21))
				{
					actionQueue.Dequeue();
					return true;
				}
				if ((action.ActionTypeID == 20 || action.ActionTypeID == 21) && FFSNetwork.IsClient && action.TargetPhaseID != (int)currentState.PhaseType && action.TargetPhaseID != 0)
				{
					actionQueue.Dequeue();
					return true;
				}
				if (action.ActionTypeID == 45 && ((ScenarioRuleClient.IsProcessingOrMessagesQueued && !GameState.WaitingForPlayerToSelectDamageResponse && !GameState.WaitingForPlayerActorToAvoidDamageResponse) || Choreographer.s_Choreographer.m_MessageQueue.Count > 0))
				{
					Console.LogWarning("Unable to process ConfirmAction until SRL and choreographer are done processing their messages");
					return false;
				}
				if (action.TargetPhaseID != (int)currentState.PhaseType && action.TargetPhaseID != 0)
				{
					incorrectActionsDetectedCounter++;
					if (incorrectActionsDetectedCounter == 1)
					{
						Console.LogWarning("Incorrect action detected (Action: " + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " | TargetPhase: " + ((ActionPhaseType)action.TargetPhaseID/*cast due to .constrained prefix*/).ToString() + "). CurrentPhase: " + currentState.PhaseType.ToString() + ". Desync countdown started.");
					}
					else
					{
						Console.LogWarning("Incorrect action detected. Counter: " + incorrectActionsDetectedCounter + "/" + MaxConsecutiveIncorrectActionsAllowed);
					}
					if (incorrectActionsDetectedCounter >= MaxConsecutiveIncorrectActionsAllowed)
					{
						NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
						NetworkPlayer myPlayer = PlayerRegistry.MyPlayer;
						string text = ((!FFSNetwork.Manager.DisplayControllableNameForIncorrectActionErrors) ? string.Empty : ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject?.GetName());
						string text2 = string.Empty;
						if (action.BinaryDataIncludesLoggingDetails && action.CustomBinaryData != null)
						{
							text2 = Encoding.ASCII.GetString(action.CustomBinaryData);
						}
						throw new Exception("Error processing action. Timed out after " + FFSNetwork.Manager.IncorrectActionDetectedTimeOutDuration + " seconds waiting for a suitable phase to play the action in a " + PlayerRegistry.Participants.Count + "-player game. Acting Player: " + ((player != null) ? (player.Username + " (ID: " + player.PlayerID + ", " + (player.IsClient ? "CLIENT" : "HOST") + ((!text.IsNullOrEmpty()) ? (", " + text) : "") + ")") : "NULL") + ", Action: " + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " (Specifics: " + ((!text2.IsNullOrEmpty()) ? text2 : "-") + "), TargetPhase: " + ((ActionPhaseType)action.TargetPhaseID/*cast due to .constrained prefix*/).ToString() + ". My Player: " + ((myPlayer != null) ? (myPlayer.Username + " (ID: " + myPlayer.PlayerID + ", " + (myPlayer.IsClient ? "CLIENT" : "HOST") + ")") : "NULL") + ", Phase: " + currentState.PhaseType.ToString() + ".");
					}
					return false;
				}
				incorrectActionsDetectedCounter = 0;
				ProcessAction(action);
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
			return false;
		}
	}

	private static bool ProcessAction(GameAction action)
	{
		Console.LogInfo("Processing action STARTED.");
		processingAction = true;
		actionQueue.Dequeue();
		if (currentState.StateType == ActionProcessorStateType.ProcessOneAndHalt)
		{
			SetState(ActionProcessorStateType.Halted);
		}
		string text = string.Empty;
		try
		{
			if (FFSNetwork.IsHost)
			{
				switch ((GameActionType)action.ActionTypeID)
				{
				case GameActionType.BuyBlessing:
					if (action.SupplementaryDataToken is BlessingToken blessingToken)
					{
						text = text + " Blessed Character ID: " + blessingToken.CharacterID;
					}
					break;
				case GameActionType.CampaignAssignPersonalQuest:
					if (action.SupplementaryDataToken is CampaignPersonalQuestData campaignPersonalQuestData)
					{
						text = text + " Character ID: " + campaignPersonalQuestData.CharacterID;
						text = text + " Character Name: " + campaignPersonalQuestData.CharacterName;
						text = text + " Personal Quest ID: " + campaignPersonalQuestData.PersonalQuestID;
					}
					break;
				}
			}
		}
		catch
		{
		}
		Console.LogCoreInfo("[[Processing GameAction #" + action.ActionID + " (" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " @ " + ((ActionPhaseType)action.TargetPhaseID/*cast due to .constrained prefix*/).ToString() + ") initiated by " + PlayerRegistry.GetPlayer(action.PlayerID).Username + "(PlayerID: " + action.PlayerID + ")." + text + "]]", customFlag: true);
		if (action.Execute())
		{
			FinishProcessingAction(action);
			return true;
		}
		return false;
	}

	public static void FinishProcessingAction(GameAction action)
	{
		if (!FFSNetwork.IsOnline)
		{
			return;
		}
		if (FFSNetwork.IsHost && action.IsValid)
		{
			action.ActionID = ++PlayerRegistry.MyPlayer.state.LatestProcessedActionID;
			LogAction(action);
			if (!action.DoNotForwardAction)
			{
				Synchronizer.ForwardGameActionToClients(action);
			}
		}
		processingAction = false;
		Console.LogInfo("Processing action FINISHED. Remaining in queue: " + actionQueue.Count);
		if (currentState.StateType == ActionProcessorStateType.ProcessOneAndSwitchBack)
		{
			Console.LogInfo("Switching back to previous state.");
			SetState(previousState.StateType, previousState.PhaseType);
		}
	}

	public static void LogAction(GameAction action)
	{
		Console.LogInfo("Action #" + action.ActionID + " logged.");
		processedActionLog.Add(action);
	}

	public static List<GameAction> GetProcessedActions(int firstActionID, int lastActionID)
	{
		if (lastActionID < firstActionID)
		{
			lastActionID = firstActionID;
		}
		return processedActionLog.FindAll((GameAction x) => x.ActionID >= firstActionID && x.ActionID <= lastActionID);
	}

	public static void OnRestartRound()
	{
		actionQueue.Clear();
		currentState = new ProcessorState(ActionProcessorStateType.Halted, ActionPhaseType.StartOfRound);
		previousState = default(ProcessorState);
		savedState = default(ProcessorState);
		Timing.KillCoroutines(actionProcessingRoutine);
		ActionProcessorCoroutineHelper.StopAllCoroutinesOnHelper();
		processingRoutineRunning = false;
		readyToProcessNextAction = false;
	}

	public static void Shutdown()
	{
		currentState = default(ProcessorState);
		previousState = default(ProcessorState);
		savedState = default(ProcessorState);
		actionQueue.Clear();
		processedActionLog.Clear();
		Timing.KillCoroutines(actionProcessingRoutine);
		ActionProcessorCoroutineHelper.StopAllCoroutinesOnHelper();
		processingRoutineRunning = false;
		readyToProcessNextAction = false;
		processingAction = false;
		incorrectActionsDetectedCounter = 0;
	}
}
