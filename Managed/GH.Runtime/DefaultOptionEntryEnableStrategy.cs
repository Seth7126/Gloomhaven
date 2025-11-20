using System;
using AsmodeeNet.Utils.Extensions;
using GLOOM;

public class DefaultOptionEntryEnableStrategy : IOptionEntryStrategy
{
	public void Enable(UIOptionEntry uiOptionEntry, bool enable)
	{
		string translation = LocalizationManager.GetTranslation(uiOptionEntry.MDescriptionLoc);
		int num = translation.IndexOf(":", StringComparison.Ordinal);
		translation = ((num == -1) ? (enable ? translation : ("<color=#" + uiOptionEntry.DisabledColorText.ToHex() + ">" + translation + "</color>")) : ((!enable) ? ("<color=#" + uiOptionEntry.DisabledColorText.ToHex() + ">" + translation.SubstringFromXToY(0, num) + translation.Substring(num) + "</color>") : ("<color=#" + UIInfoTools.Instance.mainColor.ToHex() + ">" + translation.SubstringFromXToY(0, num) + "</color>" + translation.Substring(num))));
		uiOptionEntry.SetDescriptionText(translation);
	}
}
