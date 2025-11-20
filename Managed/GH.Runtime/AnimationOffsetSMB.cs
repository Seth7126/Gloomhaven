using Chronos;
using SharedLibrary.Client;
using UnityEngine;

public class AnimationOffsetSMB : StateMachineBehaviour
{
	public bool randomized;

	[Range(0f, 1f)]
	public float normalizedTime;

	private bool _appliedAnimationOffset;

	public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
	{
		anim.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		if (!_appliedAnimationOffset)
		{
			_appliedAnimationOffset = true;
			anim.Play(stateInfo.fullPathHash, layerIndex, randomized ? ((float)SharedClient.GlobalRNG.NextDouble()) : normalizedTime);
		}
	}
}
