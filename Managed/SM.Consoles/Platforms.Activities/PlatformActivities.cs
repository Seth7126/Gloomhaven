#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Platforms.Utils;

namespace Platforms.Activities;

public class PlatformActivities : IPlatformActivities, IDisposable
{
	private readonly IActivitiesRequestsController _requestsController;

	private readonly Dictionary<string, IActivityBase> _baseActivitiesMap = new Dictionary<string, IActivityBase>();

	private readonly Dictionary<string, IActivity> _activitiesMap = new Dictionary<string, IActivity>();

	private readonly Dictionary<string, ITask> _tasksMap = new Dictionary<string, ITask>();

	private readonly Dictionary<string, ISubTask> _subTaskMap = new Dictionary<string, ISubTask>();

	private readonly Dictionary<string, IChallenge> _challengesMap = new Dictionary<string, IChallenge>();

	private readonly Dictionary<string, IActivity> _taskToActivityMap = new Dictionary<string, IActivity>();

	private readonly Dictionary<string, ITask> _subTaskToTaskMap = new Dictionary<string, ITask>();

	private readonly Dictionary<string, IActivity> _subTaskToActivityMap = new Dictionary<string, IActivity>();

	private readonly List<string> _idsBuffer = new List<string>();

	private readonly List<string> _inProgressIDsBuffer = new List<string>();

	private readonly List<string> _completeIDsBuffer = new List<string>();

	private readonly List<string> _filteredActivities = new List<string>();

	private readonly List<string> _removeBuffer = new List<string>();

	private List<string> _activeFilters = new List<string>();

	private ActivitiesProgressData _dependentData;

	private ActivitiesProgressData _independentData;

	private DebugFlags _sendDebug = DebugFlags.Log | DebugFlags.Warning | DebugFlags.Error;

	public DebugFlags SendDebug
	{
		get
		{
			return _sendDebug;
		}
		set
		{
			_sendDebug = value;
		}
	}

	public PlatformActivities(IActivitiesProvider activitiesProvider, IActivitiesRequestsController requestsController)
	{
		IActivity[] allActivities = activitiesProvider.GetAllActivities();
		IChallenge[] allChallenges = activitiesProvider.GetAllChallenges();
		_requestsController = requestsController;
		_dependentData = new ActivitiesProgressData();
		_independentData = new ActivitiesProgressData();
		_idsBuffer.Clear();
		_inProgressIDsBuffer.Clear();
		_completeIDsBuffer.Clear();
		_removeBuffer.Clear();
		SortActivitiesToMaps(allActivities, allChallenges);
		void SortActivitiesToMaps(IActivity[] activities, IChallenge[] challenges)
		{
			ClearActivitiesMaps();
			for (int i = 0; i < activities.Length; i++)
			{
				IActivity activity = activities[i];
				if (!IsUnique(activity.ID))
				{
					throw new Exception("[PlatformActivities] Activity ID " + activity.ID + " is not unique!");
				}
				_baseActivitiesMap.Add(activity.ID, activity);
				_activitiesMap.Add(activity.ID, activity);
				ITask[] tasks = activity.Tasks;
				foreach (ITask task in tasks)
				{
					if (!IsUnique(task.ID))
					{
						throw new Exception("[PlatformActivities] Task ID " + activity.ID + "/" + task.ID + " is not unique!");
					}
					_baseActivitiesMap.Add(task.ID, task);
					_tasksMap.Add(task.ID, task);
					_taskToActivityMap.Add(task.ID, activity);
					for (int k = 0; k < task.SubTasks.Length; k++)
					{
						ISubTask subTask = task.SubTasks[i];
						if (!IsUnique(subTask.ID))
						{
							throw new Exception("[PlatformActivities] SubTask ID " + activity.ID + "/" + task.ID + "/" + subTask.ID + " is not unique!");
						}
						_baseActivitiesMap.Add(subTask.ID, subTask);
						_subTaskMap.Add(subTask.ID, subTask);
						_subTaskToTaskMap.Add(subTask.ID, task);
						_subTaskToActivityMap.Add(subTask.ID, activity);
					}
				}
				bool IsUnique(string id)
				{
					return !_baseActivitiesMap.ContainsKey(id);
				}
			}
			foreach (IChallenge challenge in challenges)
			{
				_challengesMap.Add(challenge.ID, challenge);
			}
		}
	}

