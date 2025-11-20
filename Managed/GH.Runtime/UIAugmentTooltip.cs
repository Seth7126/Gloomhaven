using System.Collections.Generic;
using System.Text;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAugmentTooltip : MonoBehaviour
{
	[SerializeField]
	public TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	public Transform descriptionContainer;

	[SerializeField]
	public LayoutGroup layoutGroupPadding;

	private GameObject currentDescriptionObject;

	private ConsumeButton consumeButton;

	public void Initialize(ConsumeButton consume)
	{
		if (consumeButton == consume)
		{
			return;
		}
		consumeButton = consume;
		List<ElementInfusionBoardManager.EElement> elements = consume.abilityConsume.ConsumeData.Elements;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < elements.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append((i == elements.Count - 1) ? (LocalizationManager.GetTranslation("AND") + " ") : ", ");
			}
			stringBuilder.Append("<color=#" + ColorUtility.ToHtmlStringRGB(UIInfoTools.Instance.GetElementHighlightColor(elements[i])) + ">");
			stringBuilder.Append(LocalizationManager.GetTranslation($"GUI_ELEMENT_{elements[i]}"));
			stringBuilder.Append($" <sprite name={elements[i]}></color>");
		}
		titleText.text = string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ELEMENT"), stringBuilder);
		descriptionText.gameObject.SetActive(value: false);
		if (currentDescriptionObject != null)
		{
			Object.Destroy(currentDescriptionObject.gameObject);
		}
		currentDescriptionObject = Object.Instantiate(consume.Description.transform.GetChild(1).gameObject, descriptionContainer);
		titleText.GetComponent<LayoutElement>().enabled = false;
		titleText.ForceMeshUpdate();
		RectTransform component = GetComponent<RectTransform>();
		float x = Mathf.Max((currentDescriptionObject.transform as RectTransform).sizeDelta.x, titleText.preferredWidth) + (float)layoutGroupPadding.padding.left + (float)layoutGroupPadding.padding.right;
		component.sizeDelta = new Vector2(x, component.sizeDelta.y);
	}

	public void Initialize(string titleLoc, string textLoc)
	{
		consumeButton = null;
		if (currentDescriptionObject != null)
		{
			Object.Destroy(currentDescriptionObject.gameObject);
		}
		titleText.text = LocalizationManager.GetTranslation(titleLoc);
		descriptionText.text = LocalizationManager.GetTranslation(textLoc);
		descriptionText.gameObject.SetActive(value: true);
		titleText.ForceMeshUpdate();
		descriptionText.ForceMeshUpdate();
		RectTransform component = GetComponent<RectTransform>();
		component.sizeDelta.Set(Mathf.Max(descriptionText.renderedWidth, titleText.renderedWidth) + (float)layoutGroupPadding.padding.left + (float)layoutGroupPadding.padding.right, component.sizeDelta.y);
	}
}
