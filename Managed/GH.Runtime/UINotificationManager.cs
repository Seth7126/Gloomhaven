#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINotificationManager : Singleton<UINotificationManager>
{
	public class NotificationData
	{
		public string titleLoc;

		public string message;

		public GameObject content;

		public Sprite icon;

		public NotificationDataButton dataButton1;

		public NotificationDataButton dataButton2;
	}

	public class NotificationDataButton
	{
		public string buttonTextLoc;

		public Action clickCallback;

		public bool hideOnClick = true;

		public NotificationDataButton(Action clickCallback, string buttonTextLoc)
		{
			this.clickCallback = clickCallback;
			this.buttonTextLoc = buttonTextLoc;
		}
	}

	private class Notification : INotificationControl
	{
		public delegate void NotificationEventHandler(Notification notification);

		private bool isTemporary;

		public readonly string ID;

		private readonly UINotification notification;

		public readonly string Tag;

		public bool IsTemporary => isTemporary;

		public int ButtonCount => notification.EnabledButtons;

		public event NotificationEventHandler OnHide;

		private event BasicEventHandler onHidden;

		public Notification(string id, UINotification uiNotification, bool isTemporary, string tag = null)
		{
			this.isTemporary = isTemporary;
			ID = id;
			notification = uiNotification;
			Tag = tag;
			notification.OnHidden.AddListener(OnHidden);
		}

		public void Hide(bool instant = false)
		{
			notification.Hide(instant);
			this.OnHide?.Invoke(this);
		}

		private void OnHidden()
		{
			notification.OnHidden.RemoveListener(OnHidden);
			this.onHidden?.Invoke();
		}

		public void RegisterToOnHidden(Action callback)
		{
			onHidden += delegate
			{
				callback();
			};
		}

		public void EnableControllerClick()
		{
			notification.EnableControllerClick();
		}

		public void DisableControllerClick()
		{
			notification.DisableControllerClick();
		}

		public void EnableNavigation(bool select = false)
		{
			notification.EnableNavigation(select);
		}

		public void DisableNavigation()
		{
			notification.DisableNavigation();
		}
	}

	public interface INotificationControl
	{
		void Hide(bool instant = false);

		void RegisterToOnHidden(Action callback);
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private UIControllerKeyTip controllerFocusTip;

	[SerializeField]
	private UINotification notificationPrefab;

	[SerializeField]
	private RectTransform notificationContainer;

	[SerializeField]
	private List<UINotification> poolNotifications = new List<UINotification>();

	[SerializeField]
	[UsedImplicitly]
	private Canvas _canvas;

	private Dictionary<string, Notification> idNotifications = new Dictionary<string, Notification>();

	private List<Notification> activeNotifications = new List<Notification>();

	private HashSet<Component> hideRequests = new HashSet<Component>();

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < poolNotifications.Count; i++)
		{
			UINotification notification = poolNotifications[i];
			notification.OnHidden.AddListener(delegate
			{
				OnHiddenNotification(notification);
			});
		}
		controllerArea.OnEnabledArea.AddListener(OnControllerAreaEnabled);
		controllerArea.OnDisabledArea.AddListener(OnControllerAreaDisabled);
		controllerArea.OnFocused.AddListener(delegate
		{
			EnableNavigation(forceSelect: true);
		});
		controllerArea.OnUnfocused.AddListener(DisableNavigation);
		UpdateCanvasVisibility();
	}

	private void OnControllerAreaEnabled()
	{
		int num = 0;
		Notification notification = null;
		for (int i = 0; i < activeNotifications.Count; i++)
		{
			num += activeNotifications[i].ButtonCount;
			if (num > 0)
			{
				notification = activeNotifications[i];
			}
			activeNotifications[i].DisableControllerClick();
		}
		if (num > 1)
		{
			controllerFocusTip.Show();
			controllerArea.SetKeyActionAutofocus(KeyAction.CONFIRM_NOTIFICATION);
			if (controllerArea.IsFocused)
			{
				EnableNavigation();
			}
			return;
		}
		controllerArea.SetKeyActionAutofocus(KeyAction.None);
		controllerFocusTip.Hide();
		notification?.EnableControllerClick();
		if (controllerArea.IsFocused)
		{
			controllerArea.Unfocus();
		}
	}

	private void OnControllerAreaDisabled()
	{
		controllerFocusTip.Hide();
		for (int i = 0; i < activeNotifications.Count; i++)
		{
			activeNotifications[i].DisableControllerClick();
			activeNotifications[i].DisableNavigation();
		}
	}

	private void EnableNavigation(bool forceSelect = false)
	{
		for (int i = 0; i < activeNotifications.Count; i++)
		{
			activeNotifications[i].EnableNavigation(i == 0 && (forceSelect || EventSystem.current.currentSelectedGameObject == null));
		}
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < activeNotifications.Count; i++)
		{
			activeNotifications[i].DisableNavigation();
		}
	}

	public void Hide(Component request)
	{
		controllerArea.enabled = false;
		hideRequests.Add(request);
		window.Hide();
		UpdateCanvasVisibility();
	}

	public void Show(Component request)
	{
		hideRequests.Remove(request);
		if (hideRequests.Count == 0)
		{
			UpdateCanvasVisibility();
			window.Show();
			if (activeNotifications.Any((Notification it) => it.ButtonCount > 0))
			{
				controllerArea.enabled = true;
			}
		}
	}

	public INotificationControl ShowNotification(string titleLoc, string text, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			message = text
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(string titleLoc, GameObject content, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			content = content
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(string titleLoc, string text, Action buttonCallback, string buttonTextLoc, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			message = text,
			dataButton1 = null
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(NotificationData data, float durationDisplay, string id = null, string notificationTag = null)
	{
		if (string.IsNullOrEmpty(id) || !idNotifications.ContainsKey(id))
		{
			UINotification notification = GetNotification();
			Notification notification2 = new Notification(id, notification, durationDisplay > 0f, notificationTag);
			if (data.dataButton1 != null && data.dataButton1.hideOnClick)
			{
				Action buttonClickCallback = data.dataButton1.clickCallback;
				data.dataButton1.clickCallback = delegate
				{
					notification2.Hide();
					buttonClickCallback?.Invoke();
				};
			}
			if (data.dataButton2 != null && data.dataButton2.hideOnClick)
			{
				Action buttonClickCallback2 = data.dataButton2.clickCallback;
				data.dataButton2.clickCallback = delegate
				{
					notification2.Hide();
					buttonClickCallback2?.Invoke();
				};
			}
			notification.Show(data, id);
			if (durationDisplay > 0f)
			{
				Coroutine waitCoroutine = StartCoroutine(WaitNotification(delegate
				{
					notification2.Hide();
				}, durationDisplay));
				notification2.OnHide += delegate
				{
					StopCoroutine(waitCoroutine);
				};
			}
			notification2.OnHide += OnHideNotification;
			if (!string.IsNullOrEmpty(id))
			{
				idNotifications[id] = notification2;
			}
			activeNotifications.Add(notification2);
			UpdateCanvasVisibility();
			if (data.dataButton1 != null || data.dataButton2 != null)
			{
				controllerArea.enabled = true;
				if (controllerArea.IsEnabled && window.IsOpen)
				{
					OnControllerAreaEnabled();
				}
			}
			return notification2;
		}
		Debug.LogWarningGUI("Notification already exists " + id);
		return idNotifications[id];
	}

	private void OnHideNotification(Notification notification)
	{
		if (!string.IsNullOrEmpty(notification.ID))
		{
			idNotifications.Remove(notification.ID);
		}
		activeNotifications.Remove(notification);
		UpdateCanvasVisibility();
		if (notification.ButtonCount > 0)
		{
			if (controllerArea.enabled && (activeNotifications.Count == 0 || activeNotifications.All((Notification it) => it.ButtonCount == 0)))
			{
				controllerArea.enabled = false;
			}
			else if (controllerArea.IsEnabled && window.IsOpen)
			{
				OnControllerAreaEnabled();
			}
		}
	}

	public INotificationControl ShowPermanentNotification(string id, string titleLoc, string text, string notificationTag = null)
	{
		return ShowPermanentNotification(id, titleLoc, text, null, null, notificationTag);
	}

	public INotificationControl ShowPermanentNotification(string id, string titleLoc, string text, Action buttonCallback, string buttonTextLoc, string notificationTag = null)
	{
		return ShowPermanentNotification(id, new NotificationData
		{
			titleLoc = titleLoc,
			message = text,
			dataButton1 = null
		}, notificationTag);
	}

	public INotificationControl ShowPermanentNotification(string id, NotificationData data, string notificationTag = null)
	{
		return ShowNotification(data, -1f, id, notificationTag);
	}

	public void HideNotification(string id, bool instant = false)
	{
		if (!string.IsNullOrEmpty(id) && idNotifications.ContainsKey(id))
		{
			idNotifications[id].Hide(instant);
		}
	}

	public void HideNotificationsByTag(string notificationTag, bool instant = true, bool includeTemporary = true)
	{
		if (string.IsNullOrEmpty(notificationTag))
		{
			return;
		}
		foreach (Notification item in activeNotifications.FindAll((Notification it) => it.Tag == notificationTag && (includeTemporary || !it.IsTemporary)))
		{
			item.Hide(instant);
		}
	}

	public void ClearNotifications(bool instant = true)
	{
		StopAllCoroutines();
		while (activeNotifications.Count > 0)
		{
			activeNotifications[0].Hide(instant);
		}
	}

	private IEnumerator WaitNotification(Action callback, float waitTime)
	{
		float time = 0f;
		while (time < waitTime)
		{
			if (window.IsOpen)
			{
				time += Timekeeper.instance.m_GlobalClock.deltaTime;
			}
			yield return null;
		}
		callback();
	}

	private UINotification GetNotification()
	{
		UINotification notification;
		if (poolNotifications.Count > 0)
		{
			notification = poolNotifications[poolNotifications.Count - 1];
			poolNotifications.RemoveAt(poolNotifications.Count - 1);
			notification.transform.SetAsLastSibling();
		}
		else
		{
			notification = UnityEngine.Object.Instantiate(notificationPrefab, notificationContainer);
			notification.OnHidden.AddListener(delegate
			{
				OnHiddenNotification(notification);
			});
		}
		return notification;
	}

	private void OnHiddenNotification(UINotification notification)
	{
		poolNotifications.Add(notification);
	}

	private void UpdateCanvasVisibility()
	{
		_canvas.enabled = hideRequests.Count == 0 && activeNotifications.Count > 0;
	}
}
