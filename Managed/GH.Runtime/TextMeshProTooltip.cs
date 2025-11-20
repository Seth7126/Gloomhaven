using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TextMeshProUGUI TextMeshPro;

	[SerializeField]
	private UITooltip.Corner corner = UITooltip.Corner.Auto;

	[SerializeField]
	private Vector2 anchoredOffset = Vector2.zero;

	[SerializeField]
	private float width = 100f;

	[SerializeField]
	private Vector3 wordPivotPosition = new Vector2(0f, 0f);

	private bool isHovered;

	private int showingTooltipId = -1;

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovered = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovered = false;
		if (showingTooltipId != -1)
		{
			showingTooltipId = -1;
			UITooltip.Hide();
		}
	}

	private void LateUpdate()
	{
		if (!isHovered)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(TextMeshPro, InputManager.CursorPosition, UIManager.Instance.UICamera);
		if (num != -1)
		{
			if (num != showingTooltipId)
			{
				showingTooltipId = num;
				TMP_LinkInfo tMP_LinkInfo = TextMeshPro.textInfo.linkInfo[num];
				BuidTooltip(LocalizationManager.GetTranslation(tMP_LinkInfo.GetLinkID()));
				UITooltip.Show();
				Vector3 bottomLeft = TextMeshPro.textInfo.characterInfo[tMP_LinkInfo.linkTextfirstCharacterIndex].bottomLeft;
				Vector3 vector = TextMeshPro.textInfo.characterInfo[tMP_LinkInfo.linkTextfirstCharacterIndex + tMP_LinkInfo.linkTextLength - 1].topRight - bottomLeft;
				vector.Scale(wordPivotPosition);
				UITooltip.GetTransform().position = base.transform.TransformPoint(bottomLeft + vector);
				(UITooltip.GetTransform().transform as RectTransform).anchoredPosition += anchoredOffset;
			}
		}
		else if (showingTooltipId != -1)
		{
			showingTooltipId = -1;
			UITooltip.Hide();
		}
	}

	private void BuidTooltip(string text)
	{
		UITooltip.SetWidth(width);
		UITooltip.AddTitle(text);
		UITooltip.SetVerticalControls(autoAdjusted: true);
		UITooltip.AnchorToRect(base.transform as RectTransform, corner);
		UITooltip.SetAnchoredOffset(anchoredOffset);
		UITooltip.SetScreenBound(enable: false, 0f);
	}
}
