using System.IO;
using Gloomhaven;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundView : MonoBehaviour
{
	public GameObject m_BG_Image;

	[SerializeField]
	private GameObject m_Logo;

	[SerializeField]
	private GameObject m_Blur;

	private GameObject m_VideoPlayerGO;

	private VideoPlayer m_VideoPlayer;

	private bool m_Disabled;

	public void LogoSetActive(bool active)
	{
		m_Logo.SetActive(active);
	}

	public void BlurSetActive(bool active)
	{
		m_Blur.SetActive(active);
	}

	private void OnEnable()
	{
		if (m_Disabled)
		{
			return;
		}
		try
		{
			m_VideoPlayerGO = (Singleton<MainVideoProvider>.IsInitialized ? Singleton<MainVideoProvider>.Instance.VideoPlayer.gameObject : null);
			if ((m_BG_Image == null || m_VideoPlayerGO == null) && m_VideoPlayerGO != null)
			{
				VideoPlayer component = m_VideoPlayerGO.GetComponent<VideoPlayer>();
				m_VideoPlayerGO.GetComponent<Camera>().enabled = false;
				component.enabled = false;
				component.Stop();
			}
		}
		catch
		{
			m_Disabled = true;
		}
	}

	private void Update()
	{
		if (m_Disabled || SceneController.Instance.IsLoading || SceneController.Instance.ScenarioIsLoading)
		{
			return;
		}
		try
		{
			if (m_VideoPlayer == null)
			{
				m_VideoPlayer = m_VideoPlayerGO.GetComponent<VideoPlayer>();
			}
			if (m_VideoPlayer.enabled || !(m_BG_Image != null) || SceneController.Instance.MainMenuVideos == null || SceneController.Instance.MainMenuVideos.Count <= 0)
			{
				return;
			}
			if (SceneController.Instance.MainMenuVideoHashSet.Count == SceneController.Instance.MainMenuVideos.Count)
			{
				SceneController.Instance.MainMenuVideoHashSet.Clear();
			}
			int num = GloomUtility.ExclusiveRandomNumber(SharedClient.GlobalRNG, SceneController.Instance.MainMenuVideoHashSet, SceneController.Instance.MainMenuVideos.Count - 1);
			SceneController.Instance.MainMenuVideoHashSet.Add(num);
			if (SceneController.Instance.MainMenuVideos.Count > num && File.Exists(SceneController.Instance.MainMenuVideos[num]))
			{
				if ((bool)m_BG_Image && m_BG_Image.activeSelf)
				{
					m_BG_Image.SetActive(value: false);
				}
				m_VideoPlayer.url = SceneController.Instance.MainMenuVideos[num];
				m_VideoPlayer.SetDirectAudioVolume(0, AudioController.GetGlobalVolume());
				m_VideoPlayerGO.GetComponent<Camera>().enabled = true;
				m_VideoPlayer.enabled = true;
				m_VideoPlayer.Play();
			}
		}
		catch
		{
			m_Disabled = true;
		}
	}
}
