using UnityEngine;

namespace Script.GUI.Popups;

public class LongPressHandler : LongPressHandlerBase
{
	[SerializeField]
	private KeyAction _keyActionToWait;

	protected override KeyAction KeyActionToWait => _keyActionToWait;
}
