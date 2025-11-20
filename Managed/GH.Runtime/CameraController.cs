#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using InControl;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using Script.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Utilities;
using VolumetricFogAndMist;

public class CameraController : MonoBehaviour
{
	[Flags]
	public enum ECameraInput
	{
		None = 0,
		Move = 1,
		Zoom = 2,
		Rotate = 4
	}

	public static CameraController s_CameraController;

	public Camera m_Camera;

	public GameObject m_DebugMenu;

	public float m_CameraRotationSpeed = 180f;

	[Tooltip("The height in meters focus point lifted above the ground")]
	public float m_FocusPointHeight = 1.5f;

	[Range(0f, 1f)]
	public float m_FocusSpeed = 0.75f;

	public bool m_EnableEnemyForceRotation = true;

	public bool m_EnablePlayerForceRotation = true;

	public bool m_EnableRotateBack = true;

	public Bounds m_FocalBounds;

	public static bool s_Rotated;

	public static bool s_Translated;

	public static Plane s_GroundPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

	public static Vector3 s_InitialFocusPoint = new Vector3(0f, 0f, 0f);

	public static float s_InitialHorizontalAngle;

	private ECameraInput m_CameraInputDisabled;

	public float m_ZoomWheelScalar = 50f;

	public const float c_minFOV = 30f;

	public const float c_minFOVRestricted = 56.5f;

	public const float c_MaximumFOV = 75f;

	private const float c_ZoomInterpScalar = 10f;

	private const float c_FocalPointScalar = 5f;

	private const float c_HorizontalRotationScalar = 25f;

	private GameObject m_InitialCamera;

	private float m_InitialCameraHeight;

	private Vector3 m_FocalPoint = new Vector3(0f, 0f, 0f);

	private Vector3 m_oldTargetFocalPoint;

	private float m_DeltaAngle;

	private float m_OldLength;

	private float m_TargetZoom;

	private Vector3 m_TranslationStartMousePosOnPlane;

	private Vector3 m_TranslationStartTargetFocalPoint;

	private Vector3 m_CameraToFocalTargetDiff;

	private float m_CameraStartAngle;

	private bool m_RotationStarted;

	private bool m_TranslationStarted;

	private bool m_MapInputSource;

	private float m_CameraDefaultRadius;

	private float m_DefaultZoomFactor;

	private bool m_HasFocus = true;

	private float m_CameraGameHorizontalAngle;

	private Vector3 m_CameraLeftVector;

	private bool m_TranslationKeyPressed;

	private int m_ForceRotate;

	private bool m_ForceRotated;

	[Space]
	[Header("FreeCam Settings")]
	private float m_CameraSensitivity = 90f;

	[SerializeField]
	private float m_LeftJoySpeed = 5f;

	[SerializeField]
	private float m_RightJoySpeed = 70f;

	[SerializeField]
	private float m_ShiftJoySpeed = 5f;

	private float m_ClimbSpeed = 4f;

	private int m_CameraMoveSpeedTextID = 2;

	private float m_RotationX;

	private float m_RotationY;

	private bool m_IsFreeCamEnabled;

	private float m_LastRealTime;

	private Vector3 m_CurrentCameraPos;

	private Quaternion m_CurrentCameraRot;

	private bool m_HoldFreeCamPosition;

	private bool isRotatingLeft;

	private bool isRotatingRight;

	private float rotateSpeed = 4f;

	private int rotateSpeedTextID = 3;

	private float rotateDepth = 3f;

	private Vector3 rotateDirection;

	private Vector3 rotatePoint;

	private bool smoothRotating;

	private bool smoothRotatingRight;

	private float newSmoothRotateAngle;

	private float distanceFromTheTarget;

	private bool startComplete;

	private bool isInitialised;

	private RequestCounter _disableInputCounter;

	private IEnumerator m_CameraToProfileRoutine;

	private Coroutine m_CameraZoomFOVRoutine;

	public Action<bool> OnInputEnableChanged;

	private const string LOG_CAMERA_FORMAT = "<color=#A3DA77FF>[CAMERA]</color> {0}";

	private ICameraBehavior overriddenBehavior;

	private CameraTargetFocalFollowController _targetFocalFollowController;

	private BasicControl _basicControl;

	public Vector3 m_TargetFocalPoint { get; set; }

	public bool m_EnableCameraShake { get; set; }

	public Vector3 m_CameraShakePosition { get; set; }

	public Quaternion m_CameraShakeRotation { get; set; }

	public bool m_IsCameraCodeControlDisabled { get; set; }

	public float m_CameraMoveSpeed { get; set; }

	public float m_ZoomOutExtraHeight { get; set; }

	public float m_DefaultFOV { get; set; }

	public float m_MinimumFOV { get; set; }

	public float m_ExtraMinimumFOV { get; set; }

	public float m_ZoomFactor => CalculateZoomFactor(m_Camera.fieldOfView);

	public bool StartComplete => startComplete;

	public Vector3 FocusPoint => m_FocalPoint;

	public bool InputEnabled
	{
		get
		{
			if (_disableInputCounter.Empty)
			{
				return !_targetFocalFollowController.IsFollowingTarget;
			}
			return false;
		}
	}

	public bool ZoomInputEnabled
	{
		get
		{
			if (InputEnabled)
			{
				return !m_CameraInputDisabled.HasFlag(ECameraInput.Zoom);
			}
			return false;
		}
	}

	public bool RotateInputEnabled
	{
		get
		{
			if (InputEnabled && !m_CameraInputDisabled.HasFlag(ECameraInput.Rotate))
			{
				SaveData instance = SaveData.Instance;
				if ((object)instance == null)
				{
					return true;
				}
				return instance.Global?.CurrentGameState != EGameState.Map;
			}
			return false;
		}
	}

	public bool MoveInputEnabled
	{
		get
		{
			if (InputEnabled && !m_CameraInputDisabled.HasFlag(ECameraInput.Move))
			{
				return !m_EnableCameraShake;
			}
			return false;
		}
	}

	public bool SnappingEnabled => InputEnabled;

	public bool CameraFollowOn
	{
		get
		{
			if (Main.s_DevMode || Main.s_InternalRelease)
			{
				if (DebugMenu.DebugMenuNotNull)
				{
					return !DebugMenu.Instance.FollowCameraDisabled;
				}
				return true;
			}
			return true;
		}
	}

	public float Zoom => m_TargetZoom;

	public ICameraBehavior OverriddenBehavior => overriddenBehavior;

	public ECameraInput CameraInputDisabled
	{
		get
		{
			return m_CameraInputDisabled;
		}
		set
		{
			m_CameraInputDisabled = value;
		}
	}

	public float TotalMinimumFOV => m_MinimumFOV + m_ExtraMinimumFOV;

