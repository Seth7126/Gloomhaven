using System;
using AsmodeeNet.Utils.Extensions;
using GLOOM;

public class DisableOptionEntryStrategy : CustomOptionEntryEnableStrategy
{
	public override void Enable(UIOptionEntry uiOptionEntry, bool enable)
	{
		uiOptionEntry.gameObject.SetActive(enable);
		string translation = LocalizationManager.GetTranslation(uiOptionEntry.MDescriptionLoc);
		int num = translation.IndexOf(":", StringComparison.Ordinal);
		if (num == -1)
		{
			uiOptionEntry.SetDescriptionText(translation);
		}
		else if (enable)
		{
			uiOptionEntry.SetDescriptionText("<color=#" + UIInfoTools.Instance.mainColor.ToHex() + ">" + translation.SubstringFromXToY(0, num) + "</color>" + translation.Substring(num));
		}
	}
}
