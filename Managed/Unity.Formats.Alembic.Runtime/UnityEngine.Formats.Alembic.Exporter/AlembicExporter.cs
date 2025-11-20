using System.Collections;
using UnityEngine.Formats.Alembic.Util;

namespace UnityEngine.Formats.Alembic.Exporter;

[ExecuteInEditMode]
public class AlembicExporter : MonoBehaviour
{
	[SerializeField]
	private AlembicRecorder m_recorder = new AlembicRecorder();

	[SerializeField]
	private bool m_captureOnStart = true;

	[SerializeField]
	private bool m_ignoreFirstFrame = true;

	[SerializeField]
	private int m_maxCaptureFrame;

	private int m_prevFrame;

	private bool m_firstFrame;

	public AlembicRecorder Recorder => m_recorder;

	public bool CaptureOnStart
	{
		get
		{
			return m_captureOnStart;
		}
		set
		{
			m_captureOnStart = value;
		}
	}

	public bool IgnoreFirstFrame
	{
		get
		{
			return m_ignoreFirstFrame;
		}
		set
		{
			m_ignoreFirstFrame = value;
		}
	}

	public int MaxCaptureFrame
	{
		get
		{
			return m_maxCaptureFrame;
		}
		set
		{
			m_maxCaptureFrame = value;
		}
	}

	private void InitializeOutputPath()
	{
		AlembicRecorderSettings settings = m_recorder.Settings;
		if (string.IsNullOrEmpty(settings.OutputPath))
		{
			settings.OutputPath = "Output/" + base.gameObject.name + ".abc";
		}
	}

	private IEnumerator ProcessRecording()
	{
		yield return new WaitForEndOfFrame();
		if (!m_recorder.Recording || Time.frameCount == m_prevFrame)
		{
			yield break;
		}
		m_prevFrame = Time.frameCount;
		if (m_captureOnStart && m_ignoreFirstFrame && m_firstFrame)
		{
			m_firstFrame = false;
			yield break;
		}
		m_recorder.ProcessRecording();
		if (m_maxCaptureFrame > 0 && m_recorder.FrameCount >= m_maxCaptureFrame)
		{
			EndRecording();
		}
	}

	public void BeginRecording()
	{
		m_firstFrame = true;
		m_prevFrame = -1;
		m_recorder.BeginRecording();
		AlembicExporterAnalytics.SendAnalytics(m_recorder.Settings);
	}

	public void EndRecording()
	{
		m_recorder.EndRecording();
	}

	public void OneShot()
	{
		BeginRecording();
		m_recorder.ProcessRecording();
		EndRecording();
	}

	private void OnEnable()
	{
		InitializeOutputPath();
	}

	private void Start()
	{
		if (m_captureOnStart)
		{
			BeginRecording();
		}
	}

	private void Update()
	{
		if (m_recorder.Recording)
		{
			StartCoroutine(ProcessRecording());
		}
	}

	private void OnDisable()
	{
		EndRecording();
	}

	private void OnDestroy()
	{
		if (Recorder != null)
		{
			Recorder.Dispose();
		}
	}
}
