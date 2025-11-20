using UnityEngine.Events;

namespace UnityEngine.UI.Tweens;

public struct Vector3Tween : ITweenValue
{
	public class Vector3TweenCallback : UnityEvent<Vector3>
	{
	}

	public class Vector3TweenFinishCallback : UnityEvent
	{
	}

	private Vector3 m_StartVector3;

	private Vector3 m_TargetVector3;

	private float m_Duration;

	private bool m_IgnoreTimeScale;

	private TweenEasing m_Easing;

	private Vector3TweenCallback m_Target;

	private Vector3TweenFinishCallback m_Finish;

	public Vector3 startVector3
	{
		get
		{
			return m_StartVector3;
		}
		set
		{
			m_StartVector3 = value;
		}
	}

	public Vector3 targetVector3
	{
		get
		{
			return m_TargetVector3;
		}
		set
		{
			m_TargetVector3 = value;
		}
	}

	public float duration
	{
		get
		{
			return m_Duration;
		}
		set
		{
			m_Duration = value;
		}
	}

	public bool ignoreTimeScale
	{
		get
		{
			return m_IgnoreTimeScale;
		}
		set
		{
			m_IgnoreTimeScale = value;
		}
	}

	public TweenEasing easing
	{
		get
		{
			return m_Easing;
		}
		set
		{
			m_Easing = value;
		}
	}

	public void TweenValue(float floatPercentage)
	{
		if (ValidTarget())
		{
			m_Target.Invoke(Vector3.Lerp(m_StartVector3, m_TargetVector3, floatPercentage));
		}
	}

	public void AddOnChangedCallback(UnityAction<Vector3> callback)
	{
		if (m_Target == null)
		{
			m_Target = new Vector3TweenCallback();
		}
		m_Target.AddListener(callback);
	}

	public void AddOnFinishCallback(UnityAction callback)
	{
		if (m_Finish == null)
		{
			m_Finish = new Vector3TweenFinishCallback();
		}
		m_Finish.AddListener(callback);
	}

	public bool GetIgnoreTimescale()
	{
		return m_IgnoreTimeScale;
	}

	public float GetDuration()
	{
		return m_Duration;
	}

	public bool ValidTarget()
	{
		return m_Target != null;
	}

	public void Finished()
	{
		if (m_Finish != null)
		{
			m_Finish.Invoke();
		}
	}
}
