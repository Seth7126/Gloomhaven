using System.Collections;
using InControl;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInputScroll : ControllerInputElement
{
	[SerializeField]
	private ScrollRect m_Scroll;

	[SerializeField]
	private float m_ScrollSpeed = 0.3f;

	[SerializeField]
	private bool m_IsAbsoluteSpeed;

	[SerializeField]
	private UIControllerKeyTip m_ControllerKeyTip;

	[SerializeField]
	private Hotkey m_hotkey;

	[SerializeField]
	private bool useMoveAltControl;

	[SerializeField]
	private Image m_outline;

	[SerializeField]
	private GameObject m_outlineVFX;

	[SerializeField]
	private bool _disableWhenInEscMenu;

	private static int _globalDisableRequestsCount;

	private static bool _globalEnabled => _globalDisableRequestsCount == 0;

	public static void SetGlobalEnabled(bool enabled)
	{
		_globalDisableRequestsCount += ((!enabled) ? 1 : (-1));
	}

	private void Awake()
	{
		if (m_Scroll == null)
		{
			m_Scroll = GetComponent<ScrollRect>();
		}
		if ((bool)m_hotkey)
		{
			m_hotkey.Initialize(Singleton<UINavigation>.Instance.Input, null, null, activate: false);
		}
		if (InputManager.GamePadInUse)
		{
			if (m_outline != null)
			{
				m_outline.gameObject.SetActive(value: false);
			}
			if (m_outlineVFX != null)
			{
				m_outlineVFX.SetActive(value: false);
			}
		}
	}

	protected void OnDestroy()
	{
		if (m_hotkey != null)
		{
			m_hotkey.Deinitialize();
		}
	}

	private void OnValidate()
	{
		if (m_Scroll == null)
		{
			m_Scroll = GetComponent<ScrollRect>();
		}
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		if (InputManager.GamePadInUse)
		{
			if (m_outline != null)
			{
				m_outline.gameObject.SetActive(value: true);
			}
			if (m_outlineVFX != null)
			{
				m_outlineVFX.SetActive(value: true);
			}
		}
		if ((bool)m_hotkey)
		{
			m_hotkey.DisplayHotkey(active: true);
		}
		else if (m_ControllerKeyTip != null)
		{
			m_ControllerKeyTip.SetText(useMoveAltControl ? Singleton<InputManager>.Instance.PlayerControl.UIUpAlt : Singleton<InputManager>.Instance.PlayerControl.UIUp);
			m_ControllerKeyTip.Show();
		}
		StopAllCoroutines();
		if (Singleton<InputManager>.Instance.PlayerControl != null)
		{
			StartCoroutine(Scroll());
		}
	}

	private IEnumerator Scroll()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return null;
			if (!_globalEnabled || (_disableWhenInEscMenu && Singleton<ESCMenu>.Instance.IsOpen) || !m_Scroll.verticalScrollbar.IsInteractable())
			{
				continue;
			}
			float num = (useMoveAltControl ? Singleton<InputManager>.Instance.PlayerControl.UIMoveAlt : Singleton<InputManager>.Instance.PlayerControl.UIMove).LastValue.y;
			if (!Mathf.Approximately(num, float.Epsilon))
			{
				if (m_IsAbsoluteSpeed)
				{
					num /= m_Scroll.content.rect.height - m_Scroll.viewport.rect.height;
				}
				m_Scroll.verticalNormalizedPosition = Mathf.Clamp01(m_Scroll.verticalNormalizedPosition + num * m_ScrollSpeed * InControl.InputManager.Sensitivity);
			}
		}
	}

	protected override void OnDisabledControllerControl()
	{
		StopAllCoroutines();
		base.OnDisabledControllerControl();
		if (InputManager.GamePadInUse)
		{
			if (m_outline != null)
			{
				m_outline.gameObject.SetActive(value: false);
			}
			if (m_outlineVFX != null)
			{
				m_outlineVFX.SetActive(value: false);
			}
		}
		if ((bool)m_hotkey)
		{
			m_hotkey.DisplayHotkey(active: false);
		}
		else
		{
			m_ControllerKeyTip?.Hide();
		}
	}
}
