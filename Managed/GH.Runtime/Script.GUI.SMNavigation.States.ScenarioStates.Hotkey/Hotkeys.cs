using System.Linq;
using JetBrains.Annotations;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

public class Hotkeys : HotkeyContainer
{
	public enum HotkeyPositionState
	{
		Scenario,
		WorldMap,
		Mercenaries,
		HowToPlay
	}

	public struct HotkeyPosition
	{
		public HotkeyPositionState State;

		public Vector2 Position;

		public int SortOrder;
	}

	private const int DefaultSortOrder = 1100;

	private const int HowToPlaySortOrder = 1201;

	[SerializeField]
	private InputDisplayData _inputDisplayData;

	[SerializeField]
	private HotkeyOrderConfig _hotkeyOrderConfig;

	[SerializeField]
	private Canvas _canvas;

	private readonly HotkeyPosition[] _hotkeyPositions = new HotkeyPosition[4]
	{
		new HotkeyPosition
		{
			State = HotkeyPositionState.Scenario,
			Position = new Vector2(0f, 0f),
			SortOrder = 1100
		},
		new HotkeyPosition
		{
			State = HotkeyPositionState.WorldMap,
			Position = new Vector2(300f, 0f),
			SortOrder = 1100
		},
		new HotkeyPosition
		{
			State = HotkeyPositionState.Mercenaries,
			Position = new Vector2(710f, 34f),
			SortOrder = 1100
		},
		new HotkeyPosition
		{
			State = HotkeyPositionState.HowToPlay,
			Position = new Vector2(0f, 0f),
			SortOrder = 1201
		}
	};

	private RectTransform _rectTransform;

	private HotkeyPositionState? _currentState;

	private HotkeyPositionState? _prevState;

	public static Hotkeys Instance { get; private set; }

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		_rectTransform = base.transform as RectTransform;
		SM.Gamepad.Hotkey.SetLocalizationClient(new I2LocalizationClient());
		Initialize(_inputDisplayData, Singleton<UINavigation>.Instance.Input, _hotkeyOrderConfig);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void SetState(HotkeyPositionState state)
	{
		_prevState = _currentState;
		_currentState = state;
		HotkeyPosition hotkeyPosition = _hotkeyPositions.Single((HotkeyPosition position) => position.State == state);
		_rectTransform.anchoredPosition = hotkeyPosition.Position;
		_canvas.sortingOrder = hotkeyPosition.SortOrder;
	}

	public void SetPreviousState()
	{
		if (_prevState.HasValue)
		{
			SetState(_prevState.Value);
		}
	}

	public void SetActiveState(bool state)
	{
		base.gameObject.SetActive(state);
	}
}
