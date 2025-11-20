using UnityEngine;

public interface ITutorial
{
	string TutorialID { get; }

	string TutorialFileName { get; }

	string TitleLocText { get; }

	string DescriptionLocText { get; }

	Sprite Image { get; }
}
