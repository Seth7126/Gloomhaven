#define DEBUG
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

internal abstract class NetworkProperty_Mecanim : NetworkProperty
{
	public MecanimMode MecanimMode;

	public MecanimDirection MecanimDirection;

	public float MecanimDamping;

	public int MecanimLayer;

	public override bool WantsOnSimulateAfter => MecanimMode != MecanimMode.Disabled;

	public void Settings_Mecanim(MecanimMode mode, MecanimDirection direction, float damping, int layer)
	{
		MecanimMode = mode;
		MecanimDirection = direction;
		MecanimDamping = damping;
		MecanimLayer = layer;
	}

	public override void OnSimulateAfter(NetworkObj obj)
	{
		if (MecanimMode == MecanimMode.Disabled)
		{
			return;
		}
		NetworkState networkState = (NetworkState)obj.Root;
		if (networkState.Animators.Count > 0)
		{
			if (MecanimMode == MecanimMode.LayerWeight)
			{
				if (ShouldPullDataFromMecanim(networkState))
				{
					PullMecanimLayer(networkState);
				}
				else
				{
					PushMecanimLayer(networkState);
				}
			}
			else if (ShouldPullDataFromMecanim(networkState))
			{
				PullMecanimValue(networkState);
			}
			else
			{
				PushMecanimValue(networkState);
			}
		}
		else if (networkState.MecanimWarningTimeout < Time.time)
		{
			BoltLog.Warn("You have not assigned a mecanim animator to the state on {0}, but you have properties set to use mecanim", networkState.Entity.UnityObject.gameObject.name);
			networkState.MecanimWarningTimeout = Time.time + 1f;
		}
	}

	protected bool ShouldPullDataFromMecanim(NetworkState state)
	{
		if (MecanimDirection == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl))
		{
			if (state.Animators.Count > 1)
			{
				BoltLog.Warn("Property '{0}' set to 'UsingAnimatorMethods' but several animators have been specified, only the first one added will be used.", PropertyName);
			}
			return true;
		}
		return false;
	}

	protected virtual void PullMecanimValue(NetworkState state)
	{
	}

	protected virtual void PushMecanimValue(NetworkState state)
	{
	}

	private void PullMecanimLayer(NetworkState state)
	{
		if (!(state.Animator == null) && state.Animator.gameObject.activeSelf)
		{
			float layerWeight = state.Animator.GetLayerWeight(MecanimLayer);
			float @float = state.Storage.Values[state[this]].Float0;
			state.Storage.Values[state[this]].Float0 = layerWeight;
			if (NetworkValue.Diff(layerWeight, @float))
			{
				state.Storage.PropertyChanged(state.OffsetProperties + OffsetProperties);
			}
		}
	}

	private void PushMecanimLayer(NetworkState state)
	{
		for (int i = 0; i < state.Animators.Count; i++)
		{
			if (state.Animators[i].gameObject.activeSelf)
			{
				float layerWeight = state.Animator.GetLayerWeight(MecanimLayer);
				float @float = state.Storage.Values[state[this]].Float0;
				if (NetworkValue.Diff(layerWeight, @float))
				{
					state.Animators[i].SetLayerWeight(MecanimLayer, @float);
				}
			}
		}
	}
}
