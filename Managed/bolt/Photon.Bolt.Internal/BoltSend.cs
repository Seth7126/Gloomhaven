using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt.Internal;

public class BoltSend : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void FixedUpdate()
	{
		BoltCore._timer.Stop();
		BoltCore._timer.Reset();
		BoltCore._timer.Start();
		BoltCore.Send();
		BoltCore._timer.Stop();
		DebugInfo.SendTime = DebugInfo.GetStopWatchElapsedMilliseconds(BoltCore._timer);
	}
}
