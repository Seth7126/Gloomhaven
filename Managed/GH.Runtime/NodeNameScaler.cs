#define ENABLE_LOGS
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NodeNameScaler : MonoBehaviour
{
	private const float minScale = 1f;

	private const float maxScale = 2.1f;

	private RectTransform myRect;

	private CameraController cameraController;

	private float previousFoV;

	private void Awake()
	{
		myRect = base.transform as RectTransform;
	}

	private void Start()
	{
		cameraController = CameraController.s_CameraController;
		if (cameraController == null)
		{
			Debug.LogWarning("Camera controller returns null. Map node names will not be scaled.");
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (cameraController.m_Camera.fieldOfView != previousFoV)
		{
			float num = Mathf.Lerp(1f, 2.1f, Mathf.InverseLerp(cameraController.m_MinimumFOV, cameraController.m_DefaultFOV, cameraController.m_Camera.fieldOfView));
			myRect.localScale = new Vector3(num, num, 1f);
			previousFoV = cameraController.m_Camera.fieldOfView;
		}
	}
}
