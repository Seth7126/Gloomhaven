using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGenerateObjectPools
{
	public static List<GameObject> GlobalEffects => new List<GameObject>
	{
		GlobalSettings.Instance.m_DefaultHitEffects.DefaultStandardHitEffect,
		GlobalSettings.Instance.m_DefaultHitEffects.DefaultShieldHitEffect,
		GlobalSettings.Instance.m_DefaultHitEffects.DefaultCriticalHitEffect,
		GlobalSettings.Instance.m_GlobalParticles.DefaultHealEffect,
		GlobalSettings.Instance.m_GlobalParticles.DefaultDeathDissolve,
		GlobalSettings.Instance.m_GlobalParticles.DefaultPositiveCondition,
		GlobalSettings.Instance.m_GlobalParticles.DefaultNegativeCondition,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseStart,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseArrive,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseBits,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailBrown,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailDistort,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilityIdle,
		GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilitySmoke,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.AttackBuffTargetEffect,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.ShieldActiveBonusTargetEffect,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.RetaliateActiveBonusTargetEffect,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainShield,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainRetaliate,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainDisarm,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainImmobilize,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainPoison,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStun,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainWound,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainBless,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainCurse,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainStrengthen,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainMuddle,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainInvisibility,
		GlobalSettings.Instance.m_ActiveBonusBuffTargetEffects.GainAddTarget,
		GlobalSettings.Instance.m_MagicEffects.RetaliateHit,
		GlobalSettings.Instance.m_MagicEffects.RetaliateTarget,
		GlobalSettings.Instance.m_WaypointHolder,
		GlobalSettings.Instance.m_GenericHexStar
	};

	public static IEnumerator GenerateObjectPools(List<GameObject> players, List<GameObject> summons, List<GameObject> enemies)
	{
		foreach (GameObject player in players)
		{
			try
			{
				ObjectPool.CreatePool(player, 1);
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred while attempting to autogenerate the object pool.\n" + ex.Message + "\n" + ex.StackTrace);
			}
			yield return null;
		}
		foreach (GameObject summon in summons)
		{
			try
			{
				ObjectPool.CreatePool(summon, 1);
			}
			catch (Exception ex2)
			{
				Debug.LogError("An exception occurred while attempting to autogenerate the object pool.\n" + ex2.Message + "\n" + ex2.StackTrace);
			}
			yield return null;
		}
		foreach (GameObject enemy in enemies)
		{
			try
			{
				ObjectPool.CreatePool(enemy, 1);
			}
			catch (Exception ex3)
			{
				Debug.LogError("An exception occurred while attempting to autogenerate the object pool.\n" + ex3.Message + "\n" + ex3.StackTrace);
			}
			yield return null;
		}
		foreach (GameObject globalEffect in GlobalEffects)
		{
			try
			{
				if (globalEffect != null)
				{
					ObjectPool.CreatePool(globalEffect, 1);
				}
			}
			catch (Exception ex4)
			{
				Debug.LogError("An exception occurred while attempting to autogenerate the object pool.\n" + ex4.Message + "\n" + ex4.StackTrace);
			}
			yield return null;
		}
	}
}
