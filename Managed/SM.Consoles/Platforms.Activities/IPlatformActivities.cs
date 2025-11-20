namespace Platforms.Activities;

public interface IPlatformActivities
{
	bool IsExists(string id);

	bool IsSaveIndependent(string id);

	bool AllChildrenComplete(string id);

	void MakeAvailable(string id);

	void MakeUnavailable(string id);

	void Start(string id);

	void End(string id, ActivityResult endResult);

	void End(string id, int score);

	void SetVisibilityFilters(string[] includeFilterTags);

	ActivitiesProgressData GetCrossSaveProgress();

	void SetCrossSaveProgress(ActivitiesProgressData crossSaveProgress);

	ActivitiesProgressData GetSaveRelatedProgress();

	void SetSaveRelatedProgress(ActivitiesProgressData saveRelatedProgress);

	void ClearActivitiesProgressView();

	void UpdateAvailableActivitiesView(ActivitiesProgressData saveRelatedProgressData, ActivitiesProgressData crossSaveProgressData);

	void UpdateResumedActivitiesView(ActivitiesProgressData progressData);

	void UpdateStartedActivitiesView(ActivitiesProgressData progressData);

	void UpdateEndedActivitiesView(ActivitiesProgressData progressData);
}
