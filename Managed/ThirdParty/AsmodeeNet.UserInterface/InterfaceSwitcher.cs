using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Foundation.Localization;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace AsmodeeNet.UserInterface;

public class InterfaceSwitcher : MonoBehaviour
{
	private const string _documentation = "[Ctrl] + [Alt] + S ➜ Small\n[Ctrl] + [Alt] + R ➜ Regular\n[Ctrl] + [Alt] + B ➜ Big\n\n[Ctrl] + [Alt] + [F1], [F2] ... ➜ Language ";

	private LocalizationManager.Language[] _languages;

	private Key[] _keyCodes = new Key[9]
	{
		Key.F1,
		Key.F2,
		Key.F3,
		Key.F4,
		Key.F5,
		Key.F6,
		Key.F7,
		Key.F8,
		Key.F9
	};

	private void Awake()
	{
		_languages = CoreApplication.Instance.LocalizationManager.xliffFiles.Select((TextAsset x) => XliffUtility.GetXliffTargetLangFromXml(x.text)).Distinct().ToArray();
	}

	private void Update()
	{
		if (!KeyCombinationChecker.IsDebugKeyCombination())
		{
			return;
		}
		if (InputSystemUtilities.GetKeyDown(Key.S))
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayMode = Preferences.DisplayMode.Small;
			return;
		}
		if (InputSystemUtilities.GetKeyDown(Key.R))
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayMode = Preferences.DisplayMode.Regular;
			return;
		}
		if (InputSystemUtilities.GetKeyDown(Key.B))
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayMode = Preferences.DisplayMode.Big;
			return;
		}
		for (int i = 0; i < _keyCodes.Length; i++)
		{
			if (InputSystemUtilities.GetKeyDown(_keyCodes[i]))
			{
				int num = Mathf.Min(i, _languages.Length - 1);
				CoreApplication.Instance.LocalizationManager.CurrentLanguage = _languages[num];
			}
		}
	}
}
