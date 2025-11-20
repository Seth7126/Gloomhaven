using Platforms.Activities;
using UnityEngine;

namespace Script.PlatformLayer;

[CreateAssetMenu(fileName = "Challenge", menuName = "Activities/Challenge", order = 4)]
public class GhChallenge : GhActivityBase, IChallenge, IActivityBase
{
}
