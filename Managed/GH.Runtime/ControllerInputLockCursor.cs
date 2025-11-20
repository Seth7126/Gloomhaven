using UnityEngine.EventSystems;

public class ControllerInputLockCursor : ControllerInputElement
{
	private ControllerOverrideInput input;

	private void Awake()
	{
		input = base.gameObject.AddComponent<ControllerOverrideInput>();
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		EventSystem.current.currentInputModule.inputOverride = input;
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		EventSystem.current.currentInputModule.inputOverride = null;
	}
}
