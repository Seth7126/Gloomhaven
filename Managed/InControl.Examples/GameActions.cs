using InControl;

public class GameActions : PlayerActionSet
{
	public readonly PlayerAction Up;

	public readonly PlayerAction Down;

	public readonly PlayerAction Left;

	public readonly PlayerAction Right;

	public readonly PlayerTwoAxisAction Move;

	public readonly PlayerAction Attack;

	public readonly PlayerAction Defend;

	public GameActions()
	{
		Up = CreatePlayerAction("Move Up");
		Down = CreatePlayerAction("Move Down");
		Left = CreatePlayerAction("Move Left");
		Right = CreatePlayerAction("Move Right");
		Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
		Attack = CreatePlayerAction("Attack");
		Defend = CreatePlayerAction("Defend");
	}

	public static GameActions CreateWithDefaultBindings()
	{
		GameActions gameActions = new GameActions();
		gameActions.Up.AddDefaultBinding(Key.UpArrow);
		gameActions.Down.AddDefaultBinding(Key.DownArrow);
		gameActions.Left.AddDefaultBinding(Key.LeftArrow);
		gameActions.Right.AddDefaultBinding(Key.RightArrow);
		gameActions.Attack.AddDefaultBinding(Key.Space);
		gameActions.Defend.AddDefaultBinding(Key.LeftAlt);
		return gameActions;
	}
}
