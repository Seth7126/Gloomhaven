#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoCamera : MonoBehaviour
{
	public VideoPlayer m_VideoPlayer;

	public Camera m_Camera;

	public GameObject[] m_GameobjectsToDisable;

	public static VideoCamera s_This;

	private Action m_OnCompletedCallback;

	private bool m_ReenableDisabledObjects;

	public static bool IsPlaying
	{
		get
		{
			if (s_This?.m_VideoPlayer != null)
			{
				return s_This.m_VideoPlayer.isPlaying;
			}
			return false;
		}
	}

	private void Start()
	{
		s_This = this;
	}

	private void OnDestroy()
	{
		s_This = null;
		if (m_VideoPlayer != null)
		{
			m_VideoPlayer.errorReceived -= VideoPlayerErrorReceived;
			m_VideoPlayer.loopPointReached -= EndReached;
		}
	}

	public bool PlayFullscreenVideo(string name, Action onCompletedCallback, bool reenableDisabledObjects)
	{
		m_OnCompletedCallback = onCompletedCallback;
		try
		{
			List<string> list;
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				list = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "Resources", "Data", "StreamingAssets", "Movies"), name + ".mov").ToList();
			}
			else if (Application.platform == RuntimePlatform.Switch)
			{
				list = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies_MP4_30"), name + ".mp4").ToList();
			}
			else if (Application.platform == RuntimePlatform.GameCoreXboxOne)
			{
				string[] source = name.Split('/');
				list = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies_MOV_30", source.FirstOrDefault()), source.LastOrDefault() + ".mov").ToList();
			}
			else
			{
				string[] source2 = name.Split('/');
				list = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies", source2.FirstOrDefault()), source2.LastOrDefault() + ".mov").ToList();
			}
			GameObject[] gameobjectsToDisable;
			if (list.Count == 0 || !PlatformLayer.FileSystem.ExistsFile(list[0]))
			{
				if (reenableDisabledObjects)
				{
					gameobjectsToDisable = m_GameobjectsToDisable;
					for (int i = 0; i < gameobjectsToDisable.Length; i++)
					{
						gameobjectsToDisable[i].SetActive(value: true);
					}
					m_Camera.enabled = false;
					return false;
				}
				m_OnCompletedCallback?.Invoke();
				return true;
			}
			gameobjectsToDisable = m_GameobjectsToDisable;
			for (int i = 0; i < gameobjectsToDisable.Length; i++)
			{
				gameobjectsToDisable[i].SetActive(value: false);
			}
			m_ReenableDisabledObjects = reenableDisabledObjects;
			m_Camera.enabled = true;
			m_VideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
			m_VideoPlayer.url = list[0];
			m_VideoPlayer.SetDirectAudioVolume(0, AudioController.GetGlobalVolume());
			m_VideoPlayer.errorReceived -= VideoPlayerErrorReceived;
			m_VideoPlayer.errorReceived += VideoPlayerErrorReceived;
			m_VideoPlayer.loopPointReached -= EndReached;
			m_VideoPlayer.loopPointReached += EndReached;
			m_VideoPlayer.enabled = true;
			m_VideoPlayer.Play();
			Debug.Log("before => Volumen: " + AudioController.GetGlobalVolume() + " " + m_VideoPlayer.GetDirectAudioVolume(0) + " Platform:" + Application.platform.ToString() + " canSetDirectAudioVolume:" + m_VideoPlayer.canSetDirectAudioVolume + " audioTrackCount:" + m_VideoPlayer.audioTrackCount + " controlledAudioTrackCount:" + m_VideoPlayer.controlledAudioTrackCount + " IsAudioTrackEnabled:" + m_VideoPlayer.IsAudioTrackEnabled(0) + " audioChannelCount:" + m_VideoPlayer.GetAudioChannelCount(0));
			m_VideoPlayer.prepareCompleted += delegate
			{
				Debug.Log("prepareCompleted => Volumen: " + AudioController.GetGlobalVolume() + " " + m_VideoPlayer.GetDirectAudioVolume(0) + " Platform:" + Application.platform.ToString() + " canSetDirectAudioVolume:" + m_VideoPlayer.canSetDirectAudioVolume + " audioTrackCount:" + m_VideoPlayer.audioTrackCount + " controlledAudioTrackCount:" + m_VideoPlayer.controlledAudioTrackCount + " IsAudioTrackEnabled:" + m_VideoPlayer.IsAudioTrackEnabled(0) + " audioChannelCount:" + m_VideoPlayer.GetAudioChannelCount(0));
			};
			m_VideoPlayer.started += delegate
			{
				Debug.Log("started => Volumen: " + AudioController.GetGlobalVolume() + " " + m_VideoPlayer.GetDirectAudioVolume(0) + " Platform:" + Application.platform.ToString() + " canSetDirectAudioVolume:" + m_VideoPlayer.canSetDirectAudioVolume + " audioTrackCount:" + m_VideoPlayer.audioTrackCount + " controlledAudioTrackCount:" + m_VideoPlayer.controlledAudioTrackCount + " IsAudioTrackEnabled:" + m_VideoPlayer.IsAudioTrackEnabled(0) + " audioChannelCount:" + m_VideoPlayer.GetAudioChannelCount(0));
			};
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
			if (reenableDisabledObjects)
			{
				GameObject[] gameobjectsToDisable = m_GameobjectsToDisable;
				for (int i = 0; i < gameobjectsToDisable.Length; i++)
				{
					gameobjectsToDisable[i].SetActive(value: true);
				}
				m_Camera.enabled = false;
				return false;
			}
			m_OnCompletedCallback?.Invoke();
			return true;
		}
	}

	private void VideoPlayerErrorReceived(VideoPlayer source, string message)
	{
		Debug.LogWarning("Error received when trying to play a video: " + message + "\nStopping video player and moving on");
		m_VideoPlayer.Stop();
		if (m_ReenableDisabledObjects)
		{
			GameObject[] gameobjectsToDisable = m_GameobjectsToDisable;
			for (int i = 0; i < gameobjectsToDisable.Length; i++)
			{
				gameobjectsToDisable[i].SetActive(value: true);
			}
			m_Camera.enabled = false;
			m_VideoPlayer.enabled = false;
		}
		m_OnCompletedCallback?.Invoke();
	}

	private void EndReached(VideoPlayer vp)
	{
		try
		{
			vp.Stop();
			if (m_ReenableDisabledObjects)
			{
				GameObject[] gameobjectsToDisable = m_GameobjectsToDisable;
				for (int i = 0; i < gameobjectsToDisable.Length; i++)
				{
					gameobjectsToDisable[i].SetActive(value: true);
				}
				m_Camera.enabled = false;
				m_VideoPlayer.enabled = false;
			}
			m_OnCompletedCallback?.Invoke();
		}
		catch
		{
		}
	}

	public void Stop()
	{
		try
		{
			if (!m_VideoPlayer.isPlaying)
			{
				return;
			}
			m_VideoPlayer.Stop();
			if (m_ReenableDisabledObjects)
			{
				GameObject[] gameobjectsToDisable = m_GameobjectsToDisable;
				for (int i = 0; i < gameobjectsToDisable.Length; i++)
				{
					gameobjectsToDisable[i].SetActive(value: true);
				}
				m_Camera.enabled = false;
				m_VideoPlayer.enabled = false;
			}
		}
		catch
		{
		}
	}
}
