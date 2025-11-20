using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public enum WeaponTypes
	{
		Melee,
		Projectile,
		Cosmetic
	}

	public WeaponTypes WeaponType;

	public GameObject OnHitAudioBucketList;

	public void PlayHitFX(CharacterManager.SurfaceTypes surface)
	{
		SFXBucket sFXBucket = OnHitAudioBucketList.GetComponent<SFXBucketList>().m_AudioBuckets.Select((GameObject x) => x.GetComponent<SFXBucket>()).ToList().SingleOrDefault((SFXBucket x) => x.m_SFXBucket.TargetSurface == surface);
		if (sFXBucket == null)
		{
			Debug.LogError("The weapon " + base.gameObject.name + " does not have an audio bucket definition for surface " + surface.ToString() + ". Unable to play audio clip.");
		}
		else
		{
			SFXBucket.PlayClip(sFXBucket, base.gameObject.transform);
		}
	}

	public void PlayFromBucket(GameObject bucketGO)
	{
		SFXBucket component = bucketGO.GetComponent<SFXBucket>();
		if (component == null)
		{
			Debug.LogError("Could not find SFXBucket Component on GameObject " + bucketGO.name);
		}
		else
		{
			SFXBucket.PlayClip(component, base.gameObject.transform);
		}
	}
}
