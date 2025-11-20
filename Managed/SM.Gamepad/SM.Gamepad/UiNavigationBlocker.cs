namespace SM.Gamepad;

public class UiNavigationBlocker
{
	public string Tag { get; } = "untuged";

	public UiNavigationBlocker(string blockerTag)
	{
		Tag = blockerTag;
	}
}
