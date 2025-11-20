using System.Collections;
using System.Diagnostics;
using SRDebugger.Services;
using SRF;
using SRF.Service;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRDebugger.Profiler;

public class SRPProfilerService : SRServiceBase<IProfilerService>, IProfilerService
{
	private const int FrameBufferSize = 400;

	private readonly CircularBuffer<ProfilerFrame> _frameBuffer = new CircularBuffer<ProfilerFrame>(400);

	private ProfilerLateUpdateListener _lateUpdateListener;

	private double _updateDuration;

	private double _renderStartTime;

	private double _renderDuration;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	public float AverageFrameTime { get; private set; }

	public float LastFrameTime { get; private set; }

	public CircularBuffer<ProfilerFrame> FrameBuffer => _frameBuffer;

	protected override void Awake()
	{
		base.Awake();
		_lateUpdateListener = base.gameObject.AddComponent<ProfilerLateUpdateListener>();
		_lateUpdateListener.OnLateUpdate = OnLateUpdate;
		base.CachedGameObject.hideFlags = HideFlags.NotEditable;
		base.CachedTransform.SetParent(Hierarchy.Get("SRDebugger"), worldPositionStays: true);
		RenderPipelineManager.beginFrameRendering += RenderPipelineOnBeginFrameRendering;
		StartCoroutine(EndOfFrameCoroutine());
	}

	protected override void Update()
	{
		base.Update();
		EndFrame();
		if (FrameBuffer.Count > 0)
		{
			ProfilerFrame value = FrameBuffer.Back();
			value.FrameTime = Time.unscaledDeltaTime;
			FrameBuffer[FrameBuffer.Count - 1] = value;
		}
		LastFrameTime = Time.unscaledDeltaTime;
		int num = Mathf.Min(20, FrameBuffer.Count);
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			num2 += FrameBuffer[FrameBuffer.Count - 1 - i].FrameTime;
		}
		AverageFrameTime = (float)num2 / (float)num;
		_stopwatch.Start();
	}

	private IEnumerator EndOfFrameCoroutine()
	{
		WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
		while (true)
		{
			yield return endOfFrame;
			_renderDuration = _stopwatch.Elapsed.TotalSeconds - _renderStartTime;
		}
	}

	protected void PushFrame(double totalTime, double updateTime, double renderTime)
	{
		_frameBuffer.PushBack(new ProfilerFrame
		{
			OtherTime = totalTime - updateTime - renderTime,
			UpdateTime = updateTime,
			RenderTime = renderTime
		});
	}

	private void OnLateUpdate()
	{
		_updateDuration = _stopwatch.Elapsed.TotalSeconds;
	}

	private void RenderPipelineOnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		_renderStartTime = _stopwatch.Elapsed.TotalSeconds;
	}

	private void EndFrame()
	{
		if (_stopwatch.IsRunning)
		{
			PushFrame(_stopwatch.Elapsed.TotalSeconds, _updateDuration, _renderDuration);
			_stopwatch.Reset();
			_stopwatch.Start();
		}
		_updateDuration = (_renderDuration = 0.0);
	}
}
