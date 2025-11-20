using System.Collections.Generic;
using EPOOutline;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

public class WorldspaceUITools : MonoBehaviour
{
	public static WorldspaceUITools Instance;

	private List<Outlinable> m_AllOutlinables = new List<Outlinable>();

	private List<Outlinable> m_HoveredOutlines = new List<Outlinable>();

	private List<Outlinable> m_AbilityFocusOutlines = new List<Outlinable>();

	private Dictionary<CActor, List<Outlinable>> m_ActorOutlinables = new Dictionary<CActor, List<Outlinable>>();

	private List<WorldspacePanelUIController> _panelUIControllers = new List<WorldspacePanelUIController>();

	private List<(WorldspacePanelUIController, float)> _panelUIControllersBuffer = new List<(WorldspacePanelUIController, float)>();

	private bool m_OutlinesEnabled;

	public GameObject WorldspaceGUIPrefabLevel;

	public GameObject WorldspaceTileBehaviourUIPrefab;

	[SerializeField]
	private Canvas worldspaceCanvas;

	private Camera worldspaceCamera;

	public Camera WorldspaceCamera => worldspaceCamera;

	public Canvas WorldspaceCanvas => worldspaceCanvas;

	public bool OutlinesEnabled => m_OutlinesEnabled;

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		m_OutlinesEnabled = false;
		SaveData.Instance.Global.DisableAbilityFocusOutlinesChanged += DisableAbilityFocusOutlinesChanged;
	}

	private void DisableAbilityFocusOutlinesChanged(bool value)
	{
		foreach (Outlinable abilityFocusOutline in m_AbilityFocusOutlines)
		{
			abilityFocusOutline.OutlineParameters.Enabled = !value;
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void Init(Camera camera)
	{
		worldspaceCamera = camera;
		m_ActorOutlinables = new Dictionary<CActor, List<Outlinable>>();
	}

	public void AddOutlinableToList(Outlinable outlinable)
	{
		m_AllOutlinables.Add(outlinable);
		outlinable.OutlineParameters.Enabled = m_OutlinesEnabled;
	}

	public void RemoveOutlinableFromList(Outlinable outlinable)
	{
		outlinable.OutlineParameters.Enabled = false;
		m_AllOutlinables.Remove(outlinable);
	}

	public void AddWorldspacePanelUIController(WorldspacePanelUIController panel)
	{
		_panelUIControllers.Add(panel);
	}

	public void RemovePanelUIController(WorldspacePanelUIController panel)
	{
		_panelUIControllers.Remove(panel);
	}

	[UsedImplicitly]
	private void Update()
	{
		if (!m_OutlinesEnabled && InputManager.GetIsPressed(KeyAction.HIGHLIGHT))
		{
			ActivateAllOutlines(active: true);
		}
		if (m_OutlinesEnabled && (InputManager.GetWasReleased(KeyAction.HIGHLIGHT) || !Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(KeyAction.HIGHLIGHT).Enabled))
		{
			ActivateAllOutlines(active: false);
		}
		OrderWorldspaceUIPanels();
	}

	private void OrderWorldspaceUIPanels()
	{
		if (worldspaceCamera == null)
		{
			return;
		}
		Vector3 position = worldspaceCamera.transform.position;
		_panelUIControllersBuffer.Clear();
		foreach (WorldspacePanelUIController panelUIController in _panelUIControllers)
		{
			_panelUIControllersBuffer.Add((panelUIController, Vector3.Distance(panelUIController.TargetPosition, position)));
		}
		_panelUIControllersBuffer.Sort(SortFunc);
		bool flag = false;
		for (int i = 0; i < _panelUIControllersBuffer.Count; i++)
		{
			if (_panelUIControllersBuffer[i].Item1 != _panelUIControllers[i])
			{
				flag = true;
			}
		}
		if (flag)
		{
			for (int j = 0; j < _panelUIControllersBuffer.Count; j++)
			{
				_panelUIControllersBuffer[j].Item1.transform.SetAsLastSibling();
				_panelUIControllers[j] = _panelUIControllersBuffer[j].Item1;
			}
		}
	}

	private int SortFunc((WorldspacePanelUIController, float) x, (WorldspacePanelUIController, float) y)
	{
		return y.Item2.CompareTo(x.Item2);
	}

	private void ActivateAllOutlines(bool active)
	{
		for (int num = m_AllOutlinables.Count - 1; num >= 0; num--)
		{
			m_OutlinesEnabled = active;
			if ((bool)m_AllOutlinables[num])
			{
				m_AllOutlinables[num].OutlineParameters.Enabled = m_OutlinesEnabled;
			}
			else
			{
				m_AllOutlinables.Remove(m_AllOutlinables[num]);
			}
		}
	}

	public void EnableHoveredOutline(Outlinable hoveredOutlinable)
	{
		if (!SaveData.Instance.Global.DisableHoverOutlines)
		{
			m_HoveredOutlines.Add(hoveredOutlinable);
			hoveredOutlinable.OutlineParameters.Enabled = true;
		}
	}

	public void DisableHoveredOutline(Outlinable hoveredOutlinable)
	{
		if (!SaveData.Instance.Global.DisableHoverOutlines)
		{
			m_HoveredOutlines.Remove(hoveredOutlinable);
			if (!m_AbilityFocusOutlines.Contains(hoveredOutlinable))
			{
				hoveredOutlinable.OutlineParameters.Enabled = m_OutlinesEnabled;
			}
		}
	}

	public void DisableAllHoveredOutlines()
	{
		if (SaveData.Instance.Global.DisableHoverOutlines)
		{
			return;
		}
		foreach (Outlinable hoveredOutline in m_HoveredOutlines)
		{
			if (!m_AbilityFocusOutlines.Contains(hoveredOutline))
			{
				hoveredOutline.OutlineParameters.Enabled = m_OutlinesEnabled;
			}
		}
		m_HoveredOutlines.Clear();
	}

	public void EnableAbilityFocusOutline(Outlinable abilityFocusOutlinable)
	{
		m_AbilityFocusOutlines.Add(abilityFocusOutlinable);
		if (!SaveData.Instance.Global.DisableAbilityFocusOutlines)
		{
			abilityFocusOutlinable.OutlineParameters.Enabled = true;
		}
	}

	public void DisableAbilityFocusOutline(Outlinable abilityFocusOutlinable)
	{
		m_AbilityFocusOutlines.Remove(abilityFocusOutlinable);
		if (!SaveData.Instance.Global.DisableAbilityFocusOutlines && !m_HoveredOutlines.Contains(abilityFocusOutlinable))
		{
			abilityFocusOutlinable.OutlineParameters.Enabled = m_OutlinesEnabled;
		}
	}

	public void RegisterActorOutlinable(CActor actor, List<Outlinable> outlinables)
	{
		if (m_ActorOutlinables.TryGetValue(actor, out var _))
		{
			foreach (Outlinable outlinable in outlinables)
			{
				if (!m_ActorOutlinables[actor].Contains(outlinable))
				{
					m_ActorOutlinables[actor].Add(outlinable);
				}
			}
			return;
		}
		m_ActorOutlinables.Add(actor, outlinables);
	}

	public void DeregisterActorOutlinable(CActor actor)
	{
		m_ActorOutlinables.Remove(actor);
	}

	public List<Outlinable> GetActorOutlinable(CActor actor)
	{
		m_ActorOutlinables.TryGetValue(actor, out var value);
		return value;
	}
}
