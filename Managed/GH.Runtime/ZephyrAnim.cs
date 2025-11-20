using UnityEngine;

public class ZephyrAnim : MonoBehaviour
{
	public string positionProperty = "_ObjPos";

	private Vector3 moved;

	private Vector3 rotated;

	private bool firstPass = true;

	[SerializeField]
	private bool moveUpdate;

	[SerializeField]
	private bool getForward;

	[SerializeField]
	private string directionProperty = "_ObjDir";

	[SerializeField]
	private Renderer[] renderObjs;

	[SerializeField]
	private string rotateProperty = "_VertexRotation_Angle";

	[SerializeField]
	private string pushProperty = "_VectorPush_DirStrength";

	public bool animate;

	public float animateRotate;

	public float animatePush;

	private void Start()
	{
	}

	private void Update()
	{
		if (positionProperty == null)
		{
			return;
		}
		if (moveUpdate)
		{
			if (base.transform.position != moved || base.transform.eulerAngles != rotated || firstPass)
			{
				SetMaterialPropertyOrientation();
			}
			moved = base.transform.position;
			rotated = base.transform.eulerAngles;
			firstPass = false;
		}
		if (animate)
		{
			SetMaterialPropertyAnimation();
		}
	}

	private void OnEnable()
	{
		SetMaterialPropertyOrientation();
		moved = base.transform.position;
		rotated = base.transform.eulerAngles;
	}

	private void SetMaterialPropertyOrientation()
	{
		if (renderObjs == null)
		{
			Renderer component = GetComponent<Renderer>();
			component.material.SetVector(positionProperty, base.transform.position);
			if (getForward)
			{
				component.material.SetVector(directionProperty, base.transform.forward);
			}
			return;
		}
		Renderer[] array = renderObjs;
		foreach (Renderer renderer in array)
		{
			renderer.material.SetVector(positionProperty, base.transform.position);
			if (getForward)
			{
				renderer.material.SetVector(directionProperty, base.transform.forward);
			}
		}
	}

	private void SetMaterialPropertyAnimation()
	{
		if (renderObjs == null)
		{
			Renderer component = GetComponent<Renderer>();
			component.material.SetFloat(rotateProperty, animateRotate);
			component.material.SetFloat(pushProperty, animatePush);
			return;
		}
		Renderer[] array = renderObjs;
		foreach (Renderer obj in array)
		{
			obj.material.SetFloat(rotateProperty, animateRotate);
			obj.material.SetFloat(pushProperty, animatePush);
		}
	}
}
