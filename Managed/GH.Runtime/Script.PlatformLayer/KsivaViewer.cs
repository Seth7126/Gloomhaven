#define ENABLE_LOGS
using AsmodeeNet.Foundation;
using Platforms.ProsOrHydra;
using SM.Utils;
using TMPro;
using UnityEngine;

namespace Script.PlatformLayer;

[RequireComponent(typeof(TMP_Text))]
public class KsivaViewer : MonoBehaviour
{
	private TMP_Text _text;

	private IKsivaProvider HydraAnalytics => global::PlatformLayer.Platform.HydraKsivaProvider;

	private void Awake()
	{
		_text = GetComponent<TMP_Text>();
		_text.text = string.Empty;
	}

	private void OnEnable()
	{
		if (HydraAnalytics == null)
		{
			LogUtils.LogWarning("KsivaViewer OnEnable Analytics is not initialized.");
			return;
		}
		if (!string.IsNullOrEmpty(HydraAnalytics.Ksiva))
		{
			_text.text = HydraAnalytics.Ksiva;
		}
		else
		{
			_text.text = string.Empty;
		}
		HydraAnalytics.EventKsivaReceived += OnKsivaUpdated;
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			if (HydraAnalytics == null)
			{
				LogUtils.LogWarning("KsivaViewer OnDisable Analytics is not initialized.");
			}
			else
			{
				HydraAnalytics.EventKsivaReceived -= OnKsivaUpdated;
			}
		}
	}

	private void OnKsivaUpdated(string ksiva)
	{
		_text.text = ksiva;
	}
}
