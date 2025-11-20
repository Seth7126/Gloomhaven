using System;
using System.Collections.Generic;

namespace SRDebugger;

public interface IOptionContainer
{
	bool IsDynamic { get; }

	event Action<OptionDefinition> OptionAdded;

	event Action<OptionDefinition> OptionRemoved;

	IEnumerable<OptionDefinition> GetOptions();
}
