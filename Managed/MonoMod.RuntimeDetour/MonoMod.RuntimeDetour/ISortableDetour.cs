using System;
using System.Collections.Generic;

namespace MonoMod.RuntimeDetour;

public interface ISortableDetour : IDetour, IDisposable
{
	uint GlobalIndex { get; }

	int Priority { get; set; }

	string ID { get; set; }

	IEnumerable<string> Before { get; set; }

	IEnumerable<string> After { get; set; }
}
