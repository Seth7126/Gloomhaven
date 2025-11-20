using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Compendium/Styles/Text Style")]
public class TextSyle : Style<TextMeshProUGUI>
{
	public TMP_FontAsset font;

	public int textSize;

	public Color color;

	public override void Apply(TextMeshProUGUI go)
	{
		go.font = font;
		go.fontSize = textSize;
		go.color = color;
	}
}
