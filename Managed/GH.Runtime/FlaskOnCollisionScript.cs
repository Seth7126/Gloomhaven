#define ENABLE_LOGS
using System.Collections;
using Chronos;
using UnityEngine;

public class FlaskOnCollisionScript : MonoBehaviour
{
	public GameObject flaskExplosion;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter()
	{
		StartCoroutine(FlaskImpact());
	}

	private IEnumerator FlaskImpact()
	{
		base.gameObject.GetComponent<Collider>().enabled = false;
		yield return Timekeeper.instance.WaitForSeconds(0.02f);
		if (flaskExplosion != null)
		{
			Object.Instantiate(flaskExplosion, base.gameObject.transform.position, base.gameObject.transform.rotation);
		}
		else
		{
			Debug.Log("No explosion effect defined!");
		}
		base.gameObject.GetComponent<MeshRenderer>().enabled = false;
		yield return Timekeeper.instance.WaitForSeconds(1f);
		Object.Destroy(base.gameObject);
		yield return null;
	}
}
