using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatLogFilterSetting : MonoBehaviour
{
	[SerializeField]
	private CombatLogFilter filter;

	[SerializeField]
	private ButtonSwitch toggle;

	private Action<CombatLogFilter, bool> onValueChanged;

	private void Awake()
	{
		toggle.OnValueChanged.AddListener(delegate(bool enable)
		{
			onValueChanged?.Invoke(filter, enable);
		});
	}

	private void OnDestroy()
	{
		toggle.OnValueChanged.RemoveAllListeners();
	}

	public void Init(Action<CombatLogFilter, bool> onValueChanged)
	{
		this.onValueChanged = onValueChanged;
		toggle.SetValue(!SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter));
	}

	public void EnableNavigation(bool select = false)
	{
		toggle.SetNavigation(Navigation.Mode.Vertical);
		if (select)
		{
			EventSystem.current.SetSelectedGameObject(toggle.gameObject);
		}
	}

	public void DisableNavigation()
	{
		toggle.DisableNavigation();
	}
}
