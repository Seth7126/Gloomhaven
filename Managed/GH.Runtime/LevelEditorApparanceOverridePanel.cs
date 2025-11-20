#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;

public class LevelEditorApparanceOverridePanel : MonoBehaviour
{
	private enum EApparanceOverrideType
	{
		Prop,
		Room,
		Wall,
		Scenario
	}

	public TextMeshProUGUI DescriptionText;

	public TMP_Dropdown BiomeDropDown;

	public TMP_Dropdown SubBiomeDropDown;

	public TMP_Dropdown ThemeDropDown;

	public TMP_Dropdown SubThemeDropDown;

	public TMP_Dropdown ToneDropDown;

	public TMP_InputField SeedInput;

	public GameObject BlockerObject;

	public TextMeshProUGUI BlockerText;

	private EApparanceOverrideType m_ApparanceOverrideType;

	private CObjectProp m_PropBeingDisplayed;

	private GameObject m_PropObjectBeingDisplayed;

	private CMap m_RoomBeingDisplayed;

	private int m_WallIndexBeingDisplayed;

	private ProceduralStyle[] m_ProceduralStylesBeingEdited;

	private CApparanceOverrideDetails m_CurrentApparanceOverride;

	private const float cPropObjectCheckTimeout = 10f;

	private IEnumerator m_PropSpawnCheckRoutine;

	private bool m_WaitingForOnEnableToResolveProp;

	private bool m_Initialised;

	private void EnsurePanelInitialised()
	{
		if (!m_Initialised)
		{
			DLCRegistry.EDLCKey ownedDlcFlag = PlatformLayer.DLC.OwnedDLCAsFlag();
			List<ScenarioPossibleRoom.EBiome> list = GlobalSettings.Instance.ApparanceEnumFilter.AvailableBiomes().ToList();
			List<ScenarioPossibleRoom.ESubBiome> list2 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableSubBiomes().ToList();
			List<ScenarioPossibleRoom.ETheme> list3 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableThemes().ToList();
			List<ScenarioPossibleRoom.ESubTheme> list4 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableSubThemes().ToList();
			List<ScenarioPossibleRoom.ETone> list5 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableTones().ToList();
			DLCRegistry.RemoveBiomesBasedOnOwnedDLC(ownedDlcFlag, list);
			DLCRegistry.RemoveSubBiomesBasedOnOwnedDLC(ownedDlcFlag, list2);
			DLCRegistry.RemoveThemeBasedOnOwnedDLC(ownedDlcFlag, list3);
			DLCRegistry.RemoveSubThemesBasedOnOwnedDLC(ownedDlcFlag, list4);
			DLCRegistry.RemoveToneBasedOnOwnedDLC(ownedDlcFlag, list5);
			BiomeDropDown.options.Clear();
			BiomeDropDown.AddOptions(list.Select((ScenarioPossibleRoom.EBiome b) => b.ToString()).ToList());
			SubBiomeDropDown.options.Clear();
			SubBiomeDropDown.AddOptions(list2.Select((ScenarioPossibleRoom.ESubBiome sb) => sb.ToString()).ToList());
			ThemeDropDown.options.Clear();
			ThemeDropDown.AddOptions(list3.Select((ScenarioPossibleRoom.ETheme t) => t.ToString()).ToList());
			SubThemeDropDown.options.Clear();
			SubThemeDropDown.AddOptions(list4.Select((ScenarioPossibleRoom.ESubTheme st) => st.ToString()).ToList());
			ToneDropDown.options.Clear();
			ToneDropDown.AddOptions(list5.Select((ScenarioPossibleRoom.ETone t) => t.ToString()).ToList());
			m_Initialised = true;
		}
	}

	private void Awake()
	{
		EnsurePanelInitialised();
	}

	public void SetDescriptionText(string desc)
	{
		DescriptionText.text = desc;
	}

