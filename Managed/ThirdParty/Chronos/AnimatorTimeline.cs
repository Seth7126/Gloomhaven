using UnityEngine;

namespace Chronos;

public class AnimatorTimeline : ComponentTimeline<Animator>
{
	private float _speed;

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
			AdjustProperties();
		}
	}

	private int recordedFrames => Mathf.Clamp((int)(base.timeline.recordingDuration * 60f), 1, 10000);

	public AnimatorTimeline(Timeline timeline, Animator component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Animator source)
	{
		_speed = source.speed;
	}

	public override void AdjustProperties(float timeScale)
	{
		if (timeScale > 0f)
		{
			base.component.speed = speed * timeScale;
		}
		else
		{
			base.component.speed = 0f;
		}
	}

	public override void OnStartOrReEnable()
	{
		if (base.timeline.rewindable)
		{
			base.component.StartRecording(recordedFrames);
		}
	}

	public override void OnDisable()
	{
		if (base.timeline.rewindable)
		{
			base.component.StopRecording();
		}
	}

	public override void Update()
	{
		if (!base.timeline.rewindable)
		{
			return;
		}
		float timeScale = base.timeline.timeScale;
		if (base.timeline.lastTimeScale >= 0f && timeScale < 0f)
		{
			base.component.StopRecording();
			if (base.component.recorderStartTime < 0f)
			{
				Debug.LogWarning("Animator timeline failed to record for unknown reasons.\nSee: http://forum.unity3d.com/threads/341203/", base.component);
				return;
			}
			base.component.StartPlayback();
			base.component.playbackTime = base.component.recorderStopTime;
		}
		else if (base.component.recorderMode == AnimatorRecorderMode.Playback && timeScale > 0f)
		{
			base.component.StopPlayback();
			base.component.StartRecording(recordedFrames);
		}
		else if (timeScale < 0f && base.component.recorderMode == AnimatorRecorderMode.Playback)
		{
			float playbackTime = Mathf.Max(base.component.recorderStartTime, base.component.playbackTime + base.timeline.deltaTime);
			base.component.playbackTime = playbackTime;
		}
	}
}
