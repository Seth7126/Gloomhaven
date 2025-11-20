using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using JetBrains.Annotations;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIModeSelector<T> : MonoBehaviour
{
	[Serializable]
	protected class ModeConfig
	{
		public Toggle toggle;

		public List<UIOptionEntry> entries;

		public T Value;

		public void RefreshEnabled()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].Enable(toggle.isOn);
			}
		}
	}

	[Serializable]
	public class ModeSelectorEvent : UnityEvent<T>
	{
	}

	[SerializeField]
	protected List<ModeConfig> m_Modes;

	[SerializeField]
	private UiNavigationGroup _uiNavigationGroup;

	public ModeSelectorEvent OnModeChanged;

	protected ModeConfig selectedMode;

	protected virtual void Awake()
	{
		foreach (ModeConfig mode2 in m_Modes)
		{
			if (mode2.toggle.isOn)
			{
				selectedMode = mode2;
			}
			mode2.RefreshEnabled();
			ModeConfig mode1 = mode2;
			mode2.toggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				OnValueChanged(isOn, mode1);
			});
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		foreach (ModeConfig mode in m_Modes)
		{
			mode.toggle.onValueChanged.RemoveAllListeners();
		}
		OnModeChanged.RemoveAllListeners();
	}

	public void SelectNext()
	{
		if (selectedMode == null)
		{
			m_Modes.First().toggle.isOn = true;
			return;
		}
		int num = m_Modes.IndexOf(selectedMode);
		m_Modes[(num + 1) % m_Modes.Count].toggle.isOn = true;
	}

	public void SelectPrevious()
	{
		if (selectedMode == null)
		{
			m_Modes.First().toggle.isOn = true;
			return;
		}
		int num = m_Modes.IndexOf(selectedMode);
		m_Modes[(num == 0) ? (m_Modes.Count - 1) : (num - 1)].toggle.isOn = true;
	}

	private void OnValueChanged(bool value, ModeConfig mode)
	{
		mode.RefreshEnabled();
		if (value && selectedMode != mode)
		{
			selectedMode = mode;
			OnModeChanged.Invoke(GetSelectedMode());
		}
	}

	public virtual void SetMode(T mode)
	{
		SetMode(m_Modes.First(delegate(ModeConfig it)
		{
			ref T value = ref it.Value;
			object obj = mode;
			return value.Equals(obj);
		}));
	}

	protected void SetMode(ModeConfig mode)
	{
		if (InputManager.GamePadInUse)
		{
			ActivateToggleByModeGamepad(mode);
		}
		else
		{
			ActivateToggleByMode(mode);
		}
	}

	public T GetSelectedMode()
	{
		return selectedMode.Value;
	}

	public bool HasSelectedMode()
	{
		return selectedMode != null;
	}

	private void ActivateToggleByMode(ModeConfig mode)
	{
		selectedMode = mode;
		if (mode.toggle.isOn)
		{
			mode.toggle.onValueChanged.Invoke(arg0: true);
			return;
		}
		mode.toggle.isOn = true;
		foreach (ModeConfig mode2 in m_Modes)
		{
			if (mode != mode2)
			{
				mode2.toggle.isOn = false;
			}
		}
	}

	private void ActivateToggleByModeGamepad(ModeConfig mode)
	{
		selectedMode = mode;
		if (mode.toggle.isOn)
		{
			mode.toggle.onValueChanged.Invoke(arg0: true);
		}
		else
		{
			mode.toggle.isOn = true;
			foreach (ModeConfig mode2 in m_Modes)
			{
				if (mode != mode2)
				{
					mode2.toggle.isOn = false;
				}
			}
		}
		if (_uiNavigationGroup != null)
		{
			UINavigationSelectable component = mode.toggle.GetComponent<UINavigationSelectable>();
			_uiNavigationGroup.SetDefaultElementToSelect(component);
		}
	}
}
