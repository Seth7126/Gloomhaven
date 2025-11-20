#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Platforms.Activities;
using SM.Utils;

namespace Platforms.Generic;

public class ActivitiesRequestsControllerGeneric : IActivitiesRequestsController
{
	public void SendAvailable(List<string> availableActivities, Action<bool> onResult = null)
	{
		string text = "[Activities] SendAvailable:";
		foreach (string availableActivity in availableActivities)
		{
			text = string.Concat(text, "\n" + availableActivity);
		}
		text += "\n===";
		LogUtils.Log(text);
		onResult?.Invoke(obj: true);
	}

	public void SendStart(string activityId, Action<bool> onResult = null)
	{
		LogUtils.Log("[Activities] SendStart: " + activityId);
		onResult?.Invoke(obj: true);
	}

	public void SendResume(string activityId, List<string> inProgressChildren, List<string> completeChildren, Action<bool> onResult = null)
	{
		string text = "[Activities] SendResume: " + activityId;
		text += "\n\n inProgressChildren:";
		foreach (string inProgressChild in inProgressChildren)
		{
			text = string.Concat(text, "\n" + inProgressChild);
		}
		text += "\n\n completeChildren:";
		foreach (string completeChild in completeChildren)
		{
			text = string.Concat(text, "\n" + completeChild);
		}
		text += "\n===";
		LogUtils.Log(text);
		onResult?.Invoke(obj: true);
	}

	public void SendEnd(string activityId, Outcome outcome, Action<bool> onResult = null)
	{
		LogUtils.Log($"[Activities] SendEnd: {activityId} - {outcome}");
		onResult?.Invoke(obj: true);
	}

	public void SendEndWithScore(string activityId, int score, int difficulty, Action<bool> onResult = null)
	{
		LogUtils.Log($"[Activities] SendEndWithScore: {activityId} - {score} - {difficulty}");
		onResult?.Invoke(obj: true);
	}

	public void SendTerminateProgress(Action<bool> onResult = null)
	{
		LogUtils.Log("[Activities] SendTerminateProgress");
		onResult?.Invoke(obj: true);
	}
}
