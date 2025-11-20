using SM.Gamepad;
using UnityEngine;
using UnityEngine.Events;

namespace Script.GUI;

public class MonoHotkeySession : MonoBehaviour
{
	[SerializeField]
	private HotkeyContainer _hotkeyContainer;

	[SerializeField]
	private string[] _hotkeys;

	[SerializeField]
	private UnityEvent OnShow;

	[SerializeField]
	private UnityEvent OnHide;

	private IHotkeySession _hotkeySession;

	public void Show()
	{
		if (_hotkeySession == null)
		{
			_hotkeySession = _hotkeyContainer.GetSessionOrEmpty().AddOrReplaceHotkeys(_hotkeys);
		}
		OnShow?.Invoke();
	}

	public void Hide()
	{
		_hotkeySession?.Dispose();
		_hotkeySession = null;
		OnHide?.Invoke();
	}

	public bool TryGetHotkeyByExpectedEvent(string expectedEvent, out Hotkey hotkey)
	{
		return _hotkeyContainer.TryGetHotkeyByExpectedEvent(expectedEvent, out hotkey);
	}
}
