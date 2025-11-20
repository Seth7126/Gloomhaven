using UnityEngine;

public class RFX4_StartDelay : MonoBehaviour
{
	public GameObject ActivatedGameObject;

	public float Delay = 1f;

	private void OnEnable()
	{
		ActivatedGameObject.SetActive(value: false);
		Invoke("ActivateGO", Delay);
	}

	private void ActivateGO()
	{
		ActivatedGameObject.SetActive(value: true);
	}

	private void OnDisable()
	{
		CancelInvoke("ActivateGO");
	}
}
