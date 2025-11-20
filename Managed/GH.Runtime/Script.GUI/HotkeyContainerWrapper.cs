using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class HotkeyContainerWrapper : MonoBehaviour
{
	[SerializeField]
	private HotkeyContainer _hotkeyContainer;

	public IHotkeyContainer HotkeyContainer => _hotkeyContainer;
}
