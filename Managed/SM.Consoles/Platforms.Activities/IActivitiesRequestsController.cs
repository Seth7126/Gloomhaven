using System;
using System.Collections.Generic;

namespace Platforms.Activities;

public interface IActivitiesRequestsController
{
	void SendAvailable(List<string> availableActivities, Action<bool> onResult = null);

	void SendStart(string activityId, Action<bool> onResult = null);

	void SendResume(string activityId, List<string> inProgressChildren, List<string> completeChildren, Action<bool> onResult = null);

	void SendEnd(string activityId, Outcome outcome, Action<bool> onResult = null);

	void SendEndWithScore(string activityId, int score, int difficulty, Action<bool> onResult = null);

	void SendTerminateProgress(Action<bool> onResult = null);
}
