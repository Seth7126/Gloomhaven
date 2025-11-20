using System.Collections.Generic;
using SM.Gamepad;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RoomCameraButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private string _comboString;

	private int currentRoom = -1;

	private GamepadButtonsComboController Controller => Singleton<InputManager>.Instance.ButtonsComboController;

	private void Awake()
	{
		button.onClick.AddListener(OnClick);
		if (InputManager.GamePadInUse)
		{
			Controller.AddCombo(_comboString, ComboPressed);
		}
		Hide();
	}

	private void OnEnable()
	{
		Controller.SetEnabledCombo(_comboString, value: true);
	}

	private void OnDisable()
	{
		Controller.SetEnabledCombo(_comboString, value: false);
	}

	private void OnDestroy()
	{
		Controller.RemoveCombo(_comboString);
		button.onClick.RemoveListener(OnClick);
	}

	private void ComboPressed(Gamepad gamepad)
	{
		OnClick();
	}

	public void OnClick()
	{
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.CameraRoomButtonPressed));
		FocusOnRoom((currentRoom + 1) % ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count);
	}

	public void FocusOnRoom(int room)
	{
		if (currentRoom != room)
		{
			currentRoom = room;
			CameraController.s_CameraController.SetFocalPointGameObject(ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles[room][0].m_GameObject);
		}
	}

	private int FindEmptyRoom()
	{
		int num = Mathf.RoundToInt((float)Choreographer.s_Choreographer.ClientActorObjects.Count / (float)ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count);
		for (int i = 0; i < ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles.Count; i++)
		{
			List<CClientTile> list = ClientScenarioManager.s_ClientScenarioManager.PossibleStartingTiles[i];
			int num2 = 0;
			foreach (CClientTile item in list)
			{
				if (ScenarioManager.Scenario.FindPlayerAt(item.m_Tile.m_ArrayIndex) != null)
				{
					num2++;
				}
			}
			if (num2 < num)
			{
				return i;
			}
		}
		return -1;
	}

	public void FocusOnEmptyRoom()
	{
		int num = FindEmptyRoom();
		if (num != -1)
		{
			FocusOnRoom(num);
		}
	}

	public void Show()
	{
		currentRoom = -1;
		button.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		button.gameObject.SetActive(value: false);
	}
}