	[UsedImplicitly]
	private void Awake()
	{
		s_CameraController = this;
		_basicControl = new BasicControl();
		_disableInputCounter = new RequestCounter(OnInputDisabled, OnInputEnabled);
		_targetFocalFollowController = new CameraTargetFocalFollowController(this);
	}

	private void OnDestroy()
	{
		overriddenBehavior?.OnDestroy();
		s_CameraController = null;
	}

	private void Start()
	{
		m_HasFocus = Application.isFocused;
		m_InitialCamera = m_Camera.gameObject;
		m_InitialCameraHeight = m_Camera.transform.position.y;
		m_MinimumFOV = (PlatformLayer.Setting.RestrictCamera ? 56.5f : 30f);
		m_DefaultFOV = 75f;
		m_TargetZoom = m_DefaultFOV / 1.5f;
		m_TargetFocalPoint = s_InitialFocusPoint;
		m_FocalPoint = m_TargetFocalPoint;
		m_CameraInputDisabled = ECameraInput.None;
		m_ZoomOutExtraHeight = 10f;
		m_CameraMoveSpeed = 10f;
		m_IsCameraCodeControlDisabled = false;
		m_EnableCameraShake = false;
		m_CameraShakePosition = Vector3.zero;
		m_CameraShakeRotation = Quaternion.identity;
		PlayableDirector component = base.gameObject.GetComponent<PlayableDirector>();
		if (component != null)
		{
			component.played += OnStartPlaying;
			component.stopped += OnStopPlaying;
		}
		startComplete = true;
	}

