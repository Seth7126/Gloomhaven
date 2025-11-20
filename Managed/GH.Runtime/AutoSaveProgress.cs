using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveProgress : Singleton<AutoSaveProgress>
{
	private static readonly int _mask = Shader.PropertyToID("_Mask");

	private const float c_MinDisplayTime = 3f;

	private int currentSavingsInProgress;

	[SerializeField]
	private GameObject icon;

	[Header("Animation")]
	[SerializeField]
	private Image shineImage;

	[SerializeField]
	private float shineDelay = 1f;

	[SerializeField]
	private float shineDuration = 1f;

	[SerializeField]
	private Vector2 shineFrom;

	[SerializeField]
	private Vector2 shineTo;

	private LTDescr loopAnimation;

	private bool isShowing;

	private float startTime;

	private const string DebugCancel = "AutoSave";

	public bool IsShowing => isShowing;

	protected override void Awake()
	{
		base.Awake();
		icon.SetActive(value: false);
		shineImage.material = new Material(shineImage.material);
		shineImage.material.SetTextureOffset(_mask, shineFrom);
	}

	[ContextMenu("Show Progress")]
	public void ShowProgress()
	{
		if (Thread.CurrentThread == SceneController.Instance.MainThread)
		{
			currentSavingsInProgress++;
			if (!icon.activeSelf)
			{
				StartAnimation();
			}
		}
	}

	[ContextMenu("Hide Progress")]
	public void HideProgress()
	{
		if (Thread.CurrentThread == SceneController.Instance.MainThread)
		{
			currentSavingsInProgress = Mathf.Max(0, currentSavingsInProgress - 1);
			if (currentSavingsInProgress == 0)
			{
				StartCoroutine(StopAnimation(waitForMinTime: true));
			}
		}
	}

	private void StartAnimation()
	{
		if (!isShowing)
		{
			startTime = Time.realtimeSinceStartup;
			isShowing = true;
			icon.SetActive(value: true);
			CancelAnimation();
			LoopAnimation();
		}
	}

	private void LoopAnimation()
	{
		loopAnimation = LeanTween.value(icon, delegate(Vector2 val)
		{
			shineImage.material.SetTextureOffset(_mask, val);
		}, shineFrom, shineTo, shineDuration).setDelay(shineDelay).setOnComplete(LoopAnimation);
	}

	private IEnumerator StopAnimation(bool waitForMinTime)
	{
		if (!isShowing)
		{
			yield break;
		}
		if (waitForMinTime)
		{
			while (Time.realtimeSinceStartup - startTime < 3f)
			{
				yield return null;
			}
		}
		isShowing = false;
		CancelAnimation();
		icon.SetActive(value: false);
	}

	private void CancelAnimation()
	{
		if (loopAnimation != null)
		{
			LeanTween.cancel(loopAnimation.id, "AutoSave");
			loopAnimation = null;
		}
	}

	private void OnDisable()
	{
		isShowing = false;
		CancelAnimation();
		icon.SetActive(value: false);
	}
}
