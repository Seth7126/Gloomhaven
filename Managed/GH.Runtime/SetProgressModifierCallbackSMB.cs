using System.Collections;
using Chronos;
using UnityEngine;
using WorldspaceUI;

public class SetProgressModifierCallbackSMB : StateMachineBehaviour
{
	[Tooltip("Attack Modifiers Reveal Timeout in seconds after attack animation is started")]
	public float AttackModiersRevealTimeout;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Map)
		{
			CoroutineHelper.RunCoroutine(ProgressModifiers());
		}
	}

	public IEnumerator ProgressModifiers()
	{
		while (TimeManager.IsFrozen)
		{
			yield return null;
		}
		float num = AttackModiersRevealTimeout - GlobalSettings.Instance.m_AttackModifierSettings.RollingModDuration * (float)(AttackModBar.s_LongestAttackModSequence - 1);
		if (num >= 0f)
		{
			AttackModBar.s_FrozenForAttackModifiers = false;
			yield return new WaitForSecondsFrozen(num);
			AttackModBar.s_AttackModifierBarFlowCanBegin = true;
		}
		else
		{
			TimeManager.FreezeTime();
			AttackModBar.s_AttackModifierBarFlowCanBegin = true;
			yield return new WaitForSecondsFrozen(Mathf.Abs(num));
			TimeManager.UnfreezeTime();
			AttackModBar.s_FrozenForAttackModifiers = false;
		}
	}
}