	public void Dispose()
	{
		ClearActivitiesMaps();
		_idsBuffer.Clear();
		_inProgressIDsBuffer.Clear();
		_completeIDsBuffer.Clear();
		_removeBuffer.Clear();
		_dependentData = null;
		_independentData = null;
	}

	public bool IsExists(string id)
	{
		return FindBaseActivity(id) != null;
	}

	public bool IsSaveIndependent(string id)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase == null)
		{
			_sendDebug.LogError("Invalid activity ID: " + id + ";");
			return false;
		}
		return activityBase.SaveIndependent;
	}

	public bool AllChildrenComplete(string id)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase == null)
		{
			_sendDebug.LogError("[PlatformActivities] There is no activity with ID \"" + id + "\"");
			return false;
		}
		if (!(activityBase is IActivity { Tasks: var tasks }))
		{
			if (!(activityBase is ITask { SubTasks: var subTasks }))
			{
				if (activityBase is ISubTask)
				{
					_sendDebug.LogWarning("[PlatformActivities] Activity \"" + id + "\" is a subtask and doesn't have any children");
					return true;
				}
				return false;
			}
			foreach (ISubTask activityBase2 in subTasks)
			{
				if (!ActivityComplete(activityBase2))
				{
					return false;
				}
			}
			return true;
		}
		foreach (ITask activityBase3 in tasks)
		{
			if (!ActivityComplete(activityBase3))
			{
				return false;
			}
		}
		return true;
		bool ActivityComplete(IActivityBase activityBase4)
		{
			ActivitiesProgressData activitiesProgressData = _dependentData;
			if (activityBase4.SaveIndependent)
			{
				activitiesProgressData = _independentData;
			}
			if ((activitiesProgressData.Ended.ContainsKey(activityBase4.ID) && activitiesProgressData.Ended[activityBase4.ID].Contains(ActivityResult.Completed)) || (activitiesProgressData.NeedsToBeSendAsEnded.ContainsKey(activityBase4.ID) && activitiesProgressData.NeedsToBeSendAsEnded[activityBase4.ID].Contains(ActivityResult.Completed)))
			{
				return true;
			}
			return false;
		}
	}

	public void MakeAvailable(string id)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase is IActivity)
		{
			OnActivityTryMakeAvailable(id);
		}
		else if (activityBase is ITask)
		{
			OnTaskTryMakeAvailable(id);
		}
		else if (activityBase is ISubTask)
		{
			OnSubTaskTryMakeAvailable(id);
		}
		else
		{
			OnChallengeTryMakeAvailable(id);
		}
	}

	public void MakeUnavailable(string id)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase is IActivity)
		{
			OnActivityTryMakeUnavailable(id);
		}
		else if (activityBase is ITask)
		{
			OnTaskTryMakeUnavailable(id);
		}
		else if (activityBase is ISubTask)
		{
			OnSubTaskTryMakeUnavailable(id);
		}
		else
		{
			OnChallengeTryMakeUnavailable(id);
		}
	}

	public void Start(string id)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase is IActivity)
		{
			OnActivityTryStart(id);
		}
		else if (activityBase is ITask)
		{
			OnTaskTryStart(id);
		}
		else if (activityBase is ISubTask)
		{
			OnSubTaskTryStart(id);
		}
		else
		{
			OnChallengeTryStart(id);
		}
	}

	public void End(string id, ActivityResult endResult)
	{
		IActivityBase activityBase = FindBaseActivity(id);
		if (activityBase is IActivity)
		{
			OnActivityTryEnd(id, endResult);
		}
		else if (activityBase is ITask)
		{
			OnTaskTryEnd(id, endResult);
		}
		else if (activityBase is ISubTask)
		{
			OnSubTaskTryEnd(id, endResult);
		}
	}

	public void End(string id, int score)
	{
		if (FindBaseActivity(id) is IChallenge)
		{
			OnChallengeTryEndWithScore(id, score);
		}
	}

	public void SetVisibilityFilters(string[] includeFilterTags)
	{
		_activeFilters.Clear();
		foreach (string item in includeFilterTags)
		{
			_activeFilters.Add(item);
		}
	}

	public ActivitiesProgressData GetCrossSaveProgress()
	{
		LogData("INDEPENDENT data", _independentData);
		return _independentData.Clone();
	}

	public void SetCrossSaveProgress(ActivitiesProgressData crossSaveProgress)
	{
		_independentData = crossSaveProgress.Clone();
		LogData("New INDEPENDENT data", _independentData);
	}

	public ActivitiesProgressData GetSaveRelatedProgress()
	{
		LogData("DEPENDENT data", _dependentData);
		return _dependentData.Clone();
	}

	public void SetSaveRelatedProgress(ActivitiesProgressData saveRelatedProgress)
	{
		_dependentData = saveRelatedProgress.Clone();
		LogData("New DEPENDENT data", _dependentData);
	}

	public void ClearActivitiesProgressView()
	{
		_sendDebug.Log("[PlatformActivities] Clearing current progress");
		_requestsController.SendTerminateProgress();
	}

	public void UpdateAvailableActivitiesView(ActivitiesProgressData saveRelatedProgressData, ActivitiesProgressData crossSaveProgressData)
	{
		UpdateAvailableActivities(saveRelatedProgressData, crossSaveProgressData);
	}

	public void UpdateResumedActivitiesView(ActivitiesProgressData progressData)
	{
		UpdateResumedActivities(progressData);
	}

	public void UpdateStartedActivitiesView(ActivitiesProgressData progressData)
	{
		UpdateStartedActivities(progressData);
	}

	public void UpdateEndedActivitiesView(ActivitiesProgressData progressData)
	{
		UpdateEndedActivities(progressData);
	}

	private void UpdateAvailableActivities(ActivitiesProgressData saveRelatedProgressData, ActivitiesProgressData crossSaveProgressData)
	{
		_sendDebug.Log("[PlatformActivities] UPDATE AVAILABLE ACTIVITIES INFO");
		_filteredActivities.Clear();
		FilterByTagsNonAloc(saveRelatedProgressData.Available, _activeFilters, _idsBuffer);
		_filteredActivities.AddRange(_idsBuffer);
		FilterByTagsNonAloc(crossSaveProgressData.Available, _activeFilters, _idsBuffer);
		_filteredActivities.AddRange(_idsBuffer);
		_requestsController.SendAvailable(_filteredActivities);
		_sendDebug.Log(string.Format("[{0}] All available({1}):", "PlatformActivities", saveRelatedProgressData.Available.Count + crossSaveProgressData.Available.Count));
		foreach (string item in saveRelatedProgressData.Available)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item + ";");
		}
		foreach (string item2 in crossSaveProgressData.Available)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item2 + ";");
		}
		string arg = string.Join(", ", _activeFilters);
		_sendDebug.Log(string.Format("[{0}] Filtered({1}) available({2}):", "PlatformActivities", arg, _filteredActivities.Count));
		foreach (string filteredActivity in _filteredActivities)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + filteredActivity + ";");
		}
		_sendDebug.Log("[PlatformActivities] ^^^^^^^^");
	}

	private void UpdateResumedActivities(ActivitiesProgressData data)
	{
		_sendDebug.Log(string.Format("[{0}] Resuming \"in progress\" activities({1}):", "PlatformActivities", data.Active.Count));
		foreach (string item in data.Active)
		{
			if (!IsActivity(item))
			{
				continue;
			}
			IActivity activity = _activitiesMap[item];
			_idsBuffer.Clear();
			ITask[] tasks = activity.Tasks;
			foreach (ITask task in tasks)
			{
				_idsBuffer.Add(task.ID);
				ISubTask[] subTasks = task.SubTasks;
				foreach (ISubTask subTask in subTasks)
				{
					_idsBuffer.Add(subTask.ID);
				}
			}
			GetInProgressChildren(data, _idsBuffer, _inProgressIDsBuffer);
			GetCompleteChildren(data, _idsBuffer, _completeIDsBuffer);
			SendActivityAsResumed(item, _inProgressIDsBuffer, _completeIDsBuffer);
		}
		static void GetCompleteChildren(ActivitiesProgressData savedData, IEnumerable<string> children, List<string> result)
		{
			result.Clear();
			foreach (string child in children)
			{
				if (savedData.Ended.ContainsKey(child) && savedData.Ended[child].Contains(ActivityResult.Completed))
				{
					result.Add(child);
				}
			}
		}
		static void GetInProgressChildren(ActivitiesProgressData savedData, IEnumerable<string> children, List<string> result)
		{
			result.Clear();
			foreach (string child2 in children)
			{
				if (savedData.Active.Contains(child2))
				{
					result.Add(child2);
				}
			}
		}
		bool IsActivity(string id)
		{
			return _activitiesMap.ContainsKey(id);
		}
	}

	private void UpdateStartedActivities(ActivitiesProgressData progressData)
	{
		_sendDebug.Log(string.Format("[{0}] Started activities({1}):", "PlatformActivities", progressData.NeedsTobeSendAsStarted.Count));
		_removeBuffer.Clear();
		foreach (string item in progressData.NeedsTobeSendAsStarted)
		{
			_removeBuffer.Add(item);
			SendBaseActivityAsStarted(item);
			_sendDebug.Log("[PlatformActivities]     ID - " + item + ";");
		}
		foreach (string item2 in _removeBuffer)
		{
			progressData.NeedsTobeSendAsStarted.Remove(item2);
		}
	}

	private void UpdateEndedActivities(ActivitiesProgressData progressData)
	{
		_sendDebug.Log(string.Format("[{0}] Need to send ended with score activities({1}):", "PlatformActivities", progressData.NeedsToBeSendAsEndedWithScore.Count));
		_removeBuffer.Clear();
		foreach (KeyValuePair<string, List<int>> item in progressData.NeedsToBeSendAsEndedWithScore)
		{
			string key = item.Key;
			foreach (int item2 in item.Value)
			{
				_sendDebug.Log("[PlatformActivities]     ID - " + key + ";");
				_sendDebug.Log(string.Format("[{0}]         Result - {1};", "PlatformActivities", item2));
				_removeBuffer.Add(key);
				SendBaseActivityAsEndedWithScore(key, item2);
			}
		}
		_sendDebug.Log(string.Format("[{0}] Send - {1};", "PlatformActivities", _removeBuffer.Count));
		foreach (string item3 in _removeBuffer)
		{
			progressData.NeedsToBeSendAsEndedWithScore.Remove(item3);
		}
		_sendDebug.Log(string.Format("[{0}] Need to send ended activities({1}):", "PlatformActivities", progressData.NeedsToBeSendAsEnded.Count));
		_removeBuffer.Clear();
		foreach (KeyValuePair<string, List<ActivityResult>> item4 in progressData.NeedsToBeSendAsEnded)
		{
			string key2 = item4.Key;
			_sendDebug.Log("[PlatformActivities]     ID - " + key2 + ";");
			foreach (ActivityResult item5 in item4.Value)
			{
				_sendDebug.Log(string.Format("[{0}]         Result - {1};", "PlatformActivities", item5));
				_removeBuffer.Add(key2);
				SendBaseActivityAsEnded(key2, item5);
			}
		}
		_sendDebug.Log(string.Format("[{0}] Send - {1};", "PlatformActivities", _removeBuffer.Count));
		foreach (string item6 in _removeBuffer)
		{
			progressData.NeedsToBeSendAsEnded.Remove(item6);
		}
	}

	private void ClearActivitiesMaps()
	{
		_baseActivitiesMap.Clear();
		_activitiesMap.Clear();
		_tasksMap.Clear();
		_subTaskMap.Clear();
		_taskToActivityMap.Clear();
		_subTaskToTaskMap.Clear();
		_subTaskToActivityMap.Clear();
		_challengesMap.Clear();
	}

	private void LogData(string dataLabel, ActivitiesProgressData data)
	{
		_sendDebug.Log("[PlatformActivities] " + dataLabel + ":");
		_sendDebug.Log(string.Format("[{0}] Available activities({1}):", "PlatformActivities", data.Available.Count));
		foreach (string item in data.Available)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item + ";");
		}
		_sendDebug.Log(string.Format("[{0}] In progress activities({1}):", "PlatformActivities", data.Active.Count));
		foreach (string item2 in data.Active)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item2 + ";");
		}
		_sendDebug.Log(string.Format("[{0}] Ended activities({1}):", "PlatformActivities", data.Ended.Count));
		foreach (KeyValuePair<string, List<ActivityResult>> item3 in data.Ended)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item3.Key + " with results:");
			foreach (ActivityResult item4 in item3.Value)
			{
				_sendDebug.Log(string.Format("[{0}]         Result - {1};", "PlatformActivities", item4));
			}
		}
		_sendDebug.Log(string.Format("[{0}] Ended with score activities({1}):", "PlatformActivities", data.EndedWithScore.Count));
		foreach (KeyValuePair<string, List<int>> item5 in data.EndedWithScore)
		{
			_sendDebug.Log("[PlatformActivities]     ID - " + item5.Key + " with score:");
			foreach (int item6 in item5.Value)
			{
				_sendDebug.Log(string.Format("[{0}]         Score - {1};", "PlatformActivities", item6));
			}
		}
	}

	private void OnActivityTryMakeAvailable(string activityID)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + activityID + ": OnActivityTryMakeAvailable");
		if (IsAvailable(activityID))
		{
			_sendDebug.Log("[PlatformActivities] Activity " + activityID + " already available");
		}
		else
		{
			MarkAsAvailable(activityID);
		}
	}

	private void OnActivityTryMakeUnavailable(string activityID)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + activityID + ": OnActivityTryMakeUnavailable");
		if (!IsAvailable(activityID))
		{
			_sendDebug.Log("[PlatformActivities] Activity " + activityID + " already unavailable");
			return;
		}
		IActivity activity = _activitiesMap[activityID];
		MarkAsUnavailable(activityID);
		ITask[] tasks = activity.Tasks;
		foreach (ITask task in tasks)
		{
			OnTaskTryMakeUnavailable(task.ID);
		}
	}

	private void OnActivityTryStart(string activityID)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + activityID + ": OnActivityTryStart");
		if (IsActive(activityID))
		{
			_sendDebug.Log("[PlatformActivities] Activity " + activityID + " already in progress");
			return;
		}
		MarkAsStarted(activityID);
		NeedsToBeSendAsStarted(activityID);
	}

	private void OnActivityTryEnd(string activityID, ActivityResult result)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + activityID + ": OnActivityTryEnd");
		if (!IsActive(activityID))
		{
			_sendDebug.LogError("[PlatformActivities] Activity " + activityID + " tries to end without start");
			return;
		}
		MarkAsEnded(activityID, result);
		NeedsToBeSendAsEnded(activityID, result);
		ITask[] tasks = _activitiesMap[activityID].Tasks;
		foreach (ITask task in tasks)
		{
			OnTaskTryEnd(task.ID, result);
		}
	}

	private void OnTaskTryMakeAvailable(string taskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + taskID + ": OnTaskTryMakeAvailable");
		if (IsAvailable(taskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + taskID + " already available");
			return;
		}
		IActivity activity = _taskToActivityMap[taskID];
		OnActivityTryMakeAvailable(activity.ID);
		MarkAsAvailable(taskID);
	}

	private void OnTaskTryMakeUnavailable(string taskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + taskID + ": OnTaskTryMakeUnavailable");
		if (!IsAvailable(taskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + taskID + " already unavailable");
			return;
		}
		MarkAsUnavailable(taskID);
		ISubTask[] subTasks = _tasksMap[taskID].SubTasks;
		foreach (ISubTask subTask in subTasks)
		{
			OnSubTaskTryMakeUnavailable(subTask.ID);
		}
	}

	private void OnTaskTryStart(string taskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + taskID + ": OnTaskTryStart");
		if (IsActive(taskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + taskID + " already in progress");
			return;
		}
		IActivity activity = _taskToActivityMap[taskID];
		OnActivityTryStart(activity.ID);
		MarkAsStarted(taskID);
		NeedsToBeSendAsStarted(taskID);
	}

	private void OnTaskTryEnd(string taskID, ActivityResult result)
	{
		_sendDebug.Log("[PlatformActivities] Task " + taskID + ": OnTaskTryEnd");
		if (!IsActive(taskID))
		{
			_sendDebug.LogError("[PlatformActivities] Task " + taskID + " tries to end without start");
			return;
		}
		MarkAsEnded(taskID, result);
		NeedsToBeSendAsEnded(taskID, result);
		ISubTask[] subTasks = _tasksMap[taskID].SubTasks;
		foreach (ISubTask subTask in subTasks)
		{
			OnSubTaskTryEnd(subTask.ID, result);
		}
	}

	private void OnSubTaskTryMakeAvailable(string subTaskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + subTaskID + ": OnSubTaskTryMakeAvailable");
		if (IsAvailable(subTaskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + subTaskID + " already available");
			return;
		}
		ITask task = _subTaskToTaskMap[subTaskID];
		OnTaskTryMakeAvailable(task.ID);
		MarkAsAvailable(subTaskID);
	}

	private void OnSubTaskTryMakeUnavailable(string subTaskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + subTaskID + ": OnSubTaskTryMakeUnavailable");
		if (!IsAvailable(subTaskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + subTaskID + " already unavailable");
		}
		else
		{
			MarkAsUnavailable(subTaskID);
		}
	}

	private void OnSubTaskTryStart(string subTaskID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + subTaskID + ": OnSubTaskTryStart");
		if (IsActive(subTaskID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + subTaskID + " already inprogress");
			return;
		}
		ITask task = _subTaskToTaskMap[subTaskID];
		OnTaskTryStart(task.ID);
		MarkAsStarted(subTaskID);
		NeedsToBeSendAsStarted(subTaskID);
	}

	private void OnSubTaskTryEnd(string subTaskID, ActivityResult result)
	{
		_sendDebug.Log("[PlatformActivities] Task " + subTaskID + ": OnSubTaskTryEnd");
		if (!IsActive(subTaskID))
		{
			_sendDebug.LogError("[PlatformActivities] Task " + subTaskID + " tries to end without start");
			return;
		}
		MarkAsEnded(subTaskID, result);
		NeedsToBeSendAsEnded(subTaskID, result);
	}

	private void OnChallengeTryMakeAvailable(string challengeID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + challengeID + ": OnChallengeTryMakeAvailable");
		if (IsAvailable(challengeID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + challengeID + " already available");
		}
		else
		{
			MarkAsAvailable(challengeID);
		}
	}

	private void OnChallengeTryMakeUnavailable(string challengeID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + challengeID + ": OnChallengeTryMakeUnavailable");
		if (!IsAvailable(challengeID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + challengeID + " already unavailable");
		}
		else
		{
			MarkAsUnavailable(challengeID);
		}
	}

	private void OnChallengeTryStart(string challengeID)
	{
		_sendDebug.Log("[PlatformActivities] Task " + challengeID + ": OnChallengeTryStart");
		if (IsActive(challengeID))
		{
			_sendDebug.Log("[PlatformActivities] Task " + challengeID + " already inprogress");
			return;
		}
		MarkAsStarted(challengeID);
		NeedsToBeSendAsStarted(challengeID);
	}

	private void OnChallengeTryEndWithScore(string challengeID, int score)
	{
		_sendDebug.Log(string.Format("[{0}] Task {1}: OnChallengeTryEndWithScore, {2};", "PlatformActivities", challengeID, score));
		if (!IsActive(challengeID))
		{
			_sendDebug.LogError("[PlatformActivities] Task " + challengeID + " tries to end without start");
			return;
		}
		MarkAsEndedWithScore(challengeID, score);
		NeedsToBeSendAsEndedWithScore(challengeID, score);
	}

	private IActivityBase FindBaseActivity(string baseActivityID)
	{
		if (_activitiesMap.ContainsKey(baseActivityID))
		{
			return _activitiesMap[baseActivityID];
		}
		if (_tasksMap.ContainsKey(baseActivityID))
		{
			return _tasksMap[baseActivityID];
		}
		if (_subTaskMap.ContainsKey(baseActivityID))
		{
			return _subTaskMap[baseActivityID];
		}
		if (_challengesMap.ContainsKey(baseActivityID))
		{
			return _challengesMap[baseActivityID];
		}
		return null;
	}

	private bool IsAvailable(string baseActivityID)
	{
		if (!_dependentData.Available.Contains(baseActivityID))
		{
			return _independentData.Available.Contains(baseActivityID);
		}
		return true;
	}

	private void MarkAsAvailable(string baseActivityID)
	{
		if (FindBaseActivity(baseActivityID).SaveIndependent)
		{
			_independentData.Available.Add(baseActivityID);
		}
		else
		{
			_dependentData.Available.Add(baseActivityID);
		}
		_sendDebug.Log("[PlatformActivities] Activity " + baseActivityID + " marked as available");
	}

	private void MarkAsUnavailable(string baseActivityID)
	{
		if (FindBaseActivity(baseActivityID).SaveIndependent)
		{
			_independentData.Available.Remove(baseActivityID);
		}
		else
		{
			_dependentData.Available.Remove(baseActivityID);
		}
		_sendDebug.Log("[PlatformActivities] Activity " + baseActivityID + " marked as unavailable");
	}

	private bool IsActive(string baseActivityID)
	{
		if (!_dependentData.Active.Contains(baseActivityID))
		{
			return _independentData.Active.Contains(baseActivityID);
		}
		return true;
	}

	private void MarkAsStarted(string baseActivityID)
	{
		if (FindBaseActivity(baseActivityID).SaveIndependent)
		{
			_independentData.Active.Add(baseActivityID);
		}
		else
		{
			_dependentData.Active.Add(baseActivityID);
		}
		_sendDebug.Log("[PlatformActivities] Activity " + baseActivityID + " marked as active");
	}

	private bool WasEnded(string baseActivityID)
	{
		if (!_dependentData.Ended.ContainsKey(baseActivityID))
		{
			return _independentData.Ended.ContainsKey(baseActivityID);
		}
		return true;
	}

	private bool WasEnded(string baseActivityID, out int count)
	{
		bool flag = _dependentData.Ended.ContainsKey(baseActivityID);
		bool flag2 = _independentData.Ended.ContainsKey(baseActivityID);
		bool num = flag || flag2;
		if (!num)
		{
			count = 0;
			return num;
		}
		if (flag)
		{
			count = _dependentData.Ended[baseActivityID].Count;
			return num;
		}
		count = _independentData.Ended[baseActivityID].Count;
		return num;
	}

	private void MarkAsEnded(string baseActivityID, ActivityResult result)
	{
		ActivitiesProgressData activitiesProgressData = (FindBaseActivity(baseActivityID).SaveIndependent ? _independentData : _dependentData);
		activitiesProgressData.Active.Remove(baseActivityID);
		if (!activitiesProgressData.Ended.ContainsKey(baseActivityID))
		{
			activitiesProgressData.Ended.Add(baseActivityID, new List<ActivityResult>());
		}
		activitiesProgressData.Ended[baseActivityID].Add(result);
		_sendDebug.Log(string.Format("[{0}] Activity {1} marked as ended - {2};", "PlatformActivities", baseActivityID, result));
	}

	private void MarkAsEndedWithScore(string baseActivityID, int score)
	{
		ActivitiesProgressData activitiesProgressData = (FindBaseActivity(baseActivityID).SaveIndependent ? _independentData : _dependentData);
		activitiesProgressData.Active.Remove(baseActivityID);
		if (!activitiesProgressData.EndedWithScore.ContainsKey(baseActivityID))
		{
			activitiesProgressData.EndedWithScore.Add(baseActivityID, new List<int>());
		}
		activitiesProgressData.EndedWithScore[baseActivityID].Add(score);
		_sendDebug.Log(string.Format("[{0}] Activity {1} marked as ended with score - {2};", "PlatformActivities", baseActivityID, score));
	}

	private void NeedsToBeSendAsStarted(string baseActivityID)
	{
		List<string> list = (FindBaseActivity(baseActivityID).SaveIndependent ? _independentData.NeedsTobeSendAsStarted : _dependentData.NeedsTobeSendAsStarted);
		if (!list.Contains(baseActivityID))
		{
			_sendDebug.Log("[PlatformActivities] Activity " + baseActivityID + " needs to be started");
			list.Add(baseActivityID);
		}
	}

	private void SendBaseActivityAsStarted(string baseActivityToSend)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + baseActivityToSend + " started");
		_requestsController.SendStart(baseActivityToSend);
	}

	private void NeedsToBeSendAsEnded(string baseActivityID, ActivityResult result)
	{
		Dictionary<string, List<ActivityResult>> dictionary = (FindBaseActivity(baseActivityID).SaveIndependent ? _independentData.NeedsToBeSendAsEnded : _dependentData.NeedsToBeSendAsEnded);
		_sendDebug.Log(string.Format("[{0}] Activity {1} needs to be ended with result {2};", "PlatformActivities", baseActivityID, result));
		if (!dictionary.ContainsKey(baseActivityID))
		{
			dictionary.Add(baseActivityID, new List<ActivityResult>());
		}
		dictionary[baseActivityID].Add(result);
	}

	private void NeedsToBeSendAsEndedWithScore(string baseActivityID, int score)
	{
		Dictionary<string, List<int>> dictionary = (FindBaseActivity(baseActivityID).SaveIndependent ? _independentData.NeedsToBeSendAsEndedWithScore : _dependentData.NeedsToBeSendAsEndedWithScore);
		_sendDebug.Log(string.Format("[{0}] Activity {1} needs to be ended with score {2};", "PlatformActivities", baseActivityID, score));
		if (!dictionary.ContainsKey(baseActivityID))
		{
			dictionary.Add(baseActivityID, new List<int>());
		}
		dictionary[baseActivityID].Add(score);
	}

	private void SendBaseActivityAsEnded(string baseActivityToSend, ActivityResult resultToSend)
	{
		_sendDebug.Log(string.Format("[{0}] Activity {1} ended with result {2};", "PlatformActivities", baseActivityToSend, resultToSend));
		Outcome outcome = Outcome.Failed;
		switch (resultToSend)
		{
		case ActivityResult.Completed:
			outcome = Outcome.Completed;
			break;
		case ActivityResult.Abandoned:
			outcome = Outcome.Abandoned;
			break;
		}
		_requestsController.SendEnd(baseActivityToSend, outcome);
	}

	private void SendBaseActivityAsEndedWithScore(string baseActivityToSend, int scoreToSend)
	{
		_sendDebug.Log(string.Format("[{0}] Activity {1} ended with score {2};", "PlatformActivities", baseActivityToSend, scoreToSend));
		_requestsController.SendEndWithScore(baseActivityToSend, scoreToSend, 0);
	}

	private void FilterByTagsNonAloc(List<string> listToFilter, List<string> includeTags, List<string> result)
	{
		result.Clear();
		foreach (string item in listToFilter)
		{
			IActivityBase activityBase = (_baseActivitiesMap.ContainsKey(item) ? _baseActivitiesMap[item] : ((!_challengesMap.ContainsKey(item)) ? null : _challengesMap[item]));
			if (activityBase == null)
			{
				_sendDebug.LogError("There is no activity or challenge with key " + item + "!");
				continue;
			}
			bool flag = true;
			foreach (string includeTag in includeTags)
			{
				bool flag2 = false;
				string[] filterTags = activityBase.FilterTags;
				for (int i = 0; i < filterTags.Length; i++)
				{
					if (filterTags[i].Equals(includeTag))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result.Add(activityBase.ID);
			}
		}
	}

	private void SendActivityAsResumed(string activityToSend, List<string> inProgressChildren, List<string> completedChildren)
	{
		_sendDebug.Log("[PlatformActivities] Activity " + activityToSend + " resumed");
		_sendDebug.Log("[PlatformActivities]     InProgress children:");
		foreach (string inProgressChild in inProgressChildren)
		{
			_sendDebug.Log("[PlatformActivities]         ID - " + inProgressChild + ";");
		}
		_sendDebug.Log("[PlatformActivities]     Completed children:");
		foreach (string completedChild in completedChildren)
		{
			_sendDebug.Log("[PlatformActivities]         ID - " + completedChild + ";");
		}
		_requestsController.SendResume(activityToSend, inProgressChildren, completedChildren);
	}
}
