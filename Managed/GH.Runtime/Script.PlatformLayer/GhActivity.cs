using Platforms.Activities;
using UnityEngine;

namespace Script.PlatformLayer;

[CreateAssetMenu(fileName = "Activity", menuName = "Activities/Activity", order = 1)]
public class GhActivity : GhActivityBase, IActivity, IActivityBase
{
	[SerializeField]
	private GhTask[] _tasks;

	public ITask[] Tasks => _tasks;
}
