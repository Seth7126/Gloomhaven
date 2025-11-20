using System;
using AsmodeeNet.Utils.Extensions;
using UnityEngine;

namespace AsmodeeNet.Foundation;

public class Preferences
{
	public enum Orientation
	{
		Unknown,
		Horizontal,
		Vertical
	}

	public enum DisplayMode
	{
		Unknown,
		Small,
		Regular,
		Big
	}

	private float _aspect;

	private Orientation _interfaceOrientation;

	private const string _kDisplayModeKey = "DisplayMode";

	private DisplayMode _displayMode;

	public float Aspect
	{
		get
		{
			UpdateAspect();
			return _aspect;
		}
	}

	public Orientation InterfaceOrientation
	{
		get
		{
			UpdateInterfaceOrientation();
			return _interfaceOrientation;
		}
	}

	public DisplayMode InterfaceDisplayMode
	{
		get
		{
			if (_displayMode == DisplayMode.Unknown)
			{
				if (KeyValueStore.HasKey("DisplayMode"))
				{
					try
					{
						string value = KeyValueStore.GetString("DisplayMode");
						_displayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), value);
					}
					catch
					{
					}
				}
				if (_displayMode == DisplayMode.Unknown)
				{
					switch (SystemInfo.deviceType)
					{
					case DeviceType.Console:
						_displayMode = DisplayMode.Small;
						break;
					case DeviceType.Handheld:
						_displayMode = ((ScreenExtension.DiagonalLengthInch < 8f) ? DisplayMode.Small : DisplayMode.Regular);
						break;
					case DeviceType.Desktop:
						_displayMode = ((ScreenExtension.DiagonalLengthInch < 16f) ? DisplayMode.Regular : DisplayMode.Big);
						break;
					}
				}
			}
			return _displayMode;
		}
		set
		{
			_displayMode = value;
			KeyValueStore.SetString("DisplayMode", _displayMode.ToString());
			if (this.InterfaceDisplayModeDidChange != null)
			{
				this.InterfaceDisplayModeDidChange();
			}
		}
	}

	public bool IsDesktop => SystemInfo.deviceType == DeviceType.Desktop;

	public event Action AspectDidChange;

	public event Action InterfaceOrientationDidChange;

	public event Action InterfaceDisplayModeDidChange;

	public void Update()
	{
		UpdateAspect();
		UpdateInterfaceOrientation();
	}

	private void UpdateAspect()
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return;
		}
		float aspect = main.aspect;
		if (!Mathf.Approximately(aspect, _aspect))
		{
			_aspect = aspect;
			if (this.AspectDidChange != null)
			{
				this.AspectDidChange();
			}
		}
	}

	private void UpdateInterfaceOrientation()
	{
		Orientation orientation = ((!(Aspect < 1f)) ? Orientation.Horizontal : Orientation.Vertical);
		if (orientation != _interfaceOrientation)
		{
			_interfaceOrientation = orientation;
			if (this.InterfaceOrientationDidChange != null)
			{
				this.InterfaceOrientationDidChange();
			}
		}
	}
}
