using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class GameController : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct GameplayActions
	{
		private GameController m_Wrapper;

		public InputAction ControllerMove => m_Wrapper.m_Gameplay_ControllerMove;

		public InputAction MouseMove => m_Wrapper.m_Gameplay_MouseMove;

		public InputAction MousePosition => m_Wrapper.m_Gameplay_MousePosition;

		public InputAction Escape => m_Wrapper.m_Gameplay_Escape;

		public InputAction ScrollMove => m_Wrapper.m_Gameplay_ScrollMove;

		public InputAction Back => m_Wrapper.m_Gameplay_Back;

		public InputAction LeftTrigger => m_Wrapper.m_Gameplay_LeftTrigger;

		public InputAction RightTrigger => m_Wrapper.m_Gameplay_RightTrigger;

		public InputAction MouseDelta => m_Wrapper.m_Gameplay_MouseDelta;

		public InputAction LeftShoulder => m_Wrapper.m_Gameplay_LeftShoulder;

		public InputAction RightShoulder => m_Wrapper.m_Gameplay_RightShoulder;

		public InputAction DPad => m_Wrapper.m_Gameplay_DPad;

		public InputAction SelectKey => m_Wrapper.m_Gameplay_SelectKey;

		public InputAction AButton => m_Wrapper.m_Gameplay_AButton;

		public bool enabled => Get().enabled;

		public GameplayActions(GameController wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Gameplay;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(GameplayActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IGameplayActions instance)
		{
			if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
			{
				ControllerMove.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnControllerMove;
				ControllerMove.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnControllerMove;
				ControllerMove.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnControllerMove;
				MouseMove.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseMove;
				MouseMove.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseMove;
				MouseMove.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseMove;
				MousePosition.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMousePosition;
				MousePosition.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMousePosition;
				MousePosition.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMousePosition;
				Escape.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnEscape;
				Escape.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnEscape;
				Escape.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnEscape;
				ScrollMove.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnScrollMove;
				ScrollMove.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnScrollMove;
				ScrollMove.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnScrollMove;
				Back.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
				Back.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
				Back.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
				LeftTrigger.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftTrigger;
				LeftTrigger.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftTrigger;
				LeftTrigger.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftTrigger;
				RightTrigger.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightTrigger;
				RightTrigger.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightTrigger;
				RightTrigger.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightTrigger;
				MouseDelta.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseDelta;
				MouseDelta.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseDelta;
				MouseDelta.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMouseDelta;
				LeftShoulder.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftShoulder;
				LeftShoulder.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftShoulder;
				LeftShoulder.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnLeftShoulder;
				RightShoulder.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightShoulder;
				RightShoulder.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightShoulder;
				RightShoulder.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnRightShoulder;
				DPad.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDPad;
				DPad.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDPad;
				DPad.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDPad;
				SelectKey.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnSelectKey;
				SelectKey.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnSelectKey;
				SelectKey.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnSelectKey;
				AButton.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAButton;
				AButton.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAButton;
				AButton.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnAButton;
			}
			m_Wrapper.m_GameplayActionsCallbackInterface = instance;
			if (instance != null)
			{
				ControllerMove.started += instance.OnControllerMove;
				ControllerMove.performed += instance.OnControllerMove;
				ControllerMove.canceled += instance.OnControllerMove;
				MouseMove.started += instance.OnMouseMove;
				MouseMove.performed += instance.OnMouseMove;
				MouseMove.canceled += instance.OnMouseMove;
				MousePosition.started += instance.OnMousePosition;
				MousePosition.performed += instance.OnMousePosition;
				MousePosition.canceled += instance.OnMousePosition;
				Escape.started += instance.OnEscape;
				Escape.performed += instance.OnEscape;
				Escape.canceled += instance.OnEscape;
				ScrollMove.started += instance.OnScrollMove;
				ScrollMove.performed += instance.OnScrollMove;
				ScrollMove.canceled += instance.OnScrollMove;
				Back.started += instance.OnBack;
				Back.performed += instance.OnBack;
				Back.canceled += instance.OnBack;
				LeftTrigger.started += instance.OnLeftTrigger;
				LeftTrigger.performed += instance.OnLeftTrigger;
				LeftTrigger.canceled += instance.OnLeftTrigger;
				RightTrigger.started += instance.OnRightTrigger;
				RightTrigger.performed += instance.OnRightTrigger;
				RightTrigger.canceled += instance.OnRightTrigger;
				MouseDelta.started += instance.OnMouseDelta;
				MouseDelta.performed += instance.OnMouseDelta;
				MouseDelta.canceled += instance.OnMouseDelta;
				LeftShoulder.started += instance.OnLeftShoulder;
				LeftShoulder.performed += instance.OnLeftShoulder;
				LeftShoulder.canceled += instance.OnLeftShoulder;
				RightShoulder.started += instance.OnRightShoulder;
				RightShoulder.performed += instance.OnRightShoulder;
				RightShoulder.canceled += instance.OnRightShoulder;
				DPad.started += instance.OnDPad;
				DPad.performed += instance.OnDPad;
				DPad.canceled += instance.OnDPad;
				SelectKey.started += instance.OnSelectKey;
				SelectKey.performed += instance.OnSelectKey;
				SelectKey.canceled += instance.OnSelectKey;
				AButton.started += instance.OnAButton;
				AButton.performed += instance.OnAButton;
				AButton.canceled += instance.OnAButton;
			}
		}
	}

	public interface IGameplayActions
	{
		void OnControllerMove(InputAction.CallbackContext context);

		void OnMouseMove(InputAction.CallbackContext context);

		void OnMousePosition(InputAction.CallbackContext context);

		void OnEscape(InputAction.CallbackContext context);

		void OnScrollMove(InputAction.CallbackContext context);

		void OnBack(InputAction.CallbackContext context);

		void OnLeftTrigger(InputAction.CallbackContext context);

		void OnRightTrigger(InputAction.CallbackContext context);

		void OnMouseDelta(InputAction.CallbackContext context);

		void OnLeftShoulder(InputAction.CallbackContext context);

		void OnRightShoulder(InputAction.CallbackContext context);

		void OnDPad(InputAction.CallbackContext context);

		void OnSelectKey(InputAction.CallbackContext context);

		void OnAButton(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Gameplay;

	private IGameplayActions m_GameplayActionsCallbackInterface;

	private readonly InputAction m_Gameplay_ControllerMove;

	private readonly InputAction m_Gameplay_MouseMove;

	private readonly InputAction m_Gameplay_MousePosition;

	private readonly InputAction m_Gameplay_Escape;

	private readonly InputAction m_Gameplay_ScrollMove;

	private readonly InputAction m_Gameplay_Back;

	private readonly InputAction m_Gameplay_LeftTrigger;

	private readonly InputAction m_Gameplay_RightTrigger;

	private readonly InputAction m_Gameplay_MouseDelta;

	private readonly InputAction m_Gameplay_LeftShoulder;

	private readonly InputAction m_Gameplay_RightShoulder;

	private readonly InputAction m_Gameplay_DPad;

	private readonly InputAction m_Gameplay_SelectKey;

	private readonly InputAction m_Gameplay_AButton;

	private int m_NewcontrolschemeSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public GameplayActions Gameplay => new GameplayActions(this);

	public InputControlScheme NewcontrolschemeScheme
	{
		get
		{
			if (m_NewcontrolschemeSchemeIndex == -1)
			{
				m_NewcontrolschemeSchemeIndex = asset.FindControlSchemeIndex("New control scheme");
			}
			return asset.controlSchemes[m_NewcontrolschemeSchemeIndex];
		}
	}

	public GameController()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"GameController\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Gameplay\",\r\n            \"id\": \"e2f1c7fc-0bd7-4a1f-9705-770c4148bbcd\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"ControllerMove\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"2763678f-c876-4793-b709-fb1955699bf2\",\r\n                    \"expectedControlType\": \"Touch\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MouseMove\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"dd1a080f-257c-41a5-a5d9-b49022fd5752\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MousePosition\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"084ab038-2d4e-4253-8761-30d12dc004d0\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Escape\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"97abf9f2-fe8a-489a-8741-dc3d79a2460b\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ScrollMove\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"9daf7740-54cf-4687-8108-41fe94ec31fa\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Back\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"22fcc938-7ada-4098-9382-307169917419\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"LeftTrigger\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"721af04f-fe8f-41de-8472-3ca49497cc1c\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightTrigger\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c3ad6125-699c-4e62-a8a8-a41753883bb7\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MouseDelta\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"75a8e4bb-384a-42c3-b650-b98f8b93e321\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"LeftShoulder\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"23232ab2-012d-467d-b280-9c6718ec7804\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightShoulder\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d63a816b-1f4a-4c50-8056-be8dd66074ab\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"DPad\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"bf4bb691-1d20-4767-a46b-fd6dc917db2c\",\r\n                    \"expectedControlType\": \"Digital\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"SelectKey\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c554ed94-f9c7-4c2a-a969-bbb6fc2354f5\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"AButton\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"cdd62cc8-35d7-4fe3-8765-4b07469a2db7\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3559e3ee-d535-415e-a11b-c74d4494b92d\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Escape\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6593394c-90e0-4c71-b254-d60875ad9666\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"AxisDeadzone(min=0.5)\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ScrollMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b92f47d0-e6f0-4472-8116-f2900b63c604\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Back\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"91aa6944-f423-40c9-86f9-2cbefb347683\",\r\n                    \"path\": \"<Gamepad>/leftTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"LeftTrigger\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bd72e9b8-a2c7-49ce-91a2-9e8afa8e27ad\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MouseDelta\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"513dc3f1-dc85-4d39-958b-fdaa06895576\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"LeftShoulder\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"97fc03b4-213c-454e-85c7-5bc02b2286b2\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"DPad\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"54e3dffb-1280-49a4-8410-6f1b0c5b1737\",\r\n                    \"path\": \"<Gamepad>/select\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"SelectKey\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"48784321-d20c-4ac0-8297-2262d59c9527\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"AButton\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cc7e27dc-e996-4eb5-925f-e65dc4bb5438\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"RightTrigger\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ed06eb80-5a24-43ba-a274-9e4064c615f7\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone(min=0.5)\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ControllerMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"94c5b8f2-f21d-4405-8f22-8e7de819baea\",\r\n                    \"path\": \"<VirtualMouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MouseMove\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b0790d32-0fa5-4012-932a-312e50f0880a\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MousePosition\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8ac08a9d-e38c-46b1-866c-73a16ac82c80\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"RightShoulder\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"New control scheme\",\r\n            \"bindingGroup\": \"New control scheme\",\r\n            \"devices\": []\r\n        }\r\n    ]\r\n}");
		m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
		m_Gameplay_ControllerMove = m_Gameplay.FindAction("ControllerMove", throwIfNotFound: true);
		m_Gameplay_MouseMove = m_Gameplay.FindAction("MouseMove", throwIfNotFound: true);
		m_Gameplay_MousePosition = m_Gameplay.FindAction("MousePosition", throwIfNotFound: true);
		m_Gameplay_Escape = m_Gameplay.FindAction("Escape", throwIfNotFound: true);
		m_Gameplay_ScrollMove = m_Gameplay.FindAction("ScrollMove", throwIfNotFound: true);
		m_Gameplay_Back = m_Gameplay.FindAction("Back", throwIfNotFound: true);
		m_Gameplay_LeftTrigger = m_Gameplay.FindAction("LeftTrigger", throwIfNotFound: true);
		m_Gameplay_RightTrigger = m_Gameplay.FindAction("RightTrigger", throwIfNotFound: true);
		m_Gameplay_MouseDelta = m_Gameplay.FindAction("MouseDelta", throwIfNotFound: true);
		m_Gameplay_LeftShoulder = m_Gameplay.FindAction("LeftShoulder", throwIfNotFound: true);
		m_Gameplay_RightShoulder = m_Gameplay.FindAction("RightShoulder", throwIfNotFound: true);
		m_Gameplay_DPad = m_Gameplay.FindAction("DPad", throwIfNotFound: true);
		m_Gameplay_SelectKey = m_Gameplay.FindAction("SelectKey", throwIfNotFound: true);
		m_Gameplay_AButton = m_Gameplay.FindAction("AButton", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
