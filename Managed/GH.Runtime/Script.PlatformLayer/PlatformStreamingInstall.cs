#define ENABLE_LOGS
using System;
using Platforms;
using UnityEngine;

namespace Script.PlatformLayer;

public class PlatformStreamingInstall : MonoBehaviour
{
	private IPlatform _platform;

	private IPlatformStreamingInstall _platformStreamingInstall;

	private bool _fakeStreamingInstallIsEnabled;

	private int _fakeEta = 70;

	private float _fakeProgress;

	private bool _fakeAllFilesAccessible;

	private bool _fakeAllFilesDownloaded;

	private int _totalEta = 140;

	private float _timeSinceLastUpdate;

	public int EstimatedRequiredTime
	{
		get
		{
			if (_fakeStreamingInstallIsEnabled)
			{
				return _fakeEta;
			}
			if (_platform.IsSupportStreamingInstall)
			{
				return _platformStreamingInstall.EstimatedRequiredTime;
			}
			return 0;
		}
	}

	public float NormalizedProgress
	{
		get
		{
			if (_fakeStreamingInstallIsEnabled)
			{
				return _fakeProgress;
			}
			if (_platform.IsSupportStreamingInstall)
			{
				return _platformStreamingInstall.NormalizedProgress;
			}
			return 1f;
		}
	}

	public bool AllFilesAccessible
	{
		get
		{
			if (_fakeStreamingInstallIsEnabled)
			{
				return _fakeAllFilesAccessible;
			}
			if (_platform.IsSupportStreamingInstall)
			{
				return _platformStreamingInstall.AllFilesAccessible;
			}
			return true;
		}
	}

	public bool AllFilesDownloaded
	{
		get
		{
			if (_fakeStreamingInstallIsEnabled)
			{
				return _fakeAllFilesDownloaded;
			}
			if (_platform.IsSupportStreamingInstall)
			{
				return _platformStreamingInstall.AllFilesDownloaded;
			}
			return true;
		}
	}

	public event Action<float, int> EventProgressChanged;

	public event Action EventFilesAccessible;

	public event Action EventFilesDownloaded;

	private void Update()
	{
		if (_fakeStreamingInstallIsEnabled && Time.time - _timeSinceLastUpdate > 1f)
		{
			_timeSinceLastUpdate = Time.time;
			_fakeEta--;
			_fakeProgress = 1f - (float)_fakeEta / (float)_totalEta;
			this.EventProgressChanged?.Invoke(_fakeProgress, _fakeEta);
			if (_fakeEta <= 0)
			{
				_fakeAllFilesAccessible = true;
				_fakeAllFilesDownloaded = true;
				this.EventFilesAccessible?.Invoke();
				this.EventFilesDownloaded?.Invoke();
				_fakeStreamingInstallIsEnabled = false;
			}
		}
	}

	public void Initialize(IPlatform platform)
	{
		_platform = platform;
		_platformStreamingInstall = _platform.PlatformStreamingInstall;
		if (_platformStreamingInstall == null || !_platform.IsSupportStreamingInstall)
		{
			Debug.LogWarning("[PlatformStreamingInstall] Initialized called, but Streaming Install is not supported on " + global::PlatformLayer.Instance.PlatformID + " platform.");
		}
		else
		{
			SubscribeToNotificationEvents();
		}
	}

	public void OnDestroy()
	{
		if (_platformStreamingInstall == null || !_platform.IsSupportStreamingInstall)
		{
			Debug.LogWarning("[PlatformStreamingInstall] Destroy called, but there is nothing to destroy. Streaming install is not supported on this platform.");
		}
		else
		{
			UnSubscribeFromNotificationEvents();
		}
	}

	private void SubscribeToNotificationEvents()
	{
		_platformStreamingInstall.EventProgressChanged += OnEventProgressChanged;
		_platformStreamingInstall.EventFilesAccessible += OnEventFilesAccessible;
		_platformStreamingInstall.EventFilesDownloaded += OnEventFilesDownloaded;
	}

	private void UnSubscribeFromNotificationEvents()
	{
		_platformStreamingInstall.EventProgressChanged -= OnEventProgressChanged;
		_platformStreamingInstall.EventFilesAccessible -= OnEventFilesAccessible;
		_platformStreamingInstall.EventFilesDownloaded -= OnEventFilesDownloaded;
	}

	private void OnEventProgressChanged(float normalizedProgress, int estimatedTimeRequired)
	{
		this.EventProgressChanged?.Invoke(normalizedProgress, estimatedTimeRequired);
	}

	private void OnEventFilesAccessible()
	{
		this.EventFilesAccessible?.Invoke();
	}

	private void OnEventFilesDownloaded()
	{
		this.EventFilesDownloaded?.Invoke();
	}
}
