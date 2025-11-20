using System;
using Platforms.Activities;

namespace Script.PlatformLayer;

public class SampleActivitiesProvider : IActivitiesProvider
{
	private class FakeActivity : IActivity, IActivityBase
	{
		private readonly ITask[] _tasks = Array.Empty<ITask>();

		private readonly string[] _tags = Array.Empty<string>();

		private string _id;

		public string ID => _id;

		public bool SaveIndependent => true;

		public string[] FilterTags => _tags;

		public ITask[] Tasks => _tasks;

		public FakeActivity(string id)
		{
			_id = id;
		}
	}

	public static string[] Activities = new string[6] { "PhoenixPoint_Victory", "PROG_PX1_MISS", "PROG_PX10_MISS", "PROG_PX11", "PROG_PX12", "PROG_PX13_MISS" };

	private readonly IActivity[] _activities;

	private readonly IChallenge[] _challenges;

	public SampleActivitiesProvider()
	{
		_activities = new IActivity[Activities.Length];
		_challenges = Array.Empty<IChallenge>();
		for (int i = 0; i < Activities.Length; i++)
		{
			_activities[i] = new FakeActivity(Activities[i]);
		}
	}

	public IActivity[] GetAllActivities()
	{
		return _activities;
	}

	public IChallenge[] GetAllChallenges()
	{
		return _challenges;
	}
}
