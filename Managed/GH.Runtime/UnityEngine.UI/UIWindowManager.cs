#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using SM.Utils;

namespace UnityEngine.UI;

public class UIWindowManager : Singleton<UIWindowManager>
{
	private List<IEscapable> escapableListeners = new List<IEscapable>();

	private HashSet<UIWindowID> extraSkipHideWindows = new HashSet<UIWindowID>();

	private HashSet<UIWindowID> extraSkipShowWindows = new HashSet<UIWindowID>();

	private List<IEscapable> frameEscapables;

	protected override void Awake()
	{
		base.Awake();
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, Escape);
	}

	protected override void OnDestroy()
	{
		if (Singleton<UIWindowManager>.Instance.frameEscapables != null)
		{
			Singleton<UIWindowManager>.Instance.frameEscapables.Clear();
		}
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, Escape);
		escapableListeners.Clear();
		base.OnDestroy();
	}

	public void HideOrShowWindows(bool onlyHide = false, bool forceHideAll = false, bool popUpsOnly = false)
	{
		HashSet<UIWindow> windows = UIWindow.GetWindows();
		HashSet<UIWindow> skip = new HashSet<UIWindow>();
		if (!forceHideAll)
		{
			foreach (UIWindow item in windows.Where((UIWindow uIWindow2) => uIWindow2.IsOpen && (uIWindow2.escapeKeyAction == UIWindow.EscapeKeyAction.Skip || extraSkipHideWindows.Contains(uIWindow2.ID))))
			{
				skip.Add(item);
				foreach (UIWindowID it in item.escapeActionAdditionalWindows)
				{
					UIWindow uIWindow = windows.FirstOrDefault((UIWindow w) => w.ID == it);
					if (uIWindow != null)
					{
						skip.Add(uIWindow);
					}
				}
			}
		}
		foreach (UIWindow item2 in from uIWindow2 in windows
			where !skip.Contains(uIWindow2)
			orderby (uIWindow2.escapeKeyAction != UIWindow.EscapeKeyAction.HideOnlyThis) ? 1 : 0
			select uIWindow2)
		{
			if ((!popUpsOnly || item2.IsPopUp) && item2.escapeKeyAction != UIWindow.EscapeKeyAction.None && item2.IsOpen && (forceHideAll || item2.escapeKeyAction == UIWindow.EscapeKeyAction.Hide || item2.escapeKeyAction == UIWindow.EscapeKeyAction.Toggle || item2.escapeKeyAction == UIWindow.EscapeKeyAction.HideOnlyThis || (item2.escapeKeyAction == UIWindow.EscapeKeyAction.HideIfFocused && item2.IsFocused)))
			{
				bool num = !forceHideAll && item2.escapeKeyAction == UIWindow.EscapeKeyAction.HideOnlyThis;
				item2.Hide();
				onlyHide = true;
				if (num)
				{
					break;
				}
			}
		}
		if (onlyHide)
		{
			return;
		}
		foreach (UIWindow item3 in windows.Where((UIWindow uIWindow2) => !extraSkipShowWindows.Contains(uIWindow2.ID)))
		{
			if ((!popUpsOnly || item3.IsPopUp) && !item3.IsOpen && item3.escapeKeyAction == UIWindow.EscapeKeyAction.Toggle)
			{
				item3.Show();
			}
		}
	}

	public void Escape()
	{
		bool isGameNotSaving = !SaveData.Instance.IsSavingThreadActive;
		frameEscapables = escapableListeners.Where((IEscapable x) => isGameNotSaving || x.IsAllowedToEscapeDuringSave).ToList();
		if (frameEscapables.Count <= 0)
		{
			return;
		}
		List<IEscapable> list = escapableListeners.ToList();
		list.Reverse();
		list.Sort((IEscapable x, IEscapable y) => x.Order().CompareTo(y.Order()));
		IEscapable escapable = null;
		foreach (IEscapable item in list)
		{
			global::Debug.LogGUI($"Escape {item}");
			if (item.Escape())
			{
				global::Debug.LogGUI($"Escaped {item} and stopped");
				escapable = item;
				break;
			}
		}
		if (escapable != null && frameEscapables.Contains(escapable))
		{
			IEscapable escapable2 = list.FirstOrDefault((IEscapable it) => it is UIWindow uIWindow && uIWindow.escapeKeyAction == UIWindow.EscapeKeyAction.Toggle);
			if (escapable2 != null && escapable2 != escapable)
			{
				global::Debug.LogGUI($"Escape {escapable2}");
				escapable2.Escape();
			}
		}
		frameEscapables.Clear();
	}

	public void ForceHideWindows(bool onlyHide = false)
	{
		LogUtils.Log($"HIDE: {onlyHide}");
		foreach (UIWindow window in UIWindow.GetWindows())
		{
			if (window.escapeKeyAction != UIWindow.EscapeKeyAction.None && window.IsOpen && (window.escapeKeyAction == UIWindow.EscapeKeyAction.Hide || window.escapeKeyAction == UIWindow.EscapeKeyAction.Toggle || window.escapeKeyAction == UIWindow.EscapeKeyAction.HideOnlyThis || (window.escapeKeyAction == UIWindow.EscapeKeyAction.HideIfFocused && window.IsFocused)))
			{
				window.Hide();
				onlyHide = true;
				if (window.escapeKeyAction == UIWindow.EscapeKeyAction.HideOnlyThis)
				{
					break;
				}
			}
		}
	}

	public void AddSkipHideWindows(params UIWindowID[] windowIds)
	{
		foreach (UIWindowID item in windowIds)
		{
			extraSkipHideWindows.Add(item);
		}
	}

	public void RemoveSkipHideWindows(params UIWindowID[] windowIds)
	{
		foreach (UIWindowID item in windowIds)
		{
			extraSkipHideWindows.Remove(item);
		}
	}

	public void AddSkipShowWindows(params UIWindowID[] windowIds)
	{
		foreach (UIWindowID item in windowIds)
		{
			extraSkipShowWindows.Add(item);
		}
	}

	public void RemoveSkipShowWindows(params UIWindowID[] windowIds)
	{
		foreach (UIWindowID item in windowIds)
		{
			extraSkipShowWindows.Remove(item);
		}
	}

	public static void RegisterEscapable(IEscapable element)
	{
		if (!(Singleton<UIWindowManager>.Instance == null) && !CoreApplication.IsQuitting)
		{
			UnregisterEscapable(element);
			Singleton<UIWindowManager>.Instance.escapableListeners.Add(element);
			global::Debug.LogGUI("Added escapable " + element?.ToString() + " (" + string.Join(", ", Singleton<UIWindowManager>.Instance.escapableListeners) + ")");
		}
	}

	public static void UnregisterEscapable(IEscapable element)
	{
		if (!(Singleton<UIWindowManager>.Instance == null) && !CoreApplication.IsQuitting)
		{
			if (Singleton<UIWindowManager>.Instance.escapableListeners.Remove(element))
			{
				global::Debug.LogGUI("Removed escapable " + element?.ToString() + " (" + string.Join(", ", Singleton<UIWindowManager>.Instance.escapableListeners) + ")");
			}
			Singleton<UIWindowManager>.Instance.frameEscapables?.Remove(element);
		}
	}
}
