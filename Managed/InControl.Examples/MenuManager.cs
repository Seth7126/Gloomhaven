using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
	public BindingsMenu BindingsMenu;

	private static MenuManager instance;

	private static bool applicationIsQuitting;

	private static readonly object lockObject = new object();

	public MenuActions MenuActions { get; private set; }

	public GameActions GameActions { get; private set; }

	public static MenuManager Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				return null;
			}
			lock (lockObject)
			{
				if (instance == null)
				{
					instance = Object.FindObjectOfType(typeof(MenuManager)) as MenuManager;
				}
				return instance;
			}
		}
	}

	private void Awake()
	{
		InputManager.OnSetup -= SetupInput;
		InputManager.OnSetup += SetupInput;
		InputManager.OnReset -= ResetInput;
		InputManager.OnReset += ResetInput;
	}

	private void SetupInput()
	{
		MenuActions = MenuActions.CreateWithDefaultBindings();
		GameActions = GameActions.CreateWithDefaultBindings();
		InControlInputModule component = EventSystem.current.GetComponent<InControlInputModule>();
		if (component != null)
		{
			component.AddMoveAction(MenuActions.Move, MoveActionSourceType.DPad);
			component.SubmitAction = MenuActions.Submit;
			component.CancelAction = MenuActions.Cancel;
		}
		BindingsMenu.Show(GameActions);
	}

	private void ResetInput()
	{
		if (MenuActions != null)
		{
			MenuActions.Destroy();
			MenuActions = null;
		}
		if (GameActions != null)
		{
			GameActions.Destroy();
			GameActions = null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		instance = null;
		applicationIsQuitting = false;
	}

	private void OnApplicationQuit()
	{
		applicationIsQuitting = true;
		instance = null;
	}
}