	public void SetPropDisplayed(CObjectProp prop)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Prop)
		{
			m_RoomBeingDisplayed = null;
			m_WallIndexBeingDisplayed = -1;
		}
		SetDescriptionText("Set Apparance override at <b>Prop Level</b>");
		m_ApparanceOverrideType = EApparanceOverrideType.Prop;
		if (prop == null)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		else if (m_PropBeingDisplayed == null || m_PropBeingDisplayed.PropGuid != prop.PropGuid || m_PropObjectBeingDisplayed == null)
		{
			if (m_PropSpawnCheckRoutine != null)
			{
				StopCoroutine(m_PropSpawnCheckRoutine);
				m_PropSpawnCheckRoutine = null;
			}
			m_PropSpawnCheckRoutine = ContinuouslyCheckForPropsObject(prop);
			m_WaitingForOnEnableToResolveProp = false;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(m_PropSpawnCheckRoutine);
			}
			else
			{
				m_WaitingForOnEnableToResolveProp = true;
			}
		}
	}

	private void OnEnable()
	{
		if (m_WaitingForOnEnableToResolveProp && m_PropSpawnCheckRoutine != null)
		{
			m_WaitingForOnEnableToResolveProp = false;
			StartCoroutine(m_PropSpawnCheckRoutine);
		}
	}

	private IEnumerator ContinuouslyCheckForPropsObject(CObjectProp prop)
	{
		bool timedOut = false;
		float timeAtStartOfCheck = Timekeeper.instance.m_GlobalClock.unscaledTime;
		GameObject propGameObject = null;
		bool propGameObjectExists = false;
		BlockerObject.SetActive(value: true);
		BlockerText.text = "Locating Prop's Apparance Object";
		while (!propGameObjectExists && !timedOut)
		{
			yield return null;
			if (Timekeeper.instance.m_GlobalClock.unscaledTime - timeAtStartOfCheck > 10f)
			{
				timedOut = true;
				break;
			}
			propGameObject = LevelEditorController.s_Instance.MapSceneRoot.FindInChildren(prop.InstanceName);
			if (propGameObject != null)
			{
				propGameObjectExists = true;
				break;
			}
		}
		if (propGameObjectExists && !timedOut)
		{
			m_PropBeingDisplayed = prop;
			m_PropObjectBeingDisplayed = propGameObject;
			m_ProceduralStylesBeingEdited = m_PropObjectBeingDisplayed.GetComponentsInChildren<ProceduralStyle>();
			if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
			{
				BlockerObject.SetActive(value: true);
				BlockerText.text = "Unable to edit Prop's apparance settings";
				m_PropBeingDisplayed = null;
				m_PropObjectBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
			}
			else
			{
				BlockerObject.SetActive(value: false);
				BlockerText.text = "-";
			}
		}
		else
		{
			BlockerObject.SetActive(value: true);
			BlockerText.text = "Unable to edit Prop's apparance settings";
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetRoomDisplayed(CMap map)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Room)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
		}
		SetDescriptionText("Set Apparance override at <b>Room Level</b> <i>(NB: This will affect the look of all child components like props etc)</i>");
		m_ApparanceOverrideType = EApparanceOverrideType.Room;
		if (map == null)
		{
			m_RoomBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		else if (m_RoomBeingDisplayed == null || m_RoomBeingDisplayed.MapGuid != map.MapGuid || m_WallIndexBeingDisplayed >= 0)
		{
			m_WallIndexBeingDisplayed = -1;
			m_RoomBeingDisplayed = map;
			GameObject map2 = Singleton<ObjectCacheService>.Instance.GetMap(m_RoomBeingDisplayed);
			if (map2 != null)
			{
				m_ProceduralStylesBeingEdited = map2.GetComponents<ProceduralStyle>();
				if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
				{
					BlockerObject.SetActive(value: true);
					BlockerText.text = "Unable to edit Rooms's apparance settings";
					m_RoomBeingDisplayed = null;
					m_ProceduralStylesBeingEdited = null;
				}
				else
				{
					BlockerText.text = "-";
					BlockerObject.SetActive(value: false);
				}
			}
			else
			{
				m_RoomBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
			}
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetWallDisplayed(CMap parentRoom, int wallIndex)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Wall)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
		}
		SetDescriptionText("Set Apparance override at <b>Wall Level</b>");
		m_ApparanceOverrideType = EApparanceOverrideType.Wall;
		if (parentRoom == null || wallIndex < 0)
		{
			m_RoomBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
			m_WallIndexBeingDisplayed = -1;
		}
		else if (m_RoomBeingDisplayed == null || m_RoomBeingDisplayed.MapGuid != parentRoom.MapGuid || m_WallIndexBeingDisplayed != wallIndex)
		{
			m_RoomBeingDisplayed = parentRoom;
			m_WallIndexBeingDisplayed = wallIndex;
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(m_RoomBeingDisplayed);
			if (map != null)
			{
				ProceduralWall[] componentsInChildren = map.GetComponentsInChildren<ProceduralWall>();
				if (m_WallIndexBeingDisplayed < componentsInChildren.Length)
				{
					ProceduralWall proceduralWall = componentsInChildren[m_WallIndexBeingDisplayed];
					m_ProceduralStylesBeingEdited = proceduralWall.GetComponents<ProceduralStyle>();
					if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
					{
						BlockerObject.SetActive(value: true);
						BlockerText.text = "Unable to edit Wall's apparance settings";
						m_RoomBeingDisplayed = null;
						m_ProceduralStylesBeingEdited = null;
						m_WallIndexBeingDisplayed = -1;
					}
					else
					{
						BlockerText.text = "-";
						BlockerObject.SetActive(value: false);
					}
				}
				else
				{
					m_RoomBeingDisplayed = null;
					m_ProceduralStylesBeingEdited = null;
					m_WallIndexBeingDisplayed = -1;
				}
			}
			else
			{
				m_RoomBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
				m_WallIndexBeingDisplayed = -1;
			}
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetScenarioDisplayed()
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Scenario)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_RoomBeingDisplayed = null;
			m_WallIndexBeingDisplayed = -1;
		}
		SetDescriptionText("Set Apparance override at <b>Scenario Level</b> <i>(NB: This will affect the look of all child components, like Rooms, props, walls, etc)</i>");
		m_ApparanceOverrideType = EApparanceOverrideType.Scenario;
		GameObject mapSceneRoot = LevelEditorController.s_Instance.MapSceneRoot;
		if (mapSceneRoot != null)
		{
			m_ProceduralStylesBeingEdited = mapSceneRoot.GetComponents<ProceduralStyle>();
			if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
			{
				BlockerObject.SetActive(value: true);
				BlockerText.text = "Unable to edit Scecnario's apparance settings";
				m_ProceduralStylesBeingEdited = null;
			}
			else
			{
				BlockerText.text = "-";
				BlockerObject.SetActive(value: false);
			}
		}
		else
		{
			m_ProceduralStylesBeingEdited = null;
		}
		RefreshPanelForCurrentOverride();
	}

	public void RefreshPanelForCurrentOverride()
	{
		EnsurePanelInitialised();
		m_CurrentApparanceOverride = null;
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0 || (m_ApparanceOverrideType == EApparanceOverrideType.Prop && m_PropBeingDisplayed == null) || (m_ApparanceOverrideType == EApparanceOverrideType.Room && m_RoomBeingDisplayed == null) || (m_ApparanceOverrideType == EApparanceOverrideType.Wall && (m_RoomBeingDisplayed == null || m_WallIndexBeingDisplayed < 0)))
		{
			BiomeDropDown.SetValueWithoutNotify(0);
			SubBiomeDropDown.SetValueWithoutNotify(0);
			ThemeDropDown.SetValueWithoutNotify(0);
			SubThemeDropDown.SetValueWithoutNotify(0);
			ToneDropDown.SetValueWithoutNotify(0);
			SeedInput.SetTextWithoutNotify(string.Empty);
			return;
		}
		string text = string.Empty;
		switch (m_ApparanceOverrideType)
		{
		case EApparanceOverrideType.Prop:
			text = m_PropBeingDisplayed.PropGuid;
			break;
		case EApparanceOverrideType.Room:
			text = m_RoomBeingDisplayed.MapGuid;
			break;
		case EApparanceOverrideType.Wall:
			text = CApparanceOverrideDetails.GetWallGUID(m_RoomBeingDisplayed, m_WallIndexBeingDisplayed);
			break;
		case EApparanceOverrideType.Scenario:
			text = "ScenarioApparanceOverrideGUID";
			break;
		}
		m_CurrentApparanceOverride = SaveData.Instance.Global.CurrentEditorLevelData.GetApparanceOverrideDetailsForGUID(text);
		int objectCount;
		if (m_CurrentApparanceOverride == null)
		{
			m_CurrentApparanceOverride = new CApparanceOverrideDetails(text);
			m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			m_CurrentApparanceOverride.OverrideBiome = m_ProceduralStylesBeingEdited[0].Biome;
			m_CurrentApparanceOverride.OverrideSubBiome = m_ProceduralStylesBeingEdited[0].SubBiome;
			m_CurrentApparanceOverride.OverrideTheme = m_ProceduralStylesBeingEdited[0].Theme;
			m_CurrentApparanceOverride.OverrideSubTheme = m_ProceduralStylesBeingEdited[0].SubTheme;
			m_CurrentApparanceOverride.OverrideTone = m_ProceduralStylesBeingEdited[0].Tone;
			m_ProceduralStylesBeingEdited[0].FetchObjectHierarchyValues(out var objectIndex, out objectCount);
			m_CurrentApparanceOverride.OverrideSiblingIndex = objectIndex;
		}
		if (m_CurrentApparanceOverride.OverrideSiblingIndex == -1)
		{
			m_ProceduralStylesBeingEdited[0].FetchObjectHierarchyValues(out var objectIndex2, out objectCount);
			m_CurrentApparanceOverride.OverrideSiblingIndex = objectIndex2;
		}
		BiomeDropDown.SetValueWithoutNotify(BiomeDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_CurrentApparanceOverride.OverrideBiome.ToString()));
		SubBiomeDropDown.SetValueWithoutNotify(SubBiomeDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_CurrentApparanceOverride.OverrideSubBiome.ToString()));
		ThemeDropDown.SetValueWithoutNotify(ThemeDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_CurrentApparanceOverride.OverrideTheme.ToString()));
		SubThemeDropDown.SetValueWithoutNotify(SubThemeDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_CurrentApparanceOverride.OverrideSubTheme.ToString()));
		ToneDropDown.SetValueWithoutNotify(ToneDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_CurrentApparanceOverride.OverrideTone.ToString()));
		SeedInput.SetTextWithoutNotify(m_CurrentApparanceOverride.OverrideSeed.ToString());
	}

	public void SaveOverrideToLevel()
	{
		Debug.LogFormat("Saving Override of type {0}, with GUID {1}", m_ApparanceOverrideType.ToString(), m_CurrentApparanceOverride.GUID);
		switch (m_ApparanceOverrideType)
		{
		case EApparanceOverrideType.Prop:
			if (m_PropBeingDisplayed != null)
			{
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Room:
			if (m_RoomBeingDisplayed != null)
			{
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Wall:
			if (m_RoomBeingDisplayed != null && m_WallIndexBeingDisplayed >= 0)
			{
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Scenario:
			LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			break;
		}
	}

	public void OnSetBiomePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.EBiome eBiome = ((ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))).Single((ScenarioPossibleRoom.EBiome s) => s.ToString() == BiomeDropDown.options[BiomeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Biome = eBiome;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideBiome = eBiome;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetSubBiomePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ESubBiome eSubBiome = ((ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))).Single((ScenarioPossibleRoom.ESubBiome s) => s.ToString() == SubBiomeDropDown.options[SubBiomeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].SubBiome = eSubBiome;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSubBiome = eSubBiome;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetThemePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ETheme eTheme = ((ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))).Single((ScenarioPossibleRoom.ETheme s) => s.ToString() == ThemeDropDown.options[ThemeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Theme = eTheme;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideTheme = eTheme;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetSubThemePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ESubTheme eSubTheme = ((ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))).Single((ScenarioPossibleRoom.ESubTheme s) => s.ToString() == SubThemeDropDown.options[SubThemeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].SubTheme = eSubTheme;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSubTheme = eSubTheme;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetTonePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ETone eTone = ((ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))).Single((ScenarioPossibleRoom.ETone s) => s.ToString() == ToneDropDown.options[ToneDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Tone = eTone;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideTone = eTone;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetCustomSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
		{
			return;
		}
		if (int.TryParse(SeedInput.text, out var result))
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = result;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = result;
			}
		}
		SaveOverrideToLevel();
	}

	public void OnSeedFieldEndedEditing()
	{
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
		{
			return;
		}
		if (int.TryParse(SeedInput.text, out var result))
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = result;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = result;
			}
		}
		SaveOverrideToLevel();
	}

	public void OnDecrementSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			int num = m_ProceduralStylesBeingEdited[0].Seed - 1;
			if (num < 0)
			{
				num = 0;
			}
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = num;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			}
			SeedInput.SetTextWithoutNotify(m_ProceduralStylesBeingEdited[0].Seed.ToString());
			SaveOverrideToLevel();
		}
	}

	public void OnIncrementSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = m_ProceduralStylesBeingEdited[i].Seed + 1;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			}
			SeedInput.SetTextWithoutNotify(m_ProceduralStylesBeingEdited[0].Seed.ToString());
			SaveOverrideToLevel();
		}
	}

	public void OnRandomiseSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			int num = SharedClient.GlobalRNG.Next();
			SeedInput.SetTextWithoutNotify(num.ToString());
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = num;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = num;
			}
			SaveOverrideToLevel();
		}
	}
}
