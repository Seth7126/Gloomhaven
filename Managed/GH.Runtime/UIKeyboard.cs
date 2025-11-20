using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Script.GUI.Configuration;
using Script.GUI.Controller.Keyboard;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.Events;

public class UIKeyboard : LocalizedListener, IShowActivity
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode>
	{
	}

	[SerializeField]
	private List<UIKeyboardKey> keysPool;

	[SerializeField]
	private FlowLayoutGroup container;

	[SerializeField]
	private List<UIKeyboardKey> fixedKeys;

	[SerializeField]
	private KeyboardConfig defaultConfig;

	[SerializeField]
	private List<KeyboardConfig> languageConfigs;

	public KeyCodeEvent OnSelectedKeyCode;

	private KeyboardConfig loadedConfig;

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	public bool IsActive => base.gameObject.activeSelf;

	private void Awake()
	{
		foreach (UIKeyboardKey fixedKey in fixedKeys)
		{
			fixedKey.OnClicked.AddListener(OnClickedKey);
		}
		foreach (UIKeyboardKey item in keysPool)
		{
			item.OnClicked.AddListener(OnClickedKey);
		}
		OnLanguageChanged();
	}

	protected void OnDestroy()
	{
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
	}

	protected override void OnLanguageChanged()
	{
		Setup(languageConfigs.FirstOrDefault((KeyboardConfig it) => it.Language == LocalizationManager.CurrentLanguage.ToLower()) ?? defaultConfig);
	}

	private void Setup(KeyboardConfig config)
	{
		if (loadedConfig == config)
		{
			return;
		}
		loadedConfig = config;
		container.columnsCount = config.Rows.Select((KeyboardRow it) => it.Keys.Count).ToList();
		HelperTools.NormalizePool(ref keysPool, keysPool[0].gameObject, container.transform, config.Rows.Sum((KeyboardRow it) => it.Keys.Count), delegate(UIKeyboardKey slot)
		{
			slot.OnClicked.AddListener(OnClickedKey);
		});
		int num = 0;
		for (int num2 = 0; num2 < config.Rows.Count; num2++)
		{
			for (int num3 = 0; num3 < config.Rows[num2].Keys.Count; num3++)
			{
				KeyCode keyCode = config.Rows[num2].Keys[num3];
				keysPool[num].name = keyCode.ToString();
				keysPool[num].SetKeyCode(keyCode);
				num++;
			}
		}
		for (int num4 = 0; num4 < fixedKeys.Count; num4++)
		{
			fixedKeys[num4].transform.SetAsLastSibling();
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			OnShow?.Invoke();
			OnActivityChanged?.Invoke(obj: true);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		OnHide?.Invoke();
		OnActivityChanged?.Invoke(obj: false);
	}

	private void OnClickedKey(KeyCode keyCode)
	{
		OnSelectedKeyCode.Invoke(keyCode);
	}
}
