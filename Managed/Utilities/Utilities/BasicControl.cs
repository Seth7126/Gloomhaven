using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Utilities;

public class BasicControl : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct GamepadActions
	{
		private BasicControl m_Wrapper;

		public InputAction RightStickX => m_Wrapper.m_Gamepad_RightStickX;

		public InputAction RightStickY => m_Wrapper.m_Gamepad_RightStickY;

		public InputAction LeftStickX => m_Wrapper.m_Gamepad_LeftStickX;

		public InputAction LeftStickY => m_Wrapper.m_Gamepad_LeftStickY;

		public bool enabled => Get().enabled;

		public GamepadActions(BasicControl wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Gamepad;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(GamepadActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IGamepadActions instance)
		{
			if (m_Wrapper.m_GamepadActionsCallbackInterface != null)
			{
				RightStickX.started -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickX;
				RightStickX.performed -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickX;
				RightStickX.canceled -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickX;
				RightStickY.started -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickY;
				RightStickY.performed -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickY;
				RightStickY.canceled -= m_Wrapper.m_GamepadActionsCallbackInterface.OnRightStickY;
				LeftStickX.started -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickX;
				LeftStickX.performed -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickX;
				LeftStickX.canceled -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickX;
				LeftStickY.started -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickY;
				LeftStickY.performed -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickY;
				LeftStickY.canceled -= m_Wrapper.m_GamepadActionsCallbackInterface.OnLeftStickY;
			}
			m_Wrapper.m_GamepadActionsCallbackInterface = instance;
			if (instance != null)
			{
				RightStickX.started += instance.OnRightStickX;
				RightStickX.performed += instance.OnRightStickX;
				RightStickX.canceled += instance.OnRightStickX;
				RightStickY.started += instance.OnRightStickY;
				RightStickY.performed += instance.OnRightStickY;
				RightStickY.canceled += instance.OnRightStickY;
				LeftStickX.started += instance.OnLeftStickX;
				LeftStickX.performed += instance.OnLeftStickX;
				LeftStickX.canceled += instance.OnLeftStickX;
				LeftStickY.started += instance.OnLeftStickY;
				LeftStickY.performed += instance.OnLeftStickY;
				LeftStickY.canceled += instance.OnLeftStickY;
			}
		}
	}

	public struct MouseActions
	{
		private BasicControl m_Wrapper;

		public InputAction MoveX => m_Wrapper.m_Mouse_MoveX;

		public InputAction MoveY => m_Wrapper.m_Mouse_MoveY;

		public InputAction Mouse => m_Wrapper.m_Mouse_Mouse;

		public InputAction Scroll => m_Wrapper.m_Mouse_Scroll;

		public InputAction ScrollVector2 => m_Wrapper.m_Mouse_ScrollVector2;

		public InputAction MoveXRaw => m_Wrapper.m_Mouse_MoveXRaw;

		public InputAction MoveYRaw => m_Wrapper.m_Mouse_MoveYRaw;

		public bool enabled => Get().enabled;

		public MouseActions(BasicControl wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Mouse;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(MouseActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IMouseActions instance)
		{
			if (m_Wrapper.m_MouseActionsCallbackInterface != null)
			{
				MoveX.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveX;
				MoveX.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveX;
				MoveX.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveX;
				MoveY.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveY;
				MoveY.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveY;
				MoveY.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveY;
				Mouse.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnMouse;
				Mouse.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnMouse;
				Mouse.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnMouse;
				Scroll.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnScroll;
				Scroll.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnScroll;
				Scroll.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnScroll;
				ScrollVector2.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnScrollVector2;
				ScrollVector2.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnScrollVector2;
				ScrollVector2.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnScrollVector2;
				MoveXRaw.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveXRaw;
				MoveXRaw.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveXRaw;
				MoveXRaw.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveXRaw;
				MoveYRaw.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveYRaw;
				MoveYRaw.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveYRaw;
				MoveYRaw.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnMoveYRaw;
			}
			m_Wrapper.m_MouseActionsCallbackInterface = instance;
			if (instance != null)
			{
				MoveX.started += instance.OnMoveX;
				MoveX.performed += instance.OnMoveX;
				MoveX.canceled += instance.OnMoveX;
				MoveY.started += instance.OnMoveY;
				MoveY.performed += instance.OnMoveY;
				MoveY.canceled += instance.OnMoveY;
				Mouse.started += instance.OnMouse;
				Mouse.performed += instance.OnMouse;
				Mouse.canceled += instance.OnMouse;
				Scroll.started += instance.OnScroll;
				Scroll.performed += instance.OnScroll;
				Scroll.canceled += instance.OnScroll;
				ScrollVector2.started += instance.OnScrollVector2;
				ScrollVector2.performed += instance.OnScrollVector2;
				ScrollVector2.canceled += instance.OnScrollVector2;
				MoveXRaw.started += instance.OnMoveXRaw;
				MoveXRaw.performed += instance.OnMoveXRaw;
				MoveXRaw.canceled += instance.OnMoveXRaw;
				MoveYRaw.started += instance.OnMoveYRaw;
				MoveYRaw.performed += instance.OnMoveYRaw;
				MoveYRaw.canceled += instance.OnMoveYRaw;
			}
		}
	}

	public struct TouchpadActions
	{
		private BasicControl m_Wrapper;

		public InputAction Touch0 => m_Wrapper.m_Touchpad_Touch0;

		public InputAction Touch1 => m_Wrapper.m_Touchpad_Touch1;

		public InputAction Touch2 => m_Wrapper.m_Touchpad_Touch2;

		public bool enabled => Get().enabled;

		public TouchpadActions(BasicControl wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Touchpad;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(TouchpadActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(ITouchpadActions instance)
		{
			if (m_Wrapper.m_TouchpadActionsCallbackInterface != null)
			{
				Touch0.started -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch0;
				Touch0.performed -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch0;
				Touch0.canceled -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch0;
				Touch1.started -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch1;
				Touch1.performed -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch1;
				Touch1.canceled -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch1;
				Touch2.started -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch2;
				Touch2.performed -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch2;
				Touch2.canceled -= m_Wrapper.m_TouchpadActionsCallbackInterface.OnTouch2;
			}
			m_Wrapper.m_TouchpadActionsCallbackInterface = instance;
			if (instance != null)
			{
				Touch0.started += instance.OnTouch0;
				Touch0.performed += instance.OnTouch0;
				Touch0.canceled += instance.OnTouch0;
				Touch1.started += instance.OnTouch1;
				Touch1.performed += instance.OnTouch1;
				Touch1.canceled += instance.OnTouch1;
				Touch2.started += instance.OnTouch2;
				Touch2.performed += instance.OnTouch2;
				Touch2.canceled += instance.OnTouch2;
			}
		}
	}

	public interface IGamepadActions
	{
		void OnRightStickX(InputAction.CallbackContext context);

		void OnRightStickY(InputAction.CallbackContext context);

		void OnLeftStickX(InputAction.CallbackContext context);

		void OnLeftStickY(InputAction.CallbackContext context);
	}

	public interface IMouseActions
	{
		void OnMoveX(InputAction.CallbackContext context);

		void OnMoveY(InputAction.CallbackContext context);

		void OnMouse(InputAction.CallbackContext context);

		void OnScroll(InputAction.CallbackContext context);

		void OnScrollVector2(InputAction.CallbackContext context);

		void OnMoveXRaw(InputAction.CallbackContext context);

		void OnMoveYRaw(InputAction.CallbackContext context);
	}

	public interface ITouchpadActions
	{
		void OnTouch0(InputAction.CallbackContext context);

		void OnTouch1(InputAction.CallbackContext context);

		void OnTouch2(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Gamepad;

	private IGamepadActions m_GamepadActionsCallbackInterface;

	private readonly InputAction m_Gamepad_RightStickX;

	private readonly InputAction m_Gamepad_RightStickY;

	private readonly InputAction m_Gamepad_LeftStickX;

	private readonly InputAction m_Gamepad_LeftStickY;

	private readonly InputActionMap m_Mouse;

	private IMouseActions m_MouseActionsCallbackInterface;

	private readonly InputAction m_Mouse_MoveX;

	private readonly InputAction m_Mouse_MoveY;

	private readonly InputAction m_Mouse_Mouse;

	private readonly InputAction m_Mouse_Scroll;

	private readonly InputAction m_Mouse_ScrollVector2;

	private readonly InputAction m_Mouse_MoveXRaw;

	private readonly InputAction m_Mouse_MoveYRaw;

	private readonly InputActionMap m_Touchpad;

	private ITouchpadActions m_TouchpadActionsCallbackInterface;

	private readonly InputAction m_Touchpad_Touch0;

	private readonly InputAction m_Touchpad_Touch1;

	private readonly InputAction m_Touchpad_Touch2;

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

	public GamepadActions Gamepad => new GamepadActions(this);

	public MouseActions Mouse => new MouseActions(this);

	public TouchpadActions Touchpad => new TouchpadActions(this);

	public BasicControl()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"BasicControl\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"id\": \"b2858f0b-1786-4ca4-bcec-cbc424c0bf05\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"RightStickX\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"5f7695d1-ae0f-478f-9fbe-749d82db2344\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"RightStickY\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"e87b42ea-2cef-4687-98a7-bf16742cb016\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"LeftStickX\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"97b769ec-487b-4ab4-b381-868c1f9457d4\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"LeftStickY\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"180e19ec-12d1-46f4-a9a2-b7bad4c52837\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c57a76f5-f963-4b62-ad92-f8e141c196ce\",\r\n                    \"path\": \"<Gamepad>/rightStick/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"RightStickX\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e1f7469d-2032-44f8-a785-ceddabfb3561\",\r\n                    \"path\": \"<Gamepad>/rightStick/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"RightStickY\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b75dbfd7-3176-48e5-abbf-f76d2d36c106\",\r\n                    \"path\": \"<Gamepad>/leftStick/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"LeftStickX\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7c435257-ee2f-494c-a945-01d6bc58fea8\",\r\n                    \"path\": \"<Gamepad>/leftStick/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"LeftStickY\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Mouse\",\r\n            \"id\": \"3be6db71-bd7e-4839-90e6-e658c5c863bb\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"MoveX\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"7f24165d-37ac-4f89-a8a0-0e30780da6d6\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MoveY\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"186f0ba5-a9d2-49d6-88ef-e50e46841e72\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Mouse\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"98bf5064-4cd4-4bdf-b55d-4ea51096da59\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Scroll\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"4030ed80-d23e-49b9-b820-cdf0069b8355\",\r\n                    \"expectedControlType\": \"Axis\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"ScrollVector2\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"5a92afb7-9366-4017-86a1-bc2c341d654f\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MoveXRaw\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"a078a442-8736-4c58-a168-99414bec7d72\",\r\n                    \"expectedControlType\": \"Analog\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"MoveYRaw\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"df9ef3c8-2e84-42b0-9534-fa37679268f3\",\r\n                    \"expectedControlType\": \"Analog\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"738a172a-482d-44ac-96f8-9f3e97ca0ced\",\r\n                    \"path\": \"<Mouse>/delta/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MoveX\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3e147d6b-3412-4c77-9b6e-2971b60f2c82\",\r\n                    \"path\": \"<Mouse>/delta/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MoveY\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"859ce37c-09d0-4bc4-912d-d4857b41d564\",\r\n                    \"path\": \"<Mouse>/delta\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Mouse\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a4a80fb8-91f8-4e60-b0e5-75c827c85f4a\",\r\n                    \"path\": \"<Mouse>/scroll/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Scroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d3cd36f5-66e2-48f8-9d0b-60003e186609\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ScrollVector2\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"49b17113-31dd-46d6-95c8-ce6d1f98148c\",\r\n                    \"path\": \"<Mouse>/delta/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MoveXRaw\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"483cfc26-8e65-4030-b849-893c3d2e16cd\",\r\n                    \"path\": \"<Mouse>/delta/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"MoveYRaw\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Touchpad\",\r\n            \"id\": \"46cb863d-23cf-438d-a05f-450d9b255afe\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Touch0\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"260671f1-f564-4b74-9591-8f4469c23676\",\r\n                    \"expectedControlType\": \"Touch\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Touch1\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"546817fb-b8c5-4b62-a707-708f2fddbf83\",\r\n                    \"expectedControlType\": \"Touch\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Touch2\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"9e494b25-70a5-487a-b06c-19586ba424aa\",\r\n                    \"expectedControlType\": \"Touch\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d01d43fa-9f04-4ac4-bc7a-f53f1a5b73ee\",\r\n                    \"path\": \"<Touchscreen>/touch0\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Touch0\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"de51d809-e878-4a25-b0df-e39373b4a453\",\r\n                    \"path\": \"<Touchscreen>/touch1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Touch1\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c0d3b44b-a1fd-4975-a3b3-ca96fb77e323\",\r\n                    \"path\": \"<Touchscreen>/touch2\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Touch2\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": []\r\n}");
		m_Gamepad = asset.FindActionMap("Gamepad", throwIfNotFound: true);
		m_Gamepad_RightStickX = m_Gamepad.FindAction("RightStickX", throwIfNotFound: true);
		m_Gamepad_RightStickY = m_Gamepad.FindAction("RightStickY", throwIfNotFound: true);
		m_Gamepad_LeftStickX = m_Gamepad.FindAction("LeftStickX", throwIfNotFound: true);
		m_Gamepad_LeftStickY = m_Gamepad.FindAction("LeftStickY", throwIfNotFound: true);
		m_Mouse = asset.FindActionMap("Mouse", throwIfNotFound: true);
		m_Mouse_MoveX = m_Mouse.FindAction("MoveX", throwIfNotFound: true);
		m_Mouse_MoveY = m_Mouse.FindAction("MoveY", throwIfNotFound: true);
		m_Mouse_Mouse = m_Mouse.FindAction("Mouse", throwIfNotFound: true);
		m_Mouse_Scroll = m_Mouse.FindAction("Scroll", throwIfNotFound: true);
		m_Mouse_ScrollVector2 = m_Mouse.FindAction("ScrollVector2", throwIfNotFound: true);
		m_Mouse_MoveXRaw = m_Mouse.FindAction("MoveXRaw", throwIfNotFound: true);
		m_Mouse_MoveYRaw = m_Mouse.FindAction("MoveYRaw", throwIfNotFound: true);
		m_Touchpad = asset.FindActionMap("Touchpad", throwIfNotFound: true);
		m_Touchpad_Touch0 = m_Touchpad.FindAction("Touch0", throwIfNotFound: true);
		m_Touchpad_Touch1 = m_Touchpad.FindAction("Touch1", throwIfNotFound: true);
		m_Touchpad_Touch2 = m_Touchpad.FindAction("Touch2", throwIfNotFound: true);
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
