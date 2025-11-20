using UnityEngine.Events;

namespace UnityEngine.UI.Tweens;

public struct Vector2Tween : ITweenValue
{
	public class Vector2TweenCallback : UnityEvent<Vector2>
	{
	}

	public class Vector2TweenFinishCallback : UnityEvent
	{
	}

	private Vector2 m_StartVector2;

	private Vector2 m_TargetVector2;

	private float m_Duration;

	private bool m_IgnoreTimeScale;

	private TweenEasing m_Easing;

	private Vector2TweenCallback m_Target;

	private Vector2TweenFinishCallback m_Finish;

	public Vector2 startVector2
	{
		get
		{
			return m_StartVector2;
		}
		set
		{
			m_StartVector2 = value;
		}
	}

	public Vector2 targetVector2
	{
		get
		{
			return m_TargetVector2;
		}
		set
		{
			m_TargetVector2 = value;
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
			m_Target.Invoke(Vector2.Lerp(m_StartVector2, m_TargetVector2, floatPercentage));
		}
	}

	public void AddOnChangedCallback(UnityAction<Vector2> callback)
	{
		if (m_Target == null)
		{
			m_Target = new Vector2TweenCallback();
		}
		m_Target.AddListener(callback);
	}

	public void AddOnFinishCallback(UnityAction callback)
	{
		if (m_Finish == null)
		{
			m_Finish = new Vector2TweenFinishCallback();
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
