using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorEventsPanel : MonoBehaviour
{
	[Header("Main Event Panel")]
	public GameObject EventDisplayPanel;

	[Space]
	public GameObject EventItemPrefab;

	public Transform EventListItemParent;

	[Space]
	public TMP_Dropdown EventTypeDropDown;

	[Space]
	public LayoutElement RepeatsElement;

	public Toggle RepeatsToggle;

	[Space]
	public LayoutElement ResourceElement;

	public TextMeshProUGUI ResourceTitle;

	public InputField ResourceInput;

	[Space]
	public LayoutElement Resource1DropDownElement;

	public TextMeshProUGUI Resource1DropDownTitle;

	public TMP_Dropdown Resource1DropDown;

	[Space]
	public LayoutElement Resource2Element;

	public TextMeshProUGUI Resource2Title;

	public InputField Resource2Input;

	[Space]
	public LayoutElement Resource2DropDownElement;

	public TextMeshProUGUI Resource2DropDownTitle;

	public TMP_Dropdown Resource2DropDown;

	[Space]
	public LayoutElement Resource3Element;

	public TextMeshProUGUI Resource3Title;

	public InputField Resource3Input;

	[Space]
	public LayoutElement FloatResourceElement;

	public TextMeshProUGUI FloatResourceTitle;

	public InputField FloatResourceInput;

	[Space]
	public LayoutElement CameraProfileElement;

	public LevelEditorCameraProfilePanel CameraProfilePanel;

	[Space]
	public Button ApplyButton;

	public Button DeleteButton;

	[Space]
	public LevelEditorEventTriggerPanel EventTrigger;

	[Space]
	[Header("Per Party Size Config")]
	public GameObject PerPartySizeConfigGO;

	public TMP_Dropdown PartySizeOneConfigDropDown;

	public TMP_Dropdown PartySizeTwoConfigDropDown;

	public TMP_Dropdown PartySizeThreeConfigDropDown;

	public TMP_Dropdown PartySizeFourConfigDropDown;

	public const char PerPartySizeConfigDelimiter = '|';

	public const string OPTION_NONE = "None";

	private LevelEditorEventItem m_CurrentEventItem;

	private List<LevelEditorEventItem> m_EventItems = new List<LevelEditorEventItem>();

	private void Awake()
	{
		EventTypeDropDown.options.Clear();
		EventTypeDropDown.AddOptions(CLevelEvent.LevelEventTypes.Select((CLevelEvent.ELevelEventType s) => s.ToString()).ToList());
	}

	public void RefreshUIWithLoadedLevel()
	{
		foreach (LevelEditorEventItem eventItem in m_EventItems)
		{
			UnityEngine.Object.Destroy(eventItem.gameObject);
		}
		m_EventItems.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents != null)
		{
			for (int i = 0; i < SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Count; i++)
			{
				AddEventItemToUI(SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents[i], i);
			}
		}
	}

	private LevelEditorEventItem AddEventItemToUI(CLevelEvent eventToAddFor, int index)
	{
		LevelEditorEventItem component = UnityEngine.Object.Instantiate(EventItemPrefab, EventListItemParent).GetComponent<LevelEditorEventItem>();
		component.Init(eventToAddFor, index, EventItemClicked);
		m_EventItems.Add(component);
		return component;
	}

	private void ConfigureUIForEventType(CLevelEvent.ELevelEventType type)
	{
		switch (type)
		{
		case CLevelEvent.ELevelEventType.RevealCharacterAfterAnim:
			ResourceTitle.text = "Character to reveal:";
			Resource2Title.text = "Animation Prefab resource path:";
			FloatResourceTitle.text = "Reveal delay:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: true);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SetShortRestCard:
			ResourceTitle.text = "Card to try draw on next ShortRest:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SetCharacterSelectedRoundCards:
			ResourceTitle.text = "Character to set cards for:";
			Resource2Title.text = "First Card name:";
			Resource3Title.text = "Second Card name:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: true);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SetCameraPosition:
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.EditSpawner:
			Resource1DropDownTitle.text = "Spawner To Edit:";
			Resource2DropDownTitle.text = "Spawner Edit Type:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.UnlockDoor:
			Resource1DropDownTitle.text = "Door to Unlock";
			Resource2DropDownTitle.text = "Should open door too:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.CloseDoor:
			Resource1DropDownTitle.text = "Door to close";
			Resource2DropDownTitle.text = "Should lock door too:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.DeactivateModifier:
			Resource1DropDownTitle.text = "Modifier to Deactivate";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.ActivateModifier:
			Resource1DropDownTitle.text = "Modifier to Activate";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
			Resource1DropDownTitle.text = "Modifier to Trigger";
			Resource2DropDownTitle.text = "Non-current Actor GUID Override";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.DeactivateObjective:
			Resource1DropDownTitle.text = "Objective to Deactivate";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.ActivateObjective:
			Resource1DropDownTitle.text = "Objective to Activate";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			Resource2DropDownTitle.text = "Actor to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
			Resource1DropDownTitle.text = "Prop to Spawn:";
			Resource2DropDownTitle.text = "Actor to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
			Resource1DropDownTitle.text = "Prop to Spawn:";
			Resource2DropDownTitle.text = "Prop to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			Resource2DropDownTitle.text = "Prop to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
			Resource1DropDownTitle.text = "Modifier to edit";
			Resource2DropDownTitle.text = "Hidden state to set:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
			ResourceTitle.text = "Active Name to Remove:";
			Resource2Title.text = "Active Name to Remove:";
			Resource3Title.text = "Active Name to Remove:";
			Resource1DropDownTitle.text = "Non-current Actor GUID Override";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: true);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
			Resource1DropDownTitle.text = "Difficult terrain to remove";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.LoseGoalChestRewardChoice:
			ResourceTitle.text = "Number of rewards to lose";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.KillActor:
			Resource1DropDownTitle.text = "Actor to kill:";
			RepeatsElement.gameObject.SetActive(value: false);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
			Resource1DropDownTitle.text = "Prop to kill:";
			RepeatsElement.gameObject.SetActive(value: false);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
			Resource1DropDownTitle.text = "Actor to animate";
			Resource2DropDownTitle.text = "Parameter to trigger:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: false);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		case CLevelEvent.ELevelEventType.TriggerPlayableDirectorOnGameObject:
			ResourceTitle.text = "GameObject name to trigger playable director on:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: false);
			Resource3Element.gameObject.SetActive(value: false);
			FloatResourceElement.gameObject.SetActive(value: false);
			CameraProfileElement.gameObject.SetActive(value: false);
			Resource1DropDownElement.gameObject.SetActive(value: false);
			Resource2DropDownElement.gameObject.SetActive(value: false);
			break;
		}
		bool active = type == CLevelEvent.ELevelEventType.SpawnMonsterOnProp || type == CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle;
		PerPartySizeConfigGO.SetActive(active);
	}

	public void ConfigureResourceDropDownsForItem(LevelEditorEventItem itemClicked)
	{
		switch (itemClicked.ItemEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list5 = ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner t) => t.SpawnerGuid).ToList();
			Resource1DropDown.AddOptions(list5);
			int valueWithoutNotify5 = Mathf.Max(0, list5.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify5);
			Resource2DropDown?.ClearOptions();
			List<string> list6 = CLevelEvent.LevelEventSpawnerEditTypes.Select((CLevelEvent.ELevelEventSpawnerEditType t) => t.ToString()).ToList();
			Resource2DropDown.AddOptions(list6);
			int valueWithoutNotify6 = Mathf.Max(0, list6.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify6);
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list16 = ScenarioManager.CurrentScenarioState.DoorProps.Select((CObjectProp t) => t.PropGuid).ToList();
			Resource1DropDown.AddOptions(list16);
			int valueWithoutNotify16 = Mathf.Max(0, list16.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify16);
			Resource2DropDown?.ClearOptions();
			List<string> list17 = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list17);
			int valueWithoutNotify17 = Mathf.Max(0, list17.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify17);
			break;
		}
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list2 = ScenarioManager.CurrentScenarioState.DoorProps.Select((CObjectProp t) => t.PropGuid).ToList();
			Resource1DropDown.AddOptions(list2);
			int valueWithoutNotify2 = Mathf.Max(0, list2.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify2);
			Resource2DropDown?.ClearOptions();
			List<string> list3 = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list3);
			int valueWithoutNotify3 = Mathf.Max(0, list3.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify3);
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
		case CLevelEvent.ELevelEventType.ActivateModifier:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list4 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list4);
			int valueWithoutNotify4 = Mathf.Max(0, list4.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify4);
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list9 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list9);
			int valueWithoutNotify9 = Mathf.Max(0, list9.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify9);
			Resource2DropDown?.ClearOptions();
			list9 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
			list9.AddRange(from t in ScenarioManager.CurrentScenarioState.Props
				where t.PropHealthDetails?.HasHealth ?? false
				select t.PropGuid);
			list9.Insert(0, "None");
			Resource2DropDown.AddOptions(list9);
			valueWithoutNotify9 = Mathf.Max(0, list9.IndexOf(itemClicked.ItemEvent.EventFourthResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify9);
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list13 = (from s in ScenarioManager.CurrentScenarioState.AllObjectives
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list13);
			int valueWithoutNotify13 = Mathf.Max(0, list13.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify13);
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list22 = MonsterClassManager.MonsterAndObjectClasses.Select((CMonsterClass t) => t.ID).ToList();
			Resource1DropDown.AddOptions(list22);
			int valueWithoutNotify22 = Mathf.Max(0, list22.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify22);
			if (itemClicked.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnActor)
			{
				Resource2DropDown?.ClearOptions();
				List<string> list23 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
				Resource2DropDown.AddOptions(list23);
				int valueWithoutNotify23 = Mathf.Max(0, list23.IndexOf(itemClicked.ItemEvent.EventSecondResource));
				Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify23);
			}
			else
			{
				if (itemClicked.ItemEvent.EventType != CLevelEvent.ELevelEventType.SpawnMonsterOnProp)
				{
					break;
				}
				Resource2DropDown?.ClearOptions();
				List<string> list24 = ScenarioManager.CurrentScenarioState.Props.Select((CObjectProp t) => t.PropGuid).ToList();
				Resource2DropDown.AddOptions(list24);
				int valueWithoutNotify24 = Mathf.Max(0, list24.IndexOf(itemClicked.ItemEvent.EventSecondResource));
				Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify24);
				PartySizeOneConfigDropDown.options.Clear();
				PartySizeTwoConfigDropDown.options.Clear();
				PartySizeThreeConfigDropDown.options.Clear();
				PartySizeFourConfigDropDown.options.Clear();
				PartySizeOneConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeTwoConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeThreeConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeFourConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				if (itemClicked.ItemEvent.EventThirdResource.IsNOTNullOrEmpty())
				{
					List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource2 = GetPerPartySizeConfigsFromResource(itemClicked.ItemEvent.EventThirdResource);
					if (perPartySizeConfigsFromResource2.Count == 4)
					{
						PartySizeOneConfigDropDown.value = (int)perPartySizeConfigsFromResource2[0];
						PartySizeTwoConfigDropDown.value = (int)perPartySizeConfigsFromResource2[1];
						PartySizeThreeConfigDropDown.value = (int)perPartySizeConfigsFromResource2[2];
						PartySizeFourConfigDropDown.value = (int)perPartySizeConfigsFromResource2[3];
					}
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list = MonsterClassManager.MonsterAndObjectClasses.Select((CMonsterClass t) => t.ID).ToList();
			Resource1DropDown.AddOptions(list);
			int valueWithoutNotify = Mathf.Max(0, list.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify);
			PartySizeOneConfigDropDown.options.Clear();
			PartySizeTwoConfigDropDown.options.Clear();
			PartySizeThreeConfigDropDown.options.Clear();
			PartySizeFourConfigDropDown.options.Clear();
			PartySizeOneConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeTwoConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeThreeConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeFourConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			if (itemClicked.ItemEvent.EventThirdResource.IsNOTNullOrEmpty())
			{
				List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource = GetPerPartySizeConfigsFromResource(itemClicked.ItemEvent.EventThirdResource);
				if (perPartySizeConfigsFromResource.Count == 4)
				{
					PartySizeOneConfigDropDown.value = (int)perPartySizeConfigsFromResource[0];
					PartySizeTwoConfigDropDown.value = (int)perPartySizeConfigsFromResource[1];
					PartySizeThreeConfigDropDown.value = (int)perPartySizeConfigsFromResource[2];
					PartySizeFourConfigDropDown.value = (int)perPartySizeConfigsFromResource[3];
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list11 = CObjectProp.PropTypes.Select((EPropType t) => t.ToString()).ToList();
			list11.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType t) => t.ToString()).ToList());
			Resource1DropDown.AddOptions(list11);
			int valueWithoutNotify11 = Mathf.Max(0, list11.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify11);
			Resource2DropDown?.ClearOptions();
			List<string> list12 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
			Resource2DropDown.AddOptions(list12);
			int valueWithoutNotify12 = Mathf.Max(0, list12.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify12);
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list20 = CObjectProp.PropTypes.Select((EPropType t) => t.ToString()).ToList();
			list20.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType t) => t.ToString()).ToList());
			Resource1DropDown.AddOptions(list20);
			int valueWithoutNotify20 = Mathf.Max(0, list20.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify20);
			Resource2DropDown?.ClearOptions();
			List<string> list21 = ScenarioManager.CurrentScenarioState.Props.Select((CObjectProp t) => t.PropGuid).ToList();
			Resource2DropDown.AddOptions(list21);
			int valueWithoutNotify21 = Mathf.Max(0, list21.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify21);
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list14 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list14);
			int valueWithoutNotify14 = Mathf.Max(0, list14.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify14);
			Resource2DropDown?.ClearOptions();
			List<string> list15 = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list15);
			int valueWithoutNotify15 = Mathf.Max(0, list15.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify15);
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list8 = (from t in ScenarioManager.CurrentScenarioState.Props
				where t is CObjectDifficultTerrain
				select t.PropGuid).ToList();
			Resource1DropDown.AddOptions(list8);
			int valueWithoutNotify8 = Mathf.Max(0, list8.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify8);
			break;
		}
		case CLevelEvent.ELevelEventType.KillActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list19 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
			Resource1DropDown.AddOptions(list19);
			int valueWithoutNotify19 = Mathf.Max(0, list19.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify19);
			break;
		}
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list7 = (from t in ScenarioManager.CurrentScenarioState.Props
				where t.PropHealthDetails?.HasHealth ?? false
				select t.PropGuid).ToList();
			Resource1DropDown.AddOptions(list7);
			int valueWithoutNotify7 = Mathf.Max(0, list7.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify7);
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list18 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
			list18.AddRange(from t in ScenarioManager.CurrentScenarioState.Props
				where t.PropHealthDetails?.HasHealth ?? false
				select t.PropGuid);
			list18.Insert(0, "None");
			Resource1DropDown.AddOptions(list18);
			int valueWithoutNotify18 = Mathf.Max(0, list18.IndexOf(itemClicked.ItemEvent.EventFourthResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify18);
			break;
		}
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list10 = ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState t) => t.ActorGuid).ToList();
			Resource1DropDown.AddOptions(list10);
			int valueWithoutNotify10 = Mathf.Max(0, list10.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify10);
			break;
		}
		default:
			Resource1DropDown?.ClearOptions();
			Resource1DropDown?.SetValueWithoutNotify(0);
			Resource2DropDown?.ClearOptions();
			Resource2DropDown?.SetValueWithoutNotify(0);
			break;
		}
	}

	public static List<ScenarioManager.EPerPartySizeConfig> GetPerPartySizeConfigsFromResource(string resource)
	{
		List<ScenarioManager.EPerPartySizeConfig> list = new List<ScenarioManager.EPerPartySizeConfig>();
		List<string> list2 = resource.Split('|').ToList();
		if (list2.Count == 4)
		{
			foreach (string item in list2)
			{
				list.Add((ScenarioManager.EPerPartySizeConfig)Enum.Parse(typeof(ScenarioManager.EPerPartySizeConfig), item, ignoreCase: true));
			}
		}
		return list;
	}

	public void EventItemClicked(LevelEditorEventItem itemClicked)
	{
		m_CurrentEventItem = itemClicked;
		EventTypeDropDown.SetValueWithoutNotify((int)m_CurrentEventItem.ItemEvent.EventType);
		RepeatsToggle.SetIsOnWithoutNotify(m_CurrentEventItem.ItemEvent.Repeats);
		ResourceInput.text = m_CurrentEventItem.ItemEvent.EventResource;
		Resource2Input.text = m_CurrentEventItem.ItemEvent.EventSecondResource;
		Resource3Input.text = m_CurrentEventItem.ItemEvent.EventThirdResource;
		FloatResourceInput.text = m_CurrentEventItem.ItemEvent.EventFloatResource.ToString();
		EventTrigger.InitForMessageTrigger(m_CurrentEventItem.ItemEvent.DisplayTrigger);
		CameraProfilePanel.InitForProfile(m_CurrentEventItem.ItemEvent.EventCameraProfile);
		ConfigureResourceDropDownsForItem(m_CurrentEventItem);
		ConfigureUIForEventType(m_CurrentEventItem.ItemEvent.EventType);
		EventDisplayPanel.SetActive(value: true);
	}

	public void OnAddEventPressed()
	{
		CLevelEvent cLevelEvent = new CLevelEvent();
		SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Add(cLevelEvent);
		m_CurrentEventItem = AddEventItemToUI(cLevelEvent, SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Count - 1);
		EventItemClicked(m_CurrentEventItem);
	}

	public void OnApplyPressed()
	{
		m_CurrentEventItem.ItemEvent.EventType = CLevelEvent.LevelEventTypes.SingleOrDefault((CLevelEvent.ELevelEventType s) => s.ToString() == EventTypeDropDown.options[EventTypeDropDown.value].text);
		m_CurrentEventItem.ItemEvent.Repeats = RepeatsToggle.isOn;
		m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
		m_CurrentEventItem.ItemEvent.EventFloatResource = (string.IsNullOrEmpty(FloatResourceInput.text) ? 0f : Convert.ToSingle(FloatResourceInput.text));
		switch (m_CurrentEventItem.ItemEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			string eventResource7 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.Spawners.Count)
			{
				eventResource7 = ScenarioManager.CurrentScenarioState.Spawners[Resource1DropDown.value].SpawnerGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource7;
			string eventSecondResource4 = CLevelEvent.LevelEventSpawnerEditTypes[0].ToString();
			if (Resource2DropDown.value < CLevelEvent.LevelEventSpawnerEditTypes.Length)
			{
				eventSecondResource4 = CLevelEvent.LevelEventSpawnerEditTypes[Resource2DropDown.value].ToString();
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource4;
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
		{
			string eventResource8 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.DoorProps.Count)
			{
				eventResource8 = ScenarioManager.CurrentScenarioState.DoorProps[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource8;
			string eventSecondResource5 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource5 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource5;
			break;
		}
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			string eventResource3 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.DoorProps.Count)
			{
				eventResource3 = ScenarioManager.CurrentScenarioState.DoorProps[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource3;
			string eventSecondResource2 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource2 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource2;
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
		case CLevelEvent.ELevelEventType.ActivateModifier:
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			string eventResource11 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource11 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource11;
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			string eventResource10 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource10 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource10;
			if (Resource2DropDown.gameObject.activeSelf)
			{
				m_CurrentEventItem.ItemEvent.EventFourthResource = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			string eventResource12 = string.Empty;
			if (Resource1DropDown.value < MonsterClassManager.MonsterAndObjectClasses.Count)
			{
				eventResource12 = MonsterClassManager.MonsterAndObjectClasses[Resource1DropDown.value].ID;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource12;
			string eventSecondResource7 = string.Empty;
			if (m_CurrentEventItem.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnActor)
			{
				if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
				{
					eventSecondResource7 = ScenarioManager.CurrentScenarioState.ActorStates[Resource2DropDown.value].ActorGuid;
				}
			}
			else if (m_CurrentEventItem.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnProp)
			{
				if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.Props.Count)
				{
					eventSecondResource7 = ScenarioManager.CurrentScenarioState.Props[Resource2DropDown.value].PropGuid;
				}
				m_CurrentEventItem.ItemEvent.EventThirdResource = string.Join('|'.ToString(), (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource7;
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			string eventResource6 = string.Empty;
			if (Resource1DropDown.value < MonsterClassManager.MonsterAndObjectClasses.Count)
			{
				eventResource6 = MonsterClassManager.MonsterAndObjectClasses[Resource1DropDown.value].ID;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource6;
			m_CurrentEventItem.ItemEvent.EventThirdResource = string.Join('|'.ToString(), (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			string eventResource5 = string.Empty;
			List<string> list3 = CObjectProp.PropTypes.Select((EPropType x) => x.ToString()).ToList();
			list3.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType x) => x.ToString()).ToList());
			if (Resource1DropDown.value < list3.Count)
			{
				eventResource5 = list3[Resource1DropDown.value];
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource5;
			string eventSecondResource3 = string.Empty;
			if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
			{
				eventSecondResource3 = ScenarioManager.CurrentScenarioState.ActorStates[Resource2DropDown.value].ActorGuid;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource3;
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			string eventResource2 = string.Empty;
			List<string> list = CObjectProp.PropTypes.Select((EPropType x) => x.ToString()).ToList();
			list.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType x) => x.ToString()).ToList());
			if (Resource1DropDown.value < list.Count)
			{
				eventResource2 = list[Resource1DropDown.value];
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource2;
			string eventSecondResource = string.Empty;
			if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.Props.Count)
			{
				eventSecondResource = ScenarioManager.CurrentScenarioState.Props[Resource2DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource;
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			string eventResource9 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource9 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource9;
			string eventSecondResource6 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource6 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource6;
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
		{
			string eventResource4 = string.Empty;
			List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p is CObjectDifficultTerrain).ToList();
			if (Resource1DropDown.value < list2.Count)
			{
				eventResource4 = list2[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource4;
			break;
		}
		case CLevelEvent.ELevelEventType.KillActor:
		{
			string eventResource = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
			{
				eventResource = ScenarioManager.CurrentScenarioState.ActorStates[Resource1DropDown.value].ActorGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource;
			break;
		}
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
			m_CurrentEventItem.ItemEvent.EventResource = Resource1DropDown.options[Resource1DropDown.value].text;
			break;
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
			m_CurrentEventItem.ItemEvent.EventResource = ResourceInput.text;
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
			m_CurrentEventItem.ItemEvent.EventFourthResource = Resource1DropDown.options[Resource1DropDown.value].text;
			break;
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
			m_CurrentEventItem.ItemEvent.EventResource = Resource1DropDown.options[Resource1DropDown.value].text;
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			break;
		default:
			m_CurrentEventItem.ItemEvent.EventResource = ResourceInput.text;
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
			break;
		}
		CameraProfilePanel.OnApplyPressed();
		EventTrigger.SaveValuesToTrigger();
	}

	public void OnDeletePressed()
	{
		if (m_CurrentEventItem != null)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.RemoveAt(m_CurrentEventItem.ItemIndex);
			m_EventItems.Remove(m_CurrentEventItem);
			UnityEngine.Object.Destroy(m_CurrentEventItem.gameObject);
			m_CurrentEventItem = null;
			EventDisplayPanel.SetActive(value: false);
		}
	}

	public void OnEventTypeDropDownChanged(int valueChangeTo)
	{
		m_CurrentEventItem.ItemEvent.EventType = CLevelEvent.LevelEventTypes[EventTypeDropDown.value];
		ConfigureUIForEventType(m_CurrentEventItem.ItemEvent.EventType);
		ConfigureResourceDropDownsForItem(m_CurrentEventItem);
	}
}
