using InControl;

public class DevicePlayerActionControlsProvider : PlayerActionControlsProvider<InputControlType>
{
	public DevicePlayerActionControlsProvider(PlayerActionControls playerActionControls)
		: base(playerActionControls)
	{
	}

	protected override bool TryGetControlFromBindings(PlayerActionBindingSources bindings, out InputControlType control)
	{
		if (bindings.DeviceBindingSource != null)
		{
			control = bindings.DeviceBindingSource.Control;
			return true;
		}
		control = InputControlType.None;
		return false;
	}
}
