namespace TMPro;

public static class TextMeshProProfanityFilterExtensions
{
	public static void SetTextCensored(this TMP_Text tmpText, string text)
	{
		tmpText.text = string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			text.GetCensoredStringAsync(delegate(string censuredText)
			{
				tmpText.text = censuredText;
			});
		}
	}
}
