using System;
using System.Reflection;

namespace MonoMod.RuntimeDetour;

public interface IDetourRuntimePlatform
{
	IntPtr GetNativeStart(MethodBase method);

	MethodInfo CreateCopy(MethodBase method);

	bool TryCreateCopy(MethodBase method, out MethodInfo dm);

	void Pin(MethodBase method);

	void Unpin(MethodBase method);

	MethodBase GetDetourTarget(MethodBase from, MethodBase to);
}
