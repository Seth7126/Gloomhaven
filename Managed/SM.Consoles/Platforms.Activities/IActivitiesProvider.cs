namespace Platforms.Activities;

public interface IActivitiesProvider
{
	IActivity[] GetAllActivities();

	IChallenge[] GetAllChallenges();
}
