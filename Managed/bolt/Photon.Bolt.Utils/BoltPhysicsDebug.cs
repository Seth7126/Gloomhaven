using Photon.Bolt.LagCompensation;
using UnityEngine;

namespace Photon.Bolt.Utils;

public class BoltPhysicsDebug : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		BoltPhysics.DrawSnapshot();
	}
}
