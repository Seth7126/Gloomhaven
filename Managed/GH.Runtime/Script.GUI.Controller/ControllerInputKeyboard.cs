#define ENABLE_LOGS
using System;
using Platforms;
using SM.Utils;
using TMPro;
using UnityEngine;

namespace Script.GUI.Controller;

public class ControllerInputKeyboard : ControllerInputElement
{
	[SerializeField]
	private TMP_InputField m_KeyboardInputField;

	[SerializeField]
	private UIKeyboard keyboard;

	public event Action OnPlatformSpecificCallback;

	private void Awake()
	{
		keyboard.OnSelectedKeyCode.AddListener(ProcessKeyCode);
	}

	private void OnDestroy()
	{
		keyboard.OnSelectedKeyCode.RemoveListener(ProcessKeyCode);
	}

	private void ProcessKeyCode(KeyCode keyCode)
	{
		if (keyCode == KeyCode.Delete)
		{
			Undo();
			return;
		}
		char[] array = KeyCodeConverter.ConvertToValue(keyCode).ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			m_KeyboardInputField.ProcessEvent(new Event
			{
				character = array[i]
			});
		}
		m_KeyboardInputField.ForceLabelUpdate();
	}

	private void Undo()
	{
		string text = m_KeyboardInputField.text;
		if (text.Length != 0)
		{
			m_KeyboardInputField.text = text.Substring(0, text.Length - 1);
		}
	}

	public void ActivateSystemKeyboard()
	{
		LogUtils.Log("[ControllerInputKeyboard] System OnScreen Keyboard is about to be shown for " + m_KeyboardInputField.name + " input field.");
		IPlatform platform = global::PlatformLayer.Platform;
		string platformUniqueUserID = platform.UserManagement.GetCurrentUser().GetPlatformUniqueUserID();
		platform.PlatformInput.ShowOnScreenKeyboard(platformUniqueUserID, m_KeyboardInputField.text, string.Empty, string.Empty, (uint)m_KeyboardInputField.characterLimit, PlatformSpecificCallbackDelegate);
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		keyboard.Show();
	}

	private void PlatformSpecificCallbackDelegate(bool aborted, string text)
	{
		if (!aborted)
		{
			m_KeyboardInputField.text = text;
		}
		OnDisabledControllerControl();
		this.OnPlatformSpecificCallback?.Invoke();
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		keyboard.Hide();
	}
}
