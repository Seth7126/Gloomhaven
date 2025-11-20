using System.Collections.Generic;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

public class HexSelect_Control : MonoBehaviour
{
	public enum HexType
	{
		NegativeEffect,
		PositiveEffect,
		Move,
		InvalidPlacement,
		ExitDungeonRevealed,
		ExitDungeonUnrevealed,
		ActiveBonusCondition,
		DifficultTerrain,
		Objective
	}

	public enum HexMode
	{
		Reach,
		PossibleTarget,
		Selected,
		SelectedAOEWithoutTarget,
		Cursor
	}

	private static readonly int _crossHair = Shader.PropertyToID("_CrossHair");

	private static readonly int _wOn = Shader.PropertyToID("_W_On");

	private static readonly int _swOn = Shader.PropertyToID("_SW_On");

	private static readonly int _seOn = Shader.PropertyToID("_SE_On");

	private static readonly int _eOn = Shader.PropertyToID("_E_On");

	private static readonly int _neOn = Shader.PropertyToID("_NE_On");

	private static readonly int _nwOn = Shader.PropertyToID("_NW_On");

	private static readonly int _targetFrameIntensity = Shader.PropertyToID("_TargetFrameIntensity");

	private static readonly int _borderFlameIntensity = Shader.PropertyToID("_BorderFlameIntensity");

	private static readonly int _borderLineIntensity = Shader.PropertyToID("_BorderLineIntensity");

	private static readonly int _hexIntensity = Shader.PropertyToID("_HexIntensity");

	private static readonly int _hexColour = Shader.PropertyToID("_HexColour");

	public const float RecycleDelay = 0.3f;

	private bool m_Initialised;

	private HexType m_CachedType;

	public HexType Type;

	private HexMode m_CachedMode;

	public HexMode Mode;

	private bool m_CachedHover;

	public bool Hover;

	private bool m_CachedW_On;

	private bool m_CachedNw_On;

	private bool m_CachedNe_On;

	private bool m_CachedE_On;

	private bool m_CachedSw_On;

	private bool m_CachedSe_On;

	public bool nw_On;

	public bool ne_On;

	public bool e_On;

	public bool se_On;

	public bool sw_On;

	public bool w_On;

	public bool borderParticleBitsByDefault = true;

	private int m_HoverFlagsCached;

	private int m_HoverFlags;

	private bool enableRayCast = true;

	private float hexBaseRotation;

	public bool disableCrossHair;

	private bool TargetFrame;

	private bool[] isOn;

	private float sparkRateMin;

	private float sparkRateMax;

	private float borderFlameIntensity;

	private float borderLineIntensity;

	private float hexIntensity;

	private float targetFrameIntensity;

	private Color hexColour;

	public float cursorIntensity = 0.2f;

	[Header("Hex Material Settings")]
	public float reachHexIntensity = 0.25f;

	public float reachBorderFlameIntensity = 0.2f;

	public float reachBorderLineIntensity = 0.7f;

	public float targetHexIntensity = 0.35f;

	public float targetBorderFlameIntensity = 0.35f;

	public float targetBorderLineIntensity = 0.5f;

	public float selectedHexIntensity = 0.35f;

	public float selectedBorderFlameIntensity = 0.35f;

	public float selectedBorderLineIntensity = 0.35f;

	public float selectedTargetFrameIntensity = 0.88f;

	[Header("Hover Material Adjustments")]
	public float hoverReachHexIntensityAdd = 0.2f;

	public float hoverReachBorderLineIntensityAdd = -0.2f;

	public float hoverTargetHexIntensityAdd = 0.2f;

	public float hoverTargetBorderLineIntensityAdd;

	public float hoverSelectedHexIntensityAdd = 0.2f;

	public float hoverSelectedBorderLineIntensityAdd;

	[Header("Negative Effect Hex Colours")]
	public Color negativeEffectReachColour = new Color(1f, 0.8f, 0.6f, 1f);

	public Color negativeEffectReachColourHover = new Color(0.8f, 0.8f, 0.6f, 1f);

	public Color negativeEffectPossibleTargetColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectPossibleTargetColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color negativeEffectSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Positive Effect Hex Colours")]
	public Color positiveEffectReachColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectReachColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectPossibleTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectPossibleTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Move Hex Colours")]
	public Color moveReachColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveReachColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color movePossibleTargetColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color movePossibleTargetColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveSelectedColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveSelectedColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	[Header("Difficult Terrain Hex Colours")]
	public Color difficultTerrainReachColour = new Color(1f, 0.8f, 0.6f, 1f);

	public Color difficultTerrainReachColourHover = new Color(0.8f, 0.8f, 0.6f, 1f);

	public Color difficultTerrainPossibleTargetColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainPossibleTargetColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color difficultTerrainSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Other Hex Colours")]
	public Color invalidPlacementColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color dungeonExitColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	public Color activeBonusConditionColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	public Color objectiveColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	[Space(10f)]
	[SerializeField]
	private Material _exampleMaterial;

	private Material m_Material;

	private ParticleSystem.Particle[] m_Particles;

	private HexSelectControlParticles _hexSelectControlParticles;

	[field: Header("Child components to control")]
	[field: SerializeField]
	public MeshRenderer HexProjector { get; set; }

	[UsedImplicitly]
	private void Start()
	{
		RefreshHexUI();
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		m_Particles = new ParticleSystem.Particle[100];
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		HideHexParticles();
	}

