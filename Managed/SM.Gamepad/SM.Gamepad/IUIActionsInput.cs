using System;

namespace SM.Gamepad;

public interface IUIActionsInput
{
	bool GamePadInUse { get; }

	event Action<UIActionBaseEventData> MoveSelectionEvent;
}
