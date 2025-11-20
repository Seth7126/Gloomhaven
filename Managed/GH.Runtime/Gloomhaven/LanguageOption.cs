using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public abstract class LanguageOption : MonoBehaviour
{
	[SerializeField]
	protected LanguageSettings _settings;

	public abstract Selectable Selectable { get; }

	public virtual void EnableNavigation()
	{
	}

	public virtual void DisableNavigation()
	{
	}

	public abstract void Initialize(List<string> languages);

	public abstract void SelectWithoutNotify(string language);

	public abstract void SetInteractable(bool interactable, Color textColor);

	protected bool CanChangeLanguage(string language)
	{
		if (LocalizationManager.CurrentLanguage != language)
		{
			return LocalizationManager.HasLanguage(language);
		}
		return false;
	}
}