	public void SetNeighbors(CClientTile currentTile, HashSet<CClientTile> activeTiles, List<CClientTile> hoveredTiles = null)
	{
		w_On = (nw_On = (ne_On = (e_On = (sw_On = (se_On = true)))));
		m_HoverFlags = -1;
		for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.ELeft; eAdjacentPosition <= ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition++)
		{
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(currentTile.m_Tile.m_ArrayIndex.X, currentTile.m_Tile.m_ArrayIndex.Y, eAdjacentPosition);
			if (adjacentTile == null)
			{
				continue;
			}
			CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if (hoveredTiles != null && hoveredTiles.Contains(item))
			{
				m_HoverFlags &= ~(1 << (int)(eAdjacentPosition - 1));
			}
			if (activeTiles == null || activeTiles.Contains(item))
			{
				switch (eAdjacentPosition)
				{
				case ScenarioManager.EAdjacentPosition.ELeft:
					w_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ETopLeft:
					nw_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ETopRight:
					ne_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ERight:
					e_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.EBottomLeft:
					sw_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.EBottomRight:
					se_On = false;
					break;
				}
			}
		}
	}

	public void SetAllNeighborsOn()
	{
		w_On = (nw_On = (ne_On = (e_On = (sw_On = (se_On = true)))));
	}

	[ContextMenu("Refresh")]
	public void DEBUG_Refresh()
	{
		RefreshHexUI(forceUpdate: false);
	}

	[ContextMenu("Hard Refresh")]
	public void DEBUG_HardRefresh()
	{
		RefreshHexUI();
	}

	public void RefreshHexUI(bool forceUpdate = true)
	{
		if (forceUpdate || EvaluateCacheChanged() || !m_Initialised)
		{
			m_Initialised = true;
			isOn = new bool[6];
			isOn.SetValue(nw_On, 0);
			isOn.SetValue(ne_On, 1);
			isOn.SetValue(e_On, 2);
			isOn.SetValue(se_On, 3);
			isOn.SetValue(sw_On, 4);
			isOn.SetValue(w_On, 5);
			m_Material = new Material(_exampleMaterial);
			HexProjector.material = m_Material;
			if (disableCrossHair)
			{
				m_Material.SetFloat(_crossHair, 0f);
			}
			else
			{
				m_Material.SetFloat(_crossHair, 1f);
			}
			HexUpdate();
		}
	}

	private bool EvaluateCacheChanged()
	{
		if (m_CachedW_On != w_On || m_CachedNw_On != nw_On || m_CachedNe_On != ne_On || m_CachedE_On != e_On || m_CachedSe_On != se_On || m_CachedSw_On != sw_On || m_CachedHover != Hover || m_CachedMode != Mode || m_CachedType != Type || m_HoverFlags != m_HoverFlagsCached)
		{
			m_CachedW_On = w_On;
			m_CachedNw_On = nw_On;
			m_CachedNe_On = ne_On;
			m_CachedE_On = e_On;
			m_CachedSe_On = se_On;
			m_CachedSw_On = sw_On;
			m_CachedHover = Hover;
			m_CachedMode = Mode;
			m_CachedType = Type;
			m_HoverFlagsCached = m_HoverFlags;
			return true;
		}
		return false;
	}

	public void SetHover(bool hover)
	{
		Hover = hover;
		HexUpdate();
	}

	public void HexUpdate()
	{
		HexSettings();
		BorderParticleAdjustment();
		ProjectorMaterialAdjustment();
	}

	private void HexSettings()
	{
		switch (Type)
		{
		case HexType.NegativeEffect:
			switch (Mode)
			{
			case HexMode.Reach:
				if (Hover)
				{
					hexColour = negativeEffectReachColourHover;
				}
				else
				{
					hexColour = negativeEffectReachColour;
				}
				break;
			case HexMode.PossibleTarget:
				if (Hover)
				{
					hexColour = negativeEffectPossibleTargetColourHover;
				}
				else
				{
					hexColour = negativeEffectPossibleTargetColour;
				}
				break;
			case HexMode.Selected:
				if (Hover)
				{
					hexColour = negativeEffectSelectedColourHover;
				}
				else
				{
					hexColour = negativeEffectSelectedColour;
				}
				break;
			case HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					hexColour = negativeEffectSelectedAoEWithoutTargetColour;
				}
				else
				{
					hexColour = negativeEffectSelectedAoEWithoutTargetColourHover;
				}
				break;
			}
			HideUnseenGroundPlane();
			break;
		case HexType.PositiveEffect:
			switch (Mode)
			{
			case HexMode.Reach:
				if (Hover)
				{
					hexColour = positiveEffectReachColour;
				}
				else
				{
					hexColour = positiveEffectReachColourHover;
				}
				break;
			case HexMode.PossibleTarget:
				if (Hover)
				{
					hexColour = positiveEffectPossibleTargetColour;
				}
				else
				{
					hexColour = positiveEffectPossibleTargetColourHover;
				}
				break;
			case HexMode.Selected:
				if (Hover)
				{
					hexColour = positiveEffectSelectedColour;
				}
				else
				{
					hexColour = positiveEffectSelectedColourHover;
				}
				break;
			case HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					hexColour = positiveEffectSelectedAoEWithoutTargetColour;
				}
				else
				{
					hexColour = positiveEffectSelectedAoEWithoutTargetColourHover;
				}
				break;
			}
			HideUnseenGroundPlane();
			break;
		case HexType.Move:
			switch (Mode)
			{
			case HexMode.Reach:
				if (Hover)
				{
					hexColour = moveReachColour;
				}
				else
				{
					hexColour = moveReachColourHover;
				}
				break;
			case HexMode.PossibleTarget:
				if (Hover)
				{
					hexColour = movePossibleTargetColour;
				}
				else
				{
					hexColour = movePossibleTargetColourHover;
				}
				break;
			case HexMode.Selected:
				if (Hover)
				{
					hexColour = moveSelectedColour;
				}
				else
				{
					hexColour = moveSelectedColourHover;
				}
				break;
			}
			HideUnseenGroundPlane();
			break;
		case HexType.InvalidPlacement:
			hexColour = invalidPlacementColour;
			HideUnseenGroundPlane();
			break;
		case HexType.ExitDungeonRevealed:
			hexColour = dungeonExitColour;
			HideUnseenGroundPlane();
			break;
		case HexType.ExitDungeonUnrevealed:
			hexColour = dungeonExitColour;
			HideUnseenGroundPlane();
			break;
		case HexType.ActiveBonusCondition:
			hexColour = activeBonusConditionColour;
			HideUnseenGroundPlane();
			break;
		case HexType.DifficultTerrain:
			switch (Mode)
			{
			case HexMode.Reach:
				if (Hover)
				{
					hexColour = difficultTerrainReachColourHover;
				}
				else
				{
					hexColour = difficultTerrainReachColour;
				}
				break;
			case HexMode.PossibleTarget:
				if (Hover)
				{
					hexColour = difficultTerrainPossibleTargetColourHover;
				}
				else
				{
					hexColour = difficultTerrainPossibleTargetColour;
				}
				break;
			case HexMode.Selected:
				if (Hover)
				{
					hexColour = difficultTerrainSelectedColourHover;
				}
				else
				{
					hexColour = difficultTerrainSelectedColour;
				}
				break;
			case HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					hexColour = difficultTerrainSelectedAoEWithoutTargetColourHover;
				}
				else
				{
					hexColour = difficultTerrainSelectedAoEWithoutTargetColour;
				}
				break;
			}
			HideUnseenGroundPlane();
			break;
		case HexType.Objective:
			hexColour = objectiveColour;
			HideUnseenGroundPlane();
			break;
		default:
			hexColour = moveReachColour;
			HideUnseenGroundPlane();
			break;
		}
		switch (Mode)
		{
		case HexMode.Reach:
			hexIntensity = reachHexIntensity;
			borderFlameIntensity = reachBorderFlameIntensity;
			borderLineIntensity = reachBorderLineIntensity;
			sparkRateMin = 15f;
			sparkRateMax = 30f;
			TargetFrame = false;
			break;
		case HexMode.PossibleTarget:
			hexIntensity = targetHexIntensity;
			borderFlameIntensity = targetBorderFlameIntensity;
			borderLineIntensity = targetBorderLineIntensity;
			sparkRateMin = 50f;
			sparkRateMax = 70f;
			TargetFrame = false;
			break;
		case HexMode.Selected:
			hexIntensity = selectedHexIntensity;
			borderFlameIntensity = selectedBorderFlameIntensity;
			borderLineIntensity = selectedBorderLineIntensity;
			sparkRateMin = 90f;
			sparkRateMax = 120f;
			targetFrameIntensity = selectedTargetFrameIntensity;
			TargetFrame = true;
			break;
		case HexMode.Cursor:
			hexColour = moveReachColour;
			hexIntensity = cursorIntensity;
			borderFlameIntensity = targetBorderFlameIntensity;
			borderLineIntensity = targetBorderLineIntensity;
			sparkRateMin = 50f;
			sparkRateMax = 70f;
			TargetFrame = false;
			break;
		case HexMode.SelectedAOEWithoutTarget:
			break;
		}
	}

	private void BorderParticleAdjustment()
	{
		for (int i = 0; i < 6; i++)
		{
			int num = i;
			if ((borderParticleBitsByDefault && isOn[i] && Mode == HexMode.Cursor) || Mode == HexMode.PossibleTarget || Mode == HexMode.Selected || (Hover && Mode == HexMode.Reach))
			{
				SpawnHexParticles();
				Color color = hexColour;
				Color color2 = positiveEffectReachColour;
				float min = sparkRateMin;
				float max = sparkRateMax;
				ParticleSystem particleSystem = _hexSelectControlParticles.ParticleBits[num];
				if (particleSystem != null)
				{
					ParticleSystem.MainModule main = particleSystem.main;
					main.startColor = new ParticleSystem.MinMaxGradient(new Color(color.r, color2.g, color.b, 1f));
					ParticleSystem.EmissionModule emission = particleSystem.emission;
					emission.rateOverTime = new ParticleSystem.MinMaxCurve(min, max);
					_hexSelectControlParticles.ParticleBits[num].gameObject.SetActive(value: true);
				}
			}
			else if (_hexSelectControlParticles != null)
			{
				_hexSelectControlParticles.ParticleBits[num].gameObject.SetActive(value: false);
			}
			if (Hover)
			{
				Color color3 = hexColour;
				if (Type == HexType.Move)
				{
					color3.a = 0.25f;
				}
				SpawnHexParticles();
				if (_hexSelectControlParticles.ParticleHover[num] != null)
				{
					ParticleSystem.MainModule main2 = _hexSelectControlParticles.ParticleHover[num].main;
					main2.startColor = new ParticleSystem.MinMaxGradient(color3);
					int particles = _hexSelectControlParticles.ParticleHover[num].GetParticles(m_Particles);
					for (int j = 0; j < particles; j++)
					{
						m_Particles[j].startColor = color3;
					}
					_hexSelectControlParticles.ParticleHover[num].SetParticles(m_Particles, particles);
				}
			}
			int hoverFlags = m_HoverFlags;
			bool hover = Hover;
			bool active = (hoverFlags & (1 << num)) > 0 && hover;
			if (Mode == HexMode.Reach)
			{
				if (_hexSelectControlParticles != null && _hexSelectControlParticles.ParticleHover[num] != null)
				{
					_hexSelectControlParticles.ParticleHover[num].gameObject.SetActive(value: false);
				}
				continue;
			}
			SpawnHexParticles();
			if (_hexSelectControlParticles.ParticleHover[num] != null)
			{
				_hexSelectControlParticles.ParticleHover[num].gameObject.SetActive(active);
			}
		}
	}

	private void CastHexOrientation()
	{
	}

	private void ProjectorMaterialAdjustment()
	{
		if (Hover)
		{
			if (Mode == HexMode.Reach)
			{
				hexIntensity += hoverReachHexIntensityAdd;
				borderLineIntensity += hoverReachBorderLineIntensityAdd;
			}
			else if (Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor)
			{
				hexIntensity += hoverTargetHexIntensityAdd;
				borderLineIntensity += hoverTargetBorderLineIntensityAdd;
			}
			else if (Mode == HexMode.Selected)
			{
				hexIntensity += hoverSelectedHexIntensityAdd;
				borderLineIntensity += hoverSelectedBorderLineIntensityAdd;
			}
		}
		m_Material.SetColor(_hexColour, hexColour);
		m_Material.SetFloat(_hexIntensity, hexIntensity);
		m_Material.SetFloat(_borderLineIntensity, borderLineIntensity);
		m_Material.SetFloat(_borderFlameIntensity, borderFlameIntensity);
		m_Material.SetFloat(_targetFrameIntensity, targetFrameIntensity);
		float num = 0f;
		num = ((!TargetFrame) ? 0f : 1f);
		m_Material.SetFloat(_crossHair, num);
		if (nw_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_nwOn, 1);
		}
		else
		{
			m_Material.SetInt(_nwOn, 0);
		}
		if (ne_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_neOn, 1);
		}
		else
		{
			m_Material.SetInt(_neOn, 0);
		}
		if (e_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_eOn, 1);
		}
		else
		{
			m_Material.SetInt(_eOn, 0);
		}
		if (se_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_seOn, 1);
		}
		else
		{
			m_Material.SetInt(_seOn, 0);
		}
		if (sw_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_swOn, 1);
		}
		else
		{
			m_Material.SetInt(_swOn, 0);
		}
		if (w_On || Mode == HexMode.PossibleTarget || Mode == HexMode.Cursor || Mode == HexMode.Selected || Hover)
		{
			m_Material.SetInt(_wOn, 1);
		}
		else
		{
			m_Material.SetInt(_wOn, 0);
		}
	}

	private void SpawnHexParticles()
	{
		if (_hexSelectControlParticles == null)
		{
			GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.VisualEffects.HexSelectControlParticles, base.transform);
			_hexSelectControlParticles = gameObject.GetComponent<HexSelectControlParticles>();
		}
	}

	private void HideUnseenGroundPlane()
	{
		if (_hexSelectControlParticles != null)
		{
			_hexSelectControlParticles.UnseenGroundPlane.gameObject.SetActive(value: false);
		}
	}

	private void HideHexParticles()
	{
		if (_hexSelectControlParticles != null)
		{
			ObjectPool.Recycle(_hexSelectControlParticles.gameObject, 0.3f, GlobalSettings.Instance.VisualEffects.HexSelectControlParticles);
			_hexSelectControlParticles = null;
		}
	}
}
