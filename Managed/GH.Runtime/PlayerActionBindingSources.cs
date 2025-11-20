using System;
using System.Linq;
using InControl;

public class PlayerActionBindingSources : IDisposable
{
	private PlayerAction _action;

	private DeviceBindingSource _deviceBindingSource;

	public DeviceBindingSource DeviceBindingSource => _deviceBindingSource;

	public PlayerActionBindingSources(PlayerAction action)
	{
		_action = action;
		_action.OnBindingsChanged += OnBindingChanged;
		OnBindingChanged();
	}

	public void Dispose()
	{
		_action.OnBindingsChanged -= OnBindingChanged;
	}

	private void OnBindingChanged()
	{
		_deviceBindingSource = (DeviceBindingSource)_action.Bindings.FirstOrDefault((BindingSource it) => it is DeviceBindingSource);
	}
}
