using GLOOM.MainMenu;
using UnityEngine;

public class TitleModding : MonoBehaviour
{
	[SerializeField]
	private UIExportModdingManager _uiExportModdingManager;

	private void Start()
	{
		if (PlatformLayer.Modding.ModdingSupported && !SceneController.Instance.ModLoadingCompleted)
		{
			StartCoroutine(SceneController.Instance.LoadMods());
		}
	}

	private void OnEnable()
	{
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, ReturnToMenu);
	}

	private void OnDisable()
	{
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, ReturnToMenu);
	}

	private void ReturnToMenu()
	{
		if (!_uiExportModdingManager.Exporting)
		{
			base.gameObject.SetActive(value: false);
			MainMenuUIManager.Instance.mainMenu.OpenExtras();
		}
	}
}
