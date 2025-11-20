using System;
using UnityEngine;

public class cloud_self : MonoBehaviour
{
	public float speed = 0.1f;

	private cloud_behav cld_behav;

	private ParticleSystem prt_sys;

	private void Start()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks * 1000);
		cld_behav = base.gameObject.transform.parent.transform.GetComponent<cloud_behav>();
		prt_sys = base.gameObject.transform.GetComponent<ParticleSystem>();
		if (prt_sys != null)
		{
			ParticleSystem.MainModule main = prt_sys.main;
			main.startSize = UnityEngine.Random.Range(900, 2500);
			main.startRotationX = 0f;
			main.startRotationY = 0f;
			main.startRotationX = UnityEngine.Random.Range(1, 270);
		}
	}

	private void Update()
	{
		base.transform.Translate(speed, 0f, speed * Time.deltaTime);
		if (!space_cluster(base.transform.localPosition))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			cld_behav.cloud_count_cur--;
		}
	}

	private bool space_cluster(Vector3 pos)
	{
		if (((pos.x > 0f) & (pos.x < cld_behav.clusterx)) && ((pos.z > 0f) & (pos.z < cld_behav.clusterz)))
		{
			return true;
		}
		return false;
	}
}
