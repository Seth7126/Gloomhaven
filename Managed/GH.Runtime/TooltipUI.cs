using GLOOM;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private string tooltipLanguageKey;

	private bool isInitiated;

	private void Awake()
	{
		if (!isInitiated && !string.IsNullOrEmpty(tooltipLanguageKey))
		{
			Init(tooltipLanguageKey);
		}
	}

	public void ToggleEnable(bool active)
	{
		if (active)
		{
			UIManager.Instance.HighlightElement(base.gameObject);
		}
		else
		{
			UIManager.Instance.UnhighlightElement(base.gameObject);
		}
		base.transform.position += GetComponent<RectTransform>().DeltaPositionToFitTheScreen(UIManager.Instance.UICamera, 10f);
		base.gameObject.SetActive(active);
	}

	public void Init(string descriptionKey, string warningKey = null)
	{
		isInitiated = true;
		descriptionText.text = LocalizationManager.GetTranslation(descriptionKey);
		warningText.enabled = !string.IsNullOrEmpty(warningKey);
		if (warningText.enabled)
		{
			warningText.text = LocalizationManager.GetTranslation(warningKey);
		}
	}
}
