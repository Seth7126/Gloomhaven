using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Utilities;

public class PromotionVideoHandler : Singleton<PromotionVideoHandler>
{
	[SerializeField]
	private bool showAtStart;

	[ConditionalField("showAtStart", true, true)]
	[SerializeField]
	private int timesVideoShows = 3;

	[SerializeField]
	private UIWindow videoHolder;

	[SerializeField]
	private VideoPlayer videoPlayer;

	[SerializeField]
	private ClickTracker clickTracker;

	[SerializeField]
	private List<GameObject> objectsToHide;

	private int currentTimes;

	private const string PREF_KEY = "PromotionVideo";

	private Action onStopVideo;

	protected override void Awake()
	{
		base.Awake();
		try
		{
			currentTimes = KeyValueStore.GetInt("PromotionVideo");
			videoPlayer.loopPointReached += OnLoopPointReached;
			videoPlayer.prepareCompleted += OnPrepareCompleted;
			clickTracker.enabled = false;
			if (!showAtStart || currentTimes >= timesVideoShows)
			{
				return;
			}
			objectsToHide.ForEach(delegate(GameObject it)
			{
				it.SetActive(value: false);
			});
			onStopVideo = delegate
			{
				onStopVideo = null;
				objectsToHide.ForEach(delegate(GameObject it)
				{
					it.SetActive(value: true);
				});
				clickTracker.enabled = true;
			};
			clickTracker.enabled = false;
			StartCoroutine(PlayVideoInit());
		}
		catch
		{
		}
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		videoPlayer.loopPointReached -= OnLoopPointReached;
		videoPlayer.prepareCompleted -= OnPrepareCompleted;
		onStopVideo = null;
		base.OnDestroy();
	}

	private void OnLoopPointReached(VideoPlayer source)
	{
		StopVideo();
	}

	private void OnPrepareCompleted(VideoPlayer source)
	{
		videoPlayer.Play();
	}

	private IEnumerator PlayVideoInit()
	{
		yield return new WaitForEndOfFrame();
		try
		{
			PlayVideo();
			currentTimes++;
			KeyValueStore.SetInt("PromotionVideo", currentTimes);
			KeyValueStore.Save();
		}
		catch
		{
		}
	}

	public void PlayVideo()
	{
		try
		{
			AudioController.SetGlobalVolume(0f);
			videoHolder.Show();
			videoPlayer.Prepare();
		}
		catch
		{
		}
	}

	public void StopVideo()
	{
		try
		{
			videoHolder.Hide();
			AudioController.SetGlobalVolume((float)SaveData.Instance.Global.MasterVolume / 100f);
			videoPlayer.Stop();
			onStopVideo?.Invoke();
		}
		catch
		{
		}
	}

	private void Update()
	{
		try
		{
			if (videoHolder.IsOpen && ((bool)Singleton<InputManager>.Instance.PlayerControl.ActiveDevice.AnyButton || InputSystemUtilities.AnyKeyDown()))
			{
				StopVideo();
			}
		}
		catch
		{
		}
	}
}
