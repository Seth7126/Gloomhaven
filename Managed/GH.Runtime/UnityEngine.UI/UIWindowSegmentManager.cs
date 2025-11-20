using System.Collections.Generic;
using Chronos;
using JetBrains.Annotations;
using MEC;

namespace UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIWindowSegmentManager : MonoBehaviour
{
	[SerializeField]
	private float transitionInterval;

	[SerializeField]
	private List<UIWindowSegmentWrapper> windowSegments;

	private UIWindow myWindow;

	private bool forceTransition;

	[UsedImplicitly]
	private void Awake()
	{
		myWindow = GetComponent<UIWindow>();
		myWindow.onTransitionBegin.AddListener(OnWindowTransitioningStarted);
		myWindow.onHidden.AddListener(delegate
		{
			ActivateAllSegments(activate: false);
		});
		ActivateAllSegments(activate: false);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		myWindow.onTransitionBegin.RemoveListener(OnWindowTransitioningStarted);
		myWindow.onHidden.RemoveAllListeners();
	}

	private void ActivateAllSegments(bool activate)
	{
		for (int i = 0; i < windowSegments.Count; i++)
		{
			for (int j = 0; j < windowSegments[i].myList.Count; j++)
			{
				windowSegments[i].myList[j].gameObject.SetActive(activate);
			}
		}
	}

	public void StartTransitioningProcess()
	{
		Timing.RunCoroutine(ProcessTransitions());
	}

	private IEnumerator<float> ProcessTransitions()
	{
		int segmentIndex = 0;
		float nextTransitionStartTime = GetNextTransitionStartTime(segmentIndex);
		while (segmentIndex < windowSegments.Count)
		{
			if ((Timekeeper.instance.m_GlobalClock.time >= nextTransitionStartTime && !windowSegments[segmentIndex].manuallyTransitionGroup) || forceTransition)
			{
				for (int i = 0; i < windowSegments[segmentIndex].myList.Count; i++)
				{
					windowSegments[segmentIndex].myList[i].StartTransition(windowSegments[segmentIndex].groupTransitionType);
				}
				segmentIndex++;
				forceTransition = false;
				nextTransitionStartTime = GetNextTransitionStartTime(segmentIndex);
			}
			yield return 0f;
		}
	}

	private float GetNextTransitionStartTime(int segmentIndex)
	{
		float result = 0f;
		if (segmentIndex < windowSegments.Count)
		{
			result = Timekeeper.instance.m_GlobalClock.time + (windowSegments[segmentIndex].useGroupTransitionInterval ? windowSegments[segmentIndex].groupTransitionInterval : transitionInterval);
		}
		return result;
	}

	public void TriggerTransition()
	{
		forceTransition = true;
	}

	private void OnWindowTransitioningStarted(UIWindow window, UIWindow.VisualState visualState, bool isInstant)
	{
		if (visualState == UIWindow.VisualState.Shown)
		{
			Timing.RunCoroutine(ProcessTransitions());
		}
	}
}
