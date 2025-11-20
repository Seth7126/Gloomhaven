using System.Collections.Generic;
using UnityEngine;

namespace AsmodeeNet.Foundation;

[CreateAssetMenu]
public class ApplicationSettings : ScriptableObject
{
	protected List<IBaseSetting> _baseSettings;

	[Header("Core Settings")]
	[SerializeField]
	private FloatSetting _music;

	[SerializeField]
	private BoolSetting _musicState;

	[SerializeField]
	private FloatSetting _sfx;

	[SerializeField]
	private BoolSetting _sfxState;

	[SerializeField]
	private IntSetting _animationLevel;

	[SerializeField]
	private BoolSetting _fullScreen;

	public FloatSetting Music => _music;

	public BoolSetting MusicState => _musicState;

	public FloatSetting Sfx => _sfx;

	public BoolSetting SfxState => _sfxState;

	public IntSetting AnimationLevel => _animationLevel;

	public BoolSetting FullScreen => _fullScreen;

	private void OnEnable()
	{
		_baseSettings = new List<IBaseSetting>();
		_PopulateCoreSettingsList();
	}

	private void _PopulateCoreSettingsList()
	{
		_music = _music ?? new FloatSetting("Core.Music");
		_baseSettings.Add(_music);
		_musicState = _musicState ?? new BoolSetting("Core.MusicState");
		_baseSettings.Add(_musicState);
		_sfx = _sfx ?? new FloatSetting("Core.Sfx");
		_baseSettings.Add(_sfx);
		_sfxState = _sfxState ?? new BoolSetting("Core.SfxState");
		_baseSettings.Add(_sfxState);
		_animationLevel = _animationLevel ?? new IntSetting("Core.AnimationLevel");
		_baseSettings.Add(_animationLevel);
		_fullScreen = _fullScreen ?? new BoolSetting("Core.FullScreen");
		_baseSettings.Add(_fullScreen);
	}

	public void Clear()
	{
		foreach (IBaseSetting baseSetting in _baseSettings)
		{
			baseSetting.Clear();
		}
	}

	public override string ToString()
	{
		string text = "";
		foreach (IBaseSetting baseSetting in _baseSettings)
		{
			text += $"[{baseSetting}] ";
		}
		return text;
	}
}
