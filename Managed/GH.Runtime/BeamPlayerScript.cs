using Chronos;
using UnityEngine;

public class BeamPlayerScript : MonoBehaviour
{
	public GameObject beamSource;

	public GameObject sourceAttachPoint;

	public GameObject targetAttachPoint;

	public float startTime;

	public float lifeTime = 3f;

	public AnimationCurve FadeCurve;

	private float m_animTimeTaken;

	private Material beamMat;

	private float t;

	private float val;

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(beamSource, sourceAttachPoint.transform);
		beamMat = gameObject.GetComponent<LineRenderer>().material;
		t = 0f;
		val = 0f;
		m_animTimeTaken = 0f;
		beamMat.SetFloat("_BeamFade", 0f);
	}

	private void FixedUpdate()
	{
		if (t < startTime + lifeTime)
		{
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
			if (t > startTime)
			{
				m_animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime;
				val = FadeCurve.Evaluate(m_animTimeTaken / lifeTime);
				beamMat.SetFloat("_BeamFade", val);
			}
		}
	}
}
