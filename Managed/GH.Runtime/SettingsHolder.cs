using System.Collections;
using Gloomhaven;
using UnityEngine;

public class SettingsHolder : MonoBehaviour
{
	[SerializeField]
	private AudioLoadSettings _audioLoadSettings;

	[SerializeField]
	private GraphicSettings _graphicSettings;

	[SerializeField]
	private CombatLogSettings _combatLogSettings;

	[SerializeField]
	private ControlsSettings _controlsSettings;

	[SerializeField]
	private InterfaceSettings _interfaceSettings;

	[SerializeField]
	private LanguageSettings _languageSettings;

	public IEnumerator Setup()
	{
		_audioLoadSettings.Initialize();
		_graphicSettings.Initialize();
		_combatLogSettings.Initialize();
		_controlsSettings.Initialize();
		_interfaceSettings.Initialize();
		yield return StartCoroutine(_languageSettings.Initialize());
	}

	public void SetupPrimaryAudio()
	{
		_audioLoadSettings.Initialize();
	}

	public IEnumerator SetupPrimaryLanguage()
	{
		yield return StartCoroutine(_languageSettings.Initialize(reloadCardAssets: false));
	}

	public void SavePrimarySettingsOnMachine()
	{
		string value = JsonUtility.ToJson(SaveData.Instance.Global);
		PlayerPrefs.SetString("GlobalData.dat", value);
		PlayerPrefs.Save();
		string value2 = JsonUtility.ToJson(SaveData.Instance.RootData);
		PlayerPrefs.SetString("GloomSaven.dat", value2);
		PlayerPrefs.Save();
	}
}
