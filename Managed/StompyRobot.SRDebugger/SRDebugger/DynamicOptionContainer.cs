using System;
using System.Collections.Generic;

namespace SRDebugger;

public sealed class DynamicOptionContainer : IOptionContainer
{
	private readonly List<OptionDefinition> _options = new List<OptionDefinition>();

	private readonly IList<OptionDefinition> _optionsReadOnly;

	public IList<OptionDefinition> Options => _optionsReadOnly;

	public bool IsDynamic => true;

	public event Action<OptionDefinition> OptionAdded;

	public event Action<OptionDefinition> OptionRemoved;

	public DynamicOptionContainer()
	{
		_optionsReadOnly = _options.AsReadOnly();
	}

	public void AddOption(OptionDefinition option)
	{
		_options.Add(option);
		if (this.OptionAdded != null)
		{
			this.OptionAdded(option);
		}
	}

	public bool RemoveOption(OptionDefinition option)
	{
		if (_options.Remove(option))
		{
			if (this.OptionRemoved != null)
			{
				this.OptionRemoved(option);
			}
			return true;
		}
		return false;
	}

	IEnumerable<OptionDefinition> IOptionContainer.GetOptions()
	{
		return _options;
	}
}
