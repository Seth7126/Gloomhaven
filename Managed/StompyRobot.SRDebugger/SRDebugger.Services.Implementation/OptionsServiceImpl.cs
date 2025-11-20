using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SRDebugger.Internal;
using SRF.Service;
using UnityEngine;

namespace SRDebugger.Services.Implementation;

[Service(typeof(IOptionsService))]
public sealed class OptionsServiceImpl : IOptionsService
{
	private class OptionContainerEventHandler : IDisposable
	{
		private readonly OptionsServiceImpl _service;

		private readonly IOptionContainer _container;

		public OptionContainerEventHandler(OptionsServiceImpl service, IOptionContainer container)
		{
			_container = container;
			_service = service;
			container.OptionAdded += ContainerOnOptionAdded;
			container.OptionRemoved += ContainerOnOptionRemoved;
		}

		private void ContainerOnOptionAdded(OptionDefinition obj)
		{
			_service.OptionsContainerOnOptionAdded(_container, obj);
		}

		private void ContainerOnOptionRemoved(OptionDefinition obj)
		{
			_service.OptionsContainerOnOptionRemoved(_container, obj);
		}

		public void Dispose()
		{
			_container.OptionAdded -= ContainerOnOptionAdded;
			_container.OptionRemoved -= ContainerOnOptionRemoved;
		}
	}

	private class ReflectionOptionContainer : IOptionContainer
	{
		private List<OptionDefinition> _options;

		private readonly object _target;

		public bool IsDynamic => false;

		private List<OptionDefinition> Options
		{
			get
			{
				if (_options == null)
				{
					_options = SRDebuggerUtil.ScanForOptions(_target);
				}
				return _options;
			}
		}

		public event Action<OptionDefinition> OptionAdded
		{
			add
			{
			}
			remove
			{
			}
		}

		public event Action<OptionDefinition> OptionRemoved
		{
			add
			{
			}
			remove
			{
			}
		}

		public IEnumerable<OptionDefinition> GetOptions()
		{
			return Options;
		}

		public ReflectionOptionContainer(object target)
		{
			_target = target;
		}

		protected bool Equals(ReflectionOptionContainer other)
		{
			return object.Equals(other._target, _target);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((ReflectionOptionContainer)obj);
		}

		public override int GetHashCode()
		{
			return _target.GetHashCode();
		}
	}

	private readonly Dictionary<IOptionContainer, List<OptionDefinition>> _optionContainerLookup = new Dictionary<IOptionContainer, List<OptionDefinition>>();

	private readonly Dictionary<IOptionContainer, OptionContainerEventHandler> _optionContainerEventHandlerLookup = new Dictionary<IOptionContainer, OptionContainerEventHandler>();

	private readonly List<OptionDefinition> _options = new List<OptionDefinition>();

	private readonly IList<OptionDefinition> _optionsReadonly;

	public ICollection<OptionDefinition> Options => _optionsReadonly;

	public event EventHandler OptionsUpdated;

	private void OptionsContainerOnOptionAdded(IOptionContainer container, OptionDefinition optionDefinition)
	{
		if (!_optionContainerLookup.TryGetValue(container, out var value))
		{
			Debug.LogWarning("[SRDebugger] Received event from unknown option container.");
			return;
		}
		if (value.Contains(optionDefinition))
		{
			Debug.LogWarning("[SRDebugger] Received option added event from option container, but option has already been added.");
			return;
		}
		value.Add(optionDefinition);
		_options.Add(optionDefinition);
		OnOptionsUpdated();
	}

	private void OptionsContainerOnOptionRemoved(IOptionContainer container, OptionDefinition optionDefinition)
	{
		if (!_optionContainerLookup.TryGetValue(container, out var value))
		{
			Debug.LogWarning("[SRDebugger] Received event from unknown option container.");
		}
		else if (value.Remove(optionDefinition))
		{
			_options.Remove(optionDefinition);
			OnOptionsUpdated();
		}
		else
		{
			Debug.LogWarning("[SRDebugger] Received option removed event from option container, but option does not exist.");
		}
	}

	public OptionsServiceImpl()
	{
		_optionsReadonly = new ReadOnlyCollection<OptionDefinition>(_options);
	}

	public void Scan(object obj)
	{
		AddContainer(obj);
	}

	public void AddContainer(object obj)
	{
		IOptionContainer optionContainer = (obj as IOptionContainer) ?? new ReflectionOptionContainer(obj);
		AddContainer(optionContainer);
	}

	public void AddContainer(IOptionContainer optionContainer)
	{
		if (_optionContainerLookup.ContainsKey(optionContainer))
		{
			throw new Exception("An options container should only be added once.");
		}
		List<OptionDefinition> list = new List<OptionDefinition>();
		list.AddRange(optionContainer.GetOptions());
		_optionContainerLookup.Add(optionContainer, list);
		if (optionContainer.IsDynamic)
		{
			OptionContainerEventHandler value = new OptionContainerEventHandler(this, optionContainer);
			_optionContainerEventHandlerLookup.Add(optionContainer, value);
		}
		if (list.Count > 0)
		{
			_options.AddRange(list);
			OnOptionsUpdated();
		}
	}

	public void RemoveContainer(object obj)
	{
		IOptionContainer optionContainer = (obj as IOptionContainer) ?? new ReflectionOptionContainer(obj);
		RemoveContainer(optionContainer);
	}

	public void RemoveContainer(IOptionContainer optionContainer)
	{
		if (!_optionContainerLookup.ContainsKey(optionContainer))
		{
			return;
		}
		bool flag = false;
		List<OptionDefinition> list = _optionContainerLookup[optionContainer];
		_optionContainerLookup.Remove(optionContainer);
		foreach (OptionDefinition item in list)
		{
			_options.Remove(item);
			flag = true;
		}
		if (_optionContainerEventHandlerLookup.TryGetValue(optionContainer, out var value))
		{
			value.Dispose();
			_optionContainerEventHandlerLookup.Remove(optionContainer);
		}
		if (flag)
		{
			OnOptionsUpdated();
		}
	}

	private void OnOptionsUpdated()
	{
		if (this.OptionsUpdated != null)
		{
			this.OptionsUpdated(this, EventArgs.Empty);
		}
	}
}
