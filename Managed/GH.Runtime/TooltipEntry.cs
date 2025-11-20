using UnityEngine;
using UnityEngine.UI;

public class TooltipEntry : MonoBehaviour
{
	public Image TooltipIcon;

	public Text TooltipName;

	public Text TooltipValue;

	public void PopulateTooltip(string text, Color newCol, Sprite sprite = null, bool colourText = false, bool colourImage = false, string value = null)
	{
		TooltipName.text = text;
		if (sprite != null)
		{
			TooltipIcon.sprite = sprite;
		}
		if (colourText)
		{
			TooltipName.color = newCol;
		}
		if (colourImage)
		{
			TooltipIcon.color = newCol;
		}
		if (value != null)
		{
			TooltipValue.enabled = true;
			TooltipValue.text = value;
		}
	}
}
