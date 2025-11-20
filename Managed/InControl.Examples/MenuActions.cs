using InControl;

public class MenuActions : PlayerActionSet
{
	public readonly PlayerAction Up;

	public readonly PlayerAction Down;

	public readonly PlayerAction Left;

	public readonly PlayerAction Right;

	public readonly PlayerTwoAxisAction Move;

	public readonly PlayerAction Submit;

	public readonly PlayerAction Cancel;

	public MenuActions()
	{
		Up = CreatePlayerAction("Move Up");
		Down = CreatePlayerAction("Move Down");
		Left = CreatePlayerAction("Move Left");
		Right = CreatePlayerAction("Move Right");
		Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
		Submit = CreatePlayerAction("Submit");
		Cancel = CreatePlayerAction("Cancel");
	}

	public static MenuActions CreateWithDefaultBindings()
	{
		MenuActions menuActions = new MenuActions();
		menuActions.Up.AddDefaultBinding(Key.UpArrow);
		menuActions.Down.AddDefaultBinding(Key.DownArrow);
		menuActions.Left.AddDefaultBinding(Key.LeftArrow);
		menuActions.Right.AddDefaultBinding(Key.RightArrow);
		menuActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
		menuActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
		menuActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
		menuActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
		menuActions.Left.AddDefaultBinding(InputControlType.DPadLeft);
		menuActions.Right.AddDefaultBinding(InputControlType.DPadRight);
		menuActions.Up.AddDefaultBinding(InputControlType.DPadUp);
		menuActions.Down.AddDefaultBinding(InputControlType.DPadDown);
		menuActions.Submit.AddDefaultBinding(Key.Return);
		menuActions.Submit.AddDefaultBinding(Key.Space);
		menuActions.Submit.AddDefaultBinding(InputControlType.Action1);
		menuActions.Cancel.AddDefaultBinding(Key.Escape);
		menuActions.Cancel.AddDefaultBinding(InputControlType.Action2);
		return menuActions;
	}
}
