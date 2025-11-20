using System.Collections.Generic;

namespace MonoMod.RuntimeDetour;

public struct DetourConfig
{
	public bool ManualApply;

	public int Priority;

	public string ID;

	public IEnumerable<string> Before;

	public IEnumerable<string> After;
}
