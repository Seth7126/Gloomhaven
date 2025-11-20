using AsmodeeNet.Foundation;
using I2.Loc;
using UnityEngine;

public abstract class LocalizedListener : MonoBehaviour
{
	private string lastLanguage;

	protected virtual void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
		if (lastLanguage != null && lastLanguage != LocalizationManager.CurrentLanguage)
		{
			OnLanguageChanged();
		}
	}

	protected virtual void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
		if (!CoreApplication.IsQuitting)
		{
			lastLanguage = LocalizationManager.CurrentLanguage;
		}
	}

	protected abstract void OnLanguageChanged();
}
