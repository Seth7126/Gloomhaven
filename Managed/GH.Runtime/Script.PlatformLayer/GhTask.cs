using Platforms.Activities;
using UnityEngine;

namespace Script.PlatformLayer;

[CreateAssetMenu(fileName = "Task", menuName = "Activities/Task", order = 2)]
public class GhTask : GhActivityBase, ITask, IActivityBase
{
	[SerializeField]
	private GhSubTask[] _subTasks;

	public ISubTask[] SubTasks => _subTasks;
}