	public void InitCamera(Bounds? bounds = null)
	{
		m_FocalPoint = m_Camera.transform.position + m_Camera.transform.forward * 100f;
		float num = ((m_Camera.transform.position.y != m_FocalPoint.y) ? (m_Camera.transform.position.y / (m_Camera.transform.position.y - m_FocalPoint.y)) : 0f);
		m_FocalPoint = m_Camera.transform.position + (m_FocalPoint - m_Camera.transform.position) * num;
		m_TargetFocalPoint = m_FocalPoint;
		Vector3 vector = m_FocalPoint - m_Camera.transform.position;
		vector.y = 0f;
		m_CameraDefaultRadius = vector.magnitude;
		m_CameraLeftVector = -m_Camera.transform.right;
		s_InitialHorizontalAngle = 57.29578f * Mathf.Acos(Vector3.Dot(m_CameraLeftVector, m_Camera.transform.forward));
		m_InitialCameraHeight = m_Camera.transform.position.y;
		m_CameraGameHorizontalAngle = s_InitialHorizontalAngle;
		m_TargetZoom = m_Camera.fieldOfView;
		SetCameraDirection(s_InitialHorizontalAngle);
		m_DefaultZoomFactor = m_ZoomFactor;
		if (bounds.HasValue)
		{
			SetFocalBoundsForScenario(bounds.Value);
		}
		else
		{
			FocalBounds component = m_Camera.GetComponent<FocalBounds>();
			if (component == null)
			{
				SetFocalBoundsForScenario(new Vector3(0f, 0f, 0f), new Vector3(1000f, 1000f, 1000f));
			}
			else
			{
				SetFocalBoundsForScenario(component.Center, component.Bounds);
			}
		}
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			HashSet<TileBehaviour> tileBehaviors = Singleton<ObjectCacheService>.Instance.GetTileBehaviors();
			if (tileBehaviors.Count > 0)
			{
				s_CameraController.m_FocalBounds = default(Bounds);
				foreach (TileBehaviour item in tileBehaviors)
				{
					s_CameraController.m_FocalBounds.Encapsulate(item.gameObject.transform.position);
				}
			}
		}
		isInitialised = true;
	}

	private void OnApplicationFocus(bool focus)
	{
		m_HasFocus = focus;
	}

	private void OnEnable()
	{
		m_HasFocus = Application.isFocused;
		_basicControl.Enable();
	}

	private void OnDisable()
	{
		_basicControl.Disable();
	}

	public void OnStartPlaying(PlayableDirector pb)
	{
		m_IsCameraCodeControlDisabled = true;
	}

	public void OnStopPlaying(PlayableDirector pb)
	{
		m_IsCameraCodeControlDisabled = false;
	}

	public void SetCamera(GameObject newCamera)
	{
		if (newCamera == null)
		{
			m_Camera.gameObject.SetActive(value: false);
			m_InitialCamera.SetActive(value: true);
			m_Camera = m_InitialCamera.GetComponent<Camera>();
		}
		else
		{
			m_Camera.gameObject.SetActive(value: false);
			m_Camera = newCamera.GetComponent<Camera>();
			InitCamera();
		}
	}

	public void SetFocalBoundsForScenario(Vector3 center, Vector3 bounds)
	{
		SetFocalBoundsForScenario(new Bounds(center, bounds));
	}

	public void SetFocalBoundsForScenario(Bounds bounds)
	{
		m_FocalBounds = bounds;
	}

	public void SetMapInputSource(bool value)
	{
		m_MapInputSource = value;
	}

	private float CalculateZoomFactor(float zoom)
	{
		return (zoom - m_MinimumFOV) / (m_DefaultFOV - m_MinimumFOV);
	}

	private float CalculateZoomByZoomFactor(float zoomFactor)
	{
		return zoomFactor * (m_DefaultFOV - m_MinimumFOV) + m_MinimumFOV;
	}

	public float CalculateYForZoom(float zoom)
	{
		return CalculateYForZoomFactor(CalculateZoomFactor(zoom));
	}

	private float CalculateYForZoomFactor(float zoomFactor)
	{
		float num = Mathf.Max((zoomFactor - m_DefaultZoomFactor) / (1f - m_DefaultZoomFactor), 0f);
		return m_InitialCameraHeight + num * m_ZoomOutExtraHeight;
	}

	public float? CalculateZoomForY(float y)
	{
		if (m_ZoomOutExtraHeight <= 0f)
		{
			return null;
		}
		float num = (y - m_InitialCameraHeight) / m_ZoomOutExtraHeight;
		if (num <= 0f)
		{
			return null;
		}
		float zoomFactor = num * (1f - m_DefaultZoomFactor) + m_DefaultZoomFactor;
		return CalculateZoomByZoomFactor(zoomFactor);
	}

	public void ResetFocalPointGameObject()
	{
		_targetFocalFollowController.ResetPoint();
	}

	public void RemoveFocalPointGameObject(GameObject gameObject)
	{
		_targetFocalFollowController.TryRemovePoint(gameObject);
	}

	public void SetFocalPointGameObject(GameObject gameobject)
	{
		_targetFocalFollowController.SetPoint(gameobject);
	}

	public void SetOptimalViewPoint(Vector3 srcActorPos, Vector3 dstActorPos, CActor.EType actorType)
	{
		if ((actorType != CActor.EType.Enemy || m_EnableEnemyForceRotation) && (actorType != CActor.EType.HeroSummon || m_EnableEnemyForceRotation) && (actorType != CActor.EType.Player || m_EnablePlayerForceRotation))
		{
			Vector3 normalized = (Camera.main.WorldToScreenPoint(dstActorPos) - Camera.main.WorldToScreenPoint(srcActorPos)).normalized;
			if (Mathf.Acos(Mathf.Abs(Vector3.Dot(normalized, Vector3.up))) / MathF.PI * 180f < 20f)
			{
				m_ForceRotated = true;
				m_ForceRotate = 1;
			}
		}
	}

	public void ResetOptimalViewPoint()
	{
		if (m_ForceRotated && m_EnableRotateBack)
		{
			m_ForceRotate = -1;
		}
		m_ForceRotated = false;
	}

	public void SmartFocus(GameObject target, bool pauseDuringTransition = false)
	{
		if (target == null)
		{
			return;
		}
		if (m_IsFreeCamEnabled)
		{
			Debug.LogGUI("Skip SmartFocus because free cam is enabled");
			return;
		}
		Vector3 allAveragePositions = Choreographer.s_Choreographer.GetAllAveragePositions();
		Vector3 position = target.transform.position;
		Vector3 vector = (allAveragePositions + position) / 2f - m_TargetFocalPoint;
		Vector3 vector2 = m_Camera.WorldToScreenPoint(allAveragePositions - vector);
		vector2.z = 0.5f;
		Vector3 vector3 = m_Camera.WorldToScreenPoint(position - vector);
		vector3.z = 0.5f;
		Vector3 point = m_Camera.WorldToScreenPoint(position);
		point.z = 0f;
		Vector3 vector4 = vector3 - vector2;
		RectTransform screenVisibleArea = UIManager.Instance.ScreenVisibleArea;
		Vector3[] array = new Vector3[4];
		screenVisibleArea.GetWorldCorners(array);
		Vector3 min = UIManager.Instance.UICamera.WorldToScreenPoint(array[0]);
		Vector3 max = UIManager.Instance.UICamera.WorldToScreenPoint(array[2]);
		Bounds bounds = default(Bounds);
		min.z = 0f;
		bounds.min = min;
		max.z = 1f;
		bounds.max = max;
		if (bounds.Contains(point))
		{
			Debug.LogGUI("Skip SmartFocus original pos is visible");
			return;
		}
		Debug.LogGUI("SmartFocus pause transition " + pauseDuringTransition + " " + TimeManager.IsPaused);
		bounds.IntersectRay(new Ray(bounds.center, vector4.normalized), out var distance);
		Vector3 position2;
		if (Mathf.Abs(distance) > vector4.magnitude)
		{
			position2 = allAveragePositions;
		}
		else
		{
			float t = Mathf.Abs(distance) / vector4.magnitude;
			position2 = Vector3.Lerp(position, allAveragePositions, t);
		}
		_targetFocalFollowController.SetPoint(position2, pauseDuringTransition);
	}

	public void SetFocalPointBetweenTwoObjects(GameObject firstObject, GameObject secondObject)
	{
		Vector3 position = (firstObject.transform.position + secondObject.transform.position) / 2f;
		_targetFocalFollowController.SetPoint(position);
		if (!BothOjbectsOnScreen(firstObject, secondObject))
		{
			StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
		}
	}

	public void SetOverriddenBehavior(ICameraBehavior behavior)
	{
		ClearOverriddenBehavior();
		Debug.Log("[Camera] SetOverriddenBehavior " + behavior.ID);
		overriddenBehavior = behavior;
		behavior.Enable(this);
	}

	public void ClearOverriddenBehavior()
	{
		if (overriddenBehavior != null)
		{
			Debug.Log("[Camera] ClearOverriddenBehavior " + overriddenBehavior.ID);
		}
		overriddenBehavior?.Disable(this);
		overriddenBehavior = null;
	}

	public void ClearOverriddenBehavior(ECameraBehaviorType type)
	{
		if (overriddenBehavior != null)
		{
			Debug.Log("[Camera] Try ClearOverriddenBehavior " + type);
			if (overriddenBehavior.IsType(type))
			{
				ClearOverriddenBehavior();
			}
		}
	}

	public void ClearOverriddenBehavior(string id)
	{
		if (overriddenBehavior != null)
		{
			Debug.Log("[Camera] Try ClearOverriddenBehavior " + id);
			if (overriddenBehavior.ID == id)
			{
				ClearOverriddenBehavior();
			}
		}
	}

	private IEnumerator ZoomOutUntilBothObjectsVisible(GameObject firstObject, GameObject secondObject)
	{
		m_TargetZoom += Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar;
		if (!BothOjbectsOnScreen(firstObject, secondObject))
		{
			yield return Timekeeper.instance.WaitForSeconds(0.005f);
			if (m_TargetZoom < 75f)
			{
				StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
			}
		}
	}

	public void ZoomInUtilBothObjectOnBorder(GameObject firstObject, GameObject secondObject)
	{
		Vector3 position = (firstObject.transform.position + secondObject.transform.position) / 2f;
		_targetFocalFollowController.SetPoint(position);
		if (BothOjbectsOnScreen(firstObject, secondObject))
		{
			StartCoroutine(ZoomInUtilBothObjectOnBorderCoroutine(firstObject, secondObject));
		}
		else
		{
			StartCoroutine(ZoomOutUntilBothObjectsVisible(firstObject, secondObject));
		}
	}

	private IEnumerator ZoomInUtilBothObjectOnBorderCoroutine(GameObject firstObject, GameObject secondObject)
	{
		Vector3 position = (firstObject.transform.position + secondObject.transform.position) / 2f;
		_targetFocalFollowController.SetPoint(position);
		yield return null;
		m_TargetZoom -= Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar;
		if (!BothOjbectsOnScreenBorder(firstObject, secondObject))
		{
			yield return Timekeeper.instance.WaitForSeconds(0.005f);
			if (m_TargetZoom > TotalMinimumFOV)
			{
				StartCoroutine(ZoomInUtilBothObjectOnBorderCoroutine(firstObject, secondObject));
			}
		}
	}

	private bool BothOjbectsOnScreenBorder(GameObject firstObject, GameObject secondObject)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(firstObject.transform.position);
		Vector3 vector2 = Camera.main.WorldToScreenPoint(secondObject.transform.position);
		int num = (int)((float)Screen.width / 3.5f);
		int num2 = Screen.height / 5;
		if ((vector.x > (float)num && vector.x < (float)(Screen.width - num) && vector.y > (float)num2 && vector.y < (float)(Screen.height - num2)) || (vector2.x < (float)num && vector2.x > (float)(Screen.width - num) && vector2.y < (float)num2 && vector2.y > (float)(Screen.height - num2)))
		{
			return false;
		}
		return true;
	}

	private bool BothOjbectsOnScreen(GameObject firstObject, GameObject secondObject)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(firstObject.transform.position);
		Vector3 vector2 = Camera.main.WorldToScreenPoint(secondObject.transform.position);
		if (vector.x < 0f || (vector.x > (float)Screen.width && vector.y < 0f) || (vector.y > (float)Screen.height && vector2.x < 0f) || (vector2.x > (float)Screen.width && vector2.y < 0f) || vector2.y > (float)Screen.height)
		{
			return false;
		}
		return true;
	}

	public void DisableCameraInput(bool disableInput)
	{
		m_CameraInputDisabled = (disableInput ? (ECameraInput.Move | ECameraInput.Zoom | ECameraInput.Rotate) : ECameraInput.None);
	}

	public void DisableCameraInput(ECameraInput input, bool disableInput)
	{
		if (disableInput)
		{
			m_CameraInputDisabled |= input;
		}
		else if (m_CameraInputDisabled.HasFlag(input))
		{
			m_CameraInputDisabled ^= input;
		}
	}

	public void RequestDisableCameraInput(object request)
	{
		_disableInputCounter.Add(request);
	}

	public void FreeDisableCameraInput(object request)
	{
		_disableInputCounter.Remove(request);
	}

	private void OnInputEnabled()
	{
		OnInputEnableChanged?.Invoke(obj: true);
	}

	private void OnInputDisabled()
	{
		OnInputEnableChanged?.Invoke(obj: false);
	}

	public void MoveToLook(Vector3 locationToLookAt, float movementSpeed, bool keepCameraLockedUponCompletion = false, Action onCompletedCallback = null)
	{
		m_IsCameraCodeControlDisabled = true;
		distanceFromTheTarget = m_TargetFocalPoint.x - m_Camera.transform.position.x;
		m_TargetFocalPoint = locationToLookAt;
		Vector3 vector = new Vector3(locationToLookAt.x - distanceFromTheTarget, m_Camera.transform.position.y, locationToLookAt.z);
		float time = (vector - base.transform.position).magnitude / ((movementSpeed == 0f) ? 1f : movementSpeed);
		LeanTween.move(m_Camera.gameObject, vector, time).setEase(LeanTweenType.easeInOutCubic).setOnComplete((Action)delegate
		{
			m_IsCameraCodeControlDisabled = keepCameraLockedUponCompletion;
			onCompletedCallback?.Invoke();
		});
	}

	public void SetCameraWithMessageProfile(CLevelCameraProfile profileToSetTo)
	{
		if (m_CameraToProfileRoutine != null)
		{
			StopCoroutine(m_CameraToProfileRoutine);
			m_CameraToProfileRoutine = null;
		}
		m_CameraToProfileRoutine = SetCameraProfileOverTime(profileToSetTo);
		StartCoroutine(m_CameraToProfileRoutine);
	}

	private IEnumerator SetCameraProfileOverTime(CLevelCameraProfile profileToSetTo)
	{
		if (profileToSetTo.CameraPosition != null && profileToSetTo.FocalPosition != null)
		{
			RequestDisableCameraInput(this);
			Vector3 vector = GloomUtility.CVToV(profileToSetTo.CameraPosition);
			Vector3 startFocal = m_FocalPoint;
			Vector3 endFocal = GloomUtility.CVToV(profileToSetTo.FocalPosition);
			float startFOV = m_Camera.fieldOfView;
			float endFOV = profileToSetTo.CameraFieldOfView;
			float startCameraAngle = m_CameraGameHorizontalAngle;
			float endCameraAngle = Quaternion.FromToRotation(m_CameraLeftVector, (endFocal - vector).normalized).eulerAngles.y;
			float timeToTake = 0.5f;
			for (float timeElapsed = 0f; timeElapsed < timeToTake; timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime)
			{
				float num = timeElapsed / timeToTake;
				float t = num * num * (3f - 2f * num);
				m_TargetFocalPoint = Vector3.Lerp(startFocal, endFocal, t);
				m_TargetZoom = Mathf.Lerp(startFOV, endFOV, t);
				m_CameraGameHorizontalAngle = Mathf.Lerp(startCameraAngle, endCameraAngle, t);
				SetCameraDirection(m_CameraGameHorizontalAngle);
				yield return null;
			}
			m_TargetFocalPoint = endFocal;
			m_TargetZoom = endFOV;
			m_CameraGameHorizontalAngle = endCameraAngle;
			SetCameraDirection(m_CameraGameHorizontalAngle);
			FreeDisableCameraInput(this);
		}
	}

	public void ZoomFOV(float increaseAmount, float duration, Action onZoomed = null)
	{
		float zoom = Mathf.Clamp(m_TargetZoom + increaseAmount, TotalMinimumFOV, m_DefaultFOV);
		ZoomToFOV(zoom, duration, onZoomed);
	}

	public void ZoomToPercent(float percent, float duration, Action onZoomed = null)
	{
		ZoomToFOV(CalculateZoomByPercent(percent), duration, onZoomed);
	}

	public float CalculateZoomByPercent(float percent)
	{
		return TotalMinimumFOV + (m_DefaultFOV - TotalMinimumFOV) * percent;
	}

	public void ZoomToFOV(float zoom, float duration, Action onZoomed = null)
	{
		CancelZoom();
		Debug.LogFormat("<color=#A3DA77FF>[CAMERA]</color> {0}", "Zoom to " + zoom + " from " + Zoom);
		if (duration == 0f)
		{
			ResetZoomTo(zoom);
		}
		else
		{
			m_CameraZoomFOVRoutine = StartCoroutine(ZoomTo(zoom, duration, onZoomed));
		}
	}

	public void CancelZoom()
	{
		ClearOverriddenBehavior(ECameraBehaviorType.Zoom);
		if (m_CameraZoomFOVRoutine != null)
		{
			StopCoroutine(m_CameraZoomFOVRoutine);
			m_CameraZoomFOVRoutine = null;
		}
	}

	private IEnumerator ZoomTo(float zoom, float duration, Action onZoomed = null)
	{
		float startFOV = m_Camera.fieldOfView;
		float timeElapsed = 0f;
		RequestDisableCameraInput(this);
		for (; timeElapsed < duration; timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime)
		{
			float num = timeElapsed / duration;
			float t = num * num * (3f - 2f * num);
			m_TargetZoom = Mathf.Lerp(startFOV, zoom, t);
			yield return null;
		}
		m_TargetZoom = zoom;
		FreeDisableCameraInput(this);
		m_CameraZoomFOVRoutine = null;
		onZoomed?.Invoke();
	}

	public void PrintCurrentCameraDetails()
	{
		Debug.Log("Current Camera Details:\nFocalPoint:(" + m_FocalPoint.x + ", " + m_FocalPoint.y + ", " + m_FocalPoint.z + ")\nPosition:(" + m_Camera.transform.position.x + ", " + m_Camera.transform.position.y + ", " + m_Camera.transform.position.z + ")\nRotation:(" + m_Camera.transform.rotation.eulerAngles.x + ", " + m_Camera.transform.rotation.eulerAngles.y + ", " + m_Camera.transform.rotation.eulerAngles.z + ")\nFOV:" + m_Camera.fieldOfView);
	}

	private void SetCameraDirection(float angle)
	{
		if (!m_IsFreeCamEnabled)
		{
			Vector3 vector = Quaternion.AngleAxis(angle, Vector3.up) * m_CameraLeftVector;
			m_Camera.transform.position = m_FocalPoint - vector;
			UpdateCameraToFocalTargetDiff();
		}
	}

	private void UpdateCameraToFocalTargetDiff()
	{
		m_CameraToFocalTargetDiff = m_Camera.transform.position - m_FocalPoint;
		m_CameraToFocalTargetDiff.y = 0f;
		m_CameraToFocalTargetDiff.Normalize();
		m_CameraToFocalTargetDiff *= m_CameraDefaultRadius;
	}

	private void CheckDevModeKeys()
	{
		if (!Main.s_InternalRelease && !FFSNetwork.IsOnline && InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F1))
		{
			Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.ToggleMenu();
		}
		if (DebugMenu.DebugMenuNotNull && !InputManager.GetKey(UnityEngine.InputSystem.Key.LeftShift))
		{
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit1))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(1);
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit2))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(2);
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit3))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(3);
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit4))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(4);
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit5))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(5);
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.Backquote) && InputManager.GetKey(UnityEngine.InputSystem.Key.Digit6))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.StallMainThread(6);
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F2))
			{
				GloomUtility.ToggleGUI();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F3))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.TogglePause();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F4))
			{
				ToggleFreeCam();
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.PageUp))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.SpeedUp();
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.PageDown))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.SlowDown();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.LeftBracket))
			{
				RotateAround(left: true);
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.RightBracket))
			{
				RotateAround(left: false);
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F5))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.ToggleWallFade();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F6))
			{
				ResetCameraPosition();
			}
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.F7))
			{
				Singleton<DebugMenuProvider>.Instance.DebugMenuInstance.RegenerateApparance();
			}
		}
	}

	private void CheckPerfTestKeys()
	{
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.LeftShift))
		{
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F1))
			{
				PerformanceUtility.TurnOffPointLightShadows();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F2))
			{
				PerformanceUtility.Combine();
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F3))
			{
				PerformanceUtility.CanApplyChaos = !PerformanceUtility.CanApplyChaos;
				Debug.Log($"CanApplyChaos:{PerformanceUtility.CanApplyChaos}");
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F4))
			{
				UnityEngine.Object.FindObjectOfType<VolumetricFog>().enabled = false;
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F5))
			{
				UnityEngine.Object.FindObjectOfType<VolumetricFogPreT>().enabled = false;
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F6))
			{
				_ = UnityEngine.Object.FindObjectOfType<VolumetricFog>().gameObject;
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F11))
			{
				Debug.Log($"VSync was:{QualitySettings.vSyncCount}");
				QualitySettings.vSyncCount = 0;
			}
			if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.F12))
			{
				PerformanceUtility.SetShadowsOff();
			}
		}
	}

	public void SetCameraDirectionAndFocalPoint(Transform pointOfView, Vector3 focalPoint, int forwardAxis = 2)
	{
		if (!m_IsFreeCamEnabled)
		{
			m_TargetFocalPoint = focalPoint;
			Vector3 vector = forwardAxis switch
			{
				1 => pointOfView.up, 
				0 => pointOfView.right, 
				_ => pointOfView.forward, 
			};
			Vector3 normalized = (focalPoint - pointOfView.position).normalized;
			Vector3 vector2 = Mathf.Sign(Vector3.Dot(vector, normalized)) * vector;
			m_CameraGameHorizontalAngle = Quaternion.FromToRotation(m_CameraLeftVector, vector2.normalized).eulerAngles.y;
			m_CameraGameHorizontalAngle = Mathf.Round(m_CameraGameHorizontalAngle / 45f) * 45f;
			SetCameraDirection(m_CameraGameHorizontalAngle);
		}
	}

	public void RotateAround(bool left)
	{
		if (left)
		{
			if (isRotatingLeft)
			{
				isRotatingLeft = false;
				return;
			}
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
			rotateDirection = new Vector3(0f, -1f, 0f);
			isRotatingLeft = true;
			isRotatingRight = false;
		}
		else if (isRotatingRight)
		{
			isRotatingRight = false;
		}
		else
		{
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
			rotateDirection = new Vector3(0f, 1f, 0f);
			isRotatingLeft = false;
			isRotatingRight = true;
		}
	}

	private void SmoothRotate()
	{
		float num = 0f;
		if (smoothRotatingRight)
		{
			if (m_CameraGameHorizontalAngle >= 360f)
			{
				m_CameraGameHorizontalAngle -= 360f;
				newSmoothRotateAngle -= 360f;
			}
			num = m_CameraGameHorizontalAngle + m_CameraRotationSpeed * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (num > newSmoothRotateAngle)
			{
				num = newSmoothRotateAngle;
			}
		}
		else
		{
			if (m_CameraGameHorizontalAngle <= 0f)
			{
				m_CameraGameHorizontalAngle = 360f - m_CameraGameHorizontalAngle;
				newSmoothRotateAngle += 360f;
			}
			num = m_CameraGameHorizontalAngle - m_CameraRotationSpeed * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (num < newSmoothRotateAngle)
			{
				num = newSmoothRotateAngle;
			}
		}
		m_CameraGameHorizontalAngle = Mathf.Lerp(m_CameraGameHorizontalAngle, num, 0.5f);
		SetCameraDirection(m_CameraGameHorizontalAngle);
		if (Mathf.Abs(m_CameraGameHorizontalAngle - newSmoothRotateAngle) < 0.5f)
		{
			m_CameraGameHorizontalAngle = Mathf.Round(m_CameraGameHorizontalAngle);
			smoothRotating = false;
		}
	}

	public void ResetCameraRotation()
	{
		SetCameraDirection(m_CameraGameHorizontalAngle);
	}

	private void RotateAroundController()
	{
		float num = Time.realtimeSinceStartup - m_LastRealTime;
		m_Camera.transform.RotateAround(rotatePoint, rotateDirection, rotateSpeed * num);
		m_LastRealTime = Time.realtimeSinceStartup;
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.A) || InputManager.GetKey(UnityEngine.InputSystem.Key.LeftArrow) || InputManager.GetKey(UnityEngine.InputSystem.Key.S) || InputManager.GetKey(UnityEngine.InputSystem.Key.DownArrow) || InputManager.GetKey(UnityEngine.InputSystem.Key.W) || InputManager.GetKey(UnityEngine.InputSystem.Key.UpArrow) || InputManager.GetKey(UnityEngine.InputSystem.Key.D) || InputManager.GetKey(UnityEngine.InputSystem.Key.RightArrow))
		{
			isRotatingLeft = false;
			isRotatingRight = false;
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.Numpad8))
		{
			rotateDepth += 0.1f;
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.Numpad2))
		{
			if (rotateDepth - 0.1f <= 0.5f)
			{
				rotateDepth = 0.5f;
			}
			else
			{
				rotateDepth -= 0.1f;
			}
			rotatePoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, rotateDepth));
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.Numpad4))
		{
			if (isRotatingLeft)
			{
				rotateSpeed += 0.05f;
			}
			else if (rotateSpeed - 0.05f <= 0f)
			{
				rotateSpeed = 0f;
			}
			else
			{
				rotateSpeed -= 0.05f;
			}
			if (DebugMenu.DebugMenuNotNull)
			{
				DebugMenu.Instance.DisplayDebugText(rotateSpeedTextID, "Rotate Speed: " + rotateSpeed.ToString("0.00"));
			}
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.Numpad6))
		{
			if (isRotatingRight)
			{
				rotateSpeed += 0.05f;
			}
			else if (rotateSpeed - 0.05f <= 0f)
			{
				rotateSpeed = 0f;
			}
			else
			{
				rotateSpeed -= 0.05f;
			}
			if (DebugMenu.DebugMenuNotNull)
			{
				DebugMenu.Instance.DisplayDebugText(rotateSpeedTextID, "Rotate Speed: " + rotateSpeed.ToString("0.00"));
			}
		}
		CheckDevModeKeys();
	}

	public void ToggleFreeCam()
	{
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			if (m_IsFreeCamEnabled && !m_HoldFreeCamPosition)
			{
				m_HoldFreeCamPosition = true;
				Cursor.lockState = CursorLockMode.None;
				return;
			}
			m_IsFreeCamEnabled = true;
			m_HoldFreeCamPosition = false;
			m_LastRealTime = Time.realtimeSinceStartup;
			Cursor.lockState = CursorLockMode.Locked;
			m_CurrentCameraPos = m_Camera.transform.position;
			m_CurrentCameraRot = m_Camera.transform.rotation;
			m_RotationX = m_Camera.transform.eulerAngles.y;
			m_RotationY = 0f - m_Camera.transform.eulerAngles.x;
		}
	}

	public void ResetCameraPosition()
	{
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			m_IsFreeCamEnabled = false;
			m_HoldFreeCamPosition = false;
			Cursor.lockState = CursorLockMode.None;
			m_Camera.transform.position = m_CurrentCameraPos;
			m_Camera.transform.rotation = m_CurrentCameraRot;
		}
	}

	private void FreeCamController()
	{
		if (m_HoldFreeCamPosition)
		{
			CheckDevModeKeys();
			m_LastRealTime = Time.realtimeSinceStartup;
			return;
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.NumpadPlus) || InputManager.GetKey(UnityEngine.InputSystem.Key.Equals))
		{
			m_LeftJoySpeed = Mathf.Min(50f, m_LeftJoySpeed + 0.05f);
			Debug.Log($"Movement speed increased by {0.5f}, current speed {m_LeftJoySpeed}");
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.Minus))
		{
			m_LeftJoySpeed = Mathf.Max(0f, m_LeftJoySpeed - 0.05f);
			Debug.Log($"Movement speed decreased by {0.5f}, current speed {m_LeftJoySpeed}");
		}
		float num = Time.realtimeSinceStartup - m_LastRealTime;
		if (!InputManager.IsControllerModeDisabled)
		{
			m_RotationX += Singleton<InputManager>.Instance.PlayerControl.FreeCamRotate.X * m_CameraSensitivity * num;
			m_RotationY += Singleton<InputManager>.Instance.PlayerControl.FreeCamRotate.Y * m_CameraSensitivity * num;
		}
		else
		{
			m_RotationX += num * m_RightJoySpeed * _basicControl.Gamepad.RightStickX.ReadValue<float>();
			m_RotationY += num * m_RightJoySpeed * _basicControl.Gamepad.RightStickY.ReadValue<float>();
		}
		if (m_RotationX > 360f)
		{
			m_RotationX -= 360f;
		}
		if (m_RotationY > 360f)
		{
			m_RotationY -= 360f;
		}
		m_Camera.transform.localRotation = Quaternion.AngleAxis(m_RotationX, Vector3.up);
		m_Camera.transform.localRotation *= Quaternion.AngleAxis(m_RotationY, Vector3.left);
		if (!InputManager.IsControllerModeDisabled)
		{
			if (Singleton<InputManager>.Instance.PlayerControl.PanCamera != Vector2.zero)
			{
				m_Camera.transform.position += m_Camera.transform.right * num * m_CameraMoveSpeed * Singleton<InputManager>.Instance.PlayerControl.PanCamera.X;
				m_Camera.transform.position += m_Camera.transform.forward * num * m_CameraMoveSpeed * Singleton<InputManager>.Instance.PlayerControl.PanCamera.Y;
			}
			if (InputManager.GetIsPressed(KeyAction.ROTATE_CAMERA_LEFT))
			{
				m_Camera.transform.position += m_Camera.transform.up * m_ClimbSpeed * num * InputManager.GetValue(KeyAction.ROTATE_CAMERA_LEFT);
			}
			if (InputManager.GetIsPressed(KeyAction.ROTATE_CAMERA_RIGHT))
			{
				m_Camera.transform.position -= m_Camera.transform.up * m_ClimbSpeed * num * InputManager.GetValue(KeyAction.ROTATE_CAMERA_RIGHT);
			}
		}
		else
		{
			m_Camera.transform.position += num * m_LeftJoySpeed * _basicControl.Gamepad.LeftStickX.ReadValue<float>() * m_Camera.transform.right;
			m_Camera.transform.position += num * m_LeftJoySpeed * _basicControl.Gamepad.LeftStickY.ReadValue<float>() * m_Camera.transform.forward;
		}
		m_Camera.transform.position += (float)Singleton<InputManager>.Instance.PlayerControl.FreeCamMoveHorizontal * m_ShiftJoySpeed * num * m_Camera.transform.up;
		if (InputManager.GetKeyDown(UnityEngine.InputSystem.Key.End))
		{
			Cursor.lockState = ((Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None);
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.NumpadPlus))
		{
			m_CameraMoveSpeed += 0.05f;
			if (DebugMenu.DebugMenuNotNull)
			{
				DebugMenu.Instance.DisplayDebugText(m_CameraMoveSpeedTextID, "Camera Speed: " + m_CameraMoveSpeed.ToString("0.00"));
			}
		}
		if (InputManager.GetKey(UnityEngine.InputSystem.Key.NumpadMinus))
		{
			m_CameraMoveSpeed -= 0.05f;
			if (DebugMenu.DebugMenuNotNull)
			{
				DebugMenu.Instance.DisplayDebugText(m_CameraMoveSpeedTextID, "Camera Speed: " + m_CameraMoveSpeed.ToString("0.00"));
			}
		}
		CheckDevModeKeys();
		m_LastRealTime = Time.realtimeSinceStartup;
	}

	private void LateUpdate()
	{
		if (!isInitialised || m_Camera == null)
		{
			return;
		}
		if (!Main.s_DevMode && !Main.s_InternalRelease)
		{
			if (InputManager.GetKey(UnityEngine.InputSystem.Key.F2) && !InputManager.GetKeyDown(UnityEngine.InputSystem.Key.LeftShift))
			{
				if (GloomUtility.IsGUIVisible())
				{
					GloomUtility.ToggleGUI();
				}
			}
			else if (!GloomUtility.IsGUIVisible())
			{
				GloomUtility.ToggleGUI();
			}
		}
		if (SceneController.Instance.GlobalErrorMessage.ShowingMessage || m_IsCameraCodeControlDisabled || (overriddenBehavior != null && overriddenBehavior.Update(this)))
		{
			return;
		}
		if (isRotatingLeft || isRotatingRight)
		{
			RotateAroundController();
			return;
		}
		if (m_IsFreeCamEnabled)
		{
			FreeCamController();
			return;
		}
		if (Main.s_DevMode || Main.s_InternalRelease)
		{
			CheckDevModeKeys();
			CheckPerfTestKeys();
		}
		if (InputManager.GetWasPressed(KeyAction.LOS_VIEW))
		{
			if (ClientScenarioManager.s_ClientScenarioManager != null)
			{
				ClientScenarioManager.s_ClientScenarioManager.EnableUserLOSDisplay(isOn: true);
			}
		}
		else if (InputManager.GetWasReleased(KeyAction.LOS_VIEW) && ClientScenarioManager.s_ClientScenarioManager != null)
		{
			ClientScenarioManager.s_ClientScenarioManager.EnableUserLOSDisplay(isOn: false);
		}
		if (Main.s_Pause3DWorld)
		{
			return;
		}
		bool flag = false;
		if (RotateInputEnabled)
		{
			if (!smoothRotating)
			{
				if (InputManager.GetIsPressed(KeyAction.ROTATE_CAMERA_WITH_MOUSE) && (!InputManager.GetKey(UnityEngine.InputSystem.Key.LeftCtrl) ^ flag))
				{
					if (InputManager.IsControllerModeDisabled)
					{
						m_DeltaAngle = (0f - _basicControl.Mouse.MoveX.ReadValue<float>()) * 25f;
					}
					else
					{
						m_DeltaAngle = (0f - Singleton<InputManager>.Instance.PlayerControl.FreeCamRotate.X) * 25f;
					}
					setRotationalVariables();
				}
				else if (m_RotationStarted)
				{
					m_RotationStarted = false;
				}
				if ((InputManager.IsControllerModeDisabled && InputManager.GetWasPressed(KeyAction.ROTATE_CAMERA_LEFT)) || (!InputManager.IsControllerModeDisabled && InputManager.GetValue(KeyAction.ROTATE_CAMERA_LEFT) > 0.5f) || m_ForceRotate < 0)
				{
					float num = 45f - m_CameraGameHorizontalAngle % 45f;
					newSmoothRotateAngle = ((m_CameraGameHorizontalAngle > 360f) ? (m_CameraGameHorizontalAngle - 360f + num) : (m_CameraGameHorizontalAngle + num));
					smoothRotating = true;
					smoothRotatingRight = true;
					m_ForceRotate = 0;
				}
				if ((InputManager.IsControllerModeDisabled && InputManager.GetWasPressed(KeyAction.ROTATE_CAMERA_RIGHT)) || (!InputManager.IsControllerModeDisabled && InputManager.GetValue(KeyAction.ROTATE_CAMERA_RIGHT) > 0.5f) || m_ForceRotate > 0)
				{
					float num2 = m_CameraGameHorizontalAngle % 45f;
					if ((double)num2 < 0.001)
					{
						num2 = 45f;
					}
					newSmoothRotateAngle = ((m_CameraGameHorizontalAngle < 0f) ? (m_CameraGameHorizontalAngle + 360f - num2) : (m_CameraGameHorizontalAngle - num2));
					smoothRotating = true;
					smoothRotatingRight = false;
					m_ForceRotate = 0;
				}
			}
			else
			{
				SmoothRotate();
			}
		}
		if (Mathf.Abs(m_DeltaAngle) > 0f)
		{
			float num3 = m_DeltaAngle * Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * 3f;
			m_DeltaAngle = ((Mathf.Abs(m_DeltaAngle) < 0.01f) ? 0f : (m_DeltaAngle - num3));
			m_Camera.transform.position += (Vector3)m_Camera.transform.worldToLocalMatrix.GetRow(0) * num3 * m_OldLength * 0.25f;
			m_CameraGameHorizontalAngle = Quaternion.FromToRotation(m_CameraLeftVector, m_Camera.transform.forward).eulerAngles.y;
			UpdateCameraToFocalTargetDiff();
		}
		if (InputManager.GetIsPressed(KeyAction.MOVE_CAMERA_WITH_MOUSE) && (InputManager.GetKey(UnityEngine.InputSystem.Key.LeftCtrl) ^ flag))
		{
			Ray ray = m_Camera.ScreenPointToRay(InputManager.CursorPosition);
			if (s_GroundPlane.Raycast(ray, out var enter))
			{
				Vector3 point = ray.GetPoint(enter);
				if (!m_TranslationStarted)
				{
					m_TranslationStarted = true;
					m_TranslationStartTargetFocalPoint = m_FocalPoint;
					m_TranslationStartMousePosOnPlane = point;
					ResetFocalPointGameObject();
					s_Translated = false;
				}
				m_TargetFocalPoint = m_TranslationStartTargetFocalPoint + (m_TranslationStartMousePosOnPlane - point);
			}
			if (!s_Translated)
			{
				s_Translated = !((m_FocalPoint - m_TargetFocalPoint).sqrMagnitude < 0.001f);
			}
		}
		else if (m_TranslationStarted)
		{
			m_TranslationStarted = false;
		}
		m_oldTargetFocalPoint = m_TargetFocalPoint;
		if (MoveInputEnabled)
		{
			PlayerTwoAxisAction playerTwoAxisAction = (m_MapInputSource ? Singleton<InputManager>.Instance.PlayerControl.PanCameraMap : Singleton<InputManager>.Instance.PlayerControl.PanCamera);
			if (playerTwoAxisAction.X != 0f)
			{
				m_TargetFocalPoint += m_Camera.transform.right * (Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * m_CameraMoveSpeed * playerTwoAxisAction.X * InControl.InputManager.Sensitivity);
				m_TranslationKeyPressed = true;
			}
			if (playerTwoAxisAction.Y != 0f)
			{
				Vector3 forward = m_Camera.transform.forward;
				forward.y = 0f;
				forward = forward.normalized;
				m_TargetFocalPoint += forward * (Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * m_CameraMoveSpeed * playerTwoAxisAction.Y * InControl.InputManager.Sensitivity);
				m_TranslationKeyPressed = true;
			}
			m_TargetFocalPoint = new Vector3(Mathf.Max(m_FocalBounds.min.x, Mathf.Min(m_FocalBounds.max.x, m_TargetFocalPoint.x)), m_TargetFocalPoint.y, Mathf.Max(m_FocalBounds.min.z, Mathf.Min(m_FocalBounds.max.z, m_TargetFocalPoint.z)));
		}
		if (m_oldTargetFocalPoint != m_TargetFocalPoint)
		{
			m_TargetFocalPoint = new Vector3(m_TargetFocalPoint.x, 0f, m_TargetFocalPoint.z);
			ResetFocalPointGameObject();
			UpdateCameraToFocalTargetDiff();
		}
		_targetFocalFollowController.TryUpdatePoint();
		Vector3 vector = (m_TargetFocalPoint - m_FocalPoint) * Mathf.Min(Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * 5f, 1f);
		if (m_TranslationKeyPressed)
		{
			m_TranslationKeyPressed = (m_TargetFocalPoint - m_FocalPoint).sqrMagnitude > 0.01f;
		}
		else
		{
			vector *= m_FocusSpeed;
		}
		m_FocalPoint += vector;
		if (vector.sqrMagnitude < 0.01f)
		{
			_targetFocalFollowController.OnArrivedToPoint();
		}
		m_TranslationStartTargetFocalPoint += vector;
		if (ZoomInputEnabled)
		{
			IInputModulePointer inputModulePointer = EventSystem.current.currentInputModule as IInputModulePointer;
			if (!EventSystem.current.IsPointerOverGameObject() || (inputModulePointer?.GameObjectUnderPointer()?.GetComponent<IgnoreMouseScroll>() == null && inputModulePointer?.GameObjectUnderPointer()?.GetComponentInParent<IgnoreMouseScroll>() == null))
			{
				m_TargetZoom -= (float)Singleton<InputManager>.Instance.PlayerControl.MouseWheelDelta * m_ZoomWheelScalar;
			}
			if (InputManager.IsControllerModeDisabled)
			{
				m_TargetZoom -= (InputManager.GetIsPressed(KeyAction.ZOOM_IN_CAMERA) ? (Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar) : 0f);
				m_TargetZoom += (InputManager.GetIsPressed(KeyAction.ZOOM_OUT_CAMERA) ? (Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar) : 0f);
			}
			else
			{
				if (InputManager.GetIsPressed(KeyAction.ZOOM_IN_CAMERA))
				{
					float value = InputManager.GetValue(KeyAction.ZOOM_IN_CAMERA);
					m_TargetZoom -= ((value > 0.5f) ? (Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar * value * InControl.InputManager.Sensitivity) : 0f);
				}
				if (InputManager.GetIsPressed(KeyAction.ZOOM_OUT_CAMERA))
				{
					float value2 = InputManager.GetValue(KeyAction.ZOOM_OUT_CAMERA);
					m_TargetZoom += ((value2 > 0.5f) ? (Timekeeper.instance.m_GlobalClock.deltaTime * m_ZoomWheelScalar * value2 * InControl.InputManager.Sensitivity) : 0f);
				}
			}
		}
		m_TargetZoom = Mathf.Clamp(m_TargetZoom, TotalMinimumFOV, m_DefaultFOV);
		m_Camera.fieldOfView += (m_TargetZoom - m_Camera.fieldOfView) * Mathf.Min(Timekeeper.instance.m_GlobalClock.unscaledDeltaTime * 10f, 1f);
		RefreshFocusPosition();
		if (m_EnableCameraShake)
		{
			m_Camera.transform.position += m_CameraShakePosition;
			m_Camera.transform.rotation *= m_CameraShakeRotation;
		}
		if (InputManager.GamePadInUse)
		{
			m_Camera.gameObject.transform.transform.LookAt(m_FocalPoint);
		}
		void setRotationalVariables()
		{
			m_OldLength = 1f;
			if (!m_RotationStarted)
			{
				m_RotationStarted = true;
				m_CameraStartAngle = m_Camera.transform.eulerAngles.y;
			}
			s_Rotated = Mathf.Abs(m_CameraStartAngle - m_Camera.transform.eulerAngles.y) > 1f;
		}
	}

	private void RefreshFocusPosition(float? y = null)
	{
		Vector3 position = m_FocalPoint + m_CameraToFocalTargetDiff;
		if (!y.HasValue)
		{
			position.y = CalculateYForZoomFactor(m_ZoomFactor);
		}
		else
		{
			position.y = y.Value;
		}
		if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
		{
			m_Camera.transform.position = position;
			m_Camera.transform.LookAt(m_FocalPoint + Vector3.up * m_FocusPointHeight);
		}
	}

	public void ResetPositionToFocusOnTargetPoint()
	{
		ClearOverriddenBehavior(ECameraBehaviorType.Move);
		RefreshPositionToFocusOnTargetPoint();
	}

	public void RefreshPositionToFocusOnTargetPoint(float? y = null)
	{
		Vector3 vector = m_TargetFocalPoint - m_FocalPoint;
		m_FocalPoint += vector;
		m_TranslationStartTargetFocalPoint += vector;
		RefreshFocusPosition(y);
	}

	public void ResetZoomTo(float zoom)
	{
		CancelZoom();
		SetZoom(zoom);
		Debug.LogFormat("<color=#A3DA77FF>[CAMERA]</color> {0}", "Reset Zoom to " + m_TargetZoom);
	}

	public void SetZoom(float zoom)
	{
		m_TargetZoom = Mathf.Clamp(zoom, TotalMinimumFOV, m_DefaultFOV);
		m_Camera.fieldOfView = m_TargetZoom;
	}
}
