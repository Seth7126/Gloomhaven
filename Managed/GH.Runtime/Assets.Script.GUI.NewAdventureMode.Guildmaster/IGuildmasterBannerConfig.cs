using TMPro;

namespace Assets.Script.GUI.NewAdventureMode.Guildmaster;

public interface IGuildmasterBannerConfig
{
	float ContainerHeight { get; }

	bool InsertNewLineCharacter { get; }

	int NewLineIndex { get; }

	TextAlignmentOptions TextAlignmentOptions { get; }

	bool CustomTextAlignment { get; }
}
