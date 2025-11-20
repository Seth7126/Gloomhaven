using TMPro;
using UnityEngine;

public class UIOptionTextCheck : MonoBehaviour
{
	[SerializeField]
	private GameObject tick;

	[SerializeField]
	private TMP_Text description;

	[SerializeField]
	private Color uncheckColor;

	private Color enabledColor;

	private void Awake()
	{
		enabledColor = description.color;
	}

	public void EnableMod(bool enable)
	{
		tick.SetActive(enable);
		description.color = (enable ? enabledColor : uncheckColor);
	}
}
