using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SM.Gamepad;

public class HotkeyContainer : MonoBehaviour, IHotkeyContainer
{
	private const string _sessionIdPrefix = "session_";

	[SerializeField]
	private List<Hotkey> _hotkeys = new List<Hotkey>();

	private InputDisplayData _displayData;

	private IHotkeyActionInput _hotkeyActionInput;

	private IComparer<string> _hotkeyOrdering;

	private readonly Dictionary<object, Dictionary<string, Action>> _objectHotkeys = new Dictionary<object, Dictionary<string, Action>>();

	private int _sessionCounter;

	private Dictionary<string, Action> _uniqueHotkeysToDisplay = new Dictionary<string, Action>();

	[UsedImplicitly]
	private void OnDestroy()
	{
		DeInit();
	}

	public void Initialize(InputDisplayData displayData, IHotkeyActionInput hotkeyActionInput, HotkeyOrderConfig hotkeyOrderConfig = null)
	{
		_displayData = displayData;
		_hotkeyActionInput = hotkeyActionInput;
		_uniqueHotkeysToDisplay = new Dictionary<string, Action>();
		_hotkeyOrdering = ((hotkeyOrderConfig == null) ? null : hotkeyOrderConfig.GetComparer());
		UpdateHotkeys();
	}

	private void DeInit()
	{
		ClearHotkeys();
		_objectHotkeys.Clear();
	}

	private void UpdateHotkeys()
	{
		ClearHotkeys();
		for (int i = 0; i < _hotkeys.Count; i++)
		{
			_hotkeys[i].gameObject.SetActive(value: false);
		}
		_uniqueHotkeysToDisplay.Clear();
		foreach (KeyValuePair<object, Dictionary<string, Action>> objectHotkey in _objectHotkeys)
		{
			foreach (KeyValuePair<string, Action> item in objectHotkey.Value)
			{
				_uniqueHotkeysToDisplay.TryAdd(item.Key, item.Value);
			}
		}
		int num = 0;
		foreach (string modifiedUniqueKey in GetModifiedUniqueKeys(_uniqueHotkeysToDisplay.Keys))
		{
			if (num > _hotkeys.Count - 1)
			{
				break;
			}
			Hotkey hotkey = _hotkeys[num];
			hotkey.gameObject.SetActive(value: true);
			hotkey.ExpectedEvent = modifiedUniqueKey;
			hotkey.Initialize(_hotkeyActionInput, _displayData, _uniqueHotkeysToDisplay[modifiedUniqueKey]);
			hotkey.UpdateHotkeyDisplay();
			num++;
		}
	}

	private void ClearHotkeys()
	{
		for (int i = 0; i < _hotkeys.Count; i++)
		{
			_hotkeys[i].Deinitialize();
		}
		_uniqueHotkeysToDisplay.Clear();
	}

	private IEnumerable<string> GetModifiedUniqueKeys(IEnumerable<string> keys)
	{
		if (_hotkeyOrdering != null)
		{
			return keys.OrderBy((string s) => s, _hotkeyOrdering);
		}
		return keys;
	}

	public IHotkeySession GetSession(IHotkeySession session)
	{
		return new HotkeySession(this, session);
	}

	public IHotkeySession GetSession()
	{
		return new HotkeySession(this);
	}

	string IHotkeyContainer.RequestSessionId()
	{
		_sessionCounter++;
		return "session_" + _sessionCounter;
	}

	public void AddOrReplaceHotkeysForObject(string requesterObjectId, Dictionary<string, Action> hotkeysData)
	{
		_objectHotkeys[requesterObjectId] = hotkeysData;
		UpdateHotkeys();
	}

	public void RemoveHotkeysForObject(string requesterObjectId)
	{
		_objectHotkeys.Remove(requesterObjectId);
		UpdateHotkeys();
	}

	public bool TryGetHotkeyByExpectedEvent(string expectedEvent, out Hotkey hotkey)
	{
		hotkey = _hotkeys.First((Hotkey h) => h.ExpectedEvent == expectedEvent);
		return hotkey != null;
	}
}
