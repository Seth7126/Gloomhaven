using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelEvent : ISerializable
{
	[Serializable]
	public enum ELevelEventType
	{
		RevealCharacterAfterAnim,
		SetShortRestCard,
		SetCharacterSelectedRoundCards,
		SetCameraPosition,
		LoseScenario,
		WinScenario,
		EditSpawner,
		UnlockDoor,
		DeactivateModifier,
		SpawnMonsterOnActor,
		ActivateModifier,
		DeactivateObjective,
		ActivateObjective,
		SetModifierHiddenState,
		RemoveActiveBonusFromCurrentActor,
		CloseDoor,
		TriggerScenarioModifier,
		SpawnPropOnActor,
		TriggerScenarioModifierOnCurrentActor,
		RemoveDifficultTerrain,
		LoseGoalChestRewardChoice,
		SpawnMonsterOnProp,
		KillActor,
		KillPropWithHealth,
		SpawnPropOnProp,
		SpawnMonsterOnLastDestroyedObstacle,
		SetActorAnimParameter,
		TriggerPlayableDirectorOnGameObject
	}

	public enum ELevelEventSpawnerEditType
	{
		ActivateSpawner,
		DeactivateSpawner,
		TriggerSpawn,
		OnlyActivateSpawner
	}

	public static ELevelEventType[] LevelEventTypes = (ELevelEventType[])Enum.GetValues(typeof(ELevelEventType));

	public static ELevelEventSpawnerEditType[] LevelEventSpawnerEditTypes = (ELevelEventSpawnerEditType[])Enum.GetValues(typeof(ELevelEventSpawnerEditType));

	public ELevelEventType EventType { get; set; }

	public CLevelTrigger DisplayTrigger { get; set; }

	public bool Repeats { get; set; }

	public string EventResource { get; set; }

	public string EventSecondResource { get; set; }

	public string EventThirdResource { get; set; }

	public string EventFourthResource { get; set; }

	public float EventFloatResource { get; set; }

	public CLevelCameraProfile EventCameraProfile { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("EventType", EventType);
		info.AddValue("DisplayTrigger", DisplayTrigger);
		info.AddValue("Repeats", Repeats);
		info.AddValue("EventResource", EventResource);
		info.AddValue("EventSecondResource", EventSecondResource);
		info.AddValue("EventThirdResource", EventThirdResource);
		info.AddValue("EventFourthResource", EventFourthResource);
		info.AddValue("EventFloatResource", EventFloatResource);
		info.AddValue("EventCameraProfile", EventCameraProfile);
	}

	public CLevelEvent(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "EventType":
					EventType = (ELevelEventType)info.GetValue("EventType", typeof(ELevelEventType));
					break;
				case "DisplayTrigger":
					DisplayTrigger = (CLevelTrigger)info.GetValue("DisplayTrigger", typeof(CLevelTrigger));
					break;
				case "Repeats":
					Repeats = info.GetBoolean("Repeats");
					break;
				case "EventResource":
					EventResource = info.GetString("EventResource");
					break;
				case "EventSecondResource":
					EventSecondResource = info.GetString("EventSecondResource");
					break;
				case "EventThirdResource":
					EventThirdResource = info.GetString("EventThirdResource");
					break;
				case "EventFourthResource":
					EventFourthResource = info.GetString("EventFourthResource");
					break;
				case "EventFloatResource":
					EventFloatResource = info.GetSingle("EventFloatResource");
					break;
				case "EventCameraProfile":
					EventCameraProfile = (CLevelCameraProfile)info.GetValue("EventCameraProfile", typeof(CLevelCameraProfile));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CLevelEvent entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelEvent()
	{
		EventType = ELevelEventType.RevealCharacterAfterAnim;
		DisplayTrigger = new CLevelTrigger();
		Repeats = false;
		EventResource = string.Empty;
		EventSecondResource = string.Empty;
		EventThirdResource = string.Empty;
		EventFourthResource = string.Empty;
		EventFloatResource = 0f;
		EventCameraProfile = new CLevelCameraProfile();
	}

	public CLevelEvent(CLevelEvent state, ReferenceDictionary references)
	{
		EventType = state.EventType;
		DisplayTrigger = references.Get(state.DisplayTrigger);
		if (DisplayTrigger == null && state.DisplayTrigger != null)
		{
			DisplayTrigger = new CLevelTrigger(state.DisplayTrigger, references);
			references.Add(state.DisplayTrigger, DisplayTrigger);
		}
		Repeats = state.Repeats;
		EventResource = state.EventResource;
		EventSecondResource = state.EventSecondResource;
		EventThirdResource = state.EventThirdResource;
		EventFourthResource = state.EventFourthResource;
		EventFloatResource = state.EventFloatResource;
		EventCameraProfile = references.Get(state.EventCameraProfile);
		if (EventCameraProfile == null && state.EventCameraProfile != null)
		{
			EventCameraProfile = new CLevelCameraProfile(state.EventCameraProfile, references);
			references.Add(state.EventCameraProfile, EventCameraProfile);
		}
	}
}
