using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Foundation.Localization;
using AsmodeeNet.Utils;
using TMPro;
using UnityEngine;

namespace AsmodeeNet.UserInterface;

public class FontSizeHomogenizer : MonoBehaviour
{
	private const string _documentation = "Based on <i>Auto Sizing</i> all the referenced TextMeshProUGUI texts will have the same font size.";

	public TextMeshProUGUI[] labels;

	private const int _minimalUpdateCount = 2;

	private int _updateCount;

	private float _stabilizedFontSize;

	private void OnEnable()
	{
		CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange += SetNeedsFontSizeUpdate;
		CoreApplication.Instance.Preferences.AspectDidChange += SetNeedsFontSizeUpdate;
		CoreApplication.Instance.LocalizationManager.OnLanguageChanged += _OnLanguageChanged;
		SetNeedsFontSizeUpdate();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange -= SetNeedsFontSizeUpdate;
			CoreApplication.Instance.Preferences.AspectDidChange -= SetNeedsFontSizeUpdate;
			CoreApplication.Instance.LocalizationManager.OnLanguageChanged -= _OnLanguageChanged;
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		SetNeedsFontSizeUpdate();
	}

	private void _OnLanguageChanged(LocalizationManager localizationManager)
	{
		SetNeedsFontSizeUpdate();
	}

	public void SetNeedsFontSizeUpdate()
	{
		_updateCount = 2;
	}

	private void LateUpdate()
	{
		if (_updateCount > 0)
		{
			StopAllCoroutines();
			StartCoroutine(_UpdateFontSizeAndWait());
		}
	}

	private IEnumerator _UpdateFontSizeAndWait()
	{
		while (_updateCount > 0)
		{
			_UpdateFontSize();
			yield return null;
		}
	}

	private void _UpdateFontSize()
	{
		IEnumerable<TextMeshProUGUI> enumerable = labels.Where((TextMeshProUGUI x) => x.IsActive() && !string.IsNullOrEmpty(x.text));
		if (!enumerable.Any())
		{
			return;
		}
		foreach (TextMeshProUGUI item in enumerable)
		{
			item.enableAutoSizing = true;
			item.ForceMeshUpdate();
		}
		float num = enumerable.Min((TextMeshProUGUI label) => label.fontSize);
		foreach (TextMeshProUGUI item2 in enumerable)
		{
			item2.fontSize = num;
			item2.enableAutoSizing = false;
		}
		_updateCount--;
		if (_updateCount == 0 && !MathUtils.Approximately(_stabilizedFontSize, num, 1f))
		{
			_updateCount = 1;
		}
		_stabilizedFontSize = num;
	}
}
