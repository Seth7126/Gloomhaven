#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Code.State;
using JetBrains.Annotations;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInputPointer : ControllerInputElement
{
	[Serializable]
	private class CursorIcon
	{
		public ECursorType type;

		public Sprite icon;
	}

	[SerializeField]
	private Image m_Pointer;

	[SerializeField]
	private List<CursorIcon> m_Cursors;

	private readonly HashSet<Type> _allowedStateTypes = new HashSet<Type>
	{
		typeof(RoundStartScenarioState),
		typeof(HexMovementOnSelectActionState),
		typeof(UseActionScenarioState),
		typeof(SelectTargetState),
		typeof(DamageScenarioState),
		typeof(AbilityActionsScenarioState)
	};

	private static ECursorType m_CursorType;

	private static ControllerInputPointer m_Instance { get; set; }

	public static bool IsShown { get; private set; }

	public static ECursorType CursorType
	{
		get
		{
			return m_CursorType;
		}
		set
		{
			if (m_CursorType != value)
			{
				if (m_Instance != null && m_Instance.gameObject.activeSelf)
				{
					Debug.LogController($"[Cursor] Changed cursor type to {value} from {m_CursorType}");
				}
				m_CursorType = value;
				if (InputManager.GamePadInUse)
				{
					SetCursorType(value);
				}
			}
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		m_Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		m_Instance = null;
		if (Singleton<UINavigation>.Instance != null)
		{
			Singleton<UINavigation>.Instance.StateMachine.EventStateChanged -= StateMachineOnEventStateChanged;
		}
	}

	private void Start()
	{
		Singleton<UINavigation>.Instance.StateMachine.EventStateChanged += StateMachineOnEventStateChanged;
	}

	private void StateMachineOnEventStateChanged(IState obj)
	{
		if (obj != null)
		{
			if (_allowedStateTypes.Contains(obj.GetType()))
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	private void Update()
	{
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		base.transform.position = UIManager.Instance.UICamera.ScreenToWorldPoint(vector);
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		m_Pointer.enabled = true;
		SetCursorType(CursorType);
	}

	public static void SetCursorType(ECursorType type)
	{
		if (!(m_Instance == null) && m_Instance.gameObject.activeSelf)
		{
			m_Instance.m_Pointer.sprite = m_Instance.m_Cursors.First((CursorIcon it) => type == it.type).icon;
		}
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		m_Pointer.enabled = false;
	}

	public static void Show()
	{
		m_Instance.gameObject.SetActive(value: true);
		IsShown = true;
		SetCursorType(CursorType);
	}

	public static void Hide()
	{
		if (!(m_Instance == null))
		{
			m_Instance.gameObject.SetActive(value: false);
			IsShown = false;
		}
	}
}
