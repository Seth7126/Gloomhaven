using System.Collections;
using System.Linq;
using Chronos;
using JetBrains.Annotations;
using UnityEngine;

public class WaypointLine : MonoBehaviour
{
	private static readonly int _pulse = Shader.PropertyToID("_Pulse");

	public static WaypointLine s_Instance;

	public bool m_UpdateFlag;

	public bool m_UsePulse;

	public Transform m_Transform;

	private bool m_Pulse = true;

	private Vector3[] m_Waypoints;

	private LineRenderer m_LineRend;

	private Material m_LineMat;

	private float m_InitialValue;

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
		m_Transform = base.transform;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		m_LineRend?.material?.SetFloat(_pulse, m_InitialValue);
		s_Instance = null;
	}

	private void Start()
	{
		m_LineRend = GetComponent<LineRenderer>();
		m_InitialValue = m_LineRend.material.GetFloat(_pulse);
		m_LineMat = m_LineRend.material;
		m_LineRend.material = new Material(m_LineMat);
		m_UpdateFlag = true;
		if (m_UsePulse)
		{
			StartCoroutine(PulseTimeline());
		}
	}

	private void Update()
	{
		if (m_UpdateFlag)
		{
			m_LineRend = GetComponent<LineRenderer>();
			Transform[] array = (from Transform c in base.transform
				where c.gameObject.tag == "Waypoint"
				select c).ToArray();
			m_Waypoints = new Vector3[array.Length];
			for (int num = 0; num < array.Length; num++)
			{
				m_Waypoints[num] = array[num].localPosition;
			}
			m_LineRend.positionCount = m_Waypoints.Length;
			m_LineRend.SetPositions(m_Waypoints);
			m_UpdateFlag = false;
		}
	}

	private IEnumerator PulseTimeline()
	{
		while (m_UsePulse)
		{
			yield return Timekeeper.instance.WaitForSeconds(Random.Range(3, 9));
			float pulseTime = Random.Range(1.2f, 1.9f);
			float dTime = 0f;
			float startTime = Timekeeper.instance.m_GlobalClock.time;
			while (Timekeeper.instance.m_GlobalClock.time - startTime < pulseTime)
			{
				if (dTime < pulseTime)
				{
					dTime += Timekeeper.instance.m_GlobalClock.deltaTime;
					m_LineRend.material.SetFloat(_pulse, dTime / pulseTime);
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
