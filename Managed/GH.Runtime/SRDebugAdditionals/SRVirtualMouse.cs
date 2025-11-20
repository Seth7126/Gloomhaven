using SRDebugger.VirtualMouse;
using UnityEngine;
using Utilities;

namespace SRDebugAdditionals;

public class SRVirtualMouse : VirtualMouseUtilities, IVirtualMouse
{
	bool IVirtualMouse.TryStart()
	{
		return TryStart();
	}

	void IVirtualMouse.Stop()
	{
		Stop();
	}

	void IVirtualMouse.SetPosition(Vector2 position)
	{
		SetPosition(position);
	}

	void IVirtualMouse.SetScrollWheel(Vector2 scrollWheel)
	{
		SetScrollWheel(scrollWheel);
	}

	void IVirtualMouse.Press()
	{
		Press();
	}

	void IVirtualMouse.Release()
	{
		Release();
	}
}
