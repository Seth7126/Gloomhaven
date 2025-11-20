#define DEBUG
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt.Internal;

public class BoltPoll : MonoBehaviour
{
	public bool AllowImmediateShutdown = true;

	protected void Awake()
	{
		Application.runInBackground = true;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	protected void Update()
	{
		try
		{
			if (Time.timeScale != 1f && BoltRuntimeSettings.instance.overrideTimeScale)
			{
				BoltLog.Error("Time.timeScale value is incorrect: {0}f", Time.timeScale);
				Time.timeScale = 1f;
				BoltLog.Error("Time.timeScale has been set to 1.0f by Bolt");
			}
		}
		finally
		{
			BoltCore.Update();
		}
	}

	protected void FixedUpdate()
	{
		BoltCore._timer.Stop();
		BoltCore._timer.Reset();
		BoltCore._timer.Start();
		BoltCore.Poll();
		BoltCore._timer.Stop();
		DebugInfo.PollTime = DebugInfo.GetStopWatchElapsedMilliseconds(BoltCore._timer);
	}

	protected void OnDisable()
	{
		if (Application.isEditor && AllowImmediateShutdown)
		{
			BoltCore.ShutdownImmediate();
		}
	}

	protected void OnDestroy()
	{
		if (Application.isEditor && AllowImmediateShutdown)
		{
			BoltCore.ShutdownImmediate();
		}
	}

	protected void OnApplicationQuit()
	{
		BoltCore.Quit();
		if (AllowImmediateShutdown)
		{
			BoltCore.ShutdownImmediate();
		}
	}
}
