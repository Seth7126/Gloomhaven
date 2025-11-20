using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerLockOverlay : Singleton<UIMultiplayerLockOverlay>
{
	private struct LockRequestData
	{
		public readonly bool Blur;

		public readonly string TextLockKey;

		public LockRequestData(string textLockKey, bool blur)
		{
			TextLockKey = textLockKey;
			Blur = blur;
		}
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private Image blurMask;

	private const string TEXT_ID = "LOCK_OVERLAY";

	private Dictionary<object, LockRequestData> requests = new Dictionary<object, LockRequestData>();

	public void ShowLock(object request, string textLockKey = "GUI_MULTIPLAYER_GAME_PAUSED", bool blur = true)
	{
		requests[request] = new LockRequestData(textLockKey, blur);
		blurMask.enabled = requests.Any((KeyValuePair<object, LockRequestData> it) => it.Value.Blur);
		window.Show();
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		if (!textLockKey.IsNullOrEmpty())
		{
			Singleton<HelpBox>.Instance.Show((from it in requests
				select it.Value.TextLockKey into it
				where it.IsNOTNullOrEmpty()
				select it).Distinct().ToList(), "GUI_MULTIPLAYER", HelpBox.FormatTarget.NONE, "LOCK_OVERLAY");
		}
	}

	public void HideLock(object request)
	{
		if (!requests.ContainsKey(request))
		{
			return;
		}
		LockRequestData lockRequestData = requests[request];
		requests.Remove(request);
		blurMask.enabled = requests.Any((KeyValuePair<object, LockRequestData> it) => it.Value.Blur);
		if (requests.Count == 0)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Singleton<HelpBox>.Instance.Hide("LOCK_OVERLAY");
			window.Hide();
		}
		else if (lockRequestData.TextLockKey.IsNOTNullOrEmpty())
		{
			List<string> list = (from it in requests
				select it.Value.TextLockKey into it
				where it.IsNOTNullOrEmpty()
				select it).Distinct().ToList();
			if (list.Count > 0)
			{
				Singleton<HelpBox>.Instance.Show(list, "GUI_MULTIPLAYER", HelpBox.FormatTarget.NONE, "LOCK_OVERLAY");
			}
			else
			{
				Singleton<HelpBox>.Instance.Hide("LOCK_OVERLAY");
			}
		}
	}

	public void ResetLock()
	{
		requests.Clear();
		Singleton<HelpBox>.Instance.Hide("LOCK_OVERLAY");
		window.Hide();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}

	public void ToggleLock(object request, bool show)
	{
		if (show)
		{
			ShowLock(request);
		}
		else
		{
			HideLock(request);
		}
	}
}
