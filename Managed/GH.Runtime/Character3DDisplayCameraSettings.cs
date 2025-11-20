using System;
using System.Collections.Generic;
using UnityEngine;

public class Character3DDisplayCameraSettings : MonoBehaviour
{
	[Serializable]
	private class Setting
	{
		[SerializeField]
		private RenderingPath _renderingPath;

		[SerializeField]
		private string _character;

		[SerializeField]
		private DeviceType _platform;

		public RenderingPath RenderingPath => _renderingPath;

		public string Character => _character;

		public DeviceType Platform => _platform;
	}

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private RenderingPath _defaultRenderingPath = RenderingPath.Forward;

	[SerializeField]
	private List<Setting> _overrideSettings = new List<Setting>();

	public void UpdateCharacter(string _character)
	{
		foreach (Setting overrideSetting in _overrideSettings)
		{
			if (PlatformLayer.Instance.GetCurrentPlatform() == overrideSetting.Platform && _character == overrideSetting.Character)
			{
				_camera.renderingPath = overrideSetting.RenderingPath;
				return;
			}
		}
		_camera.renderingPath = _defaultRenderingPath;
	}
}
