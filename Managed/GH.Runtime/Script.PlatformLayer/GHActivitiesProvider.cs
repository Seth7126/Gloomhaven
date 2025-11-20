using Platforms.Activities;

namespace Script.PlatformLayer;

public class GHActivitiesProvider : IActivitiesProvider
{
	private IActivity[] _activities;

	private IChallenge[] _challenges;

	public IActivity[] GetAllActivities()
	{
		if (_activities == null)
		{
			CollectLinkedActivities();
		}
		return _activities;
	}

	public IChallenge[] GetAllChallenges()
	{
		if (_challenges == null)
		{
			CollectLinkedActivities();
		}
		return _challenges;
	}

	private void CollectLinkedActivities()
	{
		GhActivity[] activities = ActivitiesLinker.Instance.Activities;
		_activities = new IActivity[activities.Length];
		for (int i = 0; i < activities.Length; i++)
		{
			_activities[i] = activities[i];
		}
		GhChallenge[] challenges = ActivitiesLinker.Instance.Challenges;
		_challenges = new IChallenge[challenges.Length];
		for (int j = 0; j < challenges.Length; j++)
		{
			_challenges[j] = challenges[j];
		}
	}
}
