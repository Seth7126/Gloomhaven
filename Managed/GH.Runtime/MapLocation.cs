using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Assets.Script.GUI.Quest;
using Chronos;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.VisibilitySpheres;
using ScenarioRuleLibrary;
using Script.Extensions;
using TMPro;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.MemoryManager;

public class MapLocation : CInteractable, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[Serializable]
	public enum EMapLocationType
	{
		None,
		Village,
		Boss,
		Scenario,
		Headquarters,
		Store
	}

	public delegate bool OnClickAction(MapLocation mapNode, bool active);

	[Header("Hierarchy")]
	[SerializeField]
	private GameObject MeshParent;

	[SerializeField]
	private GameObject nameCanvasObject;

	[SerializeField]
	private RectTransform nodeTitle;

	[SerializeField]
	private GameObject lineRenderers;

	[Header("Colliders")]
	[SerializeField]
	private BoxCollider _boxCollider;

	[SerializeField]
	private BoxCollider _snappingBoxCollider;

	[Header("Properties")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private SpriteRenderer highlightRenderer;

	[SerializeField]
	private Sprite[] highlightSprites;

	[SerializeField]
	private Material nodeTitleGlowMat;

	[SerializeField]
	private GameObject visitableNodeEffect;

	[SerializeField]
	private GameObject currentBossIndicator;

	[SerializeField]
	private EffectAlphaFadeParticles nodeHoverIndicator;

	[SerializeField]
	private GameObject nodeSelectIndicator;

	[SerializeField]
	private Vector3 offsetQuestMarkerPosition = new Vector3(0.5f, 0f, 0f);

	[SerializeField]
	private AudioButtonProfile m_AudioProfile;

	[SerializeField]
	private float unfocusedOppacity = 0.5f;

	[SerializeField]
	private Color completedLocationColor = Color.gray;

	[SerializeField]
	private Vector3 centerCityPositionOffset;

	[Header("Line Renderer")]
	[SerializeField]
	private GameObject m_NodeLineRendererPrefab;

	[SerializeField]
	private Material m_ActiveLine;

	[SerializeField]
	private Material m_PastLine;

	[SerializeField]
	private int m_PathSubdivisions = 30;

	[SerializeField]
	private float m_PathSegmentOffset = 0.1f;

	[SerializeField]
	private float m_MinPathWith = 0.2f;

	[SerializeField]
	private float m_MaxPathWidth = 0.4f;

	[SerializeField]
	private float m_PathNodeAnimationTime = 0.05f;

	private const float c_HighlightedNodeScaleFactor = 1.2f;

	private GameObject m_Mesh;

	private OnClickAction m_OnClickAction;

	private OnClickAction m_OnHighlightAction;

	private bool m_IsHighlighted;

	private bool m_IsSelected;

	private Material m_NodeTitleRegularMat;

	private CLocationState m_Location;

	private CQuestState m_LocationQuest;

	private LineRenderer m_LineRenderer;

	private List<LineRenderer> m_VillageRoadRenderers;

	private List<string> m_ConnectedVillageIDs;

	private Tuple<MapLocation, int> m_ScenarioLocationPathIndex;

	private Vector3 m_DefaultNodeScale;

	private UIQuestMapMarker m_QuestMarker;

	private bool m_MouseInCollider;

	private UILocationMapMarker m_LocationMarker;

	private Coroutine pathAnimation;

	private Vector3 defaultOffset;

	private Decal decal;

	private ReferenceToObject<Material> defaultMeshMaterial;

	private ReferenceToObject<Material> highlightedMeshMaterial;

	private HashSet<Component> hideMarkerRequests = new HashSet<Component>();

	private HashSet<Component> hideLocationRequests = new HashSet<Component>();

	private const float SnappingBoxColliderSizeMultiplier = 2f;

	private readonly Vector3 MinSnappingBoxColliderSize = Vector3.zero;

	private readonly Vector3 MaxSnappingBoxColliderSize = new Vector3(15f, 15f, 20f);

	private bool forceHighlight;

	private MapLocation targetLocation;

	public EMapLocationType MapLocationType { get; private set; }

	public CLocationState Location => m_Location;

	public CQuestState LocationQuest => m_LocationQuest;

	public bool IsHighlighted => m_IsHighlighted;

	public bool IsSelected => m_IsSelected;

	public Vector3[] PathToThisNode { get; set; }

	public bool HasPendingTreasures { get; private set; }

	public Vector3[] PathToNextScenario
	{
		get
		{
			if (m_LocationQuest != null)
			{
				Tuple<MapLocation, int> scenarioLocationPathIndex = m_ScenarioLocationPathIndex;
				Vector3[] array = new Vector3[scenarioLocationPathIndex.Item2];
				for (int i = 0; i < scenarioLocationPathIndex.Item2; i++)
				{
					array[i] = PathToThisNode[i];
				}
				return array;
			}
			return null;
		}
	}

	public Vector3 CenterPosition
	{
		get
		{
			if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City)
			{
				return base.transform.position;
			}
			return base.transform.position + centerCityPositionOffset;
		}
	}

	public void Awake()
	{
		if (PlatformLayer.Setting.ForceFreeMemoryParticleSystems)
		{
			UnityEngine.Object.Destroy(visitableNodeEffect.gameObject);
			UnityEngine.Object.Destroy(currentBossIndicator.gameObject);
		}
		defaultOffset = _boxCollider.size;
		defaultOffset.Scale(offsetQuestMarkerPosition);
	}

	private void SetBoxColliderSize(Vector3 center, Vector3? size)
	{
		if (size.HasValue)
		{
			_boxCollider.size = size.Value;
			if (_snappingBoxCollider != null)
			{
				Vector3 vector = size.Value * 2f;
				vector.Clamp(in MinSnappingBoxColliderSize, in MaxSnappingBoxColliderSize);
				_snappingBoxCollider.size = vector;
			}
		}
		_boxCollider.center = center;
		if (_snappingBoxCollider != null)
		{
			_snappingBoxCollider.center = center;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CanHighlight() && m_OnHighlightAction(this, active: true))
		{
			AudioControllerUtils.PlaySound(m_AudioProfile.mouseEnterAudioItem);
			m_MouseInCollider = true;
			m_IsHighlighted = true;
			Highlight(active: true, m_IsSelected);
		}
	}

	public void OnGamepadClick()
	{
		if (InputManager.GamePadInUse && ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.WorldMap) && (m_MouseInCollider || m_IsHighlighted) && Singleton<InputManager>.Instance.PlayerControl.UISubmit.WasPressed)
		{
			ExecuteEvents.Execute(base.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_MouseInCollider = false;
		UnHighlight();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (InputManager.GamePadInUse)
		{
			AudioControllerUtils.PlaySound(m_AudioProfile.mouseDownAudioItem);
			Select();
		}
		else if (eventData.button == PointerEventData.InputButton.Left)
		{
			AudioControllerUtils.PlaySound(m_AudioProfile.mouseDownAudioItem);
			FFSNet.Console.LogInfo("ON MOUSE UP. IsStartingVillageUnlocked: " + IsStartingVillageUnlocked());
			Select();
		}
	}

	public void ForceHighlight(bool force)
	{
		forceHighlight = force;
		if (force)
		{
			if (!m_IsHighlighted && CanHighlight() && m_OnHighlightAction(this, active: true))
			{
				m_IsHighlighted = true;
				Highlight(active: true, m_IsSelected);
			}
		}
		else if (!m_MouseInCollider)
		{
			UnHighlight();
		}
	}

	private bool CanHighlight()
	{
		return IsSelectable();
	}

	private bool IsSelectable()
	{
		if (m_LocationQuest != null)
		{
			if (!AdventureState.MapState.IsCampaign && m_LocationQuest.QuestState >= CQuestState.EQuestState.Completed && Singleton<QuestManager>.Instance.IsHiddenCompletedQuests && m_LocationQuest != Singleton<UIMapMultiplayerController>.Instance.HostSelectedQuest)
			{
				return false;
			}
			return true;
		}
		if (AdventureState.MapState.IsCampaign && ((MapLocationType == EMapLocationType.Headquarters && Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.WorldMap && (Singleton<UIGuildmasterHUD>.Instance.IsAvailable(EGuildmasterMode.City) || Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption())) || (MapLocationType == EMapLocationType.Store && Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City && Singleton<UIGuildmasterHUD>.Instance.IsAvailable((Location as CStoreLocationState).StoreLocation.StoreType.Convert()))))
		{
			return true;
		}
		return false;
	}

	private bool CanUnhighlight()
	{
		if (m_LocationQuest != null && !AdventureState.MapState.IsCampaign && m_LocationQuest.QuestState >= CQuestState.EQuestState.Completed && Singleton<QuestManager>.Instance.IsHiddenCompletedQuests && m_LocationQuest != Singleton<UIMapMultiplayerController>.Instance.HostSelectedQuest)
		{
			return false;
		}
		return true;
	}

	private bool HasQuestPreview()
	{
		if (m_LocationQuest != null)
		{
			return true;
		}
		bool flag = Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.WorldMap || Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City;
		if (AdventureState.MapState.IsCampaign && MapLocationType == EMapLocationType.Headquarters && flag && (Singleton<MapChoreographer>.Instance.CityQuestLocations.Any() || Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption()))
		{
			return true;
		}
		return false;
	}

	private bool IsStartingVillageUnlocked()
	{
		if (MapLocationType == EMapLocationType.Scenario && LocationQuest != null && LocationQuest.Quest.Type != EQuestType.Job)
		{
			string startingVillage = LocationQuest.Quest.StartingVillage;
			CLocationState cLocationState = AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState x) => x.ID == startingVillage);
			if (cLocationState != null)
			{
				return cLocationState.LocationState == ELocationState.Unlocked;
			}
			return false;
		}
		return true;
	}

	private void UnHighlight()
	{
		if (!forceHighlight && !m_MouseInCollider && CanUnhighlight() && (m_OnHighlightAction == null || m_OnHighlightAction(this, active: false)))
		{
			m_IsHighlighted = false;
			Highlight(m_IsHighlighted, m_IsSelected);
		}
	}

	public void Init(CLocationState location, OnClickAction onClickAction, OnClickAction onHighlightAction, CQuestState locationQuest = null)
	{
		try
		{
			m_Location = location;
			m_LocationQuest = locationQuest;
			m_OnClickAction = onClickAction;
			m_OnHighlightAction = onHighlightAction;
			m_NodeTitleRegularMat = nameText.fontMaterial;
			LocationConfigUI locationConfig = UIInfoTools.Instance.GetLocationConfig(location.ID);
			if (locationConfig != null)
			{
				SetBoxColliderSize(locationConfig.ColliderCenter, locationConfig.ColliderSize);
			}
			offsetQuestMarkerPosition = locationConfig?.locationMarkerOffset ?? defaultOffset;
			m_ScenarioLocationPathIndex = null;
			m_VillageRoadRenderers = new List<LineRenderer>();
			m_ConnectedVillageIDs = new List<string>();
			if (m_Mesh != null)
			{
				UnityEngine.Object.Destroy(m_Mesh);
			}
			m_Mesh = UnityEngine.Object.Instantiate(GlobalSettings.Instance.AdventureLocationDecalPrefab, MeshParent.transform);
			try
			{
				decal = m_Mesh.GetComponent<Decal>();
				GlobalSettings.AdventureLocationMaterialSettings adventureLocationMaterialSettings = GlobalSettings.Instance.m_AdventureLocationMaterialSettings.FirstOrDefault((GlobalSettings.AdventureLocationMaterialSettings s) => s.ReferenceMaterial.Name == location.Mesh);
				if (adventureLocationMaterialSettings != null)
				{
					highlightedMeshMaterial = adventureLocationMaterialSettings.ReferenceMaterial;
					if (IsCompleted())
					{
						decal.SetUseAnotherColor(completedLocationColor);
						decal.ReferenceToMaterial = (defaultMeshMaterial = adventureLocationMaterialSettings.ReferenceMaterial);
					}
					else
					{
						decal.ReferenceToMaterial = (defaultMeshMaterial = adventureLocationMaterialSettings.ReferenceMaterial);
					}
					centerCityPositionOffset = adventureLocationMaterialSettings.CityLocationCenterPositionOffset;
					if (adventureLocationMaterialSettings.ShouldOverrideLocationScale)
					{
						MeshParent.transform.localScale = adventureLocationMaterialSettings.OverrideLocationScale;
					}
				}
				else
				{
					Debug.LogError("Unable to find location mesh material " + location.Mesh + " in GlobalSettings.AdventureLocationMaterials");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to find location mesh material " + location.Mesh + " in GlobalSettings.AdventureLocationMaterials\n" + ex.Message + "\n" + ex.StackTrace);
			}
			m_DefaultNodeScale = MeshParent.transform.localScale;
			offsetQuestMarkerPosition.Scale(base.transform.localScale);
			if (m_LineRenderer == null)
			{
				m_LineRenderer = UnityEngine.Object.Instantiate(m_NodeLineRendererPrefab, lineRenderers.transform).GetComponent<LineRenderer>();
			}
			m_LineRenderer.enabled = false;
			if (!(location is CHeadquartersState))
			{
				if (!(location is CVillageState))
				{
					if (!(location is CMapScenarioState))
					{
						if (location is CStoreLocationState)
						{
							MapLocationType = EMapLocationType.Store;
						}
					}
					else
					{
						MapLocationType = EMapLocationType.Scenario;
					}
				}
				else
				{
					MapLocationType = EMapLocationType.Village;
				}
			}
			else
			{
				MapLocationType = EMapLocationType.Headquarters;
			}
			ClearLocationMarker();
			ClearQuest();
			if (m_LocationQuest != null)
			{
				HasPendingTreasures = m_LocationQuest.CountTreasures() > 0;
				m_QuestMarker = Singleton<MapMarkersManager>.Instance.SpawnQuestMarker(this, offsetQuestMarkerPosition);
				if (!AdventureState.MapState.IsCampaign && m_LocationQuest.Quest.Type == EQuestType.Travel && m_LocationQuest.QuestState >= CQuestState.EQuestState.Completed)
				{
					m_LocationMarker = Singleton<MapMarkersManager>.Instance.SpawnLocationMarker(this, offsetQuestMarkerPosition, keepOrder: true);
				}
			}
			else if (MapLocationType == EMapLocationType.Village || MapLocationType == EMapLocationType.Store)
			{
				m_LocationMarker = Singleton<MapMarkersManager>.Instance.SpawnLocationMarker(this, offsetQuestMarkerPosition, keepOrder: true);
			}
			else if (MapLocationType == EMapLocationType.Headquarters)
			{
				if (AdventureState.MapState.IsCampaign)
				{
					m_QuestMarker = Singleton<MapMarkersManager>.Instance.SpawnQuestMarker(new HeadqueartQuest(), this, offsetQuestMarkerPosition);
				}
				else
				{
					m_LocationMarker = Singleton<MapMarkersManager>.Instance.SpawnHeadquartersMarker(this, offsetQuestMarkerPosition);
				}
			}
			if (m_QuestMarker != null && hideMarkerRequests.Count > 0)
			{
				m_QuestMarker.Hide();
			}
			if (hideLocationRequests.Count > 0)
			{
				HideLocation();
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Failed to initialise Map Node.\n" + ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	private void ClearQuest()
	{
		if (m_QuestMarker != null)
		{
			Singleton<MapMarkersManager>.Instance.RemoveQuestMarker(this, m_QuestMarker);
			m_QuestMarker = null;
		}
	}

	private void ClearLocationMarker()
	{
		if (m_LocationMarker != null)
		{
			m_LocationMarker.Hide();
			Singleton<MapMarkersManager>.Instance.RemoveLocationMarker(m_LocationMarker);
			m_LocationMarker = null;
		}
	}

	private void Highlight(bool active, bool isSelected = false)
	{
		highlightRenderer.enabled = active;
		highlightRenderer.sprite = highlightSprites[isSelected ? 1 : 0];
		if (base.gameObject.activeSelf)
		{
			if (active && !isSelected)
			{
				nodeHoverIndicator.gameObject.SetActive(value: true);
				nodeHoverIndicator.enabled = true;
				nodeHoverIndicator.fadeOut = false;
			}
			else if (nodeHoverIndicator.enabled)
			{
				nodeHoverIndicator.gameObject.SetActive(value: false);
				nodeHoverIndicator.enabled = false;
				nodeHoverIndicator.fadeOut = true;
			}
			nodeSelectIndicator.SetActive(isSelected);
		}
		Vector3 localScale = (active ? (m_DefaultNodeScale * 1.2f) : m_DefaultNodeScale);
		if (MeshParent != null)
		{
			ToggleMeshParent(isEnabled: false);
			MeshParent.transform.localScale = localScale;
			ToggleMeshParent(isEnabled: true);
		}
		decal.ReferenceToMaterial = (active ? highlightedMeshMaterial : defaultMeshMaterial);
		Singleton<MapChoreographer>.Instance.OnMapLocationHighlight(this, active);
		UpdateMarkers(active, isSelected);
	}

	private void ToggleMeshParent(bool isEnabled)
	{
		MeshParent.gameObject.SetActive(isEnabled);
	}

	private void UpdateMarkers(bool active, bool isSelected)
	{
		if (!HasQuestPreview())
		{
			return;
		}
		bool flag = !FFSNetwork.IsOnline || FFSNetwork.IsHost || Singleton<UIMapMultiplayerController>.Instance.IsShowingHostQuestToClient();
		if (isSelected && flag)
		{
			HidePreviewQuest();
			m_QuestMarker.Focus(focus: true);
			return;
		}
		if (active)
		{
			PreviewQuest();
			return;
		}
		HidePreviewQuest();
		if (FFSNetwork.IsOnline && FFSNetwork.IsClient && isSelected)
		{
			Singleton<UIQuestPopupManager>.Instance.UnfocusQuests();
			m_QuestMarker.Focus(focus: true);
		}
	}

	private void HidePreviewQuest()
	{
		if (!AdventureState.MapState.IsCampaign || (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City && m_LocationQuest != null))
		{
			ShowQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.HidePreview(m_LocationQuest);
		}
		else if (MapLocationType == EMapLocationType.Headquarters)
		{
			ShowQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.HidePreview(new HeadqueartQuest());
		}
		else if (m_LocationQuest.Quest.Type == EQuestType.City)
		{
			Singleton<MapChoreographer>.Instance.HeadquartersLocation.ShowQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.HidePreview(m_LocationQuest);
		}
		else
		{
			ShowQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.HidePreview(m_LocationQuest);
		}
	}

	private void PreviewQuest()
	{
		if (hideMarkerRequests.Count > 0)
		{
			return;
		}
		if (!AdventureState.MapState.IsCampaign)
		{
			HideQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.PreviewQuest(m_LocationQuest, this, offsetQuestMarkerPosition);
		}
		else if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			if (m_LocationQuest != null)
			{
				HideQuestMapMarker();
				if (m_LocationQuest.Quest.Type != EQuestType.City)
				{
					Singleton<UIQuestPopupManager>.Instance.PreviewWorldQuestFromCity(m_LocationQuest, this);
				}
				else
				{
					Singleton<UIQuestPopupManager>.Instance.PreviewQuest(m_LocationQuest, this, offsetQuestMarkerPosition);
				}
			}
		}
		else if (MapLocationType == EMapLocationType.Headquarters)
		{
			HideQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.PreviewQuest(new HeadqueartQuest(), this, offsetQuestMarkerPosition);
		}
		else if (m_LocationQuest.Quest.Type == EQuestType.City)
		{
			Singleton<MapChoreographer>.Instance.HeadquartersLocation.HideQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.PreviewQuest(m_LocationQuest, this, Singleton<MapChoreographer>.Instance.HeadquartersLocation.offsetQuestMarkerPosition, Singleton<MapChoreographer>.Instance.HeadquartersLocation.transform);
		}
		else
		{
			HideQuestMapMarker();
			Singleton<UIQuestPopupManager>.Instance.PreviewQuest(m_LocationQuest, this, offsetQuestMarkerPosition);
		}
	}

	public void UpdateMarkers()
	{
		UpdateMarkers(m_IsHighlighted, m_IsSelected);
	}

	public void Select(bool isHighlighted = true)
	{
		if (IsSelectable() && m_OnClickAction(this, active: true))
		{
			m_IsSelected = true;
			Highlight(isHighlighted, isSelected: true);
			nameText.fontMaterial = nodeTitleGlowMat;
		}
	}

	public void Deselect(bool keepHover = false)
	{
		if (IsSelectable() && m_OnClickAction(this, active: false))
		{
			m_IsSelected = false;
			Highlight(keepHover);
			nameText.fontMaterial = m_NodeTitleRegularMat;
		}
	}

	public void RefreshLocationLine(MapLocation currentLocation, bool forceRefresh = false)
	{
		bool flag;
		if (AdventureState.MapState.IsCampaign)
		{
			flag = true;
		}
		else
		{
			CQuestState locationQuest = LocationQuest;
			flag = locationQuest == null || locationQuest.QuestState != CQuestState.EQuestState.Completed;
		}
		if (((IsHighlighted || IsSelected) && IsStartingVillageUnlocked() && flag) || forceRefresh)
		{
			SetRenderer(m_LineRenderer, m_ActiveLine, currentLocation);
		}
		else
		{
			HideLocationLine();
		}
	}

	public void HideLocationLine()
	{
		StopPathAnimation();
		m_LineRenderer.positionCount = 0;
		targetLocation = null;
		if (m_ScenarioLocationPathIndex != null)
		{
			m_ScenarioLocationPathIndex.Item1.gameObject.SetActive(value: false);
		}
	}

	private void SetRenderer(LineRenderer renderer, Material mat, MapLocation targetLocation)
	{
		try
		{
			if (!targetLocation.gameObject.activeSelf && targetLocation.MapLocationType != EMapLocationType.Headquarters)
			{
				return;
			}
			renderer.enabled = true;
			renderer.material = mat;
			if (this.targetLocation == targetLocation)
			{
				if (pathAnimation != null && IsSelected)
				{
					RenderCurrentPath(renderer);
				}
				return;
			}
			if (pathAnimation != null && IsSelected)
			{
				RenderCurrentPath(renderer);
				return;
			}
			this.targetLocation = targetLocation;
			StopPathAnimation();
			CalculatePathToThisNode(renderer, targetLocation);
			if (m_ScenarioLocationPathIndex != null)
			{
				int num = 0;
				int num2 = Mathf.RoundToInt(PathToThisNode.Length / 2);
				Tuple<MapLocation, int> scenarioLocationPathIndex = m_ScenarioLocationPathIndex;
				num += num2;
				if (scenarioLocationPathIndex.Item1.Location is CMapScenarioState cMapScenarioState && cMapScenarioState.MapScenario.PathDistancePercentage.HasValue)
				{
					int num3 = Mathf.RoundToInt((float)PathToThisNode.Length * cMapScenarioState.MapScenario.PathDistancePercentage.Value);
					scenarioLocationPathIndex.Item1.transform.position = PathToThisNode[num3];
					m_ScenarioLocationPathIndex = new Tuple<MapLocation, int>(scenarioLocationPathIndex.Item1, num3);
				}
				else
				{
					scenarioLocationPathIndex.Item1.transform.position = PathToThisNode[num];
					m_ScenarioLocationPathIndex = new Tuple<MapLocation, int>(scenarioLocationPathIndex.Item1, num);
				}
			}
			if (IsHighlighted || !IsSelected)
			{
				if (MapLocationType == EMapLocationType.Village && LocationQuest.QuestState == CQuestState.EQuestState.Completed)
				{
					m_ScenarioLocationPathIndex.Item1.gameObject.SetActive(value: true);
				}
				else if (base.gameObject.activeInHierarchy)
				{
					pathAnimation = StartCoroutine(AnimateAppearPath(renderer, PathToThisNode));
				}
				else
				{
					AppearPathImmediatly(renderer, PathToThisNode);
				}
			}
			else
			{
				RenderCurrentPath(renderer);
			}
		}
		catch
		{
			Debug.LogError("Unable to find target location " + targetLocation.name + ".  Line renderer not set.");
		}
	}

	private void CalculatePathToThisNode(LineRenderer renderer, MapLocation targetLocation)
	{
		PathToThisNode = new Vector3[m_PathSubdivisions + 1];
		Vector3 centerPosition = targetLocation.CenterPosition;
		Vector3 centerPosition2 = CenterPosition;
		Vector3 vector = Vector3.Cross((centerPosition2 - centerPosition).normalized, Vector3.up);
		AnimationCurve animationCurve = new AnimationCurve();
		PathToThisNode[0] = centerPosition;
		animationCurve.AddKey(0f, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
		Vector3 vector2 = Vector3.zero;
		for (int i = 1; i < m_PathSubdivisions; i++)
		{
			vector2 += vector * UnityEngine.Random.Range(0f - m_PathSegmentOffset, m_PathSegmentOffset);
			float num = (float)i / (float)m_PathSubdivisions;
			float t = Mathf.InverseLerp(0.75f, 1f, num);
			vector2 = Vector3.Lerp(vector2, Vector3.zero, t);
			Vector3 vector3 = Vector3.Lerp(centerPosition, centerPosition2, num);
			PathToThisNode[i] = vector3 + vector2;
			animationCurve.AddKey(num, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
		}
		PathToThisNode[m_PathSubdivisions] = centerPosition2;
		animationCurve.AddKey(1f, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
		renderer.widthCurve = animationCurve;
	}

	private void RenderCurrentPath(LineRenderer renderer)
	{
		if (pathAnimation != null)
		{
			StopCoroutine(pathAnimation);
		}
		renderer.positionCount = m_PathSubdivisions + 1;
		for (int i = 0; i < PathToThisNode.Length; i++)
		{
			renderer.SetPosition(i, PathToThisNode[i]);
			if (m_ScenarioLocationPathIndex != null && m_ScenarioLocationPathIndex.Item2 == i)
			{
				m_ScenarioLocationPathIndex.Item1.gameObject.SetActive(value: true);
			}
		}
	}

	private void StopPathAnimation()
	{
		if (pathAnimation != null)
		{
			StopCoroutine(pathAnimation);
		}
		pathAnimation = null;
	}

	private IEnumerator AnimateAppearPath(LineRenderer renderer, Vector3[] positions)
	{
		renderer.positionCount = 0;
		List<Tuple<Vector3, float>> list = new List<Tuple<Vector3, float>>();
		for (int i = 0; i < positions.Length; i += 5)
		{
			list.Add(new Tuple<Vector3, float>(positions[i], VisibilitySphereYML.MinimumRadius));
		}
		Singleton<MapChoreographer>.Instance.RevealNewFog(list);
		for (int j = 0; j < positions.Length; j++)
		{
			yield return Timekeeper.instance.WaitForSeconds(m_PathNodeAnimationTime);
			renderer.positionCount = j + 1;
			renderer.SetPosition(j, positions[j]);
			if (m_ScenarioLocationPathIndex != null && m_ScenarioLocationPathIndex.Item2 == j)
			{
				m_ScenarioLocationPathIndex.Item1.gameObject.SetActive(value: true);
			}
		}
	}

	private void AppearPathImmediatly(LineRenderer renderer, Vector3[] positions)
	{
		renderer.positionCount = 0;
		List<Tuple<Vector3, float>> list = new List<Tuple<Vector3, float>>();
		for (int i = 0; i < positions.Length; i += 5)
		{
			list.Add(new Tuple<Vector3, float>(positions[i], VisibilitySphereYML.MinimumRadius));
		}
		Singleton<MapChoreographer>.Instance.RevealNewFog(list);
		for (int j = 0; j < positions.Length; j++)
		{
			renderer.positionCount = j + 1;
			renderer.SetPosition(j, positions[j]);
			if (m_ScenarioLocationPathIndex != null && m_ScenarioLocationPathIndex.Item2 == j)
			{
				m_ScenarioLocationPathIndex.Item1.gameObject.SetActive(value: true);
			}
		}
	}

	public void RevealPathFog()
	{
		if (PathToThisNode == null)
		{
			SetRenderer(m_LineRenderer, m_ActiveLine, this);
		}
		List<Tuple<Vector3, float>> list = new List<Tuple<Vector3, float>>();
		for (int i = 0; i < PathToThisNode.Length; i += 5)
		{
			list.Add(new Tuple<Vector3, float>(PathToThisNode[i], VisibilitySphereYML.MinimumRadius));
		}
		Singleton<MapChoreographer>.Instance.RevealNewFog(list);
	}

	public void AddScenarioLocation(MapLocation scenarioLocation)
	{
		m_ScenarioLocationPathIndex = new Tuple<MapLocation, int>(scenarioLocation, 0);
	}

	public void ShowLocation(Component request)
	{
		hideLocationRequests.Remove(request);
		ShowQuestMapMarker(instant: true, request);
		if (hideLocationRequests.Count <= 0)
		{
			base.gameObject.SetActive(value: true);
			m_ScenarioLocationPathIndex?.Item1.ShowLocation(request);
		}
	}

	private void ShowLocationMarker()
	{
		if (m_LocationMarker != null)
		{
			Singleton<MapMarkersManager>.Instance.RefreshVisibilityLocationMarker(m_LocationMarker);
		}
	}

	public void HideLocation(Component request)
	{
		hideLocationRequests.Add(request);
		HideQuestMapMarker(request);
		HideLocation();
		m_ScenarioLocationPathIndex?.Item1.HideLocation(request);
	}

	private void HideLocation()
	{
		if (m_QuestMarker != null)
		{
			m_QuestMarker.Hide();
		}
		if (m_LocationMarker != null)
		{
			m_LocationMarker.Hide();
		}
		base.gameObject.SetActive(value: false);
	}

	public MapLocation GetCurrentScenarioLocation()
	{
		if (m_LocationQuest != null)
		{
			return m_ScenarioLocationPathIndex.Item1;
		}
		return null;
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			StopPathAnimation();
			ClearQuest();
			ClearLocationMarker();
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			HideQuestMapMarker();
			if (m_LocationMarker != null)
			{
				m_LocationMarker.Hide();
			}
			if (PlatformLayer.Setting.ForceFreeMemoryParticleSystems)
			{
				DestroyEffects();
			}
		}
	}

	private void OnEnable()
	{
		if (PlatformLayer.Setting.ForceFreeMemoryParticleSystems)
		{
			CreateEffects();
		}
		ShowQuestMapMarker();
		ShowLocationMarker();
	}

	private void CreateEffects()
	{
		if (nodeHoverIndicator == null)
		{
			nodeHoverIndicator = UnityEngine.Object.Instantiate(GlobalSettings.Instance.MapLocationEffectsPrefabs.NodeHoverIndicatorPrefab, MeshParent.transform);
			nodeHoverIndicator.transform.localPosition = Vector3.zero;
			nodeHoverIndicator.transform.localRotation = Quaternion.identity;
			nodeHoverIndicator.transform.localScale = Vector3.one;
			nodeHoverIndicator.gameObject.SetActive(value: false);
		}
		if (nodeSelectIndicator == null)
		{
			nodeSelectIndicator = UnityEngine.Object.Instantiate(GlobalSettings.Instance.MapLocationEffectsPrefabs.NodeSelectIndicatorPrefab, MeshParent.transform);
			nodeSelectIndicator.transform.localPosition = Vector3.zero;
			nodeSelectIndicator.transform.localRotation = Quaternion.identity;
			nodeSelectIndicator.transform.localScale = Vector3.one;
			nodeSelectIndicator.gameObject.SetActive(value: false);
		}
	}

	private void DestroyEffects()
	{
		UnityEngine.Object.Destroy(nodeHoverIndicator.gameObject);
		UnityEngine.Object.Destroy(nodeSelectIndicator.gameObject);
	}

	public void ClearVillageRoads()
	{
		foreach (LineRenderer villageRoadRenderer in m_VillageRoadRenderers)
		{
			UnityEngine.Object.Destroy(villageRoadRenderer);
		}
		m_VillageRoadRenderers.Clear();
		m_ConnectedVillageIDs.Clear();
	}

	public void AddVillageRoad(MapLocation connectedVillage)
	{
		if (m_ConnectedVillageIDs.Contains(connectedVillage.Location.ID))
		{
			return;
		}
		LineRenderer component = UnityEngine.Object.Instantiate(m_NodeLineRendererPrefab, lineRenderers.transform).GetComponent<LineRenderer>();
		m_VillageRoadRenderers.Add(component);
		m_ConnectedVillageIDs.Add(connectedVillage.Location.ID);
		if (!connectedVillage.gameObject.activeSelf)
		{
			return;
		}
		component.enabled = true;
		component.material = m_ActiveLine;
		if (component.positionCount != m_PathSubdivisions + 1)
		{
			component.positionCount = m_PathSubdivisions + 1;
			Vector3 centerPosition = connectedVillage.CenterPosition;
			Vector3 centerPosition2 = CenterPosition;
			Vector3 vector = Vector3.Cross((centerPosition2 - centerPosition).normalized, Vector3.up);
			AnimationCurve animationCurve = new AnimationCurve();
			component.SetPosition(0, centerPosition);
			animationCurve.AddKey(0f, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
			Vector3 vector2 = Vector3.zero;
			for (int i = 1; i < m_PathSubdivisions; i++)
			{
				vector2 += vector * UnityEngine.Random.Range(0f - m_PathSegmentOffset, m_PathSegmentOffset);
				float num = (float)i / (float)m_PathSubdivisions;
				float t = Mathf.InverseLerp(0.75f, 1f, num);
				vector2 = Vector3.Lerp(vector2, Vector3.zero, t);
				Vector3 vector3 = Vector3.Lerp(centerPosition, centerPosition2, num);
				component.SetPosition(i, vector3 + vector2);
				animationCurve.AddKey(num, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
			}
			component.SetPosition(m_PathSubdivisions, centerPosition2);
			animationCurve.AddKey(1f, UnityEngine.Random.Range(m_MinPathWith, m_MaxPathWidth));
			component.widthCurve = animationCurve;
		}
	}

	public List<VisibilitySphereYML.VisibilitySphereDefinition> GetVillageRoadPositions()
	{
		List<VisibilitySphereYML.VisibilitySphereDefinition> list = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
		foreach (LineRenderer villageRoadRenderer in m_VillageRoadRenderers)
		{
			Vector3[] array = new Vector3[villageRoadRenderer.positionCount];
			villageRoadRenderer.GetPositions(array);
			for (int i = 5; i < array.Length; i += 5)
			{
				Vector3 vector = array[i];
				list.Add(new VisibilitySphereYML.VisibilitySphereDefinition(new CVector3(vector.x, vector.y, vector.z), VisibilitySphereYML.MinimumRadius));
			}
		}
		return list;
	}

	public void SetFocused(bool focused)
	{
		if (MapLocationType != EMapLocationType.Headquarters)
		{
			decal.Fade = (focused ? 1f : unfocusedOppacity);
		}
	}

	public bool IsCompleted()
	{
		if (LocationQuest != null && LocationQuest.QuestState >= CQuestState.EQuestState.Completed && LocationQuest.QuestState != CQuestState.EQuestState.Blocked)
		{
			return AdventureState.MapState.IsCampaign;
		}
		return false;
	}

	public void ShowQuestMapMarker(bool instant = true, Component request = null)
	{
		hideMarkerRequests.Remove(request ?? this);
		if (!(m_QuestMarker == null))
		{
			if (hideMarkerRequests.Count == 0 && m_LocationMarker == null)
			{
				m_QuestMarker.Show(instant);
			}
			else
			{
				m_QuestMarker.Hide();
			}
		}
	}

	public void HideQuestMapMarker(Component request = null)
	{
		hideMarkerRequests.Add(request ?? this);
		if (!(m_QuestMarker == null))
		{
			m_QuestMarker.Hide();
		}
	}

	public void DisableInteraction(bool disable)
	{
		_boxCollider.enabled = !disable;
	}
}
