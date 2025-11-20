using System;
using System.Globalization;
using UnityEngine;

namespace FrameTimeLogger;

public class FrameTimeRecorder : Singleton<FrameTimeRecorder>
{
	private CultureInfo _cultureInfo;

	private readonly SimpleLogger<string> _logger = new SimpleLogger<string>(new SimpleCSVConverter(), GetWriter());

	private DateTime _recordStartTime;

	private float _recordStartTotalTime;

	public bool IsRecording => _recordStartTime > DateTime.MinValue;

	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		_cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
	}

	private static ICSVWriter GetWriter()
	{
		return new EditorCSVWriter();
	}

	[ContextMenu("StartRecord")]
	public void StartRecord()
	{
		_recordStartTime = DateTime.Now;
		_recordStartTotalTime = Time.time;
	}

	[ContextMenu("StopRecording")]
	public void StopRecord()
	{
		_logger.SaveLog($"FrameTimeLog_{_recordStartTime:dd.MM.yyyy_HH.mm.ss}_{(int)(DateTime.Now - _recordStartTime).TotalSeconds}s", "FrameTime(ms)");
		_recordStartTime = DateTime.MinValue;
	}
}
