using Platforms.Activities;
using UnityEngine;

namespace Script.PlatformLayer;

[CreateAssetMenu(fileName = "SubTask", menuName = "Activities/SubTask", order = 3)]
public class GhSubTask : GhActivityBase, ISubTask, IActivityBase
{
}
