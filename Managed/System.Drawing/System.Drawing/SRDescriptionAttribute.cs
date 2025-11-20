using System.ComponentModel;

namespace System.Drawing;

[AttributeUsage(AttributeTargets.All)]
internal sealed class SRDescriptionAttribute : DescriptionAttribute
{
	private bool isReplaced;

	public override string Description
	{
		get
		{
			if (!isReplaced)
			{
				isReplaced = true;
				base.DescriptionValue = global::Locale.GetText(base.DescriptionValue);
			}
			return base.DescriptionValue;
		}
	}

	public SRDescriptionAttribute(string description)
		: base(description)
	{
	}
}
