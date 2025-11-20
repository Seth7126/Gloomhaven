using JetBrains.Annotations;

namespace SM.Gamepad;

[UsedImplicitly]
public class UIActionBaseEventData
{
	public UINavigationDirection UINavigationDirection { get; set; }

	public UINavigationSourceType UINavigationSourceType { get; set; }
}
