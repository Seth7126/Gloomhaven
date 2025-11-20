using System;
using System.Collections.Generic;

namespace SRDebugger.Services;

public interface IOptionsService
{
	ICollection<OptionDefinition> Options { get; }

	event EventHandler OptionsUpdated;

	[Obsolete("Use IOptionsService.AddContainer instead.")]
	void Scan(object obj);

	void AddContainer(object obj);

	void AddContainer(IOptionContainer optionContainer);

	void RemoveContainer(object obj);

	void RemoveContainer(IOptionContainer optionContainer);
}
