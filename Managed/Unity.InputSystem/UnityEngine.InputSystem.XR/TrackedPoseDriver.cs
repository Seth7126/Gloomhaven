using System;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;

namespace UnityEngine.InputSystem.XR;

[Serializable]
[AddComponentMenu("XR/Tracked Pose Driver (Input System)")]
public class TrackedPoseDriver : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum TrackingType
	{
		RotationAndPosition,
		RotationOnly,
		PositionOnly
	}

	public enum UpdateType
	{
		UpdateAndBeforeRender,
		Update,
		BeforeRender
	}

	[SerializeField]
	private TrackingType m_TrackingType;

	[SerializeField]
	private UpdateType m_UpdateType;

	[SerializeField]
	private InputActionProperty m_PositionInput;

	[SerializeField]
	private InputActionProperty m_RotationInput;

	private Vector3 m_CurrentPosition = Vector3.zero;

	private Quaternion m_CurrentRotation = Quaternion.identity;

	private bool m_RotationBound;

	private bool m_PositionBound;

	[Obsolete]
	[SerializeField]
	[HideInInspector]
	private InputAction m_PositionAction;

	[Obsolete]
	[SerializeField]
	[HideInInspector]
	private InputAction m_RotationAction;

	[SerializeField]
	[HideInInspector]
	private bool m_HasMigratedActions;

	public TrackingType trackingType
	{
		get
		{
			return m_TrackingType;
		}
		set
		{
			m_TrackingType = value;
		}
	}

	public UpdateType updateType
	{
		get
		{
			return m_UpdateType;
		}
		set
		{
			m_UpdateType = value;
		}
	}

	public InputActionProperty positionInput
	{
		get
		{
			return m_PositionInput;
		}
		set
		{
			if (Application.isPlaying)
			{
				UnbindPosition();
			}
			m_PositionInput = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				BindPosition();
			}
		}
	}

	public InputActionProperty rotationInput
	{
		get
		{
			return m_RotationInput;
		}
		set
		{
			if (Application.isPlaying)
			{
				UnbindRotation();
			}
			m_RotationInput = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				BindRotation();
			}
		}
	}

	public InputAction positionAction
	{
		get
		{
			return m_PositionInput.action;
		}
		set
		{
			positionInput = new InputActionProperty(value);
		}
	}

	public InputAction rotationAction
	{
		get
		{
			return m_RotationInput.action;
		}
		set
		{
			rotationInput = new InputActionProperty(value);
		}
	}

	private void BindActions()
	{
		BindPosition();
		BindRotation();
	}

	private void BindPosition()
	{
		if (m_PositionBound)
		{
			return;
		}
		InputAction action = m_PositionInput.action;
		if (action != null)
		{
			action.performed += OnPositionPerformed;
			action.canceled += OnPositionCanceled;
			m_PositionBound = true;
			if (m_PositionInput.reference == null)
			{
				action.Rename(base.gameObject.name + " - TPD - Position");
				action.Enable();
			}
		}
	}

	private void BindRotation()
	{
		if (m_RotationBound)
		{
			return;
		}
		InputAction action = m_RotationInput.action;
		if (action != null)
		{
			action.performed += OnRotationPerformed;
			action.canceled += OnRotationCanceled;
			m_RotationBound = true;
			if (m_RotationInput.reference == null)
			{
				action.Rename(base.gameObject.name + " - TPD - Rotation");
				action.Enable();
			}
		}
	}

	private void UnbindActions()
	{
		UnbindPosition();
		UnbindRotation();
	}

	private void UnbindPosition()
	{
		if (!m_PositionBound)
		{
			return;
		}
		InputAction action = m_PositionInput.action;
		if (action != null)
		{
			if (m_PositionInput.reference == null)
			{
				action.Disable();
			}
			action.performed -= OnPositionPerformed;
			action.canceled -= OnPositionCanceled;
			m_PositionBound = false;
		}
	}

	private void UnbindRotation()
	{
		if (!m_RotationBound)
		{
			return;
		}
		InputAction action = m_RotationInput.action;
		if (action != null)
		{
			if (m_RotationInput.reference == null)
			{
				action.Disable();
			}
			action.performed -= OnRotationPerformed;
			action.canceled -= OnRotationCanceled;
			m_RotationBound = false;
		}
	}

	private void OnPositionPerformed(InputAction.CallbackContext context)
	{
		m_CurrentPosition = context.ReadValue<Vector3>();
	}

	private void OnPositionCanceled(InputAction.CallbackContext context)
	{
		m_CurrentPosition = Vector3.zero;
	}

	private void OnRotationPerformed(InputAction.CallbackContext context)
	{
		m_CurrentRotation = context.ReadValue<Quaternion>();
	}

	private void OnRotationCanceled(InputAction.CallbackContext context)
	{
		m_CurrentRotation = Quaternion.identity;
	}

	protected virtual void Awake()
	{
		if (HasStereoCamera())
		{
			XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), disabled: true);
		}
	}

	protected void OnEnable()
	{
		InputSystem.onAfterUpdate += UpdateCallback;
		BindActions();
	}

	protected void OnDisable()
	{
		UnbindActions();
		InputSystem.onAfterUpdate -= UpdateCallback;
	}

	protected virtual void OnDestroy()
	{
		if (HasStereoCamera())
		{
			XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), disabled: false);
		}
	}

	protected void UpdateCallback()
	{
		if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
		{
			OnBeforeRender();
		}
		else
		{
			OnUpdate();
		}
	}

	protected virtual void OnUpdate()
	{
		if (m_UpdateType == UpdateType.Update || m_UpdateType == UpdateType.UpdateAndBeforeRender)
		{
			PerformUpdate();
		}
	}

	protected virtual void OnBeforeRender()
	{
		if (m_UpdateType == UpdateType.BeforeRender || m_UpdateType == UpdateType.UpdateAndBeforeRender)
		{
			PerformUpdate();
		}
	}

	protected virtual void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
	{
		if (m_TrackingType == TrackingType.RotationAndPosition || m_TrackingType == TrackingType.RotationOnly)
		{
			base.transform.localRotation = newRotation;
		}
		if (m_TrackingType == TrackingType.RotationAndPosition || m_TrackingType == TrackingType.PositionOnly)
		{
			base.transform.localPosition = newPosition;
		}
	}

	private bool HasStereoCamera()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			return component.stereoEnabled;
		}
		return false;
	}

	protected virtual void PerformUpdate()
	{
		SetLocalTransform(m_CurrentPosition, m_CurrentRotation);
	}

	protected void Reset()
	{
		m_HasMigratedActions = true;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (!m_HasMigratedActions)
		{
			m_PositionInput = new InputActionProperty(m_PositionAction);
			m_RotationInput = new InputActionProperty(m_RotationAction);
			m_HasMigratedActions = true;
		}
	}
}
