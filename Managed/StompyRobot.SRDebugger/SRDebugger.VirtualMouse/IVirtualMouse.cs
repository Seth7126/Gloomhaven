using UnityEngine;

namespace SRDebugger.VirtualMouse;

public interface IVirtualMouse
{
	bool TryStart();

	void Stop();

	void SetPosition(Vector2 position);

	void SetScrollWheel(Vector2 scrollWheel);

	void Press();

	void Release();
}
