#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class ControllerInputAreaManager : ControllerInputElement
{
	[Serializable]
	public class ControllerInputAreaTypeEvent : UnityEvent<string, bool>
	{
	}

	[SerializeField]
	private EControllerInputAreaType m_DefaultFocusArea;

	private List<IControllerInputArea> m_AvailableAreas = new List<IControllerInputArea>();

	private List<string> m_StackedAreas = new List<string>();

	private const string TAG = "[AREA MANAGER]";

	[SerializeField]
	public ControllerInputAreaTypeEvent OnChangedFocusedArea;

	private bool m_IsEnabled;

	public string m_FocusArea { get; private set; }

	public static bool IsEnabled
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_IsEnabled;
			}
			return false;
		}
	}

	public static ControllerInputAreaManager Instance { get; private set; }

	public EControllerInputAreaType DefaultFocusArea => m_DefaultFocusArea;

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		EnableControllerInputAreas();
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		DisableControllerInputAreas();
	}

	private static void Log(string message)
	{
		Debug.LogController("[AREA MANAGER] " + message);
	}

	public void RegisterArea(IControllerInputArea area)
	{
		m_AvailableAreas.Add(area);
		Log($"Register area {area.Id} (object {area})");
		if (IsEnabled)
		{
			Log($"Enable area {area.Id} (focused {m_FocusArea == area.Id})");
			area.EnableGroup(m_FocusArea == area.Id);
		}
		else
		{
			Log("Disable area " + area.Id);
			area.DisableGroup();
		}
	}

	public void UnregisterArea(IControllerInputArea area)
	{
		if (m_AvailableAreas.Remove(area))
		{
			Log("Unregister area " + area.Id);
		}
		UnfocusArea(area);
		RemoveStackedArea(area.Id);
	}

	public void UnregisterAllAreas()
	{
		m_AvailableAreas.Clear();
		m_StackedAreas.Clear();
		m_FocusArea = null;
	}

	private IControllerInputArea GetArea(string area)
	{
		return m_AvailableAreas.FirstOrDefault((IControllerInputArea it) => it.Id == area);
	}

	public void FocusArea(EControllerInputAreaType area, bool stack = false)
	{
		FocusArea((area == EControllerInputAreaType.None) ? null : area.ToString(), stack);
	}

	public void FocusArea(string area, bool stack = false, bool returnPrevious = false)
	{
		if (m_FocusArea == area)
		{
			return;
		}
		if (area == null)
		{
			UnfocusArea();
			return;
		}
		IControllerInputArea area2 = GetArea(area);
		if (area2 == null)
		{
			Debug.LogErrorController("[AREA MANAGER] Tried to focus on an area that is not available: " + area);
			return;
		}
		IControllerInputArea area3 = GetArea(m_FocusArea);
		if (!returnPrevious && area3 != null && area3.BlockOthersFocusWhileIsFocused)
		{
			if (area2.BlockOthersFocusWhileIsFocused)
			{
				if (stack)
				{
					m_StackedAreas.Add(m_FocusArea);
					Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
				}
				UnfocusArea();
				SetFocusArea(area2);
			}
			else if (stack)
			{
				int index = 0;
				if (m_StackedAreas.Count > 0)
				{
					index = m_StackedAreas.Count - 1;
				}
				m_StackedAreas.Insert(index, m_FocusArea);
				Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
			}
		}
		else
		{
			if (stack && m_FocusArea != null)
			{
				m_StackedAreas.Add(m_FocusArea);
				Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
			}
			UnfocusArea();
			SetFocusArea(area2);
		}
	}

	public void FocusArea(IControllerInputArea area, bool stack = false)
	{
		if (m_FocusArea == area.Id || !m_AvailableAreas.Contains(area))
		{
			return;
		}
		IControllerInputArea area2 = GetArea(m_FocusArea);
		if (area2 != null && area2.BlockOthersFocusWhileIsFocused)
		{
			if (area.BlockOthersFocusWhileIsFocused)
			{
				if (stack)
				{
					m_StackedAreas.Add(m_FocusArea);
					Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
				}
				UnfocusArea();
				SetFocusArea(area);
			}
			else if (stack)
			{
				m_StackedAreas.Insert(m_StackedAreas.Count - 1, m_FocusArea);
				Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
			}
		}
		else
		{
			if (stack && m_FocusArea != null)
			{
				m_StackedAreas.Add(m_FocusArea);
				Log("Stacked area " + m_FocusArea + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
			}
			UnfocusArea();
			SetFocusArea(area);
		}
	}

	private void SetFocusArea(IControllerInputArea area)
	{
		if (area == null)
		{
			return;
		}
		m_FocusArea = area.Id;
		Log("Set Focused Area " + area.Id);
		RemoveStackedArea(m_FocusArea);
		if (IsEnabled)
		{
			if (!area.IsFocused)
			{
				area.Focus();
			}
			OnChangedFocusedArea.Invoke(area.Id, area.IsFocused);
		}
	}

	private void UnfocusArea()
	{
		if (m_FocusArea == null)
		{
			return;
		}
		Log("Unfocus Focused " + m_FocusArea);
		if (IsEnabled)
		{
			IControllerInputArea area = GetArea(m_FocusArea);
			m_FocusArea = null;
			if (area != null && area.IsFocused)
			{
				area.Unfocus();
			}
			OnChangedFocusedArea.Invoke(m_FocusArea, arg1: false);
		}
		else
		{
			m_FocusArea = null;
		}
	}

	private bool ReturnPrevious()
	{
		string text = ((m_StackedAreas.Count > 0) ? m_StackedAreas.Last() : ((m_DefaultFocusArea == EControllerInputAreaType.None) ? null : m_DefaultFocusArea.ToString()));
		Log("Return to previous area " + text + " from area " + m_FocusArea + ")");
		if (m_FocusArea == text)
		{
			return false;
		}
		FocusArea(text, stack: false, returnPrevious: true);
		return true;
	}

	public void UnfocusArea(EControllerInputAreaType area)
	{
		UnfocusArea((area == EControllerInputAreaType.None) ? null : area.ToString());
	}

	public void UnfocusArea(string area, bool stack = false)
	{
		if (m_FocusArea == area && !SceneController.Instance.IsLoading)
		{
			ReturnPrevious();
		}
		if (stack && m_FocusArea != area && !m_StackedAreas.Contains(area))
		{
			m_StackedAreas.Add(area);
			Log("Stack unfocused area " + area + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
		}
	}

	public void UnfocusArea(IControllerInputArea area)
	{
		UnfocusArea(area.Id);
	}

	public static bool IsFocusedArea(EControllerInputAreaType area)
	{
		if (Instance == null)
		{
			return false;
		}
		if ((!Instance.m_FocusArea.IsNullOrEmpty() || area != EControllerInputAreaType.None) && !(Instance.m_FocusArea == area.ToString()))
		{
			if (area == EControllerInputAreaType.Tutorial)
			{
				return Instance.IsFocusedAreaType("Tutorial");
			}
			return false;
		}
		return true;
	}

	public static bool IsFocusedAnyArea(params EControllerInputAreaType[] areaS)
	{
		if (Instance == null)
		{
			return false;
		}
		return areaS.Any(IsFocusedArea);
	}

	public static bool IsFocusedDefaultArea()
	{
		if (Instance == null)
		{
			return false;
		}
		return IsFocusedArea(Instance.m_DefaultFocusArea);
	}

	public static bool IsFocusedArea(string area)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.m_FocusArea == area;
	}

	private bool IsFocusedAreaType(string area)
	{
		if (m_FocusArea != null)
		{
			return m_FocusArea.Contains(area);
		}
		return false;
	}

	public bool ReturnToStackedArea()
	{
		if (m_StackedAreas.Count == 0)
		{
			return false;
		}
		ReturnPrevious();
		return true;
	}

	public void EnableControllerInputAreas()
	{
		m_IsEnabled = true;
		string text = m_FocusArea ?? ((m_StackedAreas.Count == 0) ? m_DefaultFocusArea.ToString() : m_StackedAreas.Last());
		Log("Enable areas " + string.Join(", ", m_AvailableAreas.Select((IControllerInputArea it) => it.Id)) + " (focused " + text + ")");
		for (int num = 0; num < m_AvailableAreas.Count; num++)
		{
			m_AvailableAreas[num].EnableGroup(text == m_AvailableAreas[num].Id);
		}
	}

	public void DisableControllerInputAreas()
	{
		Log("Disable areas " + string.Join(", ", m_AvailableAreas.Select((IControllerInputArea it) => it.Id)));
		m_IsEnabled = false;
		for (int num = 0; num < m_AvailableAreas.Count; num++)
		{
			m_AvailableAreas[num].DisableGroup();
		}
	}

	public void ResetFocusedArea()
	{
		if (m_FocusArea != null)
		{
			Log("Reset Focused Area " + m_FocusArea);
			m_FocusArea = null;
		}
	}

	public void SetDefaultFocusArea(EControllerInputAreaType area)
	{
		Log($"Set Default Area {area}");
		m_DefaultFocusArea = area;
	}

	public void RemoveStackedArea(string id)
	{
		if (m_StackedAreas.Remove(id))
		{
			Log("Removed stacked area " + id + " (stack: " + string.Join(", ", m_StackedAreas) + ")");
		}
	}
}
