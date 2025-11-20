using System;
using System.Collections;
using UnityEngine;

public class cloud_behav : MonoBehaviour
{
	public GameObject cloud_1;

	public GameObject cloud_2;

	public GameObject cloud_3;

	public float clusterx;

	public float clustery;

	public float clusterz;

	public int cloud_count_max = 10;

	public int cloud_count_cur;

	public int spawn_interval = 3;

	private bool spawn = true;

	private GameObject new_cloud;

	private void Awake()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks * 1000);
	}

	private void Start()
	{
		for (int i = 0; i <= cloud_count_max; i++)
		{
			spawn_cloud(1);
		}
	}

	private void Update()
	{
		if (spawn)
		{
			StartCoroutine(spawn_cldwn());
			spawn = false;
			spawn_cloud(0);
		}
	}

	private void spawn_cloud(int mode)
	{
		if (cloud_count_cur < cloud_count_max)
		{
			switch (UnityEngine.Random.Range(1, 4))
			{
			case 1:
				new_cloud = UnityEngine.Object.Instantiate(cloud_1);
				break;
			case 2:
				new_cloud = UnityEngine.Object.Instantiate(cloud_2);
				break;
			case 3:
				new_cloud = UnityEngine.Object.Instantiate(cloud_3);
				break;
			}
			new_cloud.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
			new_cloud.transform.localPosition = start_cloud_pos(mode);
			cloud_count_cur++;
		}
	}

	private IEnumerator spawn_cldwn()
	{
		yield return new WaitForSeconds(spawn_interval);
		spawn = true;
	}

	private Vector3 start_cloud_pos(int mode)
	{
		float num = 0f;
		num = ((mode != 1) ? 10f : UnityEngine.Random.Range(1f, clusterx));
		float y = UnityEngine.Random.Range(1f, clustery);
		float z = UnityEngine.Random.Range(1f, clusterz);
		return new Vector3(num, y, z);
	}
}
