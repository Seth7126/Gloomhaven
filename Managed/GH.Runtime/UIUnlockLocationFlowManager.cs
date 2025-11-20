using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ControllerInputAreaLocal))]
public class UIUnlockLocationFlowManager : Singleton<UIUnlockLocationFlowManager>
{
	[SerializeField]
	[Range(0f, 1f)]
	private float zoomInLocationPercent = 0.9f;

	[SerializeField]
	private float focusMoveDuration = 1f;

	[SerializeField]
	private float focusZoomDuration = 1f;

	[SerializeField]
	private UILocationPopup popup;

	[SerializeField]
	private Button continueButton;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private string revealLocationAudioItem = "PlaySound_UIDiscoverLocation";

	private ControllerInputAreaLocal controllerArea;

	private Action continueAction;

	protected override void Awake()
	{
		base.Awake();
		if (!InputManager.GamePadInUse)
		{
			continueButton.onClick.AddListener(Continue);
		}
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		controllerArea.OnFocusedArea.AddListener(OnFocus);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Continue).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
	}

	protected override void OnDestroy()
	{
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, Continue);
		if (!InputManager.GamePadInUse)
		{
			continueButton.onClick.RemoveAllListeners();
		}
		base.OnDestroy();
	}

	private void OnFocus()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.UnlockLocationFlow);
	}

	public void ShowUnlockedLocations(List<MapLocation> locations, Action onFinished = null, float? returnZoom = null)
	{
		if (locations.Count == 0)
		{
			onFinished?.Invoke();
			return;
		}
		Clear();
		window.Show();
		Vector3 targetFocus = CameraController.s_CameraController.m_TargetFocalPoint;
		if (!returnZoom.HasValue)
		{
			returnZoom = CameraController.s_CameraController.Zoom;
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		Singleton<UIGuildmasterHUD>.Instance.Hide(this);
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			foreach (MapLocation mapLocation in locations.Where((MapLocation it) => it.LocationQuest.Quest.Type == EQuestType.City))
			{
				mapLocation.HideQuestMapMarker(this);
				callbackPromise = callbackPromise.Then(() => ShowUnlockedLocation(mapLocation.LocationQuest, mapLocation));
			}
			foreach (MapLocation mapLocation2 in locations.Where((MapLocation it) => it.LocationQuest.Quest.Type != EQuestType.City))
			{
				callbackPromise = callbackPromise.Then(() => ShowUnlockedLocation(mapLocation2.LocationQuest, Singleton<MapChoreographer>.Instance.HeadquartersLocation));
			}
		}
		else
		{
			foreach (MapLocation mapLocation3 in locations.Where((MapLocation it) => it.LocationQuest.Quest.Type != EQuestType.City))
			{
				mapLocation3.HideQuestMapMarker(this);
				callbackPromise = callbackPromise.Then(() => ShowUnlockedLocation(mapLocation3.LocationQuest, mapLocation3));
			}
			foreach (MapLocation mapLocation4 in locations.Where((MapLocation it) => it.LocationQuest.Quest.Type == EQuestType.City))
			{
				callbackPromise = callbackPromise.Then(() => ShowUnlockedLocation(mapLocation4.LocationQuest, Singleton<MapChoreographer>.Instance.HeadquartersLocation));
			}
		}
		callbackPromise.Then(delegate
		{
			Clear();
			return Focus(targetFocus, returnZoom);
		}).Done(delegate
		{
			window.Hide();
			Singleton<UIGuildmasterHUD>.Instance.Show(this);
			if (!AdventureState.MapState.MapParty.HasIntroduced(EIntroductionConcept.CityQuest.ToString()) && locations.Exists((MapLocation it) => it.LocationQuest.Quest.Type == EQuestType.City))
			{
				Singleton<UIIntroductionManager>.Instance.Show(EIntroductionConcept.CityQuest, delegate
				{
					AdventureState.MapState.MapParty.MarkIntroDone(EIntroductionConcept.CityQuest.ToString());
					OnFinished();
				});
			}
			else
			{
				OnFinished();
			}
		});
		void OnFinished()
		{
			CameraController.s_CameraController?.FreeDisableCameraInput(this);
			onFinished?.Invoke();
		}
	}

	private void Clear()
	{
		popup.Hide();
		continueButton.interactable = false;
		continueAction = null;
	}

	private ICallbackPromise ShowUnlockedLocation(CQuestState quest, MapLocation mapLocation)
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		continueAction = callbackPromise.Resolve;
		continueButton.interactable = false;
		popup.Hide();
		Focus(mapLocation).Done(delegate
		{
			AudioControllerUtils.PlaySound(revealLocationAudioItem);
			mapLocation.ShowQuestMapMarker(instant: false, this);
			popup.Show(quest);
			continueButton.interactable = true;
		});
		return callbackPromise;
	}

	public void Continue()
	{
		continueAction?.Invoke();
	}

	private ICallbackPromise Focus(MapLocation location)
	{
		return Focus(location.transform.position, CameraController.s_CameraController.CalculateZoomByPercent(zoomInLocationPercent));
	}

	private ICallbackPromise Focus(Vector3 location, float? zoomPercent)
	{
		CallbackPromise promise = new CallbackPromise();
		CameraController.s_CameraController.SetOverriddenBehavior(new MapFocusCameraBehavior(zoomPercent, location, focusMoveDuration, delegate
		{
			CameraController.s_CameraController.ClearOverriddenBehavior();
			promise.Resolve();
		}, delegate
		{
			if (promise.IsPending)
			{
				promise.Resolve();
			}
		})
		{
			DisableCancelCallbackIfFinished = true
		});
		return promise;
	}
}
