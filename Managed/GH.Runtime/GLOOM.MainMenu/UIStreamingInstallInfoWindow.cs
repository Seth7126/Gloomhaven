using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UIStreamingInstallInfoWindow : MonoBehaviour
{
	public TextMeshProUGUI _infoText;

	public TextMeshProUGUI _etaText;

	public Slider _downloadProgressSlider;

	private float _normalizedDownloadProgress;

	private int _estimatedSecondsToComplete;

	private void Start()
	{
		_infoText.text = LocalizationManager.GetTranslation("Consoles/GUI_STREAMING_INSTALL_IN_PROGRESS");
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void UpdateStreamingInstallWindow(float downloadProgress, int timeToComplete)
	{
		_normalizedDownloadProgress = downloadProgress;
		_estimatedSecondsToComplete = timeToComplete;
		UpdateUIWindow();
	}

	private void UpdateUIWindow()
	{
		string text = "";
		string text2 = "";
		int num = (int)Math.Truncate((float)_estimatedSecondsToComplete / 60f);
		int num2 = _estimatedSecondsToComplete - num * 60;
		if (num > 0)
		{
			text = num + "m";
		}
		if (num2 > 0)
		{
			text2 = num2 + "s";
		}
		_etaText.text = text + " " + text2;
		_downloadProgressSlider.value = _normalizedDownloadProgress;
	}
}
