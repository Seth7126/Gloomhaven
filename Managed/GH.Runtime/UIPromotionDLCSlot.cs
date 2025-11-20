using System;
using SM.Gamepad;
using SM.Utils;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPromotionDLCSlot : MonoBehaviour
{
	[Serializable]
	public class DLCEvent : UnityEvent<DLCRegistry.EDLCKey>
	{
	}

	[SerializeField]
	private Image promotionImage;

	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Hotkey _hotkey;

	private DLCRegistry.EDLCKey dlc;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private KeyAction _currentKeyAction;

	public DLCEvent OnClick;

	private void Awake()
	{
		if (button != null)
		{
			button.onClick.AddListener(delegate
			{
				OnClick.Invoke(dlc);
			});
		}
		if (_hotkey != null)
		{
			_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker();
			_currentKeyAction = KeyAction.UI_DLC_PROMOTION;
			if (PlatformLayer.Instance.IsDelayedInit)
			{
				_hotkey.ExpectedEvent = "DLC Promotion Xbox";
				_currentKeyAction = KeyAction.UI_DLC_PROMOTION_XBOX;
			}
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(_currentKeyAction, OnPromotionHotkey).AddBlocker(_simpleKeyActionHandlerBlocker));
		}
	}

	private void OnEnable()
	{
		_simpleKeyActionHandlerBlocker?.SetBlock(value: false);
	}

	private void OnDisable()
	{
		_simpleKeyActionHandlerBlocker?.SetBlock(value: true);
	}

	private void OnDestroy()
	{
		if (button != null)
		{
			button.onClick.RemoveAllListeners();
		}
		if (_hotkey != null)
		{
			_hotkey.Deinitialize();
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_currentKeyAction, OnPromotionHotkey);
		}
		OnClick.RemoveAllListeners();
	}

	public virtual void SetDLC(DLCRegistry.EDLCKey dlc, Sprite image)
	{
		this.dlc = dlc;
		if (title != null)
		{
			if (dlc != DLCRegistry.EDLCKey.DLC1)
			{
				title.SetTextKey(dlc.ToString());
			}
			else
			{
				title.SetTextKey("EMPTY_STRING");
			}
		}
		if (promotionImage != null)
		{
			promotionImage.sprite = image;
		}
		else
		{
			LogUtils.LogError("[UIPromotionalDLCSlot.cs] Line 33: promotionImage is poorly set! (null ref)");
		}
	}

	private void OnPromotionHotkey()
	{
		OnClick.Invoke(dlc);
	}
}
