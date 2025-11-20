using System.Collections;
using System.Text;
using GLOOM;
using TMPro;
using UnityEngine;

public class HelpBoxLine : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI tipText;

	[SerializeField]
	private GUIAnimator warningAnimation;

	[SerializeField]
	private TextMeshProUGUI warningTextAnimation;

	public void ShowWarning(string tipKey = null, string titleKey = null, string translatedPrefix = null, HelpBox.FormatTarget useWarningFormat = HelpBox.FormatTarget.ALL)
	{
		Show(tipKey, titleKey, translatedPrefix, useWarningFormat);
	}

	public void Show(string tipKey = null, string titleKey = null, string translatedPrefix = null, HelpBox.FormatTarget useWarningFormat = HelpBox.FormatTarget.NONE)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = "<color=#FF001AFF>{0}</color>";
		if (!string.IsNullOrEmpty(titleKey))
		{
			stringBuilder.Append(string.Format((useWarningFormat == HelpBox.FormatTarget.TITLE || useWarningFormat == HelpBox.FormatTarget.ALL) ? text : "<color=#eacf8c>{0}</color>", LocalizationManager.GetTranslation(titleKey)));
		}
		if (!string.IsNullOrEmpty(tipKey))
		{
			bool flag = useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TEXT;
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(flag ? string.Format(text, ": ") : ": ");
			}
			if (!string.IsNullOrEmpty(translatedPrefix))
			{
				stringBuilder.Append(string.Format(flag ? text : "<color=#d0d0d0>{0}</color>", translatedPrefix));
			}
			stringBuilder.Append(string.Format(flag ? text : "<color=#d0d0d0>{0}</color>", LocalizationManager.GetTranslation(tipKey)));
		}
		tipText.text = stringBuilder.ToString();
		if (base.gameObject.activeInHierarchy)
		{
			OnEnable();
		}
		base.gameObject.SetActive(value: true);
		warningAnimation.Stop();
	}

	public void ShowTranslated(string tip = null, string title = null, string translatedPrefix = null, bool useWarningFormat = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = "<color=#FF001AFF>{0}</color>";
		if (!string.IsNullOrEmpty(title))
		{
			stringBuilder.Append(string.Format(useWarningFormat ? text : "<color=#eacf8c>{0}</color>", title));
		}
		if (!string.IsNullOrEmpty(tip))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(useWarningFormat ? string.Format(text, ": ") : ": ");
			}
			if (!string.IsNullOrEmpty(translatedPrefix))
			{
				stringBuilder.Append(string.Format(useWarningFormat ? text : "<color=#d0d0d0>{0}</color>", translatedPrefix));
			}
			stringBuilder.Append(string.Format(useWarningFormat ? text : "<color=#d0d0d0>{0}</color>", tip));
		}
		tipText.text = stringBuilder.ToString();
		if (base.gameObject.activeInHierarchy)
		{
			OnEnable();
		}
		base.gameObject.SetActive(value: true);
		warningAnimation.Stop();
	}

	public void ShowTranslatedWarning(HelpBox.FormatTarget useWarningFormat, string tip = null, string title = null, string translatedPrefix = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = "<color=#FF001AFF>{0}</color>";
		if (!string.IsNullOrEmpty(title))
		{
			stringBuilder.Append(string.Format((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TITLE) ? text : "<color=#eacf8c>{0}</color>", title));
		}
		if (!string.IsNullOrEmpty(tip))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TEXT) ? string.Format(text, ": ") : ": ");
			}
			if (!string.IsNullOrEmpty(translatedPrefix))
			{
				stringBuilder.Append(string.Format((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TEXT) ? text : "<color=#d0d0d0>{0}</color>", translatedPrefix));
			}
			stringBuilder.Append(string.Format((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TEXT) ? text : "<color=#d0d0d0>{0}</color>", tip));
		}
		tipText.text = stringBuilder.ToString();
		if (base.gameObject.activeInHierarchy)
		{
			OnEnable();
		}
		base.gameObject.SetActive(value: true);
		warningAnimation.Stop();
	}

	public void Hide()
	{
		warningAnimation.Stop();
		base.gameObject.SetActive(value: false);
	}

	public void HighlightWarning()
	{
		warningTextAnimation.text = tipText.text;
		warningAnimation.Play();
	}

	private void OnDisable()
	{
		warningAnimation.Stop();
	}

	private void OnEnable()
	{
		StartCoroutine(WaitRegenerateSubmeshes());
	}

	private IEnumerator WaitRegenerateSubmeshes()
	{
		yield return null;
		RefreshSubmeshes();
	}

	private void RefreshSubmeshes()
	{
		TMP_SubMeshUI[] componentsInChildren = tipText.GetComponentsInChildren<TMP_SubMeshUI>();
		foreach (TMP_SubMeshUI obj in componentsInChildren)
		{
			obj.transform.SetParent(tipText.transform.parent);
			obj.transform.SetSiblingIndex(tipText.transform.GetSiblingIndex());
		}
	}
}
