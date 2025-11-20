using UnityEngine;

public class FreeCam : MonoBehaviour
{
	public float speedNormal = 3f;

	public float speedFast = 45f;

	public float mouseSensX = 2f;

	public float mouseSensY = 2f;

	private float rotY;

	private float speed;

	private void Start()
	{
	}

	private void Update()
	{
		Input.GetMouseButton(0);
		if (Input.GetMouseButton(1))
		{
			float y = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensX;
			rotY += Input.GetAxis("Mouse Y") * mouseSensY;
			rotY = Mathf.Clamp(rotY, -80f, 80f);
			base.transform.localEulerAngles = new Vector3(0f - rotY, y, 0f);
		}
		float axis = Input.GetAxis("Vertical");
		float axis2 = Input.GetAxis("Horizontal");
		if (axis != 0f)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				speed = speedFast;
			}
			else
			{
				speed = speedNormal;
			}
			Vector3 vector = new Vector3(0f, 0f, axis * speed * Time.deltaTime);
			base.transform.localPosition += base.transform.localRotation * vector;
		}
		if (axis2 != 0f)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				speed = speedFast;
			}
			else
			{
				speed = speedNormal;
			}
			Vector3 vector2 = new Vector3(axis2 * speed * Time.deltaTime, 0f, 0f);
			base.transform.localPosition += base.transform.localRotation * vector2;
		}
	}
}
