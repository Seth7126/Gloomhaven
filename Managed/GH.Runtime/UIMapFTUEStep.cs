#define ENABLE_LOGS
using System;
using System.Collections;
using Assets.Script.Misc;
using GLOO.Introduction;
using UnityEngine;

public class UIMapFTUEStep : UIIntroduceBase, IMapFTUEStep
{
	[SerializeField]
	private MapFTUEStepConfigUI config;

	private CallbackPromise promise;

	private Coroutine waitCoroutine;

	public EMapFTUEStep Step => config.Phase;

	protected bool IsActive
	{
		get
		{
			if (promise != null)
			{
				return promise.IsPending;
			}
			return false;
		}
	}

	public override void Show(Action onFinished = null)
	{
		Debug.LogGUI($"Show Step {Step}");
		Show(config, onFinished);
	}

	public virtual ICallbackPromise StartStep()
	{
		if (IsActive)
		{
			Debug.LogGUI($"Already Started Step {Step}");
			return promise;
		}
		promise = new CallbackPromise();
		StopWait();
		Debug.LogGUI($"Start Step {Step}");
		if (config.DelayStart > 0f)
		{
			waitCoroutine = StartCoroutine(WaitStart(config.DelayStart));
		}
		else
		{
			Show(OnFinishedStep);
		}
		return promise;
	}

	private IEnumerator WaitStart(float delay)
	{
		Debug.LogGUI($"Wait to show Step {Step}");
		InputManager.RequestDisableInput(this, KeyAction.UI_PAUSE);
		Singleton<InteractabilityChecker>.Instance.RequestActive(this, active: true);
		yield return new WaitForSeconds(delay);
		Singleton<InteractabilityChecker>.Instance.RequestActive(this, active: false);
		waitCoroutine = null;
		if (this is UIMapFTUEInitialStep)
		{
			while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
			{
				yield return null;
			}
		}
		Show(OnFinishedStep);
		InputManager.RequestEnableInput(this, KeyAction.UI_PAUSE);
	}

	public virtual void FinishStep()
	{
		Debug.LogGUI($"FInish Step {Step}");
		if (waitCoroutine != null)
		{
			OnFinishedStep();
		}
		else
		{
			Hide();
		}
	}

	protected virtual void OnFinishedStep()
	{
		if (IsActive)
		{
			Debug.LogGUI($"FInished Step {Step}");
			StopWait();
			promise.Resolve();
			promise = null;
		}
	}

	public void SetStepConfig(MapFTUEStepConfigUI config)
	{
		this.config = config;
	}

	private void StopWait()
	{
		if (waitCoroutine != null)
		{
			Debug.LogGUI($"Cancel wait Start Step {Step}");
			Singleton<InteractabilityChecker>.Instance.RequestActive(this, active: false);
			InputManager.RequestEnableInput(this, KeyAction.UI_PAUSE);
			StopCoroutine(waitCoroutine);
		}
		waitCoroutine = null;
	}
}
