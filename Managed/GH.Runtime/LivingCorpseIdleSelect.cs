using SharedLibrary.Client;
using UnityEngine;

public class LivingCorpseIdleSelect : MonoBehaviour
{
	public int m_PercentTwitchChance = 25;

	public void TransitionToSecondIdle()
	{
		if (SharedClient.GlobalRNG.Next(0, 100) < m_PercentTwitchChance)
		{
			MF.GameObjectAnimatorPlay(base.transform.parent.gameObject, "Idle-Run2");
		}
	}
}
